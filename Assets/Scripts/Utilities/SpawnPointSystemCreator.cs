using UnityEngine;

/// <summary>
/// Simple utility to create spawn point system in the scene
/// Run this from the Unity menu: Tools > Create Spawn Point System
/// </summary>
public class SpawnPointSystemCreator
{
    [UnityEditor.MenuItem("Tools/Create Spawn Point System")]
    public static void CreateSpawnPointSystem()
    {
        // Create main spawn point system GameObject
        GameObject spawnSystemGO = new GameObject("SpawnPointSystem");
        spawnSystemGO.transform.position = Vector3.zero;
        
        // Add the SpawnPointSetup component
        SpawnPointSetup setupComponent = spawnSystemGO.AddComponent<SpawnPointSetup>();
        
        // Configure default settings
        setupComponent.numberOfSpawnPoints = 8;
        setupComponent.spawnRadius = 12f;
        setupComponent.minHeight = 2f;
        setupComponent.maxHeight = 6f;
        setupComponent.targetRadius = 15f;
        
        // Try to find and assign SpawnManager
        SpawnManager spawnManager = Object.FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            setupComponent.spawnManager = spawnManager;
        }
        
        // Create the spawn points immediately
        setupComponent.CreateSpawnPointSystem();
        
        // Select the created object in hierarchy
        UnityEditor.Selection.activeGameObject = spawnSystemGO;
        
        Debug.Log("SpawnPointSystemCreator: Created spawn point system with 8 spawn points and 8 target points");
    }
    
    [UnityEditor.MenuItem("Tools/Clear Spawn Point System")]
    public static void ClearSpawnPointSystem()
    {
        // Find and destroy existing spawn point system
        SpawnPointSetup[] existingSystems = Object.FindObjectsOfType<SpawnPointSetup>();
        foreach (SpawnPointSetup system in existingSystems)
        {
            system.ClearSpawnPoints();
            Object.DestroyImmediate(system.gameObject);
        }
        
        Debug.Log("SpawnPointSystemCreator: Cleared all spawn point systems");
    }
}