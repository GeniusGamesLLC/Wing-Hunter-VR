using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

/// <summary>
/// Disables VR locomotion (movement and turning) when a paper is focused.
/// This prevents the player from moving while interacting with menu papers,
/// especially when scrolling content with the thumbstick.
/// Coordinates with paper animations to disable locomotion AFTER paper starts moving
/// and re-enable AFTER paper returns to board.
/// Requirements: 14.2 - Prevent scrolling input from moving the player
/// </summary>
public class LocomotionDisabler : MonoBehaviour
{
    [Header("Locomotion Providers (Auto-found if not set)")]
    [Tooltip("The continuous move provider to disable")]
    [SerializeField] private ContinuousMoveProvider moveProvider;
    [Tooltip("The continuous turn provider to disable")]
    [SerializeField] private ContinuousTurnProvider turnProvider;
    [Tooltip("The snap turn provider to disable")]
    [SerializeField] private SnapTurnProvider snapTurnProvider;
    
    [Header("References")]
    [Tooltip("The PaperManager to monitor for focus changes")]
    [SerializeField] private PaperManager paperManager;
    
    [Header("Animation Coordination")]
    [Tooltip("Delay before disabling locomotion (allows paper animation to start)")]
    [SerializeField] private float disableDelay = 0.05f;
    [Tooltip("Delay before re-enabling locomotion (allows paper to return to board)")]
    [SerializeField] private float enableDelay = 0.35f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    /// <summary>
    /// Whether locomotion is currently disabled.
    /// </summary>
    public bool IsLocomotionDisabled { get; private set; }
    
    // Track which providers were enabled before we disabled them
    private bool moveProviderWasEnabled;
    private bool turnProviderWasEnabled;
    private bool snapTurnProviderWasEnabled;
    
    // Track the currently focused paper's animator for animation events
    private PaperAnimator currentPaperAnimator;
    
    // Coroutine handles for delayed enable/disable
    private Coroutine disableCoroutine;
    private Coroutine enableCoroutine;
    
    private void Awake()
    {
        if (paperManager == null)
        {
            paperManager = GetComponentInParent<PaperManager>();
            if (paperManager == null)
            {
                paperManager = GetComponent<PaperManager>();
            }
        }
        
        // Auto-find locomotion providers if not assigned
        FindLocomotionProviders();
    }
    
    /// <summary>
    /// Finds locomotion providers in the scene if not already assigned.
    /// </summary>
    private void FindLocomotionProviders()
    {
        if (moveProvider == null)
        {
            // Try to find DynamicMoveProvider first (XRI Starter Assets), then fall back to ContinuousMoveProvider
            var dynamicMove = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets.DynamicMoveProvider>();
            if (dynamicMove != null)
            {
                moveProvider = dynamicMove;
                if (debugMode) Debug.Log($"[LocomotionDisabler] Found DynamicMoveProvider: {dynamicMove.name}");
            }
            else
            {
                moveProvider = FindObjectOfType<ContinuousMoveProvider>();
                if (moveProvider != null && debugMode)
                    Debug.Log($"[LocomotionDisabler] Found ContinuousMoveProvider: {moveProvider.name}");
            }
        }
        
        if (turnProvider == null)
        {
            turnProvider = FindObjectOfType<ContinuousTurnProvider>();
            if (turnProvider != null && debugMode)
                Debug.Log($"[LocomotionDisabler] Found ContinuousTurnProvider: {turnProvider.name}");
        }
        
        if (snapTurnProvider == null)
        {
            snapTurnProvider = FindObjectOfType<SnapTurnProvider>();
            if (snapTurnProvider != null && debugMode)
                Debug.Log($"[LocomotionDisabler] Found SnapTurnProvider: {snapTurnProvider.name}");
        }
        
        // Log warnings for missing providers
        if (moveProvider == null)
            Debug.LogWarning("[LocomotionDisabler] No move provider found in scene!");
        if (turnProvider == null && snapTurnProvider == null)
            Debug.LogWarning("[LocomotionDisabler] No turn provider found in scene!");
    }
    
    private void OnEnable()
    {
        if (paperManager != null)
        {
            paperManager.OnPaperFocused += OnPaperFocusChanged;
            if (debugMode) Debug.Log("[LocomotionDisabler] Subscribed to PaperManager.OnPaperFocused");
        }
        else
        {
            Debug.LogWarning("[LocomotionDisabler] PaperManager is null, cannot subscribe to focus events!");
        }
    }
    
    private void OnDisable()
    {
        if (paperManager != null)
        {
            paperManager.OnPaperFocused -= OnPaperFocusChanged;
        }
        
        // Cancel any pending coroutines
        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
            disableCoroutine = null;
        }
        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine);
            enableCoroutine = null;
        }
        
        // Unsubscribe from animator events
        UnsubscribeFromAnimator();
        
        // Re-enable locomotion when this component is disabled
        if (IsLocomotionDisabled)
        {
            EnableLocomotion();
        }
    }
    
    /// <summary>
    /// Called when paper focus changes.
    /// Coordinates with paper animations to disable/enable locomotion at the right time.
    /// </summary>
    private void OnPaperFocusChanged(MenuPaper paper)
    {
        if (debugMode) Debug.Log($"[LocomotionDisabler] OnPaperFocusChanged: {(paper != null ? paper.Title : "null")}");
        
        // Cancel any pending coroutines
        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
            disableCoroutine = null;
        }
        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine);
            enableCoroutine = null;
        }
        
        // Unsubscribe from previous paper's animator events
        UnsubscribeFromAnimator();
        
        if (paper != null)
        {
            // A paper is now focused
            // Subscribe to the paper's animator events if it has one
            SubscribeToAnimator(paper);
            
            // Disable locomotion after a short delay (allows animation to start)
            disableCoroutine = StartCoroutine(DisableLocomotionDelayed(disableDelay));
        }
        else
        {
            // No paper is focused - re-enable locomotion after paper returns to board
            enableCoroutine = StartCoroutine(EnableLocomotionDelayed(enableDelay));
        }
    }
    
    /// <summary>
    /// Subscribes to the paper animator's events for coordination.
    /// </summary>
    private void SubscribeToAnimator(MenuPaper paper)
    {
        if (paper == null) return;
        
        currentPaperAnimator = paper.Animator;
        if (currentPaperAnimator != null)
        {
            currentPaperAnimator.OnReturnToBoardComplete += OnPaperReturnedToBoard;
        }
    }
    
    /// <summary>
    /// Unsubscribes from the current paper animator's events.
    /// </summary>
    private void UnsubscribeFromAnimator()
    {
        if (currentPaperAnimator != null)
        {
            currentPaperAnimator.OnReturnToBoardComplete -= OnPaperReturnedToBoard;
            currentPaperAnimator = null;
        }
    }
    
    /// <summary>
    /// Called when the paper has finished returning to the board.
    /// </summary>
    private void OnPaperReturnedToBoard()
    {
        if (debugMode) Debug.Log("[LocomotionDisabler] Paper returned to board, enabling locomotion");
        
        // Paper is back on board, safe to enable locomotion immediately
        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine);
            enableCoroutine = null;
        }
        EnableLocomotion();
    }
    
    /// <summary>
    /// Disables locomotion after a delay.
    /// </summary>
    private System.Collections.IEnumerator DisableLocomotionDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisableLocomotion();
        disableCoroutine = null;
    }
    
    /// <summary>
    /// Enables locomotion after a delay.
    /// </summary>
    private System.Collections.IEnumerator EnableLocomotionDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableLocomotion();
        enableCoroutine = null;
    }
    
    /// <summary>
    /// Disables all locomotion providers.
    /// </summary>
    public void DisableLocomotion()
    {
        if (IsLocomotionDisabled) return;
        
        IsLocomotionDisabled = true;
        
        // Store current states and disable providers
        if (moveProvider != null)
        {
            moveProviderWasEnabled = moveProvider.enabled;
            moveProvider.enabled = false;
            if (debugMode) Debug.Log($"[LocomotionDisabler] Disabled move provider (was {moveProviderWasEnabled})");
        }
        
        if (turnProvider != null)
        {
            turnProviderWasEnabled = turnProvider.enabled;
            turnProvider.enabled = false;
            if (debugMode) Debug.Log($"[LocomotionDisabler] Disabled turn provider (was {turnProviderWasEnabled})");
        }
        
        if (snapTurnProvider != null)
        {
            snapTurnProviderWasEnabled = snapTurnProvider.enabled;
            snapTurnProvider.enabled = false;
            if (debugMode) Debug.Log($"[LocomotionDisabler] Disabled snap turn provider (was {snapTurnProviderWasEnabled})");
        }
        
        Debug.Log("[LocomotionDisabler] Locomotion DISABLED - paper focused");
    }
    
    /// <summary>
    /// Re-enables locomotion providers that were previously enabled.
    /// </summary>
    public void EnableLocomotion()
    {
        if (!IsLocomotionDisabled) return;
        
        IsLocomotionDisabled = false;
        
        // Restore previous states
        if (moveProvider != null && moveProviderWasEnabled)
        {
            moveProvider.enabled = true;
            if (debugMode) Debug.Log("[LocomotionDisabler] Re-enabled move provider");
        }
        
        if (turnProvider != null && turnProviderWasEnabled)
        {
            turnProvider.enabled = true;
            if (debugMode) Debug.Log("[LocomotionDisabler] Re-enabled turn provider");
        }
        
        if (snapTurnProvider != null && snapTurnProviderWasEnabled)
        {
            snapTurnProvider.enabled = true;
            if (debugMode) Debug.Log("[LocomotionDisabler] Re-enabled snap turn provider");
        }
        
        Debug.Log("[LocomotionDisabler] Locomotion ENABLED - paper unfocused");
    }
}
