using UnityEngine;

/// <summary>
/// Automatically starts the VR Duck Hunt game and ensures proper setup
/// Attach this to any GameObject in the scene
/// </summary>
public class VRGameAutoStart : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoStartGame = true;
    public bool fixUIOnStart = true;
    public float startDelay = 2f;
    
    void Start()
    {
        if (fixUIOnStart)
        {
            FixUIForVR();
        }
        
        // Fix pink ground material at runtime
        FixGroundMaterial();
        
        if (autoStartGame)
        {
            Invoke(nameof(StartGame), startDelay);
        }
    }
    
    void FixUIForVR()
    {
        // Find and fix UI Canvas positioning
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                RectTransform rect = canvas.GetComponent<RectTransform>();
                
                // Position canvas in front of player
                rect.position = new Vector3(0, 2f, 3f);
                rect.rotation = Quaternion.identity;
                rect.localScale = Vector3.one * 0.005f;
                
                Debug.Log($"Fixed VR UI positioning for: {canvas.name}");
            }
        }
    }
    
    void FixGroundMaterial()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null) return;
        
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer == null) return;
        
        // Check if material is pink (missing material)
        if (renderer.material.color == Color.magenta || renderer.material.name.Contains("Default"))
        {
            // Try to load pre-created material first
            Material groundMat = Resources.Load<Material>("GroundMaterial");
            
            if (groundMat == null)
            {
                // Create VR-compatible material at runtime
                groundMat = CreateVRMaterial();
            }
            
            if (groundMat != null)
            {
                renderer.material = groundMat;
                Debug.Log("VRGameAutoStart: Fixed pink ground material");
            }
        }
    }
    
    Material CreateVRMaterial()
    {
        // Use the most VR-compatible shader available
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Mobile/Diffuse");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Standard");
        
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            return mat;
        }
        
        return null;
    }
    
    void StartGame()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.StartGame();
            Debug.Log("VRGameAutoStart: Started the game!");
        }
        else
        {
            Debug.LogWarning("VRGameAutoStart: GameManager not found!");
        }
    }
}