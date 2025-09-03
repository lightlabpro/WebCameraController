using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class GitHubPagesController : MonoBehaviour
{
    [Header("GitHub Pages Controller")]
    [Tooltip("Paste your GitHub Pages URL here")]
    [TextArea(2, 3)]
    public string controllerUrl = "https://yourusername.github.io/unity-camera-controller";
    
    [Header("Setup Instructions")]
    [TextArea(4, 8)]
    public string setupInstructions = @"1. Create a GitHub repository (make it public)
2. Upload the 'github-controller.html' file as 'index.html'
3. Enable GitHub Pages in repository settings
4. Copy your GitHub Pages URL and paste it above
5. Your URL will look like: https://username.github.io/repo-name";
    
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
    
    private void Start()
    {
        Application.runInBackground = true;
        
        // Check if URL is still the default
        if (controllerUrl.Contains("yourusername"))
        {
            Debug.LogWarning("⚠️ Please update the controllerUrl with your actual GitHub Pages URL!");
            Debug.LogWarning("See the setup instructions in the inspector for help.");
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
        GUILayout.Label("GitHub Pages Controller Status", GUI.skin.box);
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
        
        if (GUILayout.Button("Open Setup Guide"))
        {
            Application.OpenURL("https://github.com/yourusername/unity-camera-controller#setup-guide");
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
