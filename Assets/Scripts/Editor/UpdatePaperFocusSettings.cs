using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to update paper focus settings in the AnnouncementBoard prefab.
/// Run via Tools > Update Paper Focus Settings
/// </summary>
public static class UpdatePaperFocusSettings
{
    [MenuItem("Tools/Update Paper Focus Settings")]
    public static void UpdateSettings()
    {
        // Find all MenuPaper components in the scene and prefabs
        var papers = Object.FindObjectsOfType<MenuPaper>(true);
        
        int updatedCount = 0;
        foreach (var paper in papers)
        {
            // Use SerializedObject to modify private serialized fields
            var serializedObject = new SerializedObject(paper);
            
            var focusDistanceProp = serializedObject.FindProperty("focusDistanceFromPlayer");
            var heightOffsetProp = serializedObject.FindProperty("focusHeightOffset");
            
            if (focusDistanceProp != null)
            {
                focusDistanceProp.floatValue = 0.4f;
                Debug.Log($"[UpdatePaperFocusSettings] Updated {paper.name} focusDistanceFromPlayer to 0.4");
            }
            
            if (heightOffsetProp != null)
            {
                heightOffsetProp.floatValue = -0.1f;
                Debug.Log($"[UpdatePaperFocusSettings] Updated {paper.name} focusHeightOffset to -0.1");
            }
            
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(paper);
            updatedCount++;
        }
        
        // Save all assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[UpdatePaperFocusSettings] Updated {updatedCount} papers. Remember to save the scene!");
    }
}
