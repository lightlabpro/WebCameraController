using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class UniversalController : MonoBehaviour
{
    [Header("Universal Controller")]
    [Tooltip("This controller connects automatically - no setup needed!")]
    public string controllerUrl = "https://yourusername.github.io/unity-camera-controller";
    
    [Header("Smoothing")]
    [Range(0f, 30f)]
    public float rotationLerpSpeed = 12f;
    
    [Header("Offsets")]
    public Vector3 eulerOffsetDegrees = Vector3.zero;
    
    [Header("Debug")]
    public bool logMessages = false;
    public bool showDebugInfo = true;
    
    private Quaternion _targetRot = Quaternion.identity;
    private bool _hasRot = false;
    private Coroutine _pollingCoroutine;
    private string _lastResponse = "";
    private bool _urlWarningShown = false;
    
    private void Start()
    {
        Application.runInBackground = true;
        
        // Check if URL needs to be updated
        if (controllerUrl.Contains("yourusername") && !_urlWarningShown)
        {
            Debug.LogWarning("⚠️ IMPORTANT: You need to update the controllerUrl!");
            Debug.LogWarning("1. Upload 'universal-controller.html' to GitHub as 'index.html'");
            Debug.LogWarning("2. Enable GitHub Pages in repository settings");
            Debug.LogWarning("3. Update the controllerUrl with your actual URL");
            _urlWarningShown = true;
        }
        
        StartPolling();
    }
    
    private void StartPolling()
    {
        if (_pollingCoroutine != null)
            StopCoroutine(_pollingCoroutine);
            
        _pollingCoroutine = StartCoroutine(PollOrientationData());
    }
    
    private IEnumerator PollOrientationData()
    {
        while (true)
        {
            yield return StartCoroutine(GetOrientationData());
            yield return new WaitForSeconds(0.016f); // ~60fps
        }
    }
    
    private IEnumerator GetOrientationData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(controllerUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                _lastResponse = response;
                
                // Look for orientation data in the HTML response
                if (response.Contains("Orientation:") && response.Contains("["))
                {
                    try
                    {
                        // Find the quaternion data in the HTML
                        int startIndex = response.IndexOf("Orientation: [") + 14;
                        int endIndex = response.IndexOf("]", startIndex);
                        
                        if (startIndex > 13 && endIndex > startIndex)
                        {
                            string qData = response.Substring(startIndex, endIndex - startIndex);
                            string[] qValues = qData.Split(',');
                            
                            if (qValues.Length == 4)
                            {
                                float[] q = new float[4];
                                for (int i = 0; i < 4; i++)
                                {
                                    if (float.TryParse(qValues[i].Trim(), out float val))
                                        q[i] = val;
                                }
                                
                                // Apply the orientation data
                                var phoneQ = new Quaternion(q[0], q[1], q[2], q[3]);
                                var offset = Quaternion.Euler(eulerOffsetDegrees);
                                _targetRot = offset * phoneQ;
                                _hasRot = true;
                                
                                if (logMessages)
                                    Debug.Log($"Received orientation: [{q[0]:F4}, {q[1]:F4}, {q[2]:F4}, {q[3]:F4}]");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (logMessages)
                            Debug.LogWarning($"Failed to parse orientation data: {e.Message}");
                    }
                }
            }
            else
            {
                if (logMessages)
                    Debug.LogWarning($"Failed to get orientation data: {request.error}");
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
    
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 450, 300));
        GUILayout.Label("Universal Controller Status", GUI.skin.box);
        GUILayout.Label($"Connected: {_hasRot}");
        GUILayout.Label($"Target Rotation: {_targetRot.eulerAngles}");
        GUILayout.Label($"Current Rotation: {transform.rotation.eulerAngles}");
        GUILayout.Label($"Controller URL: {controllerUrl}");
        
        if (GUILayout.Button("Restart Polling"))
        {
            StartPolling();
        }
        
        if (GUILayout.Button("Copy URL to Clipboard"))
        {
            GUIUtility.systemCopyBuffer = controllerUrl;
        }
        
        if (GUILayout.Button("Open Controller"))
        {
            Application.OpenURL(controllerUrl);
        }
        
        GUILayout.Label("Last Response Preview:");
        GUILayout.TextArea(_lastResponse.Length > 200 ? _lastResponse.Substring(0, 200) + "..." : _lastResponse, GUILayout.Height(60));
        
        GUILayout.EndArea();
    }
    
    private void OnApplicationQuit()
    {
        if (_pollingCoroutine != null)
            StopCoroutine(_pollingCoroutine);
    }
}
