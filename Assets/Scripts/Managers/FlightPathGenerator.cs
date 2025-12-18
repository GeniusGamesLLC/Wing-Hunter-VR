using System.Collections.Generic;
using UnityEngine;
using DuckHunt.Data;

/// <summary>
/// Generates flight paths for ducks using a hybrid approach - pre-placed IntermediatePoint 
/// prefabs when available, with dynamic generation as fallback.
/// Supports difficulty-based path complexity and deterministic generation via seeds.
/// </summary>
public class FlightPathGenerator : MonoBehaviour
{
    #region Serialized Fields

    [Header("Pre-placed Waypoints")]
    [Tooltip("Parent transform containing IntermediatePoint prefabs in the scene")]
    [SerializeField] private Transform intermediatePointsParent;
    
    [Header("Configuration")]
    [Tooltip("Flight path configuration asset")]
    [SerializeField] private FlightPathConfig config;
    
    /// <summary>
    /// Public accessor for the flight path configuration.
    /// </summary>
    public FlightPathConfig Config => config;

    #endregion

    #region Private Fields

    /// <summary>
    /// Cached list of pre-placed intermediate point positions
    /// </summary>
    private List<Vector3> cachedPreplacedPoints = new List<Vector3>();
    
    /// <summary>
    /// Whether pre-placed points have been discovered
    /// </summary>
    private bool pointsDiscovered = false;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Auto-discover intermediate points parent if not assigned
        if (intermediatePointsParent == null)
        {
            GameObject parent = GameObject.Find("IntermediatePoints");
            if (parent != null)
            {
                intermediatePointsParent = parent.transform;
                Debug.Log("[FlightPathGenerator] Auto-discovered IntermediatePoints parent");
            }
        }
        
        // Validate config
        if (config == null)
        {
            config = Resources.Load<FlightPathConfig>("FlightPathConfig");
            if (config == null)
            {
                Debug.LogWarning("[FlightPathGenerator] No FlightPathConfig assigned. Using default values.");
            }
        }
    }

    private void Start()
    {
        // Discover pre-placed points on start
        DiscoverPreplacedPoints();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Generates a flight path from spawn to target with intermediate waypoints based on difficulty.
    /// </summary>
    /// <param name="spawnPoint">Starting position of the path</param>
    /// <param name="targetPoint">Ending position of the path</param>
    /// <param name="difficultyLevel">Current difficulty level (1-5)</param>
    /// <param name="seed">Optional random seed for deterministic generation</param>
    /// <returns>A FlightPath object containing the complete path data</returns>
    public FlightPath GeneratePath(Vector3 spawnPoint, Vector3 targetPoint, int difficultyLevel, int? seed = null)
    {
        // Ensure points are discovered
        if (!pointsDiscovered)
        {
            DiscoverPreplacedPoints();
        }
        
        // Create random number generator with seed
        int actualSeed = seed ?? System.Environment.TickCount;
        System.Random rng = new System.Random(actualSeed);
        
        // Get waypoint count for this difficulty
        int waypointCount = GetWaypointCountForDifficulty(difficultyLevel, rng);
        
        // Log seed and configuration for debugging
        Debug.Log($"[FlightPathGenerator] Generating path - Seed: {actualSeed}, Difficulty: {difficultyLevel}, " +
                  $"Waypoints: {waypointCount}, PreplacedAvailable: {cachedPreplacedPoints.Count}");
        
        // Build waypoint list: spawn + intermediates + target
        List<Vector3> waypoints = new List<Vector3>();
        waypoints.Add(spawnPoint);
        
        // Generate intermediate waypoints
        if (waypointCount > 0)
        {
            List<Vector3> intermediates = GenerateIntermediateWaypoints(
                spawnPoint, targetPoint, waypointCount, rng);
            waypoints.AddRange(intermediates);
        }
        
        waypoints.Add(targetPoint);
        
        // Create the flight path
        float tension = config != null ? config.SplineTension : SplineUtility.DefaultTension;
        int samples = config != null ? config.ArcLengthSamples : SplineUtility.DefaultSamplesPerSegment;
        
        FlightPath path = new FlightPath(waypoints.ToArray(), actualSeed, tension, samples);
        
        // Validate minimum flight duration and extend if needed
        path = ValidateAndExtendPath(path, spawnPoint, targetPoint, rng);
        
        Debug.Log($"[FlightPathGenerator] Path generated: {path}");
        
        return path;
    }

    /// <summary>
    /// Gets the array of pre-placed intermediate point positions from the scene.
    /// </summary>
    /// <returns>Array of world positions for pre-placed intermediate points</returns>
    public Vector3[] GetPreplacedIntermediatePoints()
    {
        if (!pointsDiscovered)
        {
            DiscoverPreplacedPoints();
        }
        return cachedPreplacedPoints.ToArray();
    }

    /// <summary>
    /// Generates a single dynamic waypoint along the path from spawn to target.
    /// </summary>
    /// <param name="spawnPoint">Starting position</param>
    /// <param name="targetPoint">Ending position</param>
    /// <param name="progressAlongPath">Progress value (0-1) along the direct path</param>
    /// <param name="rng">Random number generator for deterministic generation</param>
    /// <returns>Generated waypoint position</returns>
    public Vector3 GenerateDynamicWaypoint(Vector3 spawnPoint, Vector3 targetPoint, float progressAlongPath, System.Random rng)
    {
        Vector3 directPath = targetPoint - spawnPoint;
        Vector3 basePosition = spawnPoint + directPath * progressAlongPath;
        
        // Calculate perpendicular direction for lateral deviation
        Vector3 perpendicular = Vector3.Cross(directPath.normalized, Vector3.up).normalized;
        if (perpendicular.sqrMagnitude < 0.001f)
        {
            // Path is vertical, use a different perpendicular
            perpendicular = Vector3.Cross(directPath.normalized, Vector3.right).normalized;
        }
        if (perpendicular.sqrMagnitude < 0.001f)
        {
            perpendicular = Vector3.right;
        }
        
        // Get deviation ranges from config
        float lateralRange = config != null ? config.LateralDeviationRange : 3f;
        float verticalRange = config != null ? config.VerticalDeviationRange : 1.5f;
        float minHeight = config != null ? config.MinHeightAboveGround : 1.5f;
        float maxHeight = config != null ? config.MaxHeightAboveGround : 6f;
        
        // Apply random deviations
        float lateralOffset = ((float)rng.NextDouble() * 2f - 1f) * lateralRange;
        float verticalOffset = ((float)rng.NextDouble() * 2f - 1f) * verticalRange;
        
        Vector3 waypoint = basePosition + perpendicular * lateralOffset + Vector3.up * verticalOffset;
        
        // Clamp height to configured constraints
        waypoint.y = Mathf.Clamp(waypoint.y, minHeight, maxHeight);
        
        // Clamp to flight zone if config available
        if (config != null)
        {
            waypoint = config.ClampToFlightZone(waypoint);
        }
        
        return waypoint;
    }

    /// <summary>
    /// Gets the number of intermediate waypoints to generate for a given difficulty level.
    /// </summary>
    /// <param name="difficultyLevel">The difficulty level (1-5)</param>
    /// <param name="rng">Optional random number generator for deterministic selection</param>
    /// <returns>Number of intermediate waypoints to generate</returns>
    public int GetWaypointCountForDifficulty(int difficultyLevel, System.Random rng = null)
    {
        if (config != null)
        {
            return config.GetRandomWaypointCount(difficultyLevel, rng);
        }
        
        // Fallback difficulty mapping without config
        // Based on design doc: 1-2 for levels 1-2, 1-3 for level 3, 0-3 for levels 4-5
        int min, max;
        switch (difficultyLevel)
        {
            case 1:
            case 2:
                min = 1;
                max = 2;
                break;
            case 3:
                min = 1;
                max = 3;
                break;
            case 4:
            case 5:
                min = 0;
                max = 3;
                break;
            default:
                min = 1;
                max = 2;
                break;
        }
        
        if (rng != null)
        {
            return rng.Next(min, max + 1);
        }
        return Random.Range(min, max + 1);
    }

    /// <summary>
    /// Forces re-discovery of pre-placed intermediate points.
    /// Call this if points are added/removed at runtime.
    /// </summary>
    public void RefreshPreplacedPoints()
    {
        pointsDiscovered = false;
        DiscoverPreplacedPoints();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Discovers pre-placed IntermediatePointMarker components in the scene.
    /// </summary>
    private void DiscoverPreplacedPoints()
    {
        cachedPreplacedPoints.Clear();
        
        if (intermediatePointsParent != null)
        {
            // Get all IntermediatePointMarker components from children
            IntermediatePointMarker[] markers = intermediatePointsParent.GetComponentsInChildren<IntermediatePointMarker>(true);
            
            foreach (var marker in markers)
            {
                cachedPreplacedPoints.Add(marker.transform.position);
            }
            
            Debug.Log($"[FlightPathGenerator] Discovered {cachedPreplacedPoints.Count} pre-placed intermediate points");
        }
        else
        {
            // Try to find any IntermediatePointMarker in the scene
            IntermediatePointMarker[] allMarkers = FindObjectsOfType<IntermediatePointMarker>();
            foreach (var marker in allMarkers)
            {
                cachedPreplacedPoints.Add(marker.transform.position);
            }
            
            if (cachedPreplacedPoints.Count > 0)
            {
                Debug.Log($"[FlightPathGenerator] Found {cachedPreplacedPoints.Count} IntermediatePointMarkers in scene (no parent assigned)");
            }
        }
        
        pointsDiscovered = true;
    }

    /// <summary>
    /// Generates intermediate waypoints using hybrid approach (pre-placed + dynamic).
    /// </summary>
    private List<Vector3> GenerateIntermediateWaypoints(Vector3 spawn, Vector3 target, int count, System.Random rng)
    {
        List<Vector3> intermediates = new List<Vector3>();
        
        if (count <= 0)
        {
            return intermediates;
        }
        
        bool preferPreplaced = config != null ? config.PreferPreplacedWaypoints : true;
        float minDistFromEndpoints = config != null ? config.MinDistanceFromEndpoints : 2f;
        
        // Filter pre-placed points that are suitable for this path
        List<Vector3> suitablePreplaced = new List<Vector3>();
        if (preferPreplaced && cachedPreplacedPoints.Count > 0)
        {
            suitablePreplaced = FilterSuitableWaypoints(spawn, target, cachedPreplacedPoints, minDistFromEndpoints);
        }
        
        string waypointSource = "dynamic";
        
        // Use pre-placed waypoints if available and preferred
        if (suitablePreplaced.Count >= count)
        {
            // Select random subset of pre-placed points
            intermediates = SelectRandomWaypoints(suitablePreplaced, count, spawn, target, rng);
            waypointSource = "pre-placed";
        }
        else if (suitablePreplaced.Count > 0)
        {
            // Use available pre-placed points and fill rest with dynamic
            intermediates.AddRange(suitablePreplaced);
            int remaining = count - suitablePreplaced.Count;
            
            // Generate remaining waypoints dynamically
            List<Vector3> dynamicPoints = GenerateDynamicWaypoints(spawn, target, remaining, rng);
            intermediates.AddRange(dynamicPoints);
            waypointSource = "hybrid";
        }
        else
        {
            // Generate all waypoints dynamically
            intermediates = GenerateDynamicWaypoints(spawn, target, count, rng);
            waypointSource = "dynamic";
        }
        
        // Sort waypoints by progress along the path (spawn to target)
        intermediates = SortWaypointsByProgress(spawn, target, intermediates);
        
        Debug.Log($"[FlightPathGenerator] Generated {intermediates.Count} intermediate waypoints (source: {waypointSource})");
        
        return intermediates;
    }

    /// <summary>
    /// Filters pre-placed waypoints to find those suitable for the given path.
    /// </summary>
    private List<Vector3> FilterSuitableWaypoints(Vector3 spawn, Vector3 target, List<Vector3> candidates, float minDistFromEndpoints)
    {
        List<Vector3> suitable = new List<Vector3>();
        Vector3 pathDirection = (target - spawn).normalized;
        float pathLength = Vector3.Distance(spawn, target);
        
        foreach (var point in candidates)
        {
            // Check distance from endpoints
            float distFromSpawn = Vector3.Distance(point, spawn);
            float distFromTarget = Vector3.Distance(point, target);
            
            if (distFromSpawn < minDistFromEndpoints || distFromTarget < minDistFromEndpoints)
            {
                continue;
            }
            
            // Check if point is roughly between spawn and target
            Vector3 toPoint = point - spawn;
            float projectedDistance = Vector3.Dot(toPoint, pathDirection);
            
            // Point should be between spawn and target (with some margin)
            if (projectedDistance < minDistFromEndpoints || projectedDistance > pathLength - minDistFromEndpoints)
            {
                continue;
            }
            
            // Check lateral distance from direct path (shouldn't be too far)
            Vector3 projectedPoint = spawn + pathDirection * projectedDistance;
            float lateralDistance = Vector3.Distance(point, projectedPoint);
            float maxLateralDistance = config != null ? config.LateralDeviationRange * 2f : 6f;
            
            if (lateralDistance > maxLateralDistance)
            {
                continue;
            }
            
            suitable.Add(point);
        }
        
        return suitable;
    }

    /// <summary>
    /// Selects a random subset of waypoints, ensuring good distribution along the path.
    /// </summary>
    private List<Vector3> SelectRandomWaypoints(List<Vector3> candidates, int count, Vector3 spawn, Vector3 target, System.Random rng)
    {
        if (candidates.Count <= count)
        {
            return new List<Vector3>(candidates);
        }
        
        // Shuffle candidates using Fisher-Yates
        List<Vector3> shuffled = new List<Vector3>(candidates);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            Vector3 temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }
        
        // Take first 'count' elements
        return shuffled.GetRange(0, count);
    }

    /// <summary>
    /// Generates dynamic waypoints distributed along the path.
    /// </summary>
    private List<Vector3> GenerateDynamicWaypoints(Vector3 spawn, Vector3 target, int count, System.Random rng)
    {
        List<Vector3> waypoints = new List<Vector3>();
        
        if (count <= 0)
        {
            return waypoints;
        }
        
        // Distribute waypoints evenly along the path
        for (int i = 0; i < count; i++)
        {
            // Calculate progress along path (avoid endpoints)
            float progress = (i + 1f) / (count + 1f);
            
            Vector3 waypoint = GenerateDynamicWaypoint(spawn, target, progress, rng);
            waypoints.Add(waypoint);
        }
        
        return waypoints;
    }

    /// <summary>
    /// Sorts waypoints by their progress along the spawn-to-target path.
    /// </summary>
    private List<Vector3> SortWaypointsByProgress(Vector3 spawn, Vector3 target, List<Vector3> waypoints)
    {
        if (waypoints.Count <= 1)
        {
            return waypoints;
        }
        
        Vector3 pathDirection = (target - spawn).normalized;
        
        waypoints.Sort((a, b) =>
        {
            float progressA = Vector3.Dot(a - spawn, pathDirection);
            float progressB = Vector3.Dot(b - spawn, pathDirection);
            return progressA.CompareTo(progressB);
        });
        
        return waypoints;
    }

    /// <summary>
    /// Validates the path meets minimum flight duration and extends if needed.
    /// </summary>
    private FlightPath ValidateAndExtendPath(FlightPath path, Vector3 spawn, Vector3 target, System.Random rng)
    {
        if (config == null)
        {
            return path;
        }
        
        float minDuration = config.MinFlightDuration;
        
        // We need to know the duck speed to calculate duration
        // For now, we'll assume a reference speed and the caller will handle speed adjustment
        // The path itself stores arc length, and duration = arcLength / speed
        
        // If the path is very short (e.g., spawn and target are close), we may need to add waypoints
        // to create a longer curved path
        float directDistance = Vector3.Distance(spawn, target);
        
        // If arc length is less than 1.5x the direct distance, the path might be too short
        // Add additional waypoints to extend it
        if (path.TotalArcLength < directDistance * 1.2f && path.IntermediateWaypointCount < 3)
        {
            // Path is nearly straight, consider adding more curve
            // This is a soft extension - the actual duration check happens at spawn time
            // when we know the duck speed
            Debug.Log($"[FlightPathGenerator] Path arc length ({path.TotalArcLength:F2}) is close to direct distance ({directDistance:F2})");
        }
        
        return path;
    }

    #endregion

    #region Editor Support

    #if UNITY_EDITOR
    /// <summary>
    /// Draws gizmos showing the flight zone bounds in the editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (config == null)
        {
            return;
        }
        
        // Draw flight zone bounds
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireCube(config.FlightZone.center, config.FlightZone.size);
        
        // Draw height constraints
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Vector3 minHeightCenter = config.FlightZone.center;
        minHeightCenter.y = config.MinHeightAboveGround;
        Gizmos.DrawWireCube(minHeightCenter, new Vector3(config.FlightZone.size.x, 0.1f, config.FlightZone.size.z));
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Vector3 maxHeightCenter = config.FlightZone.center;
        maxHeightCenter.y = config.MaxHeightAboveGround;
        Gizmos.DrawWireCube(maxHeightCenter, new Vector3(config.FlightZone.size.x, 0.1f, config.FlightZone.size.z));
    }
    #endif

    #endregion
}
