using UnityEngine;

namespace DuckHunt.Data
{
    /// <summary>
    /// Configuration asset for the Spawn Point System.
    /// Centralizes settings for indicators, visibility, and occluders.
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnPointConfig", menuName = "Game/Spawn Point Config")]
    public class SpawnPointConfig : ScriptableObject
    {
        [Header("Indicator Settings")]
        [Tooltip("Material used for spawn point indicators")]
        public Material SpawnIndicatorMaterial;
        
        [Tooltip("Material used for target point indicators")]
        public Material TargetIndicatorMaterial;
        
        [Tooltip("Scale of indicator spheres")]
        [Range(0.1f, 1f)]
        public float IndicatorScale = 0.3f;
        
        [Header("Visibility Settings")]
        [Tooltip("Show indicators in editor mode")]
        public bool ShowIndicatorsInEditor = true;
        
        [Tooltip("Show indicators in play mode (normally hidden)")]
        public bool ShowIndicatorsInPlayMode = false;
        
        [Tooltip("Debug override - shows indicators even when normally hidden")]
        public bool DebugShowIndicators = false;
        
        [Header("Occluder Settings")]
        [Tooltip("Default prefab to use for occluders (clouds, bushes, etc.)")]
        public GameObject DefaultOccluderPrefab;
        
        [Tooltip("Position offset for occluders relative to spawn point")]
        public Vector3 OccluderOffset = Vector3.zero;
        
        [Tooltip("Scale multiplier for occluders")]
        [Range(0.1f, 5f)]
        public float OccluderScale = 1f;
    }
}
