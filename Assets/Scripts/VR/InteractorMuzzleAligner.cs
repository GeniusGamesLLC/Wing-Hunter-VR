using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

/// <summary>
/// Aligns the XR interactor's ray origin with the gun's muzzle point.
/// This ensures the UI interaction ray matches the shooting ray direction.
/// The key component is the CurveInteractionCaster which handles far interactions.
/// </summary>
public class InteractorMuzzleAligner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GunSelectionManager gunSelectionManager;
    [SerializeField] private NearFarInteractor nearFarInteractor;
    
    [Header("Debug")]
    [SerializeField] private bool debugLogging = true;
    
    // Cached references
    private CurveInteractionCaster farCaster;
    private XRInteractorLineVisual lineVisual;
    private CurveVisualController curveVisualController;
    private Transform originalCastOrigin;
    private Transform originalLineOrigin;
    private bool isInitialized = false;
    
    private void Awake()
    {
        InitializeReferences();
    }
    
    private void Start()
    {
        // Try again in Start if Awake didn't find everything
        if (!isInitialized)
        {
            InitializeReferences();
        }
    }
    
    private void InitializeReferences()
    {
        // Auto-find GunSelectionManager if not assigned
        if (gunSelectionManager == null)
        {
            gunSelectionManager = FindObjectOfType<GunSelectionManager>();
        }
        
        // Auto-find NearFarInteractor if not assigned
        if (nearFarInteractor == null)
        {
            // Look for it on the right controller
            var rightController = GameObject.Find("Right Controller");
            if (rightController != null)
            {
                nearFarInteractor = rightController.GetComponentInChildren<NearFarInteractor>();
            }
            
            // Also try finding by name
            if (nearFarInteractor == null)
            {
                var nearFarGO = GameObject.Find("Near-Far Interactor");
                if (nearFarGO != null)
                {
                    nearFarInteractor = nearFarGO.GetComponent<NearFarInteractor>();
                }
            }
        }
        
        if (nearFarInteractor != null)
        {
            // Get the far caster component (CurveInteractionCaster handles far/ray interactions)
            farCaster = nearFarInteractor.GetComponent<CurveInteractionCaster>();
            if (farCaster == null)
            {
                farCaster = nearFarInteractor.GetComponentInChildren<CurveInteractionCaster>();
            }
            
            // Get the line visual component (may be XRInteractorLineVisual or CurveVisualController)
            lineVisual = nearFarInteractor.GetComponentInChildren<XRInteractorLineVisual>();
            curveVisualController = nearFarInteractor.GetComponentInChildren<CurveVisualController>();
            
            // Store original origins for restoration
            if (farCaster != null)
            {
                originalCastOrigin = farCaster.castOrigin;
            }
            
            if (lineVisual != null)
            {
                originalLineOrigin = lineVisual.lineOriginTransform;
            }
            
            isInitialized = farCaster != null;
            
            if (debugLogging)
            {
                Debug.Log($"InteractorMuzzleAligner: Initialized - NearFarInteractor: {nearFarInteractor != null}, " +
                          $"FarCaster: {farCaster != null}, LineVisual: {lineVisual != null}, " +
                          $"CurveVisualController: {curveVisualController != null}");
            }
        }
        else
        {
            Debug.LogWarning("InteractorMuzzleAligner: Could not find NearFarInteractor");
        }
    }
    
    private void OnEnable()
    {
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunChanged.AddListener(OnGunChanged);
            
            // Apply current gun if already selected
            if (gunSelectionManager.CurrentGun != null)
            {
                OnGunChanged(gunSelectionManager.CurrentGun);
            }
        }
    }
    
    private void OnDisable()
    {
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunChanged.RemoveListener(OnGunChanged);
        }
        
        // Restore original origins
        RestoreOriginalOrigins();
    }
    
    private void OnGunChanged(GunData gunData)
    {
        if (gunData == null || gunData.muzzlePoint == null)
        {
            if (debugLogging)
            {
                Debug.Log("InteractorMuzzleAligner: Gun or muzzle point is null, restoring original origins");
            }
            RestoreOriginalOrigins();
            return;
        }
        
        Transform muzzlePoint = gunData.muzzlePoint;
        
        // Update far caster origin - this is the key component that determines where raycasts originate
        if (farCaster != null)
        {
            farCaster.castOrigin = muzzlePoint;
            if (debugLogging)
            {
                Debug.Log($"InteractorMuzzleAligner: Set far caster origin to {muzzlePoint.name} at position {muzzlePoint.position}");
            }
        }
        
        // Update line visual origin if using XRInteractorLineVisual
        if (lineVisual != null)
        {
            lineVisual.lineOriginTransform = muzzlePoint;
            if (debugLogging)
            {
                Debug.Log($"InteractorMuzzleAligner: Set line visual origin to {muzzlePoint.name}");
            }
        }
        
        // Note: CurveVisualController gets its origin from the CurveInteractionCaster automatically,
        // so we don't need to set it separately
    }
    
    private void RestoreOriginalOrigins()
    {
        if (farCaster != null && originalCastOrigin != null)
        {
            farCaster.castOrigin = originalCastOrigin;
        }
        
        if (lineVisual != null && originalLineOrigin != null)
        {
            lineVisual.lineOriginTransform = originalLineOrigin;
        }
    }
    
    /// <summary>
    /// Manually set the muzzle point for the interactor ray origin.
    /// Call this if you need to update the muzzle point outside of gun changes.
    /// </summary>
    public void SetMuzzlePoint(Transform muzzlePoint)
    {
        if (muzzlePoint == null)
        {
            RestoreOriginalOrigins();
            return;
        }
        
        if (farCaster != null)
        {
            farCaster.castOrigin = muzzlePoint;
        }
        
        if (lineVisual != null)
        {
            lineVisual.lineOriginTransform = muzzlePoint;
        }
    }
}
