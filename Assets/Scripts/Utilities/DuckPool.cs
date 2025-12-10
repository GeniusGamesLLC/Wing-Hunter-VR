using UnityEngine;

/// <summary>
/// Specialized object pool for duck controllers
/// </summary>
public class DuckPool : MonoBehaviour
{
    [Header("Pool Configuration")]
    [Tooltip("The duck prefab to pool")]
    public GameObject duckPrefab;
    
    [Tooltip("Initial number of ducks to create in the pool")]
    public int initialPoolSize = 5;
    
    [Tooltip("Maximum number of ducks in the pool (0 = unlimited)")]
    public int maxPoolSize = 20;
    
    // The actual object pool
    private ObjectPool<DuckController> duckPool;
    
    private void Awake()
    {
        InitializePool();
    }
    
    /// <summary>
    /// Initialize the duck pool
    /// </summary>
    private void InitializePool()
    {
        if (duckPrefab == null)
        {
            Debug.LogError("DuckPool: Duck prefab is not assigned");
            return;
        }
        
        // Verify the prefab has a DuckController component
        DuckController duckController = duckPrefab.GetComponent<DuckController>();
        if (duckController == null)
        {
            Debug.LogError("DuckPool: Duck prefab does not have a DuckController component");
            return;
        }
        
        // Create the pool with this transform as parent for organization
        duckPool = new ObjectPool<DuckController>(duckPrefab, initialPoolSize, maxPoolSize, transform);
        
        Debug.Log($"DuckPool: Initialized with {initialPoolSize} ducks, max size: {maxPoolSize}");
    }
    
    /// <summary>
    /// Get a duck from the pool
    /// </summary>
    /// <returns>A duck controller ready for use</returns>
    public DuckController GetDuck()
    {
        if (duckPool == null)
        {
            Debug.LogError("DuckPool: Pool not initialized");
            return null;
        }
        
        DuckController duck = duckPool.Get();
        
        if (duck != null)
        {
            // Reset duck state for reuse
            ResetDuckForReuse(duck);
        }
        
        return duck;
    }
    
    /// <summary>
    /// Return a duck to the pool
    /// </summary>
    /// <param name="duck">The duck to return</param>
    public void ReturnDuck(DuckController duck)
    {
        if (duck == null) return;
        
        if (duckPool == null)
        {
            Debug.LogError("DuckPool: Pool not initialized, destroying duck");
            Destroy(duck.gameObject);
            return;
        }
        
        // Return to pool (ResetForReuse will be called automatically)
        duckPool.Return(duck);
    }
    
    /// <summary>
    /// Reset duck state for reuse
    /// </summary>
    /// <param name="duck">The duck to reset</param>
    private void ResetDuckForReuse(DuckController duck)
    {
        // Use the duck's built-in reset method
        duck.ResetForReuse();
    }
    
    /// <summary>
    /// Get the current number of ducks available in the pool
    /// </summary>
    public int AvailableDucks => duckPool?.PoolCount ?? 0;
    
    /// <summary>
    /// Clear the pool and destroy all pooled ducks
    /// </summary>
    public void ClearPool()
    {
        duckPool?.Clear();
        Debug.Log("DuckPool: Pool cleared");
    }
    
    private void OnDestroy()
    {
        // Clean up the pool when this component is destroyed
        ClearPool();
    }
}