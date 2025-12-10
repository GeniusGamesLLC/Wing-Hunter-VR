using UnityEngine;
using System.Collections;

/// <summary>
/// Simple test script to verify object pool functionality
/// </summary>
public class ObjectPoolTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("The duck pool to test")]
    public DuckPool duckPool;
    
    [Tooltip("Number of ducks to spawn for testing")]
    public int testDuckCount = 3;
    
    [Tooltip("Time between spawning test ducks")]
    public float spawnInterval = 1f;
    
    [Tooltip("Time before returning ducks to pool")]
    public float returnDelay = 3f;
    
    [Header("Test Actions")]
    [Tooltip("Click to run the object pool test")]
    public bool runTest = false;
    
    private void Update()
    {
        if (runTest)
        {
            runTest = false;
            StartCoroutine(RunObjectPoolTest());
        }
    }
    
    /// <summary>
    /// Runs a simple test of the object pool system
    /// </summary>
    [ContextMenu("Run Object Pool Test")]
    public System.Collections.IEnumerator RunObjectPoolTest()
    {
        if (duckPool == null)
        {
            Debug.LogError("ObjectPoolTester: DuckPool is not assigned");
            yield break;
        }
        
        Debug.Log($"ObjectPoolTester: Starting test with {testDuckCount} ducks");
        Debug.Log($"ObjectPoolTester: Initial pool count: {duckPool.AvailableDucks}");
        
        // Store references to spawned ducks
        DuckController[] testDucks = new DuckController[testDuckCount];
        
        // Spawn test ducks
        for (int i = 0; i < testDuckCount; i++)
        {
            DuckController duck = duckPool.GetDuck();
            if (duck != null)
            {
                testDucks[i] = duck;
                
                // Position the duck at a test location
                Vector3 testPosition = new Vector3(i * 2f, 2f, 0f);
                duck.transform.position = testPosition;
                
                // Initialize with simple movement
                Vector3 targetPosition = testPosition + Vector3.forward * 5f;
                duck.Initialize(testPosition, targetPosition, 2f);
                
                Debug.Log($"ObjectPoolTester: Spawned test duck {i + 1} at {testPosition}");
            }
            else
            {
                Debug.LogError($"ObjectPoolTester: Failed to get duck {i + 1} from pool");
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
        
        Debug.Log($"ObjectPoolTester: Pool count after spawning: {duckPool.AvailableDucks}");
        
        // Wait before returning ducks
        yield return new WaitForSeconds(returnDelay);
        
        // Return ducks to pool
        for (int i = 0; i < testDucks.Length; i++)
        {
            if (testDucks[i] != null)
            {
                duckPool.ReturnDuck(testDucks[i]);
                Debug.Log($"ObjectPoolTester: Returned test duck {i + 1} to pool");
            }
        }
        
        Debug.Log($"ObjectPoolTester: Final pool count: {duckPool.AvailableDucks}");
        Debug.Log("ObjectPoolTester: Test completed successfully");
    }
    
    /// <summary>
    /// Simple test that can be called from inspector
    /// </summary>
    [ContextMenu("Quick Pool Test")]
    public void QuickPoolTest()
    {
        StartCoroutine(RunObjectPoolTest());
    }
}