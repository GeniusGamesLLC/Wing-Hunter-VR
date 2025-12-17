using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DuckHunt.Data;

/// <summary>
/// Central manager for all spawn and target points in the scene.
/// Handles point discovery, indexed access, and indicator visibility control.
/// </summary>
public class SpawnPointManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Configuration asset for spawn point settings")]
    [SerializeField] private SpawnPointConfig config;
    
    // Point collections (auto-populated at runtime from parent containers)
    private SpawnPointMarker[] spawnPoints;
    private SpawnPointMarker[] targetPoints;
    
    [Header("Visibility Settings")]
    [Tooltip("Whether indicators are currently visible")]
    [SerializeField] private bool showIndicators = true;
    
    [Tooltip("Debug mode - shows indicators even during play mode")]
    [SerializeField] private bool debugShowIndicators = false;
    
    [Header("Parent References")]
    [Tooltip("Parent transform for spawn points (auto-discovered if null)")]
    [SerializeField] private Transform spawnPointsParent;
    
    [Tooltip("Parent transform for target points (auto-discovered if null)")]
    [SerializeField] private Transform targetPointsParent;
    
    /// <summary>
    /// Gets or sets the configuration asset
    /// </summary>
    public SpawnPointConfig Config
    {
        get => config;
        set => config = value;
    }
    
    /// <summary>
    /// Array of all spawn point markers in the scene
    /// </summary>
    public SpawnPointMarker[] SpawnPoints => spawnPoints;
    
    /// <summary>
    /// Array of all target point markers in the scene
    /// </summary>
    public SpawnPointMarker[] TargetPoints => targetPoints;
    
    /// <summary>
    /// Gets or sets whether indicators are visible
    /// </summary>
    public bool ShowIndicators
    {
        get => showIndicators;
        set
        {
            showIndicators = value;
            SetAllIndicatorsVisible(value);
        }
    }
    
    /// <summary>
    /// Gets or sets debug mode - when enabled, shows indicators even during play mode.
    /// Can be toggled from a settings menu in VR.
    /// </summary>
    public bool DebugShowIndicators
    {
        get => debugShowIndicators;
        set
        {
            debugShowIndicators = value;
            if (Application.isPlaying)
            {
                SetAllIndicatorsVisible(value);
                showIndicators = value;
            }
        }
    }
    
    private void Awake()
    {
        RefreshPointLists();
    }
    
    private void Start()
    {
        // Apply visibility settings from config or defaults
        if (Application.isPlaying)
        {
            bool shouldShow = false;
            
            // Check config settings if available
            if (config != null)
            {
                shouldShow = config.ShowIndicatorsInPlayMode || config.DebugShowIndicators;
                debugShowIndicators = config.DebugShowIndicators;
                
                // Spawn occluders if configured
                if (config.DefaultOccluderPrefab != null)
                {
                    SpawnAllOccluders();
                }
            }
            
            SetAllIndicatorsVisible(shouldShow);
            showIndicators = shouldShow;
        }
        else if (config != null)
        {
            // In editor mode, use config setting
            SetAllIndicatorsVisible(config.ShowIndicatorsInEditor);
            showIndicators = config.ShowIndicatorsInEditor;
        }
    }
    
    /// <summary>
    /// Spawns occluders on all spawn and target points using the default occluder prefab from config
    /// </summary>
    public void SpawnAllOccluders()
    {
        if (config == null || config.DefaultOccluderPrefab == null)
        {
            Debug.LogWarning("[SpawnPointManager] Cannot spawn occluders - config or prefab is null");
            return;
        }
        
        int count = 0;
        
        // Spawn on spawn points
        if (spawnPoints != null)
        {
            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    point.SetOccluder(config.DefaultOccluderPrefab);
                    
                    // Apply scale from config
                    if (point.OccluderInstance != null && config.OccluderScale != 1f)
                    {
                        point.OccluderInstance.transform.localScale *= config.OccluderScale;
                    }
                    
                    count++;
                }
            }
        }
        
        // Spawn on target points
        if (targetPoints != null)
        {
            foreach (var point in targetPoints)
            {
                if (point != null)
                {
                    point.SetOccluder(config.DefaultOccluderPrefab);
                    
                    // Apply scale from config
                    if (point.OccluderInstance != null && config.OccluderScale != 1f)
                    {
                        point.OccluderInstance.transform.localScale *= config.OccluderScale;
                    }
                    
                    count++;
                }
            }
        }
        
        Debug.Log($"[SpawnPointManager] Spawned {count} occluders on spawn and target points");
    }
    
    /// <summary>
    /// Clears all occluders from spawn and target points
    /// </summary>
    public void ClearAllOccluders()
    {
        if (spawnPoints != null)
        {
            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    point.ClearOccluder();
                }
            }
        }
        
        if (targetPoints != null)
        {
            foreach (var point in targetPoints)
            {
                if (point != null)
                {
                    point.ClearOccluder();
                }
            }
        }
        
        Debug.Log("[SpawnPointManager] Cleared all occluders");
    }

    /// <summary>
    /// Refreshes the spawn and target point lists by finding all SpawnPointMarker components
    /// </summary>
    public void RefreshPointLists()
    {
        // Auto-discover parent containers if not assigned
        if (spawnPointsParent == null)
        {
            GameObject spawnParent = GameObject.Find("SpawnPoints");
            if (spawnParent != null)
            {
                spawnPointsParent = spawnParent.transform;
            }
        }
        
        if (targetPointsParent == null)
        {
            GameObject targetParent = GameObject.Find("TargetPoints");
            if (targetParent != null)
            {
                targetPointsParent = targetParent.transform;
            }
        }
        
        // Find all spawn points
        List<SpawnPointMarker> foundSpawnPoints = new List<SpawnPointMarker>();
        if (spawnPointsParent != null)
        {
            SpawnPointMarker[] markers = spawnPointsParent.GetComponentsInChildren<SpawnPointMarker>(true);
            foreach (var marker in markers)
            {
                if (marker.PointType == SpawnPointType.Spawn)
                {
                    foundSpawnPoints.Add(marker);
                }
            }
        }
        else
        {
            // Fallback: find all spawn markers in scene
            SpawnPointMarker[] allMarkers = FindObjectsOfType<SpawnPointMarker>(true);
            foreach (var marker in allMarkers)
            {
                if (marker.PointType == SpawnPointType.Spawn)
                {
                    foundSpawnPoints.Add(marker);
                }
            }
        }
        
        // Sort by index for consistent ordering
        spawnPoints = foundSpawnPoints.OrderBy(p => p.PointIndex).ToArray();
        
        // Find all target points
        List<SpawnPointMarker> foundTargetPoints = new List<SpawnPointMarker>();
        if (targetPointsParent != null)
        {
            SpawnPointMarker[] markers = targetPointsParent.GetComponentsInChildren<SpawnPointMarker>(true);
            foreach (var marker in markers)
            {
                if (marker.PointType == SpawnPointType.Target)
                {
                    foundTargetPoints.Add(marker);
                }
            }
        }
        else
        {
            // Fallback: find all target markers in scene
            SpawnPointMarker[] allMarkers = FindObjectsOfType<SpawnPointMarker>(true);
            foreach (var marker in allMarkers)
            {
                if (marker.PointType == SpawnPointType.Target)
                {
                    foundTargetPoints.Add(marker);
                }
            }
        }
        
        // Sort by index for consistent ordering
        targetPoints = foundTargetPoints.OrderBy(p => p.PointIndex).ToArray();
        
        Debug.Log($"[SpawnPointManager] Discovered {spawnPoints.Length} spawn points and {targetPoints.Length} target points");
    }
    
    /// <summary>
    /// Gets a spawn point by its index
    /// </summary>
    /// <param name="index">The index of the spawn point</param>
    /// <returns>The spawn point marker, or null if not found</returns>
    public SpawnPointMarker GetSpawnPoint(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"[SpawnPointManager] No spawn points available");
            return null;
        }
        
        // Find by PointIndex property
        foreach (var point in spawnPoints)
        {
            if (point != null && point.PointIndex == index)
            {
                return point;
            }
        }
        
        Debug.LogWarning($"[SpawnPointManager] Spawn point with index {index} not found");
        return null;
    }
    
    /// <summary>
    /// Gets a target point by its index
    /// </summary>
    /// <param name="index">The index of the target point</param>
    /// <returns>The target point marker, or null if not found</returns>
    public SpawnPointMarker GetTargetPoint(int index)
    {
        if (targetPoints == null || targetPoints.Length == 0)
        {
            Debug.LogWarning($"[SpawnPointManager] No target points available");
            return null;
        }
        
        // Find by PointIndex property
        foreach (var point in targetPoints)
        {
            if (point != null && point.PointIndex == index)
            {
                return point;
            }
        }
        
        Debug.LogWarning($"[SpawnPointManager] Target point with index {index} not found");
        return null;
    }
    
    /// <summary>
    /// Gets a paired spawn and target point by index
    /// </summary>
    /// <param name="index">The index of the point pair</param>
    /// <returns>A tuple containing the spawn and target point markers</returns>
    public (SpawnPointMarker spawn, SpawnPointMarker target) GetPointPair(int index)
    {
        SpawnPointMarker spawn = GetSpawnPoint(index);
        SpawnPointMarker target = GetTargetPoint(index);
        
        if (spawn == null || target == null)
        {
            Debug.LogWarning($"[SpawnPointManager] Incomplete point pair at index {index}. Spawn: {(spawn != null ? "found" : "missing")}, Target: {(target != null ? "found" : "missing")}");
        }
        
        return (spawn, target);
    }
    
    /// <summary>
    /// Sets the visibility of all spawn and target point indicators
    /// </summary>
    /// <param name="visible">True to show indicators, false to hide</param>
    public void SetAllIndicatorsVisible(bool visible)
    {
        // Update spawn point indicators
        if (spawnPoints != null)
        {
            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    point.SetIndicatorVisible(visible);
                }
            }
        }
        
        // Update target point indicators
        if (targetPoints != null)
        {
            foreach (var point in targetPoints)
            {
                if (point != null)
                {
                    point.SetIndicatorVisible(visible);
                }
            }
        }
        
        showIndicators = visible;
        Debug.Log($"[SpawnPointManager] Set all indicators visible: {visible}");
    }
}
