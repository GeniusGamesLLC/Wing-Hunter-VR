using UnityEngine;

/// <summary>
/// Simple script to create spawn points when the scene starts
/// Attach this to any GameObject in the scene and it will create spawn points automatically
/// </summary>
public class SimpleSpawnPointCreator : MonoBehaviour
{
    [Header("Auto-Setup Configuration")]
    [Tooltip("Automatically create spawn points on Start")]
    public bool autoCreateOnStart = true;
    
    [Tooltip("Number of spawn points to create")]
    public int numberOfSpawnPoints = 8;
    
    [Tooltip("Radius from center for spawn points")]
    public float spawnRadius = 12f;
    
    [Tooltip("Height range for spawn points")]
    public Vector2 heightRange = new Vector2(2f, 6f);
    
    [Tooltip("Radius from center for target points")]
    public float targetRadius = 15f;
    
    private void Start()
    {
        if (autoCreateOnStart)
        {
            CreateSpawnPoints();
        }
    }
    
    [ContextMenu("Create Spawn Points")]
    public void CreateSpawnPoints()
    {
        // Find SpawnManager in scene
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("SimpleSpawnPointCreator: No SpawnManager found in scene!");
            return;
        }
        
        // Create spawn points array
        Transform[] spawnPoints = new Transform[numberOfSpawnPoints];
        Transform[] targetPoints = new Transform[numberOfSpawnPoints];
        
        // Create parent objects
        GameObject spawnParent = new GameObject("SpawnPoints");
        GameObject targetParent = new GameObject("TargetPoints");
        
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            // Create spawn point (arc around back and sides, leaving front clear)
            float spawnAngle = -135f + (270f / (numberOfSpawnPoints - 1)) * i;
            Vector3 spawnPos = CreatePointAtAngle(spawnAngle, spawnRadius);
            
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i:D2}");
            spawnPoint.transform.SetParent(spawnParent.transform);
            spawnPoint.transform.position = spawnPos;
            spawnPoints[i] = spawnPoint.transform;
            
            // Create target point (opposite side)
            float targetAngle = spawnAngle + 180f;
            Vector3 targetPos = CreatePointAtAngle(targetAngle, targetRadius);
            
            GameObject targetPoint = new GameObject($"TargetPoint_{i:D2}");
            targetPoint.transform.SetParent(targetParent.transform);
            targetPoint.transform.position = targetPos;
            targetPoints[i] = targetPoint.transform;
        }
        
        // Assign to SpawnManager
        spawnManager.SpawnPoints = spawnPoints;
        spawnManager.TargetPoints = targetPoints;
        
        Debug.Log($"SimpleSpawnPointCreator: Created {numberOfSpawnPoints} spawn points and assigned to SpawnManager");
    }
    
    private Vector3 CreatePointAtAngle(float angleDegrees, float radius)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float x = Mathf.Cos(angleRad) * radius;
        float z = Mathf.Sin(angleRad) * radius;
        float y = Random.Range(heightRange.x, heightRange.y);
        
        return new Vector3(x, y, z);
    }
}