using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ShootingController : MonoBehaviour
{
    [Header("Gun Selection")]
    [SerializeField] private GunSelectionManager gunSelectionManager;
    
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
    private GunData currentGunData;
    
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
        
        // Find gun selection manager if not assigned
        if (gunSelectionManager == null)
        {
            gunSelectionManager = FindObjectOfType<GunSelectionManager>();
        }
    }
    
    private void Start()
    {
        // Subscribe to gun change events
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunChanged.AddListener(OnGunChanged);
            
            // Apply current gun settings if available
            if (gunSelectionManager.CurrentGun != null)
            {
                OnGunChanged(gunSelectionManager.CurrentGun);
            }
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
    
    private void OnDestroy()
    {
        // Unsubscribe from gun change events
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunChanged.RemoveListener(OnGunChanged);
        }
    }
    
    /// <summary>
    /// Called when the selected gun changes
    /// </summary>
    /// <param name="newGunData">The newly selected gun data</param>
    private void OnGunChanged(GunData newGunData)
    {
        currentGunData = newGunData;
        
        if (currentGunData != null)
        {
            // Update ray origin to gun's muzzle point if available
            if (currentGunData.muzzlePoint != null)
            {
                rayOrigin = currentGunData.muzzlePoint;
            }
            
            // Update haptic intensity from gun data
            hapticIntensity = currentGunData.hapticIntensity;
            
            // Update audio clips if specified in gun data
            if (currentGunData.fireSound != null)
            {
                // Use gun-specific fire sound, but keep hit/miss sounds as fallback
                // We'll modify the audio playing logic to use gun-specific sounds
            }
            
            Debug.Log($"ShootingController updated for gun: {currentGunData.gunName}");
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
        // Try to use gun-specific muzzle flash first
        if (currentGunData != null && currentGunData.muzzleFlashPrefab != null && currentGunData.muzzlePoint != null)
        {
            // Instantiate gun-specific muzzle flash at muzzle point
            GameObject flash = Instantiate(currentGunData.muzzleFlashPrefab, currentGunData.muzzlePoint.position, currentGunData.muzzlePoint.rotation);
            
            // Scale the effect if specified
            if (currentGunData.muzzleFlashScale != 1.0f)
            {
                flash.transform.localScale *= currentGunData.muzzleFlashScale;
            }
            
            // Auto-destroy the effect after a short time
            Destroy(flash, 2f);
        }
        // Fallback to default muzzle flash
        else if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        // Create a simple muzzle flash if none available
        else
        {
            CreateSimpleMuzzleFlash();
        }
    }
    
    /// <summary>
    /// Create a simple muzzle flash effect if no particle system is assigned
    /// </summary>
    private void CreateSimpleMuzzleFlash()
    {
        // Create a simple muzzle flash effect using a temporary particle system
        GameObject flashGO = new GameObject("MuzzleFlash");
        flashGO.transform.position = rayOrigin.position;
        flashGO.transform.rotation = rayOrigin.rotation;
        
        ParticleSystem particles = flashGO.AddComponent<ParticleSystem>();
        
        // Configure main module
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = true;
        main.loop = false;
        main.maxParticles = 50;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startLifetime = 0.1f;
        main.startColor = new Color(1f, 0.8f, 0.2f, 1f); // Orange-yellow flash
        
        // Configure emission module
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f; // No continuous emission
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 50) // Burst of 50 particles at start
        });
        
        // Configure shape module
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.1f;
        shape.length = 0.5f;
        
        // Configure velocity over lifetime
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(10f); // Forward velocity
        
        // Configure size over lifetime
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f);
        sizeCurve.AddKey(0.1f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Configure color over lifetime for fade out
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
        
        // Add auto-destroy component
        AutoDestroyParticleSystem autoDestroy = flashGO.AddComponent<AutoDestroyParticleSystem>();
        autoDestroy.destroyDelay = 1f;
        
        particles.Play();
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
            AudioClip clipToPlay = null;
            
            // Use gun-specific fire sound if available
            if (currentGunData != null && currentGunData.fireSound != null)
            {
                clipToPlay = currentGunData.fireSound;
            }
            // Otherwise use hit/miss sounds
            else
            {
                clipToPlay = wasHit ? hitSound : missSound;
            }
            
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
    
    /// <summary>
    /// Set the gun selection manager
    /// </summary>
    /// <param name="manager">Gun selection manager to use</param>
    public void SetGunSelectionManager(GunSelectionManager manager)
    {
        // Unsubscribe from old manager
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunChanged.RemoveListener(OnGunChanged);
        }
        
        // Set new manager
        gunSelectionManager = manager;
        
        // Subscribe to new manager
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunChanged.AddListener(OnGunChanged);
            
            // Apply current gun settings
            if (gunSelectionManager.CurrentGun != null)
            {
                OnGunChanged(gunSelectionManager.CurrentGun);
            }
        }
    }
    
    /// <summary>
    /// Get the current gun data being used
    /// </summary>
    /// <returns>Current gun data or null if none selected</returns>
    public GunData GetCurrentGunData()
    {
        return currentGunData;
    }
    
    /// <summary>
    /// Check if gun selection is available
    /// </summary>
    /// <returns>True if gun selection manager is available</returns>
    public bool IsGunSelectionAvailable()
    {
        return gunSelectionManager != null && gunSelectionManager.GunCollection != null;
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