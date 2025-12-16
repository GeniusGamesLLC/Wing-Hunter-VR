using UnityEngine;

/// <summary>
/// Controls the infinite ground effect by updating the shader's player position
/// to ensure the fade effect follows the player in VR.
/// </summary>
public class InfiniteGroundController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Renderer groundRenderer;
    
    [Header("Ground Settings")]
    [SerializeField] private Color groundColor = new Color(0.25f, 0.45f, 0.2f, 1f);
    [SerializeField] private Color horizonColor = new Color(0.6f, 0.75f, 0.85f, 0f);
    [SerializeField] private float fadeStartDistance = 30f;
    [SerializeField] private float fadeEndDistance = 80f;
    
    private Material groundMaterial;
    private static readonly int PlayerPositionID = Shader.PropertyToID("_PlayerPosition");
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int HorizonColorID = Shader.PropertyToID("_HorizonColor");
    private static readonly int FadeStartID = Shader.PropertyToID("_FadeStart");
    private static readonly int FadeEndID = Shader.PropertyToID("_FadeEnd");
    
    private void Start()
    {
        // Try to find player if not assigned
        if (playerTransform == null)
        {
            // Look for XR Origin or Main Camera
            var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                playerTransform = xrOrigin.Camera?.transform ?? xrOrigin.transform;
            }
            else
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    playerTransform = mainCamera.transform;
                }
            }
        }
        
        // Try to find ground renderer if not assigned
        if (groundRenderer == null)
        {
            groundRenderer = GetComponent<Renderer>();
        }
        
        // Get material instance
        if (groundRenderer != null)
        {
            groundMaterial = groundRenderer.material;
            ApplySettings();
        }
        else
        {
            Debug.LogWarning("InfiniteGroundController: No ground renderer found!");
        }
    }
    
    private void Update()
    {
        if (groundMaterial != null && playerTransform != null)
        {
            // Update player position in shader
            Vector3 playerPos = playerTransform.position;
            groundMaterial.SetVector(PlayerPositionID, new Vector4(playerPos.x, playerPos.y, playerPos.z, 0));
        }
    }
    
    /// <summary>
    /// Apply all settings to the material
    /// </summary>
    public void ApplySettings()
    {
        if (groundMaterial == null) return;
        
        groundMaterial.SetColor(BaseColorID, groundColor);
        groundMaterial.SetColor(HorizonColorID, horizonColor);
        groundMaterial.SetFloat(FadeStartID, fadeStartDistance);
        groundMaterial.SetFloat(FadeEndID, fadeEndDistance);
    }
    
    /// <summary>
    /// Set the ground color at runtime
    /// </summary>
    public void SetGroundColor(Color color)
    {
        groundColor = color;
        if (groundMaterial != null)
        {
            groundMaterial.SetColor(BaseColorID, groundColor);
        }
    }
    
    /// <summary>
    /// Set the horizon color at runtime
    /// </summary>
    public void SetHorizonColor(Color color)
    {
        horizonColor = color;
        if (groundMaterial != null)
        {
            groundMaterial.SetColor(HorizonColorID, horizonColor);
        }
    }
    
    /// <summary>
    /// Set the fade distances at runtime
    /// </summary>
    public void SetFadeDistances(float start, float end)
    {
        fadeStartDistance = start;
        fadeEndDistance = end;
        if (groundMaterial != null)
        {
            groundMaterial.SetFloat(FadeStartID, fadeStartDistance);
            groundMaterial.SetFloat(FadeEndID, fadeEndDistance);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up material instance
        if (groundMaterial != null)
        {
            Destroy(groundMaterial);
        }
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Apply settings in editor when values change
        if (groundRenderer != null && Application.isPlaying)
        {
            ApplySettings();
        }
    }
#endif
}
