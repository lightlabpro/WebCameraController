using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class SimplePhoneController : MonoBehaviour
{
    [Header("Phone Controller")]
    public string phoneUrl = "http://192.168.1.100:8080/simple-controller.html"; // Your phone's IP
    
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
    
    [Serializable]
    private class OrientationMsg
    {
        public string type;
        public float[] q; // [x,y,z,w]
        public long t;
    }
    
    private void Start()
    {
        Application.runInBackground = true;
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
        using (UnityWebRequest request = UnityWebRequest.Get(phoneUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse the HTML response to extract orientation data
                // This is a simplified approach - you might want to create a proper API endpoint
                string response = request.downloadHandler.text;
                
                // Look for orientation data in the response
                if (response.Contains("orientation") && response.Contains("q:"))
                {
                    // Extract quaternion values from the HTML response
                    // This is a basic parsing approach
                    try
                    {
                        // Find the quaternion data in the HTML
                        int startIndex = response.IndexOf("q: [") + 4;
                        int endIndex = response.IndexOf("]", startIndex);
                        
                        if (startIndex > 3 && endIndex > startIndex)
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
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Phone Controller Status", GUI.skin.box);
        GUILayout.Label($"Connected: {_hasRot}");
        GUILayout.Label($"Target Rotation: {_targetRot.eulerAngles}");
        GUILayout.Label($"Current Rotation: {transform.rotation.eulerAngles}");
        GUILayout.Label($"Phone URL: {phoneUrl}");
        
        if (GUILayout.Button("Restart Polling"))
        {
            StartPolling();
        }
        
        GUILayout.EndArea();
    }
    
    private void OnApplicationQuit()
    {
        if (_pollingCoroutine != null)
            StopCoroutine(_pollingCoroutine);
    }
}
