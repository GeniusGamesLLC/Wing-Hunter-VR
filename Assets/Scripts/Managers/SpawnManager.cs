using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the spawning of ducks with coroutine-based timing and difficulty scaling
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Game configuration asset")]
    [SerializeField] private DuckHuntConfig gameConfig;
    
    [Header("Pool References")]
    [Tooltip("The duck pool component for object reuse")]
    public DuckPool duckPool;
    
    [Header("Spawn Configuration")]
    [Tooltip("Parent object containing spawn points as children (auto-discovered)")]
    public Transform spawnPointsParent;
    
    [Tooltip("Parent object containing target points as children (auto-discovered)")]
    public Transform targetPointsParent;
    
    [Tooltip("Array of spawn point transforms (auto-populated from parent)")]
    [SerializeField] private Transform[] SpawnPoints;
    
    [Tooltip("Array of target point transforms (auto-populated from parent)")]
    [SerializeField] private Transform[] TargetPoints;
    
    [Header("Current Settings")]
    [Tooltip("Current spawn interval in seconds")]
    [SerializeField] private float currentSpawnInterval = 2.0f;
    
    [Tooltip("Current duck speed")]
    [SerializeField] private float currentDuckSpeed = 5.0f;
    
    [Tooltip("Current difficulty level")]
    [SerializeField] private int currentDifficultyLevel = 1;
    
    // Private fields
    private Coroutine spawnCoroutine;
    private bool isSpawning = false;
    private ScoreManager scoreManager;
    private PerformanceManager performanceManager;
    private int activeDuckCount = 0;
    
    private void Awake()
    {
        // Validate game configuration
        if (gameConfig == null)
        {
            Debug.LogError("SpawnManager: No DuckHuntConfig assigned! Please assign it in the Inspector.");
        }
        
        // Find ScoreManager reference
        scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager == null)
        {
            Debug.LogWarning("SpawnManager: ScoreManager not found in scene. Missed ducks won't be tracked.");
        }
        
        // Find PerformanceManager reference
        performanceManager = FindObjectOfType<PerformanceManager>();
        if (performanceManager != null)
        {
            Debug.Log("SpawnManager: PerformanceManager found - dynamic spawn optimization enabled");
        }
        
        // Auto-discover spawn and target points from parent objects
        AutoDiscoverSpawnPoints();
        AutoDiscoverTargetPoints();
        
        // Validate spawn points
        ValidateSpawnConfiguration();
    }
    
    /// <summary>
    /// Auto-discovers spawn points from children of spawnPointsParent
    /// </summary>
    private void AutoDiscoverSpawnPoints()
    {
        // Try to find parent by name if not assigned
        if (spawnPointsParent == null)
        {
            GameObject spawnParent = GameObject.Find("SpawnPoints");
            if (spawnParent != null)
            {
                spawnPointsParent = spawnParent.transform;
            }
        }
        
        // Populate array from children
        if (spawnPointsParent != null && spawnPointsParent.childCount > 0)
        {
            SpawnPoints = new Transform[spawnPointsParent.childCount];
            for (int i = 0; i < spawnPointsParent.childCount; i++)
            {
                SpawnPoints[i] = spawnPointsParent.GetChild(i);
            }
            Debug.Log($"SpawnManager: Auto-discovered {SpawnPoints.Length} spawn points");
        }
    }
    
    /// <summary>
    /// Auto-discovers target points from children of targetPointsParent
    /// </summary>
    private void AutoDiscoverTargetPoints()
    {
        // Try to find parent by name if not assigned
        if (targetPointsParent == null)
        {
            GameObject targetParent = GameObject.Find("TargetPoints");
            if (targetParent != null)
            {
                targetPointsParent = targetParent.transform;
            }
        }
        
        // Populate array from children
        if (targetPointsParent != null && targetPointsParent.childCount > 0)
        {
            TargetPoints = new Transform[targetPointsParent.childCount];
            for (int i = 0; i < targetPointsParent.childCount; i++)
            {
                TargetPoints[i] = targetPointsParent.GetChild(i);
            }
            Debug.Log($"SpawnManager: Auto-discovered {TargetPoints.Length} target points");
        }
    }
    
    private void Start()
    {
        // Initialize with first difficulty level
        SetDifficulty(1);
    }
    
    /// <summary>
    /// Starts spawning ducks using coroutine-based timing
    /// </summary>
    public void StartSpawning()
    {
        if (isSpawning)
        {
            Debug.LogWarning("SpawnManager: Already spawning ducks");
            return;
        }
        
        if (!ValidateSpawnConfiguration())
        {
            Debug.LogError("SpawnManager: Cannot start spawning - invalid configuration");
            return;
        }
        
        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnCoroutine());
        Debug.Log($"SpawnManager: Started spawning ducks with interval {currentSpawnInterval}s and speed {currentDuckSpeed}");
    }
    
    /// <summary>
    /// Stops spawning ducks
    /// </summary>
    public void StopSpawning()
    {
        if (!isSpawning)
        {
            Debug.LogWarning("SpawnManager: Not currently spawning ducks");
            return;
        }
        
        isSpawning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        Debug.Log("SpawnManager: Stopped spawning ducks");
    }
    
    /// <summary>
    /// Sets the difficulty level and adjusts spawn parameters accordingly
    /// </summary>
    /// <param name="level">The difficulty level to set</param>
    public void SetDifficulty(int level)
    {
        currentDifficultyLevel = level;
        
        if (gameConfig != null)
        {
            DifficultySettings settings = gameConfig.GetDifficultySettings(level);
            currentSpawnInterval = settings.SpawnInterval;
            currentDuckSpeed = settings.DuckSpeed;
            
            Debug.Log($"SpawnManager: Set difficulty to level {level} - Spawn interval: {currentSpawnInterval}s, Duck speed: {currentDuckSpeed}");
        }
        else
        {
            // Fallback difficulty scaling without config
            currentSpawnInterval = Mathf.Max(0.5f, 3.0f - (level - 1) * 0.3f);
            currentDuckSpeed = 4.0f + (level - 1) * 1.5f;
            
            Debug.Log($"SpawnManager: Set difficulty to level {level} (fallback) - Spawn interval: {currentSpawnInterval}s, Duck speed: {currentDuckSpeed}");
        }
    }
    
    /// <summary>
    /// Coroutine that handles the spawning timing
    /// </summary>
    private IEnumerator SpawnCoroutine()
    {
        while (isSpawning)
        {
            // Use base spawn interval (PerformanceManager adjustments disabled by default)
            float adjustedInterval = currentSpawnInterval;
            
            // Wait for the spawn interval
            yield return new WaitForSeconds(adjustedInterval);
            
            // Spawn a duck if still spawning
            if (isSpawning)
            {
                SpawnDuck();
            }
        }
    }
    
    /// <summary>
    /// Spawns a single duck with randomized start and end positions
    /// </summary>
    private void SpawnDuck()
    {
        if (duckPool == null)
        {
            Debug.LogError("SpawnManager: Duck pool is not assigned");
            return;
        }
        
        // Select random spawn and target points
        Vector3 spawnPosition = GetRandomSpawnPosition();
        Vector3 targetPosition = GetRandomTargetPosition();
        
        // Get a duck from the pool
        DuckController duckController = duckPool.GetDuck();
        
        if (duckController != null)
        {
            // Position the duck at spawn location
            duckController.transform.position = spawnPosition;
            
            // Initialize the duck with movement parameters
            duckController.Initialize(spawnPosition, targetPosition, currentDuckSpeed);
            
            // Subscribe to duck events for cleanup and scoring
            duckController.OnDestroyed += OnDuckDestroyed;
            duckController.OnEscaped += OnDuckEscaped;
            
            // Track active duck count for performance monitoring
            activeDuckCount++;
        }
        else
        {
            Debug.LogError("SpawnManager: Failed to get duck from pool");
        }
    }
    
    /// <summary>
    /// Gets a random spawn position from the configured spawn points
    /// </summary>
    /// <returns>Random spawn position</returns>
    private Vector3 GetRandomSpawnPosition()
    {
        if (SpawnPoints != null && SpawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, SpawnPoints.Length);
            return SpawnPoints[randomIndex].position;
        }
        
        // Fallback: generate random position in a circle around the origin
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = 10f;
        float height = Random.Range(1f, 4f);
        
        return new Vector3(
            Mathf.Cos(angle) * radius,
            height,
            Mathf.Sin(angle) * radius
        );
    }
    
    /// <summary>
    /// Gets a random target position from the configured target points
    /// </summary>
    /// <returns>Random target position</returns>
    private Vector3 GetRandomTargetPosition()
    {
        if (TargetPoints != null && TargetPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, TargetPoints.Length);
            return TargetPoints[randomIndex].position;
        }
        
        // Fallback: generate random position on opposite side
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = 10f;
        float height = Random.Range(1f, 4f);
        
        return new Vector3(
            Mathf.Cos(angle + Mathf.PI) * radius, // Opposite side
            height,
            Mathf.Sin(angle + Mathf.PI) * radius
        );
    }
    
    /// <summary>
    /// Called when a duck is destroyed (hit by player)
    /// </summary>
    /// <param name="duck">The destroyed duck controller</param>
    private void OnDuckDestroyed(DuckController duck)
    {
        // Unsubscribe from events to prevent memory leaks
        duck.OnDestroyed -= OnDuckDestroyed;
        duck.OnEscaped -= OnDuckEscaped;
        
        // Decrement active duck count
        activeDuckCount = Mathf.Max(0, activeDuckCount - 1);
        
        Debug.Log("SpawnManager: Duck was destroyed (hit)");
        
        // Add score for hitting the duck
        if (scoreManager != null && gameConfig != null)
        {
            scoreManager.AddScore(gameConfig.PointsPerDuck);
        }
        
        // Return duck to pool after a short delay to allow effects to finish
        StartCoroutine(ReturnDuckToPoolDelayed(duck, 0.5f));
    }
    
    /// <summary>
    /// Called when a duck escapes (reaches target without being hit)
    /// </summary>
    /// <param name="duck">The escaped duck controller</param>
    private void OnDuckEscaped(DuckController duck)
    {
        // Unsubscribe from events to prevent memory leaks
        duck.OnDestroyed -= OnDuckDestroyed;
        duck.OnEscaped -= OnDuckEscaped;
        
        // Decrement active duck count
        activeDuckCount = Mathf.Max(0, activeDuckCount - 1);
        
        Debug.Log("SpawnManager: Duck escaped");
        
        // Notify ScoreManager that a duck was missed
        if (scoreManager != null)
        {
            scoreManager.IncrementMissed();
        }
        
        // Return duck to pool immediately since no effects need to finish
        ReturnDuckToPool(duck);
    }
    
    /// <summary>
    /// Returns a duck to the pool immediately
    /// </summary>
    /// <param name="duck">The duck to return</param>
    private void ReturnDuckToPool(DuckController duck)
    {
        if (duckPool != null && duck != null)
        {
            duckPool.ReturnDuck(duck);
        }
    }
    
    /// <summary>
    /// Returns a duck to the pool after a delay (coroutine)
    /// </summary>
    /// <param name="duck">The duck to return</param>
    /// <param name="delay">Delay in seconds</param>
    /// <returns>Coroutine enumerator</returns>
    private System.Collections.IEnumerator ReturnDuckToPoolDelayed(DuckController duck, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnDuckToPool(duck);
    }
    
    /// <summary>
    /// Validates the spawn configuration
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    private bool ValidateSpawnConfiguration()
    {
        if (duckPool == null)
        {
            Debug.LogError("SpawnManager: Duck pool is not assigned");
            return false;
        }
        
        if (SpawnPoints == null || SpawnPoints.Length == 0)
        {
            Debug.LogWarning("SpawnManager: No spawn points configured, will use fallback positions");
        }
        
        if (TargetPoints == null || TargetPoints.Length == 0)
        {
            Debug.LogWarning("SpawnManager: No target points configured, will use fallback positions");
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets the current spawning status
    /// </summary>
    /// <returns>True if currently spawning ducks</returns>
    public bool IsSpawning()
    {
        return isSpawning;
    }
    
    /// <summary>
    /// Gets the current difficulty level
    /// </summary>
    /// <returns>Current difficulty level</returns>
    public int GetCurrentDifficulty()
    {
        return currentDifficultyLevel;
    }
    
    /// <summary>
    /// Gets the current spawn interval
    /// </summary>
    /// <returns>Current spawn interval in seconds</returns>
    public float GetCurrentSpawnInterval()
    {
        return currentSpawnInterval;
    }
    
    /// <summary>
    /// Gets the current duck speed
    /// </summary>
    /// <returns>Current duck speed</returns>
    public float GetCurrentDuckSpeed()
    {
        return currentDuckSpeed;
    }
    
    /// <summary>
    /// Gets the current number of active ducks in the scene
    /// </summary>
    /// <returns>Number of active ducks</returns>
    public int GetActiveDuckCount()
    {
        return activeDuckCount;
    }
    
    /// <summary>
    /// Resets the active duck count (useful when restarting game)
    /// </summary>
    public void ResetActiveDuckCount()
    {
        activeDuckCount = 0;
    }
}