using UnityEngine;

/// <summary>
/// ScriptableObject containing configuration settings for the multi-point pathing system.
/// Controls spline interpolation, flight duration, waypoint generation, and visualization.
/// </summary>
[CreateAssetMenu(fileName = "FlightPathConfig", menuName = "Duck Hunt/Flight Path Config")]
public class FlightPathConfig : ScriptableObject
{
    #region Spline Settings
    
    [Header("Spline Settings")]
    [Tooltip("Tension parameter for Catmull-Rom spline interpolation. Lower values create tighter curves.")]
    [Range(0f, 1f)]
    public float SplineTension = 0.5f;
    
    [Tooltip("Number of samples per segment for arc-length calculation. Higher values are more accurate but slower.")]
    [Range(10, 50)]
    public int ArcLengthSamples = 20;
    
    #endregion
    
    #region Flight Duration Settings
    
    [Header("Flight Duration")]
    [Tooltip("Minimum time a duck must be in flight (seconds). Paths will be extended if needed to meet this duration.")]
    [Range(1f, 10f)]
    public float MinFlightDuration = 3f;
    
    #endregion
    
    #region Flight Zone Settings
    
    [Header("Flight Zone")]
    [Tooltip("Bounding box defining the valid flight area for ducks.")]
    public Bounds FlightZone = new Bounds(Vector3.zero, new Vector3(20f, 8f, 20f));
    
    [Tooltip("Minimum height above ground for flight paths.")]
    [Range(0.5f, 5f)]
    public float MinHeightAboveGround = 1.5f;
    
    [Tooltip("Maximum height above ground for flight paths.")]
    [Range(3f, 15f)]
    public float MaxHeightAboveGround = 6f;
    
    #endregion
    
    #region Waypoint Generation Settings
    
    [Header("Waypoint Generation")]
    [Tooltip("Maximum lateral (sideways) deviation from direct path for generated waypoints.")]
    [Range(0.5f, 10f)]
    public float LateralDeviationRange = 3f;
    
    [Tooltip("Maximum vertical deviation from direct path for generated waypoints.")]
    [Range(0.5f, 5f)]
    public float VerticalDeviationRange = 1.5f;
    
    [Tooltip("Minimum distance from spawn/target points for intermediate waypoints.")]
    [Range(0.5f, 5f)]
    public float MinDistanceFromEndpoints = 2f;
    
    [Tooltip("Whether to prefer pre-placed waypoints over dynamic generation when available.")]
    public bool PreferPreplacedWaypoints = true;
    
    #endregion
    
    #region Difficulty Waypoint Mapping
    
    [Header("Difficulty Waypoint Mapping")]
    [Tooltip("Waypoint count settings for each difficulty level. Array index corresponds to difficulty level - 1.")]
    public DifficultyWaypointSettings[] WaypointsByDifficulty = new DifficultyWaypointSettings[]
    {
        new DifficultyWaypointSettings(1, 1, 2, 0.7f),  // Level 1: 1-2 waypoints, weighted toward max
        new DifficultyWaypointSettings(2, 1, 2, 0.5f),  // Level 2: 1-2 waypoints, even distribution
        new DifficultyWaypointSettings(3, 1, 3, 0.5f),  // Level 3: 1-3 waypoints, even distribution
        new DifficultyWaypointSettings(4, 0, 3, 0.5f),  // Level 4: 0-3 waypoints, even distribution
        new DifficultyWaypointSettings(5, 0, 3, 0.3f)   // Level 5: 0-3 waypoints, weighted toward min
    };
    
    #endregion
    
    #region Visualization Settings
    
    [Header("Visualization")]
    [Tooltip("Color for spline path visualization lines.")]
    public Color SplinePathColor = Color.cyan;
    
    [Tooltip("Color for intermediate waypoint indicators.")]
    public Color IntermediateWaypointColor = new Color(1f, 0.6f, 0f, 1f); // Orange
    
    [Tooltip("Width of the spline path visualization line.")]
    [Range(0.01f, 0.2f)]
    public float SplinePathWidth = 0.05f;
    
    [Tooltip("Scale of waypoint indicator spheres.")]
    [Range(0.1f, 0.5f)]
    public float WaypointIndicatorScale = 0.25f;
    
    [Tooltip("Number of sample points for spline visualization. Higher values create smoother curves.")]
    [Range(20, 100)]
    public int SplineVisualizationSamples = 50;
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Gets the waypoint settings for a specific difficulty level.
    /// </summary>
    /// <param name="difficultyLevel">The difficulty level (1-5)</param>
    /// <returns>DifficultyWaypointSettings for the specified level</returns>
    public DifficultyWaypointSettings GetWaypointSettings(int difficultyLevel)
    {
        if (WaypointsByDifficulty == null || WaypointsByDifficulty.Length == 0)
        {
            Debug.LogWarning("[FlightPathConfig] No waypoint settings configured, returning default.");
            return new DifficultyWaypointSettings();
        }
        
        // Clamp to valid range (array is 0-indexed, difficulty is 1-indexed)
        int index = Mathf.Clamp(difficultyLevel - 1, 0, WaypointsByDifficulty.Length - 1);
        return WaypointsByDifficulty[index];
    }
    
    /// <summary>
    /// Gets a random waypoint count for the specified difficulty level.
    /// </summary>
    /// <param name="difficultyLevel">The difficulty level (1-5)</param>
    /// <param name="rng">Random number generator to use (optional)</param>
    /// <returns>Number of intermediate waypoints to generate</returns>
    public int GetRandomWaypointCount(int difficultyLevel, System.Random rng = null)
    {
        var settings = GetWaypointSettings(difficultyLevel);
        return settings.GetRandomWaypointCount(rng);
    }
    
    /// <summary>
    /// Clamps a position to be within the flight zone bounds and height constraints.
    /// </summary>
    /// <param name="position">The position to clamp</param>
    /// <returns>The clamped position</returns>
    public Vector3 ClampToFlightZone(Vector3 position)
    {
        // Clamp to flight zone bounds
        Vector3 min = FlightZone.min;
        Vector3 max = FlightZone.max;
        
        position.x = Mathf.Clamp(position.x, min.x, max.x);
        position.z = Mathf.Clamp(position.z, min.z, max.z);
        
        // Clamp height to configured constraints
        position.y = Mathf.Clamp(position.y, MinHeightAboveGround, MaxHeightAboveGround);
        
        return position;
    }
    
    /// <summary>
    /// Checks if a position is within the flight zone bounds.
    /// </summary>
    /// <param name="position">The position to check</param>
    /// <returns>True if the position is within bounds</returns>
    public bool IsWithinFlightZone(Vector3 position)
    {
        if (!FlightZone.Contains(position))
            return false;
            
        if (position.y < MinHeightAboveGround || position.y > MaxHeightAboveGround)
            return false;
            
        return true;
    }
    
    #endregion
    
    #region Validation
    
    private void OnValidate()
    {
        // Ensure min height is less than max height
        if (MinHeightAboveGround > MaxHeightAboveGround)
        {
            MinHeightAboveGround = MaxHeightAboveGround;
        }
        
        // Ensure we have at least one difficulty setting
        if (WaypointsByDifficulty == null || WaypointsByDifficulty.Length == 0)
        {
            WaypointsByDifficulty = new DifficultyWaypointSettings[]
            {
                new DifficultyWaypointSettings()
            };
        }
        
        // Validate each difficulty setting
        for (int i = 0; i < WaypointsByDifficulty.Length; i++)
        {
            if (WaypointsByDifficulty[i] != null)
            {
                WaypointsByDifficulty[i].DifficultyLevel = i + 1;
                WaypointsByDifficulty[i].Validate();
            }
        }
    }
    
    #endregion
}

/// <summary>
/// Configuration for waypoint generation at a specific difficulty level.
/// </summary>
[System.Serializable]
public class DifficultyWaypointSettings
{
    [Tooltip("The difficulty level this setting applies to.")]
    public int DifficultyLevel = 1;
    
    [Tooltip("Minimum number of intermediate waypoints.")]
    [Range(0, 5)]
    public int MinWaypoints = 1;
    
    [Tooltip("Maximum number of intermediate waypoints.")]
    [Range(0, 5)]
    public int MaxWaypoints = 2;
    
    [Tooltip("Probability of generating maximum waypoints (0 = always min, 1 = always max).")]
    [Range(0f, 1f)]
    public float ChanceOfMaxWaypoints = 0.5f;
    
    /// <summary>
    /// Creates a new DifficultyWaypointSettings with default values.
    /// </summary>
    public DifficultyWaypointSettings()
    {
        DifficultyLevel = 1;
        MinWaypoints = 1;
        MaxWaypoints = 2;
        ChanceOfMaxWaypoints = 0.5f;
    }
    
    /// <summary>
    /// Creates a new DifficultyWaypointSettings with specified parameters.
    /// </summary>
    public DifficultyWaypointSettings(int level, int minWaypoints, int maxWaypoints, float chanceOfMax)
    {
        DifficultyLevel = level;
        MinWaypoints = minWaypoints;
        MaxWaypoints = maxWaypoints;
        ChanceOfMaxWaypoints = chanceOfMax;
    }
    
    /// <summary>
    /// Gets a random waypoint count based on the configured range and probability.
    /// </summary>
    /// <param name="rng">Random number generator to use (optional, uses Unity's Random if null)</param>
    /// <returns>Number of waypoints to generate</returns>
    public int GetRandomWaypointCount(System.Random rng = null)
    {
        if (MinWaypoints == MaxWaypoints)
        {
            return MinWaypoints;
        }
        
        // Use weighted random selection
        float randomValue = rng != null ? (float)rng.NextDouble() : Random.value;
        
        // ChanceOfMaxWaypoints determines the distribution
        // Higher values weight toward max, lower values weight toward min
        int range = MaxWaypoints - MinWaypoints;
        float weightedValue = randomValue * (1f + ChanceOfMaxWaypoints) - ChanceOfMaxWaypoints * 0.5f;
        weightedValue = Mathf.Clamp01(weightedValue);
        
        return MinWaypoints + Mathf.RoundToInt(weightedValue * range);
    }
    
    /// <summary>
    /// Validates the settings to ensure min <= max.
    /// </summary>
    public void Validate()
    {
        if (MinWaypoints > MaxWaypoints)
        {
            MinWaypoints = MaxWaypoints;
        }
        
        ChanceOfMaxWaypoints = Mathf.Clamp01(ChanceOfMaxWaypoints);
    }
}
