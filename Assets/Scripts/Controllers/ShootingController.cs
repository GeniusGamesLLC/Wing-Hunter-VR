using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
    [SerializeField] private bool useDirectXRInput = true;
    
    [Header("Haptic Feedback")]
    [SerializeField] private float hapticIntensity = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool allowGunSwitching = true;
    
    // Private fields
    private bool triggerPressed = false;
    private bool wasAButtonPressed = false;
    private GunData currentGunData;
    private UnityEngine.XR.InputDevice rightController;
    private bool wasRightTriggerPressed = false;
    
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (xrController == null)
            xrController = GetComponent<ActionBasedController>();
        
        if (rayOrigin == null)
            rayOrigin = transform;
        
        if (gunSelectionManager == null)
            gunSelectionManager = FindObjectOfType<GunSelectionManager>();
        
        InitializeXRDevice();
    }
    
    private void InitializeXRDevice()
    {
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
        if (rightHandDevices.Count > 0)
        {
            rightController = rightHandDevices[0];
            Debug.Log($"ShootingController: Found right controller: {rightController.name}");
        }
        else
        {
            Debug.LogWarning("ShootingController: No right hand controller found, will retry...");
        }
    }
    
    private void Start()
    {
        if (muzzleFlash == null)
            muzzleFlash = GetComponentInChildren<ParticleSystem>();
        
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunChanged.AddListener(OnGunChanged);
            if (gunSelectionManager.CurrentGun != null)
                OnGunChanged(gunSelectionManager.CurrentGun);
        }
    }
    
    private void OnEnable()
    {
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
        if (gunSelectionManager != null)
            gunSelectionManager.OnGunChanged.RemoveListener(OnGunChanged);
    }
    
    private void Update()
    {
        if (useDirectXRInput)
            CheckDirectXRTrigger();
    }
    
    private void CheckDirectXRTrigger()
    {
        if (!rightController.isValid)
        {
            var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
            if (rightHandDevices.Count > 0)
            {
                rightController = rightHandDevices[0];
                Debug.Log($"ShootingController: Found right controller: {rightController.name}");
            }
            return;
        }
        
        // Check trigger for shooting
        float triggerValue = 0f;
        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue))
        {
            bool isTriggerPressed = triggerValue > 0.5f;
            
            if (isTriggerPressed && !wasRightTriggerPressed && !triggerPressed)
            {
                triggerPressed = true;
                PerformShot();
                Invoke(nameof(ResetTrigger), 0.1f);
            }
            
            wasRightTriggerPressed = isTriggerPressed;
        }
        
        // Check B button for gun switching
        if (allowGunSwitching && gunSelectionManager != null)
        {
            bool isBButtonPressed = false;
            if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out isBButtonPressed))
            {
                if (isBButtonPressed && !wasAButtonPressed)
                {
                    gunSelectionManager.SelectNextGun();
                }
                wasAButtonPressed = isBButtonPressed;
            }
        }
    }
    
    private void OnGunChanged(GunData newGunData)
    {
        currentGunData = newGunData;
        
        if (currentGunData != null)
        {
            if (currentGunData.muzzlePoint != null)
                rayOrigin = currentGunData.muzzlePoint;
            
            hapticIntensity = currentGunData.hapticIntensity;
            Debug.Log($"ShootingController updated for gun: {currentGunData.gunName}");
        }
    }
    
    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (!triggerPressed)
        {
            triggerPressed = true;
            PerformShot();
            Invoke(nameof(ResetTrigger), 0.1f);
        }
    }
    
    private void ResetTrigger()
    {
        triggerPressed = false;
    }
    
    private void PerformShot()
    {
        PlayMuzzleFlash();
        TriggerHapticFeedback();
        bool hitDetected = PerformRaycast();
        PlayShotAudio(hitDetected);
    }
    
    private bool PerformRaycast()
    {
        Vector3 rayStart = rayOrigin.position;
        Vector3 rayDirection = rayOrigin.forward;
        
        RaycastHit hit;
        if (Physics.Raycast(rayStart, rayDirection, out hit, rayDistance, targetLayer))
        {
            DuckController duck = hit.collider.GetComponent<DuckController>();
            if (duck != null)
            {
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

    
    private void PlayMuzzleFlash()
    {
        if (currentGunData != null && currentGunData.muzzleFlashPrefab != null && currentGunData.muzzlePoint != null)
        {
            GameObject flash = Instantiate(currentGunData.muzzleFlashPrefab, currentGunData.muzzlePoint);
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localRotation = Quaternion.identity;
            if (currentGunData.muzzleFlashScale != 1.0f)
                flash.transform.localScale = Vector3.one * currentGunData.muzzleFlashScale;
            Destroy(flash, 0.5f);
        }
        else if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
    
    private void CreateSimpleMuzzleFlash()
    {
        GameObject flashGO = new GameObject("MuzzleFlash");
        flashGO.transform.position = rayOrigin.position;
        flashGO.transform.rotation = rayOrigin.rotation;
        
        ParticleSystem particles = flashGO.AddComponent<ParticleSystem>();
        
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = true;
        main.loop = false;
        main.maxParticles = 50;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startLifetime = 0.1f;
        main.startColor = new Color(1f, 0.8f, 0.2f, 1f);
        
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 50) });
        
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.1f;
        shape.length = 0.5f;
        
        AutoDestroyParticleSystem autoDestroy = flashGO.AddComponent<AutoDestroyParticleSystem>();
        autoDestroy.destroyDelay = 1f;
        
        particles.Play();
    }
    
    private void TriggerHapticFeedback()
    {
        if (xrController != null)
            xrController.SendHapticImpulse(hapticIntensity, hapticDuration);
        
        if (rightController.isValid)
        {
            UnityEngine.XR.HapticCapabilities capabilities;
            if (rightController.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
                rightController.SendHapticImpulse(0, hapticIntensity, hapticDuration);
        }
    }
    
    private void PlayShotAudio(bool wasHit)
    {
        if (audioSource != null)
        {
            AudioClip clipToPlay = null;
            
            if (currentGunData != null && currentGunData.fireSound != null)
                clipToPlay = currentGunData.fireSound;
            else
                clipToPlay = wasHit ? hitSound : missSound;
            
            if (clipToPlay != null)
                audioSource.PlayOneShot(clipToPlay);
        }
    }
    
    public void SetRayOrigin(Transform origin) { rayOrigin = origin; }
    public void SetRayDistance(float distance) { rayDistance = distance; }
    public void SetTargetLayer(LayerMask layerMask) { targetLayer = layerMask; }
    public void SetXRController(ActionBasedController controller) { xrController = controller; }
    
    public void SetTriggerAction(InputActionReference action)
    {
        if (triggerAction != null)
            triggerAction.action.performed -= OnTriggerPressed;
        
        triggerAction = action;
        
        if (triggerAction != null && enabled)
        {
            triggerAction.action.performed += OnTriggerPressed;
            triggerAction.action.Enable();
        }
    }
    
    public void SetHapticFeedback(float intensity, float duration)
    {
        hapticIntensity = Mathf.Clamp01(intensity);
        hapticDuration = Mathf.Max(0f, duration);
    }
    
    public void SetAudioClips(AudioClip hit, AudioClip miss)
    {
        hitSound = hit;
        missSound = miss;
    }
    
    public void SetMuzzleFlash(ParticleSystem particles) { muzzleFlash = particles; }
    
    public void SetGunSelectionManager(GunSelectionManager manager)
    {
        if (gunSelectionManager != null && gunSelectionManager.OnGunChanged != null)
            gunSelectionManager.OnGunChanged.RemoveListener(OnGunChanged);
        
        gunSelectionManager = manager;
        
        if (gunSelectionManager != null)
        {
            if (gunSelectionManager.OnGunChanged != null)
                gunSelectionManager.OnGunChanged.AddListener(OnGunChanged);
            
            if (gunSelectionManager.CurrentGun != null)
                OnGunChanged(gunSelectionManager.CurrentGun);
        }
    }
    
    public GunData GetCurrentGunData() { return currentGunData; }
    public bool IsGunSelectionAvailable() { return gunSelectionManager != null && gunSelectionManager.GunCollection != null; }
    
    private void OnDrawGizmosSelected()
    {
        if (rayOrigin != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * rayDistance);
        }
    }
}
