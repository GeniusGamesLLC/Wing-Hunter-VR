using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private LayerMask targetLayer = -1;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip missSound;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    
    [Header("XR Controller")]
    [SerializeField] private ActionBasedController xrController;
    [SerializeField] private InputActionReference triggerAction;
    
    [Header("Haptic Feedback")]
    [SerializeField] private float hapticIntensity = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;
    
    // Private fields
    private bool triggerPressed = false;
    
    private void Awake()
    {
        // Get components if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (xrController == null)
        {
            xrController = GetComponent<ActionBasedController>();
        }
        
        if (rayOrigin == null)
        {
            rayOrigin = transform;
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to trigger input
        if (triggerAction != null)
        {
            triggerAction.action.performed += OnTriggerPressed;
            triggerAction.action.Enable();
        }
        else if (xrController != null && xrController.activateAction != null)
        {
            xrController.activateAction.action.performed += OnTriggerPressed;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from trigger input
        if (triggerAction != null)
        {
            triggerAction.action.performed -= OnTriggerPressed;
            triggerAction.action.Disable();
        }
        else if (xrController != null && xrController.activateAction != null)
        {
            xrController.activateAction.action.performed -= OnTriggerPressed;
        }
    }
    
    /// <summary>
    /// Called when the trigger is pressed
    /// </summary>
    /// <param name="context">Input action context</param>
    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (!triggerPressed)
        {
            triggerPressed = true;
            PerformShot();
            
            // Reset trigger state after a short delay to prevent rapid firing
            Invoke(nameof(ResetTrigger), 0.1f);
        }
    }
    
    /// <summary>
    /// Reset the trigger pressed state
    /// </summary>
    private void ResetTrigger()
    {
        triggerPressed = false;
    }
    
    /// <summary>
    /// Perform the shooting action with raycast
    /// </summary>
    private void PerformShot()
    {
        // Play muzzle flash effect
        PlayMuzzleFlash();
        
        // Trigger haptic feedback
        TriggerHapticFeedback();
        
        // Perform raycast
        bool hitDetected = PerformRaycast();
        
        // Play appropriate audio
        PlayShotAudio(hitDetected);
    }
    
    /// <summary>
    /// Perform raycast from controller position in forward direction
    /// </summary>
    /// <returns>True if a duck was hit, false otherwise</returns>
    private bool PerformRaycast()
    {
        Vector3 rayStart = rayOrigin.position;
        Vector3 rayDirection = rayOrigin.forward;
        
        // Perform the raycast
        RaycastHit hit;
        if (Physics.Raycast(rayStart, rayDirection, out hit, rayDistance, targetLayer))
        {
            // Check if we hit a duck
            DuckController duck = hit.collider.GetComponent<DuckController>();
            if (duck != null)
            {
                // Call OnHit on the struck duck
                duck.OnHit();
                
                Debug.Log($"Hit duck at position: {hit.point}");
                return true;
            }
            else
            {
                Debug.Log($"Hit object: {hit.collider.name} but it's not a duck");
            }
        }
        else
        {
            Debug.Log("Shot missed - no hit detected");
        }
        
        return false;
    }
    
    /// <summary>
    /// Play muzzle flash particle effect
    /// </summary>
    private void PlayMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
    
    /// <summary>
    /// Trigger haptic feedback on the controller
    /// </summary>
    private void TriggerHapticFeedback()
    {
        if (xrController != null)
        {
            xrController.SendHapticImpulse(hapticIntensity, hapticDuration);
        }
    }
    
    /// <summary>
    /// Play appropriate audio for hit or miss
    /// </summary>
    /// <param name="wasHit">True if shot hit a target, false if missed</param>
    private void PlayShotAudio(bool wasHit)
    {
        if (audioSource != null)
        {
            AudioClip clipToPlay = wasHit ? hitSound : missSound;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
    }
    
    /// <summary>
    /// Set the ray origin transform (usually the controller transform)
    /// </summary>
    /// <param name="origin">Transform to use as ray origin</param>
    public void SetRayOrigin(Transform origin)
    {
        rayOrigin = origin;
    }
    
    /// <summary>
    /// Set the raycast distance
    /// </summary>
    /// <param name="distance">Maximum raycast distance</param>
    public void SetRayDistance(float distance)
    {
        rayDistance = distance;
    }
    
    /// <summary>
    /// Set the target layer mask for duck detection
    /// </summary>
    /// <param name="layerMask">Layer mask for duck objects</param>
    public void SetTargetLayer(LayerMask layerMask)
    {
        targetLayer = layerMask;
    }
    
    /// <summary>
    /// Set the XR controller reference
    /// </summary>
    /// <param name="controller">ActionBasedController to use for input and haptics</param>
    public void SetXRController(ActionBasedController controller)
    {
        xrController = controller;
    }
    
    /// <summary>
    /// Set the trigger input action reference
    /// </summary>
    /// <param name="action">Input action reference for trigger input</param>
    public void SetTriggerAction(InputActionReference action)
    {
        // Unsubscribe from old action
        if (triggerAction != null)
        {
            triggerAction.action.performed -= OnTriggerPressed;
        }
        
        // Set new action
        triggerAction = action;
        
        // Subscribe to new action
        if (triggerAction != null && enabled)
        {
            triggerAction.action.performed += OnTriggerPressed;
            triggerAction.action.Enable();
        }
    }
    
    /// <summary>
    /// Set haptic feedback parameters
    /// </summary>
    /// <param name="intensity">Haptic intensity (0-1)</param>
    /// <param name="duration">Haptic duration in seconds</param>
    public void SetHapticFeedback(float intensity, float duration)
    {
        hapticIntensity = Mathf.Clamp01(intensity);
        hapticDuration = Mathf.Max(0f, duration);
    }
    
    /// <summary>
    /// Set audio clips for hit and miss sounds
    /// </summary>
    /// <param name="hit">Audio clip for successful hits</param>
    /// <param name="miss">Audio clip for missed shots</param>
    public void SetAudioClips(AudioClip hit, AudioClip miss)
    {
        hitSound = hit;
        missSound = miss;
    }
    
    /// <summary>
    /// Set the muzzle flash particle system
    /// </summary>
    /// <param name="particles">Particle system for muzzle flash effect</param>
    public void SetMuzzleFlash(ParticleSystem particles)
    {
        muzzleFlash = particles;
    }
    
    // Debug visualization in Scene view
    private void OnDrawGizmosSelected()
    {
        if (rayOrigin != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * rayDistance);
        }
    }
}