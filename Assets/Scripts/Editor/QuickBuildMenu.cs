using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Adds quick build options to the File menu for faster iteration.
/// </summary>
public static class QuickBuildMenu
{
    private const string DevModeKey = "QuickBuild_DevMode";
    
    private static bool IsDevMode
    {
        get => EditorPrefs.GetBool(DevModeKey, true);
        set => EditorPrefs.SetBool(DevModeKey, value);
    }
    
    [MenuItem("File/Patch and Run %&b", false, 212)]
    public static void PatchAndRun()
    {
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = "Build.apk",
            target = BuildTarget.Android,
            options = BuildOptions.AutoRunPlayer | BuildOptions.Development | BuildOptions.PatchPackage
        };
        
        Debug.Log("[QuickBuild] Starting Patch and Run...");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[QuickBuild] Patch and Run succeeded in {report.summary.totalTime.TotalSeconds:F1}s");
        }
        else
        {
            Debug.LogError($"[QuickBuild] Build failed: {report.summary.result}");
        }
    }
    
    [MenuItem("File/Build and Run (Dev) %#b", false, 213)]
    public static void BuildAndRunDev()
    {
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = "Build.apk",
            target = BuildTarget.Android,
            options = BuildOptions.AutoRunPlayer | BuildOptions.Development
        };
        
        Debug.Log("[QuickBuild] Starting Development Build and Run...");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[QuickBuild] Build and Run succeeded in {report.summary.totalTime.TotalSeconds:F1}s");
        }
        else
        {
            Debug.LogError($"[QuickBuild] Build failed: {report.summary.result}");
        }
    }
    
    // --- Build Mode Toggle ---
    
    [MenuItem("File/Build Settings/Switch to Dev Mode (Faster Builds)", false, 230)]
    public static void SwitchToDevMode()
    {
        IsDevMode = true;
        ApplyDevModeSettings();
        Debug.Log("[QuickBuild] Switched to DEV MODE - faster builds, larger APK");
    }
    
    [MenuItem("File/Build Settings/Switch to Dev Mode (Faster Builds)", true)]
    public static bool SwitchToDevModeValidate() => !IsDevMode;
    
    [MenuItem("File/Build Settings/Switch to Release Mode (Optimized)", false, 231)]
    public static void SwitchToReleaseMode()
    {
        IsDevMode = false;
        ApplyReleaseModeSettings();
        Debug.Log("[QuickBuild] Switched to RELEASE MODE - slower builds, optimized APK");
    }
    
    [MenuItem("File/Build Settings/Switch to Release Mode (Optimized)", true)]
    public static bool SwitchToReleaseModeValidate() => IsDevMode;

    
    [MenuItem("File/Build Settings/Show Current Mode", false, 240)]
    public static void ShowCurrentMode()
    {
        string mode = IsDevMode ? "DEV MODE (faster builds)" : "RELEASE MODE (optimized)";
        Debug.Log($"[QuickBuild] Current build mode: {mode}");
        EditorUtility.DisplayDialog("Build Mode", $"Current mode: {mode}", "OK");
    }
    
    /// <summary>
    /// Applies settings optimized for fast iteration during development.
    /// </summary>
    private static void ApplyDevModeSettings()
    {
        // Managed Stripping: Minimal (faster builds)
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Minimal);
        
        // Strip Engine Code: Disabled (faster builds)
        PlayerSettings.stripEngineCode = false;
        
        // Ensure IL2CPP and ARM64 for Quest
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        
        AssetDatabase.SaveAssets();
        
        Debug.Log("[QuickBuild] Dev mode settings applied:");
        Debug.Log("  - Managed Stripping: Minimal");
        Debug.Log("  - Strip Engine Code: Disabled");
    }
    
    /// <summary>
    /// Applies settings optimized for release builds (smaller APK, better performance).
    /// </summary>
    private static void ApplyReleaseModeSettings()
    {
        // Managed Stripping: High (smaller APK)
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.High);
        
        // Strip Engine Code: Enabled (smaller APK)
        PlayerSettings.stripEngineCode = true;
        
        // Ensure IL2CPP and ARM64 for Quest
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        
        AssetDatabase.SaveAssets();
        
        Debug.Log("[QuickBuild] Release mode settings applied:");
        Debug.Log("  - Managed Stripping: High");
        Debug.Log("  - Strip Engine Code: Enabled");
    }
    
    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var enabledScenes = new System.Collections.Generic.List<string>();
        
        foreach (var scene in scenes)
        {
            if (scene.enabled)
            {
                enabledScenes.Add(scene.path);
            }
        }
        
        return enabledScenes.ToArray();
    }
}
