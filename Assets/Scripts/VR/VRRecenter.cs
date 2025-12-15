using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

/// <summary>
/// Handles VR view recentering to fix player orientation issues.
/// Allows players to recenter their view using a button press (left menu button).
/// This solves the issue where VR headset tracking overrides XR Origin rotation at runtime.
/// </summary>
public class VRRecenter : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Input action for recenter button (typically left menu button)")]
    [SerializeField] private InputActionProperty recenterAction;
    
    [Tooltip("Use built-in menu button binding if no action is assigned")]
    [SerializeField] private bool useDefaultMenuButton = true;
    
    [Header("References")]
    [Tooltip("The XR Origin to rotate. If not set, will find automatically.")]
    [SerializeField] private XROrigin xrOrigin;
    
    [Header("Settings")]
    [Tooltip("The forward direction the player should face after recentering (world space)")]
    [SerializeField] private Vector3 targetForward = Vector3.forward;
    
    [Tooltip("Show debug messages in console")]
    [SerializeField] private bool debugMode = true;
    
    [Tooltip("Automatically recenter on game start after this delay (0 = disabled)")]
    [SerializeField] private float autoRecenterDelay = 1.5f;
    
    private Camera vrCamera;
    private InputAction menuButtonAction;
    
    void Awake()
    {
        // Find XR Origin if not assigned
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("VRRecenter: No XROrigin found in scene!");
                enabled = false;
                return;
            }
        }
        
        vrCamera = xrOrigin.Camera;
        if (vrCamera == null)
        {
            Debug.LogError("VRRecenter: No camera found on XROrigin!");
            enabled = false;
            return;
        }
        
        // Create default menu button action if needed
        if (useDefaultMenuButton && (recenterAction.action == null || string.IsNullOrEmpty(recenterAction.action.name)))
        {
            CreateDefaultMenuButtonAction();
        }
    }
    
    private void CreateDefaultMenuButtonAction()
    {
        // Create an input action for the left controller menu button
        // On Quest: this is the menu button on the left controller
        // The binding path uses {MenuButton} which maps to the appropriate button
        menuButtonAction = new InputAction("RecenterView", InputActionType.Button);
        
        // Add bindings for common VR controller menu buttons
        // Left controller menu button (Quest, Vive, Index)
        menuButtonAction.AddBinding("<XRController>{LeftHand}/{MenuButton}");
        // Also bind to left secondary button as fallback (Y button on Quest)
        menuButtonAction.AddBinding("<XRController>{LeftHand}/{SecondaryButton}");
        
        if (debugMode)
        {
            Debug.Log("VRRecenter: Created default menu button action for recentering");
        }
    }
    
    void OnEnable()
    {
        // Enable and subscribe to recenter action
        if (recenterAction.action != null && !string.IsNullOrEmpty(recenterAction.action.name))
        {
            recenterAction.action.Enable();
            recenterAction.action.performed += OnRecenterPressed;
        }
        else if (menuButtonAction != null)
        {
            menuButtonAction.Enable();
            menuButtonAction.performed += OnRecenterPressed;
        }
    }
    
    void OnDisable()
    {
        // Unsubscribe from recenter action
        if (recenterAction.action != null)
        {
            recenterAction.action.performed -= OnRecenterPressed;
        }
        
        if (menuButtonAction != null)
        {
            menuButtonAction.performed -= OnRecenterPressed;
            menuButtonAction.Disable();
        }
    }
    
    void OnDestroy()
    {
        if (menuButtonAction != null)
        {
            menuButtonAction.Dispose();
            menuButtonAction = null;
        }
    }

    
    void Start()
    {
        // Auto-recenter after delay if enabled
        if (autoRecenterDelay > 0f)
        {
            Invoke(nameof(RecenterView), autoRecenterDelay);
        }
    }
    
    private void OnRecenterPressed(InputAction.CallbackContext context)
    {
        RecenterView();
    }
    
    /// <summary>
    /// Recenters the VR view so the player faces the target forward direction.
    /// Call this method to programmatically recenter the view.
    /// </summary>
    public void RecenterView()
    {
        if (xrOrigin == null || vrCamera == null)
        {
            Debug.LogWarning("VRRecenter: Cannot recenter - missing references");
            return;
        }
        
        // Get the camera's current forward direction (projected onto XZ plane)
        Vector3 cameraForward = vrCamera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();
        
        // Get the target forward direction (projected onto XZ plane)
        Vector3 targetDir = targetForward;
        targetDir.y = 0f;
        targetDir.Normalize();
        
        // Calculate the angle difference between current and target forward
        float angleDifference = Vector3.SignedAngle(cameraForward, targetDir, Vector3.up);
        
        // Rotate the XR Origin to compensate
        xrOrigin.transform.Rotate(0f, angleDifference, 0f, Space.World);
        
        if (debugMode)
        {
            Debug.Log($"VRRecenter: Recentered view by {angleDifference:F1} degrees");
        }
    }
    
    /// <summary>
    /// Sets the target forward direction for recentering.
    /// </summary>
    /// <param name="forward">The world-space forward direction to face after recentering</param>
    public void SetTargetForward(Vector3 forward)
    {
        targetForward = forward;
    }
    
    /// <summary>
    /// Recenters the view to face a specific world position.
    /// </summary>
    /// <param name="targetPosition">The world position to face</param>
    public void RecenterToFace(Vector3 targetPosition)
    {
        if (vrCamera == null) return;
        
        Vector3 directionToTarget = targetPosition - vrCamera.transform.position;
        directionToTarget.y = 0f;
        
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            targetForward = directionToTarget.normalized;
            RecenterView();
        }
    }
}
