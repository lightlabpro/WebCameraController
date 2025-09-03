using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class PhoneGyroCameraController : MonoBehaviour
{
    [Header("Server (match server.js)")]
    public string host = "192.168.0.18"; // your PC LAN IP running node
    public int port = 8080;
    public string path = "/ws";           // keep as /ws

    [Header("Smoothing")]
    [Range(0f, 30f)]
    public float rotationLerpSpeed = 12f; // higher = snappier

    [Header("Offsets")]
    public Vector3 eulerOffsetDegrees = Vector3.zero; // fine-tune if needed

    [Header("Debug")]
    public bool logMessages = false;

    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    private Quaternion _targetRot = Quaternion.identity;
    private bool _hasRot = false;

    // thread-safe latest message buffer
    private readonly byte[] _buffer = new byte[4096];

    [Serializable]
    private class OrientationMsg
    {
        public string type;
        public float[] q; // [x,y,z,w]
        public long t;
    }

    private async void Start()
    {
        Application.runInBackground = true;

        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        var uri = new Uri($"ws://{host}:{port}{path}");
        try
        {
            await _ws.ConnectAsync(uri, _cts.Token);
            if (logMessages) Debug.Log("[WS] Connected: " + uri);
            _ = Task.Run(ReceiveLoop); // fire-and-forget
        }
        catch (Exception e)
        {
            Debug.LogError("[WS] Connect failed: " + e.Message);
        }
    }

    private async Task ReceiveLoop()
    {
        while (_ws != null && _ws.State == WebSocketState.Open)
        {
            try
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(_buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    if (logMessages) Debug.Log("[WS] Server closed");
                    break;
                }

                string json = Encoding.UTF8.GetString(_buffer, 0, result.Count);
                if (logMessages) Debug.Log(json);

                var msg = JsonUtility.FromJson<OrientationMsg>(json);
                if (msg != null && msg.type == "orientation" && msg.q != null && msg.q.Length == 4)
                {
                    // Phone sends [x,y,z,w] in right-handed coordinates; Unity uses the same handedness for quats.
                    var phoneQ = new Quaternion(msg.q[0], msg.q[1], msg.q[2], msg.q[3]);

                    // Optional offset to resolve minor axis differences or desired framing
                    var offset = Quaternion.Euler(eulerOffsetDegrees);

                    _targetRot = offset * phoneQ;
                    _hasRot = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[WS] Receive error: " + e.Message);
                await Task.Delay(200);
            }
        }
    }

    private void Update()
    {
        if (_hasRot)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                _targetRot,
                1f - Mathf.Exp(-rotationLerpSpeed * Time.deltaTime)
            );
        }
    }

    private async void OnApplicationQuit()
    {
        try
        {
            _cts?.Cancel();
            if (_ws != null && (_ws.State == WebSocketState.Open || _ws.State == WebSocketState.CloseReceived))
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
            }
        }
        catch { /* ignore */ }
    }
}
