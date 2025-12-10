using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the spawning of ducks with coroutine-based timing and difficulty scaling
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Prefab References")]
    [Tooltip("The duck prefab to spawn")]
    public GameObject DuckPrefab;
    
    [Header("Spawn Configuration")]
    [Tooltip("Array of spawn point transforms where ducks can appear")]
    public Transform[] SpawnPoints;
    
    [Tooltip("Array of target point transforms where ducks fly towards")]
    public Transform[] TargetPoints;
    
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
    private DuckHuntConfig gameConfig;
    
    private void Awake()
    {
        // Load the game configuration
        gameConfig = Resources.Load<DuckHuntConfig>("DuckHuntConfig");
        if (gameConfig == null)
        {
            Debug.LogWarning("SpawnManager: No DuckHuntConfig found in Resources folder. Using default settings.");
        }
        
        // Validate spawn points
        ValidateSpawnConfiguration();
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
            // Wait for the spawn interval
            yield return new WaitForSeconds(currentSpawnInterval);
            
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
        if (DuckPrefab == null)
        {
            Debug.LogError("SpawnManager: Duck prefab is not assigned");
            return;
        }
        
        // Select random spawn and target points
        Vector3 spawnPosition = GetRandomSpawnPosition();
        Vector3 targetPosition = GetRandomTargetPosition();
        
        // Instantiate the duck
        GameObject duckInstance = Instantiate(DuckPrefab, spawnPosition, Quaternion.identity);
        
        // Get the DuckController component and initialize it
        DuckController duckController = duckInstance.GetComponent<DuckController>();
        if (duckController != null)
        {
            duckController.Initialize(spawnPosition, targetPosition, currentDuckSpeed);
            
            // Subscribe to duck events for cleanup and scoring
            duckController.OnDestroyed += OnDuckDestroyed;
            duckController.OnEscaped += OnDuckEscaped;
        }
        else
        {
            Debug.LogError("SpawnManager: Duck prefab does not have a DuckController component");
            Destroy(duckInstance);
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
        
        Debug.Log("SpawnManager: Duck was destroyed (hit)");
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
        
        Debug.Log("SpawnManager: Duck escaped");
    }
    
    /// <summary>
    /// Validates the spawn configuration
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    private bool ValidateSpawnConfiguration()
    {
        if (DuckPrefab == null)
        {
            Debug.LogError("SpawnManager: Duck prefab is not assigned");
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
}