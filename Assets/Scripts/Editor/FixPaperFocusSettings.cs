using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to fix paper focus settings in scene instances.
/// Run via Tools > Fix Paper Focus Settings
/// </summary>
public static class FixPaperFocusSettings
{
    [MenuItem("Tools/Fix Paper Focus Settings")]
    public static void FixSettings()
    {
        // Find all MenuPaper instances in the scene
        var papers = Object.FindObjectsOfType<MenuPaper>(true);
        
        Debug.Log($"[FixPaperFocusSettings] Found {papers.Length} MenuPaper instances");
        
        foreach (var paper in papers)
        {
            // Use SerializedObject to modify private serialized fields
            var so = new SerializedObject(paper);
            
            var distanceProp = so.FindProperty("focusDistanceFromPlayer");
            var heightProp = so.FindProperty("focusHeightOffset");
            
            if (distanceProp != null && heightProp != null)
            {
                float oldDistance = distanceProp.floatValue;
                float oldHeight = heightProp.floatValue;
                
                // Set new values - 0.6m is a good reading distance, -0.15 puts it slightly below eye level
                distanceProp.floatValue = 0.6f;
                heightProp.floatValue = -0.15f;
                
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(paper);
                
                Debug.Log($"[FixPaperFocusSettings] Updated {paper.name}: distance {oldDistance} -> 0.6, height {oldHeight} -> -0.15");
            }
            else
            {
                Debug.LogWarning($"[FixPaperFocusSettings] Could not find properties on {paper.name}");
            }
        }
        
        // Mark scene dirty so it can be saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("[FixPaperFocusSettings] Done! Save the scene to persist changes.");
    }
    
    [MenuItem("Tools/Debug Paper Orientation")]
    public static void DebugPaperOrientation()
    {
        // Find all MenuPaper instances
        var papers = Object.FindObjectsOfType<MenuPaper>(true);
        
        foreach (var paper in papers)
        {
            Debug.Log($"[Paper: {paper.name}]");
            Debug.Log($"  Position: {paper.transform.position}");
            Debug.Log($"  Rotation: {paper.transform.rotation.eulerAngles}");
            Debug.Log($"  Forward (+Z): {paper.transform.forward}");
            Debug.Log($"  Back (-Z): {-paper.transform.forward}");
            
            // Check for content root
            var contentRoot = paper.ContentRoot;
            if (contentRoot != null)
            {
                Debug.Log($"  ContentRoot local position: {contentRoot.localPosition}");
                Debug.Log($"  ContentRoot local Z: {contentRoot.localPosition.z}");
            }
            
            // Check for placeholder
            var placeholder = paper.PlaceholderContent;
            if (placeholder != null)
            {
                Debug.Log($"  Placeholder local position: {placeholder.localPosition}");
                Debug.Log($"  Placeholder local Z: {placeholder.localPosition.z}");
            }
        }
    }
}
