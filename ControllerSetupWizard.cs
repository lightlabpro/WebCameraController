using UnityEngine;
using UnityEditor;

public class ControllerSetupWizard : EditorWindow
{
    private string repositoryName = "unity-camera-controller";
    private string username = "";
    private string generatedUrl = "";
    
    [MenuItem("Tools/Phone Controller Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<ControllerSetupWizard>("Controller Setup");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("📱 Phone Controller Setup Wizard", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Step 1: Enter your GitHub details", EditorStyles.boldLabel);
        username = EditorGUILayout.TextField("GitHub Username:", username);
        repositoryName = EditorGUILayout.TextField("Repository Name:", repositoryName);
        
        GUILayout.Space(10);
        
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(repositoryName))
        {
            generatedUrl = $"https://{username}.github.io/{repositoryName}";
            GUILayout.Label("Generated URL:", EditorStyles.boldLabel);
            EditorGUILayout.TextField(generatedUrl);
            
            if (GUILayout.Button("Copy URL to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = generatedUrl;
                Debug.Log($"Copied URL: {generatedUrl}");
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Open GitHub Pages Setup Guide"))
            {
                Application.OpenURL("https://docs.github.com/en/pages/quickstart");
            }
        }
        
        GUILayout.Space(20);
        
        GUILayout.Label("Step 2: Setup Instructions", EditorStyles.boldLabel);
        GUILayout.Label("1. Create a new GitHub repository (make it public)", EditorStyles.wordWrappedLabel);
        GUILayout.Label("2. Upload 'github-controller.html' as 'index.html'", EditorStyles.wordWrappedLabel);
        GUILayout.Label("3. Go to Settings → Pages → Deploy from branch", EditorStyles.wordWrappedLabel);
        GUILayout.Label("4. Select 'main' branch and '/(root)' folder", EditorStyles.wordWrappedLabel);
        GUILayout.Label("5. Wait a few minutes for deployment", EditorStyles.wordWrappedLabel);
        GUILayout.Label("6. Copy the generated URL above", EditorStyles.wordWrappedLabel);
        GUILayout.Label("7. Paste it in your Unity script", EditorStyles.wordWrappedLabel);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create README with Instructions"))
        {
            CreateReadmeFile();
        }
    }
    
    private void CreateReadmeFile()
    {
        string readmeContent = @"# Unity Phone Camera Controller

A web-based camera controller for Unity that uses your phone's gyroscope for handheld camera motion.

## Quick Setup

1. **Create GitHub Repository**
   - Go to [github.com](https://github.com) and create a new repository
   - Make it **public** (required for GitHub Pages)
   - Name it something like `unity-camera-controller`

2. **Upload Controller File**
   - Download `github-controller.html` from this project
   - Rename it to `index.html`
   - Upload it to your repository

3. **Enable GitHub Pages**
   - Go to repository → Settings → Pages
   - Source: Deploy from a branch
   - Branch: main, Folder: /(root)
   - Click Save

4. **Get Your URL**
   - Your controller will be at: `https://yourusername.github.io/your-repo-name`
   - Copy this URL

5. **Use in Unity**
   - Add `GitHubPagesController.cs` to your camera
   - Paste your URL in the `controllerUrl` field
   - Run the scene!

## How It Works

- **Phone**: Opens the web page and broadcasts orientation data
- **Unity**: Polls the URL every frame to get fresh data
- **Camera**: Moves based on phone orientation with smooth interpolation

## Features

- ✅ Real-time 60fps orientation tracking
- ✅ Smooth camera movement with configurable smoothing
- ✅ Calibration system for setting neutral position
- ✅ Works on iOS and Android
- ✅ No server setup required
- ✅ Always accessible via web

## Troubleshooting

- **Permission Denied**: Make sure to grant motion permissions on your phone
- **No Connection**: Check that your GitHub Pages URL is correct
- **Jerky Movement**: Adjust the `rotationLerpSpeed` value in Unity

## Files

- `github-controller.html` - Phone controller (upload as `index.html`)
- `GitHubPagesController.cs` - Unity script for camera control
- `ControllerSetupWizard.cs` - Editor tool for easy setup

## License

MIT License - feel free to use in your projects!
";
        
        string path = EditorUtility.SaveFilePanel("Save README", Application.dataPath, "README", "md");
        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllText(path, readmeContent);
            Debug.Log($"README saved to: {path}");
            AssetDatabase.Refresh();
        }
    }
}
