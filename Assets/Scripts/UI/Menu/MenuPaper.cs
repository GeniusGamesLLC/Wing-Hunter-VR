using UnityEngine;

/// <summary>
/// Abstract base class for all menu papers on the Announcement Board.
/// Each paper represents a distinct menu section (Settings, Debug, etc.).
/// Supports compact/expanded states for focus/unfocus behavior.
/// </summary>
public abstract class MenuPaper : MonoBehaviour
{
    [Header("Paper Configuration")]
    [SerializeField] protected string paperTitle;
    [SerializeField] protected Transform contentRoot;
    [SerializeField] protected bool isUnlockedByDefault = true;
    
    [Header("Focus/Unfocus State")]
    [Tooltip("Scale when paper is unfocused (normal state - papers stay readable)")]
    [SerializeField] protected Vector3 unfocusedScale = Vector3.one;
    [Tooltip("Scale when paper is focused (slightly larger for emphasis)")]
    [SerializeField] protected Vector3 focusedScale = new Vector3(1.1f, 1.1f, 1f);
    [Tooltip("Z offset when focused (negative moves toward user)")]
    [SerializeField] protected float focusedZOffset = -0.1f;
    [Tooltip("Transform containing placeholder content shown when unfocused")]
    [SerializeField] protected Transform placeholderContent;
    
    [Header("Paper-to-Player Movement")]
    [Tooltip("Whether to move the paper to the player when focused")]
    [SerializeField] protected bool moveToPlayerOnFocus = true;
    [Tooltip("Distance in front of player camera when focused (meters). Paper will always be at least this close.")]
    [SerializeField] protected float focusDistanceFromPlayer = 0.6f;
    [Tooltip("Height offset from player eye level (meters)")]
    [SerializeField] protected float focusHeightOffset = -0.15f;
    
    [Header("Animation")]
    [SerializeField] protected PaperAnimator paperAnimator;
    
    /// <summary>
    /// Cached pin transforms to hide when focused.
    /// </summary>
    private Transform[] pinTransforms;
    
    /// <summary>
    /// Reference to the player camera for paper-to-player positioning.
    /// </summary>
    private Transform playerCamera;

    /// <summary>
    /// The display title of this paper.
    /// </summary>
    public string Title => paperTitle;

    /// <summary>
    /// Whether this paper is currently unlocked and visible on the board.
    /// </summary>
    public bool IsUnlocked { get; protected set; }
    
    /// <summary>
    /// Whether this paper is currently focused (expanded state).
    /// </summary>
    public bool IsFocused { get; protected set; }

    /// <summary>
    /// The root transform where content UI elements are placed.
    /// </summary>
    public Transform ContentRoot => contentRoot;
    
    /// <summary>
    /// The placeholder content transform (shown when compact).
    /// </summary>
    public Transform PlaceholderContent => placeholderContent;
    
    /// <summary>
    /// The paper animator component for focus/unfocus animations.
    /// </summary>
    public PaperAnimator Animator => paperAnimator;
    
    /// <summary>
    /// The unfocused scale (normal readable size).
    /// </summary>
    public Vector3 UnfocusedScale => unfocusedScale;
    
    /// <summary>
    /// The focused scale (slightly larger for emphasis).
    /// </summary>
    public Vector3 FocusedScale => focusedScale;
    
    /// <summary>
    /// The Z offset when focused (negative moves toward user).
    /// </summary>
    public float FocusedZOffset => focusedZOffset;
    
    /// <summary>
    /// Whether to move the paper to the player when focused.
    /// </summary>
    public bool MoveToPlayerOnFocus => moveToPlayerOnFocus;
    
    /// <summary>
    /// Distance from player camera when focused.
    /// </summary>
    public float FocusDistanceFromPlayer => focusDistanceFromPlayer;
    
    /// <summary>
    /// Height offset from player eye level when focused.
    /// </summary>
    public float FocusHeightOffset => focusHeightOffset;
    
    /// <summary>
    /// Whether Initialize has been called.
    /// </summary>
    protected bool isInitialized;

    /// <summary>
    /// Self-initialize if not already initialized by PaperManager.
    /// </summary>
    protected virtual void Start()
    {
        if (!isInitialized)
        {
            Debug.Log($"[MenuPaper] Self-initializing {paperTitle} (not initialized by PaperManager)");
            Initialize();
        }
    }

    /// <summary>
    /// Called when the paper is first created. Sets initial unlock state.
    /// </summary>
    public virtual void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;
        
        IsUnlocked = isUnlockedByDefault;
        
        // Try to get animator if not assigned
        if (paperAnimator == null)
        {
            paperAnimator = GetComponent<PaperAnimator>();
        }
        
        // Set up VR interaction on the paper background (child with collider)
        SetupInteraction();
        
        // Cache pin transforms for hiding when focused
        CachePinTransforms();
        
        // Find player camera for paper-to-player movement
        FindPlayerCamera();
        
        // Set initial unfocused state (normal size)
        SetUnfocusedStateImmediate();
        
        Debug.Log($"[MenuPaper] Initialized {paperTitle}, IsUnlocked={IsUnlocked}");
    }
    
    /// <summary>
    /// Sets up VR interaction for this paper.
    /// </summary>
    protected virtual void SetupInteraction()
    {
        // Find the child with a collider (typically PaperBackground)
        var collider = GetComponentInChildren<Collider>();
        if (collider != null)
        {
            var interaction = collider.GetComponent<PaperInteraction>();
            if (interaction == null)
            {
                interaction = collider.gameObject.AddComponent<PaperInteraction>();
            }
            interaction.SetMenuPaper(this);
            
            // Also set the PaperManager
            var paperManager = GetComponentInParent<PaperManager>();
            if (paperManager == null)
            {
                paperManager = FindObjectOfType<PaperManager>();
            }
            if (paperManager != null)
            {
                interaction.SetPaperManager(paperManager);
            }
            
            Debug.Log($"[MenuPaper] SetupInteraction for {paperTitle}: Collider={collider.name}, PaperManager={(paperManager != null ? "found" : "null")}");
        }
        else
        {
            Debug.LogWarning($"[MenuPaper] No collider found for {paperTitle}");
        }
    }

    /// <summary>
    /// Called when this paper becomes the focused/selected paper.
    /// Moves paper to player position, scales up, shows full content.
    /// Override to implement additional focus behavior.
    /// Requirements: 1.3, 10.2 - Paper moves to player on focus
    /// </summary>
    public virtual void OnFocus()
    {
        IsFocused = true;
        
        // Hide pins when focused (they stay on the board)
        SetPinsVisible(false);
        
        // Play focus animation if animator exists
        if (paperAnimator != null)
        {
            if (moveToPlayerOnFocus && playerCamera != null)
            {
                // Store home position before moving
                paperAnimator.StoreHomePosition();
                
                // Calculate target position in front of player
                Vector3 targetPosition = CalculatePlayerFocusPosition();
                Quaternion targetRotation = CalculatePlayerFocusRotation();
                
                // Animate paper to player
                paperAnimator.PlayMoveToPlayerAnimation(targetPosition, targetRotation, focusedScale);
            }
            else
            {
                // Fall back to legacy focus animation (just scale/offset)
                paperAnimator.PlayFocusAnimation(focusedScale, focusedZOffset);
            }
        }
        else
        {
            // Apply focused state immediately without animation
            SetFocusedStateImmediate();
        }
        
        // Show full content, hide placeholder
        SetContentVisibility(showFullContent: true);
    }

    /// <summary>
    /// Called when this paper loses focus to another paper.
    /// Returns paper to board position, normal size, shows placeholder.
    /// Override to implement additional unfocus behavior.
    /// Requirements: 10.4 - Paper returns to board on unfocus
    /// </summary>
    public virtual void OnUnfocus()
    {
        IsFocused = false;
        
        // Show pins when unfocused (paper is back on the board)
        SetPinsVisible(true);
        
        // Play unfocus animation if animator exists
        if (paperAnimator != null)
        {
            if (moveToPlayerOnFocus && paperAnimator.HasStoredHomePosition)
            {
                // Animate paper back to board
                paperAnimator.PlayReturnToBoardAnimation(unfocusedScale);
            }
            else
            {
                // Fall back to legacy unfocus animation
                paperAnimator.PlayUnfocusAnimation(unfocusedScale);
            }
        }
        else
        {
            // Apply unfocused state immediately without animation
            SetUnfocusedStateImmediate();
        }
        
        // Show placeholder, hide full content
        SetContentVisibility(showFullContent: false);
    }
    
    /// <summary>
    /// Sets the focused state immediately without animation.
    /// </summary>
    protected virtual void SetFocusedStateImmediate()
    {
        transform.localScale = focusedScale;
        Vector3 pos = transform.localPosition;
        pos.z = focusedZOffset;
        transform.localPosition = pos;
    }
    
    /// <summary>
    /// Sets the unfocused state immediately without animation.
    /// Papers stay at normal readable size (1.0 scale).
    /// </summary>
    protected virtual void SetUnfocusedStateImmediate()
    {
        transform.localScale = unfocusedScale;
        Vector3 pos = transform.localPosition;
        // Match the label tab Z offset (-0.005) so paper sits at same depth as label
        pos.z = -0.005f;
        transform.localPosition = pos;
    }
    
    /// <summary>
    /// Sets the visibility of full content vs placeholder content.
    /// </summary>
    /// <param name="showFullContent">True to show full content, false to show placeholder.</param>
    protected virtual void SetContentVisibility(bool showFullContent)
    {
        // Show/hide full content
        if (contentRoot != null)
        {
            contentRoot.gameObject.SetActive(showFullContent);
        }
        
        // Show/hide placeholder (inverse of full content)
        if (placeholderContent != null)
        {
            placeholderContent.gameObject.SetActive(!showFullContent);
        }
    }
    
    /// <summary>
    /// Initializes the paper to unfocused state (normal size).
    /// Call this after Initialize() to set the initial visual state.
    /// </summary>
    public virtual void SetInitialUnfocusedState()
    {
        IsFocused = false;
        SetUnfocusedStateImmediate();
        SetContentVisibility(showFullContent: false);
    }

    /// <summary>
    /// Refreshes the content displayed on this paper.
    /// Must be implemented by derived classes.
    /// </summary>
    public abstract void RefreshContent();

    /// <summary>
    /// Unlocks this paper, making it visible on the board.
    /// </summary>
    public void Unlock()
    {
        IsUnlocked = true;
    }
    
    /// <summary>
    /// Caches pin transforms for showing/hiding during focus.
    /// </summary>
    private void CachePinTransforms()
    {
        var pins = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Pin"))
            {
                pins.Add(child);
            }
        }
        pinTransforms = pins.ToArray();
    }
    
    /// <summary>
    /// Shows or hides the pins on this paper.
    /// </summary>
    private void SetPinsVisible(bool visible)
    {
        if (pinTransforms == null) return;
        
        foreach (var pin in pinTransforms)
        {
            if (pin != null)
            {
                pin.gameObject.SetActive(visible);
            }
        }
    }
    
    /// <summary>
    /// Finds the player camera for paper-to-player positioning.
    /// </summary>
    protected virtual void FindPlayerCamera()
    {
        // Try to find the main camera (typically the VR headset camera)
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
        else
        {
            // Fallback: search for XR Origin camera
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null && xrOrigin.Camera != null)
            {
                playerCamera = xrOrigin.Camera.transform;
            }
        }
        
        if (playerCamera == null)
        {
            Debug.LogWarning($"[MenuPaper] Could not find player camera for {paperTitle}");
        }
    }
    
    /// <summary>
    /// Calculates the world position in front of the player for focused paper.
    /// The paper is always positioned relative to the player's head position,
    /// ensuring it never ends up behind world geometry regardless of where the player stands.
    /// Requirements: 1.3 - Paper appears in front of player at eye level
    /// </summary>
    protected virtual Vector3 CalculatePlayerFocusPosition()
    {
        if (playerCamera == null) return transform.position;
        
        // Get camera forward direction (horizontal only, ignore pitch)
        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        // If forward is zero (looking straight up/down), use a default forward
        if (cameraForward.sqrMagnitude < 0.001f)
        {
            cameraForward = Vector3.forward;
        }
        
        // Position in front of player at specified distance - always relative to player head
        // This ensures the paper is ALWAYS in front of the player, never behind world geometry
        Vector3 targetPosition = playerCamera.position + cameraForward * focusDistanceFromPlayer;
        
        // Adjust height to eye level plus offset (slightly below eye level for comfortable reading)
        targetPosition.y = playerCamera.position.y + focusHeightOffset;
        
        Debug.Log($"[MenuPaper] CalculatePlayerFocusPosition: distance={focusDistanceFromPlayer}, heightOffset={focusHeightOffset}, target={targetPosition}");
        
        return targetPosition;
    }
    
    /// <summary>
    /// Calculates the world rotation for focused paper (facing the player).
    /// Uses the same approach as BillboardToCamera - direction from camera to paper.
    /// </summary>
    protected virtual Quaternion CalculatePlayerFocusRotation()
    {
        if (playerCamera == null) return transform.rotation;
        
        // Calculate target position first (same as CalculatePlayerFocusPosition)
        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        if (cameraForward.sqrMagnitude < 0.001f)
        {
            cameraForward = Vector3.forward;
        }
        
        Vector3 targetPosition = playerCamera.position + cameraForward * focusDistanceFromPlayer;
        targetPosition.y = playerCamera.position.y + focusHeightOffset;
        
        // Direction FROM camera TO paper (same as BillboardToCamera)
        // This makes the paper's +Z point away from the camera
        // which means the front of the paper (-Z or content side) faces the camera
        Vector3 directionFromCamera = targetPosition - playerCamera.position;
        directionFromCamera.y = 0; // Keep paper upright
        directionFromCamera.Normalize();
        
        Quaternion rotation = Quaternion.LookRotation(directionFromCamera, Vector3.up);
        
        Debug.Log($"[MenuPaper] CalculatePlayerFocusRotation: directionFromCamera={directionFromCamera}, rotation={rotation.eulerAngles}");
        
        return rotation;
    }
    
    /// <summary>
    /// Sets the player camera reference manually.
    /// </summary>
    public void SetPlayerCamera(Transform camera)
    {
        playerCamera = camera;
    }
    
    /// <summary>
    /// Gets the current player camera reference.
    /// </summary>
    public Transform PlayerCamera => playerCamera;
}
