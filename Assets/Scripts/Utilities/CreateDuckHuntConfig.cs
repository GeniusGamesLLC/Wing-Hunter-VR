using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility script to create DuckHuntConfig asset
/// </summary>
public class CreateDuckHuntConfig : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Create DuckHuntConfig Asset")]
    public void CreateConfigAsset()
    {
        // Create the ScriptableObject instance
        DuckHuntConfig config = ScriptableObject.CreateInstance<DuckHuntConfig>();
        
        // Set default values as specified in the task
        config.PointsPerDuck = 10;
        config.MaxMissedDucks = 10;
        config.RaycastDistance = 100f;
        config.SoundEffectVolume = 0.8f;
        config.HapticIntensity = 0.6f;
        
        // Create the asset
        string assetPath = "Assets/Data/DuckHuntConfig.asset";
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select the created asset
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
        
        Debug.Log($"DuckHuntConfig asset created at: {assetPath}");
    }
#endif
}