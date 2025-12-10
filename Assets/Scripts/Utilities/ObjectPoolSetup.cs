using UnityEngine;

/// <summary>
/// Utility script to help set up object pooling system in existing scenes
/// </summary>
public class ObjectPoolSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [Tooltip("The duck prefab to use for the pool")]
    public GameObject duckPrefab;
    
    [Tooltip("Initial pool size")]
    public int initialPoolSize = 5;
    
    [Tooltip("Maximum pool size")]
    public int maxPoolSize = 20;
    
    [Tooltip("The SpawnManager to update with the new pool")]
    public SpawnManager spawnManager;
    
    [Header("Setup Actions")]
    [Tooltip("Click to set up the duck pool system")]
    public bool setupDuckPool = false;
    
    private void Update()
    {
        if (setupDuckPool)
        {
            setupDuckPool = false;
            SetupDuckPoolSystem();
        }
    }
    
    /// <summary>
    /// Sets up the duck pool system
    /// </summary>
    [ContextMenu("Setup Duck Pool System")]
    public void SetupDuckPoolSystem()
    {
        if (duckPrefab == null)
        {
            Debug.LogError("ObjectPoolSetup: Duck prefab is required");
            return;
        }
        
        if (spawnManager == null)
        {
            Debug.LogError("ObjectPoolSetup: SpawnManager is required");
            return;
        }
        
        // Check if DuckPool already exists
        DuckPool existingPool = FindObjectOfType<DuckPool>();
        if (existingPool != null)
        {
            Debug.LogWarning("ObjectPoolSetup: DuckPool already exists in scene");
            
            // Update the existing pool configuration
            existingPool.duckPrefab = duckPrefab;
            existingPool.initialPoolSize = initialPoolSize;
            existingPool.maxPoolSize = maxPoolSize;
            
            // Update SpawnManager reference
            spawnManager.duckPool = existingPool;
            
            Debug.Log("ObjectPoolSetup: Updated existing DuckPool configuration");
            return;
        }
        
        // Create a new GameObject for the DuckPool
        GameObject poolObject = new GameObject("DuckPool");
        poolObject.transform.SetParent(transform);
        
        // Add DuckPool component
        DuckPool duckPool = poolObject.AddComponent<DuckPool>();
        duckPool.duckPrefab = duckPrefab;
        duckPool.initialPoolSize = initialPoolSize;
        duckPool.maxPoolSize = maxPoolSize;
        
        // Update SpawnManager to use the new pool
        spawnManager.duckPool = duckPool;
        
        Debug.Log($"ObjectPoolSetup: Created DuckPool with {initialPoolSize} initial ducks, max {maxPoolSize}");
        Debug.Log("ObjectPoolSetup: Updated SpawnManager to use object pooling");
        
        // Mark scene as dirty for saving
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(spawnManager);
        UnityEditor.EditorUtility.SetDirty(duckPool);
        #endif
    }
    
    /// <summary>
    /// Validates the current setup
    /// </summary>
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        bool isValid = true;
        
        if (duckPrefab == null)
        {
            Debug.LogError("ObjectPoolSetup: Duck prefab is not assigned");
            isValid = false;
        }
        else
        {
            DuckController duckController = duckPrefab.GetComponent<DuckController>();
            if (duckController == null)
            {
                Debug.LogError("ObjectPoolSetup: Duck prefab does not have DuckController component");
                isValid = false;
            }
        }
        
        if (spawnManager == null)
        {
            Debug.LogError("ObjectPoolSetup: SpawnManager is not assigned");
            isValid = false;
        }
        else
        {
            if (spawnManager.duckPool == null)
            {
                Debug.LogWarning("ObjectPoolSetup: SpawnManager does not have a DuckPool assigned");
                isValid = false;
            }
        }
        
        DuckPool duckPool = FindObjectOfType<DuckPool>();
        if (duckPool == null)
        {
            Debug.LogWarning("ObjectPoolSetup: No DuckPool found in scene");
            isValid = false;
        }
        
        if (isValid)
        {
            Debug.Log("ObjectPoolSetup: Setup validation passed - object pooling system is properly configured");
        }
        else
        {
            Debug.LogWarning("ObjectPoolSetup: Setup validation failed - please check the configuration");
        }
    }
}