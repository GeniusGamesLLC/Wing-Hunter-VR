using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Helper utility to check Unity compilation status and wait for completion
/// </summary>
public static class UnityCompilationHelper
{
    /// <summary>
    /// Check if Unity is currently compiling
    /// </summary>
    public static bool IsCompiling()
    {
        return EditorApplication.isCompiling;
    }
    
    /// <summary>
    /// Wait for Unity compilation to complete
    /// </summary>
    public static void WaitForCompilation()
    {
        if (EditorApplication.isCompiling)
        {
            Debug.Log("Unity is compiling... waiting for completion.");
            
            // Register callback to know when compilation finishes
            EditorApplication.update += CheckCompilationStatus;
        }
        else
        {
            Debug.Log("Unity compilation is up to date.");
        }
    }
    
    private static void CheckCompilationStatus()
    {
        if (!EditorApplication.isCompiling)
        {
            Debug.Log("Unity compilation completed.");
            EditorApplication.update -= CheckCompilationStatus;
        }
    }
    
    /// <summary>
    /// Force Unity to refresh and recompile
    /// </summary>
    public static void ForceRefresh()
    {
        AssetDatabase.Refresh();
        Debug.Log("Forced Unity asset database refresh.");
    }
}
#endif