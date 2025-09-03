# 🚀 Universal Unity Camera Controller - Setup Guide

## **Goal: One URL that works for everyone!**

This setup creates a **central controller** that anyone can use without any configuration.

## **Step 1: Host the Controller (You do this once)**

1. **Create a GitHub repository**:
   - Go to [github.com](https://github.com) and create a new repository
   - Make it **public** (required for GitHub Pages)
   - Name it something like `unity-camera-controller`

2. **Upload the controller**:
   - Download `universal-controller.html` from this project
   - **Rename it to `index.html`** (this is important!)
   - Upload it to your repository

3. **Enable GitHub Pages**:
   - Go to repository → **Settings** → **Pages**
   - Source: **Deploy from a branch**
   - Branch: **main**, Folder: **/(root)**
   - Click **Save**

4. **Get your URL**:
   - Your controller will be at: `https://yourusername.github.io/unity-camera-controller`
   - **This is the URL everyone will use!**

## **Step 2: Update the Unity Script (You do this once)**

1. **Open `UniversalController.cs`**
2. **Change this line**:
   ```csharp
   public string controllerUrl = "https://yourusername.github.io/unity-camera-controller";
   ```
3. **To your actual URL**:
   ```csharp
   public string controllerUrl = "https://yourusername.github.io/unity-camera-controller";
   ```

## **Step 3: Share with Users (They do nothing!)**

Users just need to:
1. **Download your Unity project**
2. **Add `UniversalController.cs` to their camera**
3. **Run the scene** - it works immediately!

## **What This Gives You:**

✅ **One URL for everyone**  
✅ **No setup required for users**  
✅ **Always accessible** via web  
✅ **Professional sharing**  
✅ **Automatic updates** when you push changes  

## **File Structure:**

```
Your Repository/
├── index.html (renamed from universal-controller.html)
└── README.md
```

## **User Experience:**

1. **User downloads your project**
2. **Adds script to camera**
3. **Runs scene**
4. **Opens phone to your URL**
5. **Moves phone to control camera**

**That's it! No configuration, no setup, no IP addresses.**

## **Troubleshooting:**

- **Controller not working**: Make sure you renamed the file to `index.html`
- **Pages not deploying**: Wait a few minutes after enabling GitHub Pages
- **URL not accessible**: Check that your repository is public

## **Sharing:**

Once set up, users can just download your Unity project and it works immediately. The controller URL is hardcoded, so they don't need to change anything!

---

**This creates a truly universal tool that works for everyone with zero setup!** 🎉
