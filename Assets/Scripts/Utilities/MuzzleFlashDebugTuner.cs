using UnityEngine;

/// <summary>
/// Debug utility to tune muzzle flash scale at runtime.
/// X button = decrease, Y button = increase on left controller.
/// </summary>
public class MuzzleFlashDebugTuner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float scaleStep = 0.05f;
    [SerializeField] private float minScale = 0.01f;
    [SerializeField] private float maxScale = 2.0f;
    
    private float currentScale = 0.5f;
    private GunSelectionManager gunSelectionManager;
    private UnityEngine.XR.InputDevice leftController;
    private bool wasXPressed = false;
    private bool wasYPressed = false;
    private VRDebugOverlay debugOverlay;
    
    private void Start()
    {
        gunSelectionManager = FindObjectOfType<GunSelectionManager>();
        if (gunSelectionManager != null && gunSelectionManager.CurrentGun != null)
        {
            currentScale = gunSelectionManager.CurrentGun.muzzleFlashScale;
        }
        InitializeController();
        
        debugOverlay = VRDebugOverlay.Create("MuzzleFlashDebug");
        UpdateDisplay();
    }
    
    private void InitializeController()
    {
        var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, devices);
        if (devices.Count > 0) leftController = devices[0];
    }

    private void Update()
    {
        if (!leftController.isValid) InitializeController();
        
        CheckInput();
        UpdateOverlayPosition();
    }
    
    private void CheckInput()
    {
        if (!leftController.isValid) return;
        
        bool xPressed, yPressed;
        
        if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out xPressed))
        {
            if (xPressed && !wasXPressed) AdjustScale(-scaleStep);
            wasXPressed = xPressed;
        }
        
        if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out yPressed))
        {
            if (yPressed && !wasYPressed) AdjustScale(scaleStep);
            wasYPressed = yPressed;
        }
    }
    
    private void AdjustScale(float delta)
    {
        currentScale = Mathf.Clamp(currentScale + delta, minScale, maxScale);
        
        if (gunSelectionManager != null && gunSelectionManager.CurrentGun != null)
            gunSelectionManager.CurrentGun.muzzleFlashScale = currentScale;
        
        UpdateDisplay();
        Debug.Log($"Muzzle Flash Scale: {currentScale:F2}");
    }
    
    private void UpdateDisplay()
    {
        if (debugOverlay == null) return;
        
        string gunName = gunSelectionManager?.CurrentGun?.gunName ?? "Unknown";
        debugOverlay.SetText($"{gunName}\nScale: {currentScale:F2}\n(X- Y+)");
    }
    
    private void UpdateOverlayPosition()
    {
        if (debugOverlay == null || gunSelectionManager == null) return;
        
        Transform target = gunSelectionManager.CurrentGun?.muzzlePoint ?? 
                          gunSelectionManager.CurrentGunInstance?.transform;
        if (target != null)
            debugOverlay.SetFollowTarget(target, new Vector3(0, 0.15f, 0));
    }
    
    private void OnDestroy()
    {
        if (debugOverlay != null) Destroy(debugOverlay.gameObject);
    }
}
