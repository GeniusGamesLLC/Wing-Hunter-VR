using UnityEngine;

/// <summary>
/// Utility script to set up spawn points and target points in an arc formation around the player
/// This script should be attached to an empty GameObject and run once to create the spawn system
/// </summary>
public class SpawnPointSetup : MonoBehaviour
{
    [Header("Spawn Point Configuration")]
    [Tooltip("Number of spawn points to create")]
    public int numberOfSpawnPoints = 8;
    
    [Tooltip("Radius from center for spawn points")]
    public float spawnRadius = 12f;
    
    [Tooltip("Minimum height for spawn points")]
    public float minHeight = 2f;
    
    [Tooltip("Maximum height for spawn points")]
    public float maxHeight = 6f;
    
    [Tooltip("Radius from center for target points")]
    public float targetRadius = 15f;
    
    [Tooltip("Reference to the SpawnManager to assign points to")]
    public SpawnManager spawnManager;
    
    [Header("Setup Controls")]
    [Tooltip("Click to create spawn points")]
    public bool createSpawnPoints = false;
    
    [Tooltip("Click to clear existing spawn points")]
    public bool clearSpawnPoints = false;
    
    private Transform spawnPointParent;
    private Transform targetPointParent;
    
    private void Update()
    {
        // Handle setup controls in editor
        if (createSpawnPoints)
        {
            createSpawnPoints = false;
            CreateSpawnPointSystem();
        }
        
        if (clearSpawnPoints)
        {
            clearSpawnPoints = false;
            ClearSpawnPoints();
        }
    }
    
    /// <summary>
    /// Creates the complete spawn point system with spawn and target points
    /// </summary>
    [ContextMenu("Create Spawn Point System")]
    public void CreateSpawnPointSystem()
    {
        // Clear existing points first
        ClearSpawnPoints();
        
        // Create parent objects for organization
        CreateParentObjects();
        
        // Create spawn points in arc formation
        Transform[] spawnPoints = CreateSpawnPoints();
        
        // Create corresponding target points on opposite side
        Transform[] targetPoints = CreateTargetPoints();
        
        // Assign to SpawnManager if available
        AssignToSpawnManager(spawnPoints, targetPoints);
        
        Debug.Log($"SpawnPointSetup: Created {spawnPoints.Length} spawn points and {targetPoints.Length} target points");
    }
    
    /// <summary>
    /// Creates parent GameObjects to organize spawn and target points
    /// </summary>
    private void CreateParentObjects()
    {
        // Create spawn points parent
        GameObject spawnParentGO = new GameObject("SpawnPoints");
        spawnParentGO.transform.SetParent(transform);
        spawnParentGO.transform.localPosition = Vector3.zero;
        spawnPointParent = spawnParentGO.transform;
        
        // Create target points parent
        GameObject targetParentGO = new GameObject("TargetPoints");
        targetParentGO.transform.SetParent(transform);
        targetParentGO.transform.localPosition = Vector3.zero;
        targetPointParent = targetParentGO.transform;
    }
    
    /// <summary>
    /// Creates spawn points in an arc formation around the player
    /// </summary>
    /// <returns>Array of spawn point transforms</returns>
    private Transform[] CreateSpawnPoints()
    {
        Transform[] spawnPoints = new Transform[numberOfSpawnPoints];
        
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            // Calculate angle for this spawn point (distribute around 270 degrees, leaving front area clear)
            float angleOffset = -135f; // Start from back-left
            float angleRange = 270f; // Cover back and sides, leave front clear
            float angle = angleOffset + (angleRange / (numberOfSpawnPoints - 1)) * i;
            float angleRad = angle * Mathf.Deg2Rad;
            
            // Calculate position
            float x = Mathf.Cos(angleRad) * spawnRadius;
            float z = Mathf.Sin(angleRad) * spawnRadius;
            float y = Random.Range(minHeight, maxHeight);
            
            Vector3 position = new Vector3(x, y, z);
            
            // Create spawn point GameObject
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i:D2}");
            spawnPoint.transform.SetParent(spawnPointParent);
            spawnPoint.transform.position = position;
            
            // Add a visual indicator (small sphere) for editor visibility
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = "Indicator";
            indicator.transform.SetParent(spawnPoint.transform);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = Vector3.one * 0.3f;
            
            // Make indicator semi-transparent and colored
            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0f, 1f, 0f, 0.5f); // Green, semi-transparent
                mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                renderer.material = mat;
            }
            
            // Remove collider from indicator (we don't want it to interfere with gameplay)
            Collider collider = indicator.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            spawnPoints[i] = spawnPoint.transform;
        }
        
        return spawnPoints;
    }
    
    /// <summary>
    /// Creates target points on the opposite side of spawn points
    /// </summary>
    /// <returns>Array of target point transforms</returns>
    private Transform[] CreateTargetPoints()
    {
        Transform[] targetPoints = new Transform[numberOfSpawnPoints];
        
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            // Calculate angle for this target point (opposite side of corresponding spawn point)
            float angleOffset = -135f + 180f; // Opposite side
            float angleRange = 270f;
            float angle = angleOffset + (angleRange / (numberOfSpawnPoints - 1)) * i;
            float angleRad = angle * Mathf.Deg2Rad;
            
            // Calculate position
            float x = Mathf.Cos(angleRad) * targetRadius;
            float z = Mathf.Sin(angleRad) * targetRadius;
            float y = Random.Range(minHeight, maxHeight);
            
            Vector3 position = new Vector3(x, y, z);
            
            // Create target point GameObject
            GameObject targetPoint = new GameObject($"TargetPoint_{i:D2}");
            targetPoint.transform.SetParent(targetPointParent);
            targetPoint.transform.position = position;
            
            // Add a visual indicator (small cube) for editor visibility
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicator.name = "Indicator";
            indicator.transform.SetParent(targetPoint.transform);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = Vector3.one * 0.3f;
            
            // Make indicator semi-transparent and colored
            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0f, 0f, 0.5f); // Red, semi-transparent
                mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                renderer.material = mat;
            }
            
            // Remove collider from indicator
            Collider collider = indicator.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            targetPoints[i] = targetPoint.transform;
        }
        
        return targetPoints;
    }
    
    /// <summary>
    /// Assigns the created spawn and target points to the SpawnManager
    /// </summary>
    /// <param name="spawnPoints">Array of spawn point transforms</param>
    /// <param name="targetPoints">Array of target point transforms</param>
    private void AssignToSpawnManager(Transform[] spawnPoints, Transform[] targetPoints)
    {
        if (spawnManager == null)
        {
            // Try to find SpawnManager in scene
            spawnManager = FindObjectOfType<SpawnManager>();
        }
        
        if (spawnManager != null)
        {
            spawnManager.SpawnPoints = spawnPoints;
            spawnManager.TargetPoints = targetPoints;
            Debug.Log($"SpawnPointSetup: Assigned {spawnPoints.Length} spawn points and {targetPoints.Length} target points to SpawnManager");
        }
        else
        {
            Debug.LogWarning("SpawnPointSetup: No SpawnManager found. Please assign spawn and target points manually.");
        }
    }
    
    /// <summary>
    /// Clears existing spawn points
    /// </summary>
    [ContextMenu("Clear Spawn Points")]
    public void ClearSpawnPoints()
    {
        // Clear spawn points
        if (spawnPointParent != null)
        {
            DestroyImmediate(spawnPointParent.gameObject);
            spawnPointParent = null;
        }
        
        // Clear target points
        if (targetPointParent != null)
        {
            DestroyImmediate(targetPointParent.gameObject);
            targetPointParent = null;
        }
        
        // Also look for existing spawn/target point objects
        GameObject[] existingSpawnPoints = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in existingSpawnPoints)
        {
            if (obj.name.StartsWith("SpawnPoint_") || obj.name.StartsWith("TargetPoint_") || 
                obj.name == "SpawnPoints" || obj.name == "TargetPoints")
            {
                DestroyImmediate(obj);
            }
        }
        
        Debug.Log("SpawnPointSetup: Cleared existing spawn points");
    }
}