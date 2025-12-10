using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Complete scene setup for VR Duck Hunt - combines spawn points and environment setup
/// This script should be added to the scene and run once to configure everything
/// </summary>
public class VRDuckHuntSceneSetup : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("Automatically setup scene on Start")]
    public bool autoSetupOnStart = true;
    
    [Header("Spawn Point Configuration")]
    [Tooltip("Number of spawn points to create")]
    public int numberOfSpawnPoints = 8;
    
    [Tooltip("Radius from center for spawn points")]
    public float spawnRadius = 12f;
    
    [Tooltip("Height range for spawn points")]
    public Vector2 spawnHeightRange = new Vector2(2f, 6f);
    
    [Tooltip("Radius from center for target points")]
    public float targetRadius = 15f;
    
    [Header("Environment Configuration")]
    [Tooltip("Size of the ground plane")]
    public float groundSize = 50f;
    
    [Tooltip("Ground color")]
    public Color groundColor = new Color(0.2f, 0.6f, 0.2f, 1f);
    
    [Tooltip("Main light intensity")]
    public float lightIntensity = 1.2f;
    
    [Tooltip("Sky tint color")]
    public Color skyTint = new Color(0.5f, 0.8f, 1f, 1f);
    
    [Header("UI Configuration")]
    [Tooltip("Distance from player to place UI")]
    public float uiDistance = 3f;
    
    [Tooltip("Height of UI canvas")]
    public float uiHeight = 2f;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupCompleteScene();
        }
    }
    
    [ContextMenu("Setup Complete Scene")]
    public void SetupCompleteScene()
    {
        Debug.Log("VRDuckHuntSceneSetup: Starting complete scene setup...");
        
        SetupSpawnPoints();
        SetupEnvironment();
        
        Debug.Log("VRDuckHuntSceneSetup: Complete scene setup finished!");
    }
    
    /// <summary>
    /// Sets up spawn points and assigns them to SpawnManager
    /// </summary>
    private void SetupSpawnPoints()
    {
        // Find SpawnManager
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogWarning("VRDuckHuntSceneSetup: No SpawnManager found in scene. Spawn points will be created but not assigned.");
        }
        
        // Clear existing spawn points
        ClearExistingSpawnPoints();
        
        // Create spawn points array
        Transform[] spawnPoints = new Transform[numberOfSpawnPoints];
        Transform[] targetPoints = new Transform[numberOfSpawnPoints];
        
        // Create parent objects for organization
        GameObject spawnParent = new GameObject("SpawnPoints");
        GameObject targetParent = new GameObject("TargetPoints");
        
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            // Create spawn point (arc around back and sides, leaving front clear for player)
            float spawnAngle = -135f + (270f / (numberOfSpawnPoints - 1)) * i;
            Vector3 spawnPos = CreatePointAtAngle(spawnAngle, spawnRadius, spawnHeightRange);
            
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i:D2}");
            spawnPoint.transform.SetParent(spawnParent.transform);
            spawnPoint.transform.position = spawnPos;
            spawnPoints[i] = spawnPoint.transform;
            
            // Add visual indicator for spawn points (green spheres)
            CreateVisualIndicator(spawnPoint, PrimitiveType.Sphere, Color.green);
            
            // Create target point (opposite side)
            float targetAngle = spawnAngle + 180f;
            Vector3 targetPos = CreatePointAtAngle(targetAngle, targetRadius, spawnHeightRange);
            
            GameObject targetPoint = new GameObject($"TargetPoint_{i:D2}");
            targetPoint.transform.SetParent(targetParent.transform);
            targetPoint.transform.position = targetPos;
            targetPoints[i] = targetPoint.transform;
            
            // Add visual indicator for target points (red cubes)
            CreateVisualIndicator(targetPoint, PrimitiveType.Cube, Color.red);
        }
        
        // Assign to SpawnManager if found
        if (spawnManager != null)
        {
            spawnManager.SpawnPoints = spawnPoints;
            spawnManager.TargetPoints = targetPoints;
            Debug.Log($"VRDuckHuntSceneSetup: Created and assigned {numberOfSpawnPoints} spawn points to SpawnManager");
        }
        else
        {
            Debug.Log($"VRDuckHuntSceneSetup: Created {numberOfSpawnPoints} spawn points (no SpawnManager to assign to)");
        }
    }
    
    /// <summary>
    /// Sets up the complete environment (skybox, ground, lighting, UI)
    /// </summary>
    private void SetupEnvironment()
    {
        SetupSkybox();
        SetupGroundPlane();
        SetupLighting();
        SetupUIPositioning();
        
        Debug.Log("VRDuckHuntSceneSetup: Environment setup complete");
    }
    
    /// <summary>
    /// Creates a point at the specified angle and radius
    /// </summary>
    private Vector3 CreatePointAtAngle(float angleDegrees, float radius, Vector2 heightRange)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float x = Mathf.Cos(angleRad) * radius;
        float z = Mathf.Sin(angleRad) * radius;
        float y = Random.Range(heightRange.x, heightRange.y);
        
        return new Vector3(x, y, z);
    }
    
    /// <summary>
    /// Creates a visual indicator for spawn/target points
    /// </summary>
    private void CreateVisualIndicator(GameObject parent, PrimitiveType primitiveType, Color color)
    {
        GameObject indicator = GameObject.CreatePrimitive(primitiveType);
        indicator.name = "Indicator";
        indicator.transform.SetParent(parent.transform);
        indicator.transform.localPosition = Vector3.zero;
        indicator.transform.localScale = Vector3.one * 0.3f;
        
        // Remove collider (we don't want it to interfere with gameplay)
        Collider collider = indicator.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyImmediate(collider);
        }
        
        // Set color
        Renderer renderer = indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }
    }
    
    /// <summary>
    /// Clears existing spawn points from the scene
    /// </summary>
    private void ClearExistingSpawnPoints()
    {
        // Find and destroy existing spawn point objects
        GameObject spawnParent = GameObject.Find("SpawnPoints");
        if (spawnParent != null)
        {
            DestroyImmediate(spawnParent);
        }
        
        GameObject targetParent = GameObject.Find("TargetPoints");
        if (targetParent != null)
        {
            DestroyImmediate(targetParent);
        }
    }
    
    /// <summary>
    /// Sets up procedural skybox
    /// </summary>
    private void SetupSkybox()
    {
        Material skyboxMat = new Material(Shader.Find("Skybox/Procedural"));
        skyboxMat.SetFloat("_SunSize", 0.04f);
        skyboxMat.SetFloat("_SunSizeConvergence", 5f);
        skyboxMat.SetFloat("_AtmosphereThickness", 1f);
        skyboxMat.SetColor("_SkyTint", skyTint);
        skyboxMat.SetColor("_GroundColor", new Color(0.4f, 0.3f, 0.2f, 1f));
        skyboxMat.SetFloat("_Exposure", 1.3f);
        
        RenderSettings.skybox = skyboxMat;
        // Environment lighting will update automatically
    }
    
    /// <summary>
    /// Creates ground plane
    /// </summary>
    private void SetupGroundPlane()
    {
        // Remove existing ground
        GameObject existingGround = GameObject.Find("Ground");
        if (existingGround != null)
        {
            DestroyImmediate(existingGround);
        }
        
        // Create new ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = Vector3.one * groundSize;
        
        // Apply material
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = groundColor;
        groundMat.SetFloat("_Smoothness", 0.1f);
        groundMat.SetFloat("_Metallic", 0f);
        
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = groundMat;
        }
    }
    
    /// <summary>
    /// Sets up scene lighting
    /// </summary>
    private void SetupLighting()
    {
        // Configure ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        RenderSettings.ambientIntensity = 0.3f;
        
        // Find or create main light
        Light mainLight = FindObjectOfType<Light>();
        if (mainLight == null)
        {
            GameObject lightGO = new GameObject("Directional Light");
            mainLight = lightGO.AddComponent<Light>();
            mainLight.type = LightType.Directional;
        }
        
        // Configure main light
        mainLight.intensity = lightIntensity;
        mainLight.color = new Color(1f, 0.95f, 0.8f, 1f);
        mainLight.shadows = LightShadows.Soft;
        mainLight.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
    }
    
    /// <summary>
    /// Positions UI canvas for VR viewing
    /// </summary>
    private void SetupUIPositioning()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                canvas.transform.position = new Vector3(0f, uiHeight, uiDistance);
                canvas.transform.rotation = Quaternion.identity;
                canvas.transform.localScale = Vector3.one * 0.01f;
                
                Debug.Log("VRDuckHuntSceneSetup: Positioned UI canvas for VR viewing");
                break;
            }
        }
    }
}