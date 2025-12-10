using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Sets up the scene environment for VR Duck Hunt including skybox, ground, lighting, and UI positioning
/// </summary>
public class SceneEnvironmentSetup : MonoBehaviour
{
    [Header("Environment Setup")]
    [Tooltip("Automatically setup environment on Start")]
    public bool autoSetupOnStart = true;
    
    [Header("Ground Configuration")]
    [Tooltip("Size of the ground plane")]
    public float groundSize = 50f;
    
    [Tooltip("Ground material color")]
    public Color groundColor = new Color(0.2f, 0.6f, 0.2f, 1f); // Green grass color
    
    [Header("Lighting Configuration")]
    [Tooltip("Main directional light intensity")]
    public float lightIntensity = 1.2f;
    
    [Tooltip("Main directional light color")]
    public Color lightColor = new Color(1f, 0.95f, 0.8f, 1f); // Warm sunlight
    
    [Tooltip("Ambient light intensity")]
    public float ambientIntensity = 0.3f;
    
    [Header("Skybox Configuration")]
    [Tooltip("Sky tint color")]
    public Color skyTint = new Color(0.5f, 0.8f, 1f, 1f); // Light blue
    
    [Tooltip("Ground color for procedural skybox")]
    public Color skyGroundColor = new Color(0.4f, 0.3f, 0.2f, 1f); // Brown ground
    
    [Header("UI Configuration")]
    [Tooltip("Distance from player to place UI canvas")]
    public float uiDistance = 3f;
    
    [Tooltip("Height of UI canvas")]
    public float uiHeight = 2f;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupEnvironment();
        }
    }
    
    [ContextMenu("Setup Environment")]
    public void SetupEnvironment()
    {
        SetupSkybox();
        SetupGroundPlane();
        SetupLighting();
        SetupUIPositioning();
        
        Debug.Log("SceneEnvironmentSetup: Environment setup complete");
    }
    
    /// <summary>
    /// Sets up a procedural skybox for outdoor environment
    /// </summary>
    private void SetupSkybox()
    {
        // Create or find procedural skybox material
        Material skyboxMaterial = CreateProceduralSkybox();
        
        // Apply skybox to render settings
        RenderSettings.skybox = skyboxMaterial;
        
        // Update skybox in scene view
        DynamicGI.UpdateEnvironment();
        
        Debug.Log("SceneEnvironmentSetup: Skybox configured");
    }
    
    /// <summary>
    /// Creates a procedural skybox material
    /// </summary>
    /// <returns>Procedural skybox material</returns>
    private Material CreateProceduralSkybox()
    {
        // Create material with procedural skybox shader
        Material skyboxMat = new Material(Shader.Find("Skybox/Procedural"));
        
        // Configure skybox properties
        skyboxMat.SetFloat("_SunSize", 0.04f);
        skyboxMat.SetFloat("_SunSizeConvergence", 5f);
        skyboxMat.SetFloat("_AtmosphereThickness", 1f);
        skyboxMat.SetColor("_SkyTint", skyTint);
        skyboxMat.SetColor("_GroundColor", skyGroundColor);
        skyboxMat.SetFloat("_Exposure", 1.3f);
        
        return skyboxMat;
    }
    
    /// <summary>
    /// Creates a ground plane with appropriate material
    /// </summary>
    private void SetupGroundPlane()
    {
        // Check if ground already exists
        GameObject existingGround = GameObject.Find("Ground");
        if (existingGround != null)
        {
            DestroyImmediate(existingGround);
        }
        
        // Create ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = Vector3.one * groundSize;
        
        // Create and apply ground material
        Material groundMaterial = CreateGroundMaterial();
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            groundRenderer.material = groundMaterial;
        }
        
        Debug.Log("SceneEnvironmentSetup: Ground plane created");
    }
    
    /// <summary>
    /// Creates a material for the ground plane
    /// </summary>
    /// <returns>Ground material</returns>
    private Material CreateGroundMaterial()
    {
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = groundColor;
        groundMat.SetFloat("_Smoothness", 0.1f); // Slightly rough surface
        groundMat.SetFloat("_Metallic", 0f); // Non-metallic
        
        return groundMat;
    }
    
    /// <summary>
    /// Sets up ambient lighting for the scene
    /// </summary>
    private void SetupLighting()
    {
        // Configure ambient lighting
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f, 1f); // Light blue sky
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Neutral equator
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark ground
        RenderSettings.ambientIntensity = ambientIntensity;
        
        // Find or create main directional light
        Light mainLight = FindMainDirectionalLight();
        if (mainLight == null)
        {
            mainLight = CreateMainDirectionalLight();
        }
        
        // Configure main light
        mainLight.intensity = lightIntensity;
        mainLight.color = lightColor;
        mainLight.shadows = LightShadows.Soft;
        
        // Position light for nice outdoor lighting (45 degrees up, slightly to the side)
        mainLight.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
        
        Debug.Log("SceneEnvironmentSetup: Lighting configured");
    }
    
    /// <summary>
    /// Finds the main directional light in the scene
    /// </summary>
    /// <returns>Main directional light or null if not found</returns>
    private Light FindMainDirectionalLight()
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                return light;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Creates a main directional light
    /// </summary>
    /// <returns>Created directional light</returns>
    private Light CreateMainDirectionalLight()
    {
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        
        return light;
    }
    
    /// <summary>
    /// Positions UI canvas for comfortable VR viewing
    /// </summary>
    private void SetupUIPositioning()
    {
        // Find UI canvas in scene
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Canvas worldSpaceCanvas = null;
        
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                worldSpaceCanvas = canvas;
                break;
            }
        }
        
        if (worldSpaceCanvas != null)
        {
            // Position canvas in front of player at comfortable distance and height
            worldSpaceCanvas.transform.position = new Vector3(0f, uiHeight, uiDistance);
            worldSpaceCanvas.transform.rotation = Quaternion.identity;
            
            // Scale canvas appropriately for VR viewing
            worldSpaceCanvas.transform.localScale = Vector3.one * 0.01f; // Scale down for world space
            
            Debug.Log("SceneEnvironmentSetup: UI canvas positioned for VR viewing");
        }
        else
        {
            Debug.LogWarning("SceneEnvironmentSetup: No world-space UI canvas found to position");
        }
    }
    
    /// <summary>
    /// Resets the environment to default Unity settings
    /// </summary>
    [ContextMenu("Reset Environment")]
    public void ResetEnvironment()
    {
        // Reset skybox
        RenderSettings.skybox = null;
        
        // Reset ambient lighting
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1f;
        
        // Remove created ground
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            DestroyImmediate(ground);
        }
        
        Debug.Log("SceneEnvironmentSetup: Environment reset to defaults");
    }
}