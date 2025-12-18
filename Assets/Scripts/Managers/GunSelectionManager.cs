using UnityEngine;
using UnityEngine.Events;

public class GunSelectionManager : MonoBehaviour
{
    [Header("Gun Configuration")]
    [SerializeField] private GunCollection gunCollection;
    [SerializeField] private Transform gunAttachPoint;
    [SerializeField] private GameObject controllerVisual; // The controller model to hide when gun is equipped
    [SerializeField] private GameObject controllerLineVisual; // The ray line visual to hide when gun is equipped
    
    [Header("Current Gun State")]
    [SerializeField] private int currentGunIndex = 0;
    private GameObject currentGunInstance;
    private GunData currentGunData;
    
    [Header("Events")]
    public UnityEvent<GunData> OnGunChanged;
    public UnityEvent<int> OnGunIndexChanged;
    
    // Properties
    public GunData CurrentGun => currentGunData;
    public int CurrentGunIndex => currentGunIndex;
    public GameObject CurrentGunInstance => currentGunInstance;
    public GunCollection GunCollection => gunCollection;
    
    private void Start()
    {
        InitializeGunSelection();
    }
    
    private void InitializeGunSelection()
    {
        if (gunCollection == null)
        {
            Debug.LogError("GunSelectionManager: No gun collection assigned!");
            return;
        }
        
        if (!gunCollection.ValidateCollection())
        {
            Debug.LogError("GunSelectionManager: Gun collection validation failed!");
            return;
        }
        
        // Load saved gun preference or use default
        int savedGunIndex = PlayerPrefs.GetInt("SelectedGun", gunCollection.DefaultGunIndex);
        SelectGun(savedGunIndex);
    }
    
    /// <summary>
    /// Select a gun by index
    /// </summary>
    public void SelectGun(int gunIndex)
    {
        if (gunCollection == null || gunCollection.AvailableGuns == null)
        {
            Debug.LogError("Gun collection not available.");
            return;
        }
        
        // Validate index
        if (gunIndex < 0 || gunIndex >= gunCollection.AvailableGuns.Length)
        {
            Debug.LogWarning($"Invalid gun index: {gunIndex}. Using default gun.");
            gunIndex = gunCollection.DefaultGunIndex;
        }
        
        GunData newGunData = gunCollection.GetGun(gunIndex);
        if (newGunData == null || !newGunData.IsValid())
        {
            Debug.LogError($"Gun at index {gunIndex} is invalid.");
            return;
        }
        
        // Skip disabled guns - find next enabled gun
        if (!newGunData.isEnabled)
        {
            int nextEnabled = FindNextEnabledGunIndex(gunIndex);
            if (nextEnabled < 0)
            {
                Debug.LogError("No enabled guns available!");
                return;
            }
            gunIndex = nextEnabled;
            newGunData = gunCollection.GetGun(gunIndex);
        }
        
        // Destroy current gun if exists
        if (currentGunInstance != null)
        {
            DestroyImmediate(currentGunInstance);
        }
        
        // Instantiate new gun
        if (gunAttachPoint != null && newGunData.gunPrefab != null)
        {
            currentGunInstance = Instantiate(newGunData.gunPrefab, gunAttachPoint);
            
            // Apply transform settings from GunData
            currentGunInstance.transform.localPosition = newGunData.positionOffset;
            currentGunInstance.transform.localRotation = Quaternion.Euler(newGunData.rotationOffset);
            currentGunInstance.transform.localScale = Vector3.one * newGunData.scale;
            
            // Find muzzle point in the gun prefab
            Transform muzzlePoint = FindMuzzlePoint(currentGunInstance);
            if (muzzlePoint != null)
            {
                newGunData.muzzlePoint = muzzlePoint;
            }
            
            // Hide controller visual when gun is equipped
            if (controllerVisual != null)
            {
                controllerVisual.SetActive(false);
            }
            else
            {
                // Try to find and hide controller visual automatically
                HideControllerVisual();
            }
            
            // Hide controller line visual (ray) when gun is equipped
            if (controllerLineVisual != null)
            {
                controllerLineVisual.SetActive(false);
            }
            else
            {
                // Try to find and hide line visual automatically
                HideControllerLineVisual();
            }
        }
        
        // Update current gun data
        currentGunIndex = gunIndex;
        currentGunData = newGunData;
        
        // Save preference
        PlayerPrefs.SetInt("SelectedGun", gunIndex);
        PlayerPrefs.Save();
        
        // Notify listeners
        OnGunChanged?.Invoke(currentGunData);
        OnGunIndexChanged?.Invoke(currentGunIndex);
        
        Debug.Log($"Selected gun: {currentGunData.gunName}");
    }
    
    /// <summary>
    /// Select a gun by name
    /// </summary>
    public void SelectGun(string gunName)
    {
        int gunIndex = gunCollection.GetGunIndex(gunName);
        if (gunIndex >= 0)
        {
            SelectGun(gunIndex);
        }
        else
        {
            Debug.LogWarning($"Gun '{gunName}' not found in collection.");
        }
    }
    
    /// <summary>
    /// Select next enabled gun in the collection
    /// </summary>
    public void SelectNextGun()
    {
        int nextIndex = FindNextEnabledGunIndex(currentGunIndex);
        if (nextIndex >= 0)
        {
            SelectGun(nextIndex);
        }
    }
    
    /// <summary>
    /// Select previous enabled gun in the collection
    /// </summary>
    public void SelectPreviousGun()
    {
        int prevIndex = FindPreviousEnabledGunIndex(currentGunIndex);
        if (prevIndex >= 0)
        {
            SelectGun(prevIndex);
        }
    }
    
    /// <summary>
    /// Find the next enabled gun index after the given index
    /// </summary>
    private int FindNextEnabledGunIndex(int fromIndex)
    {
        int count = gunCollection.AvailableGuns.Length;
        for (int i = 1; i <= count; i++)
        {
            int checkIndex = (fromIndex + i) % count;
            GunData gun = gunCollection.GetGun(checkIndex);
            if (gun != null && gun.isEnabled && gun.IsValid())
            {
                return checkIndex;
            }
        }
        return -1; // No enabled guns found
    }
    
    /// <summary>
    /// Find the previous enabled gun index before the given index
    /// </summary>
    private int FindPreviousEnabledGunIndex(int fromIndex)
    {
        int count = gunCollection.AvailableGuns.Length;
        for (int i = 1; i <= count; i++)
        {
            int checkIndex = (fromIndex - i + count) % count;
            GunData gun = gunCollection.GetGun(checkIndex);
            if (gun != null && gun.isEnabled && gun.IsValid())
            {
                return checkIndex;
            }
        }
        return -1; // No enabled guns found
    }
    
    /// <summary>
    /// Find muzzle point in gun prefab (looks for common naming conventions)
    /// </summary>
    private Transform FindMuzzlePoint(GameObject gunInstance)
    {
        // Common muzzle point names
        string[] muzzleNames = { "MuzzlePoint", "Muzzle", "BarrelEnd", "FirePoint", "Barrel_End", "muzzle", "barrel" };
        
        foreach (string name in muzzleNames)
        {
            Transform found = FindChildRecursive(gunInstance.transform, name);
            if (found != null)
            {
                return found;
            }
        }
        
        // If no specific muzzle point found, create one at the gun's forward tip
        GameObject muzzlePoint = new GameObject("MuzzlePoint");
        muzzlePoint.transform.SetParent(gunInstance.transform);
        
        // Position it at the front of the gun (assuming gun points forward)
        Renderer gunRenderer = gunInstance.GetComponentInChildren<Renderer>();
        if (gunRenderer != null)
        {
            Bounds bounds = gunRenderer.bounds;
            Vector3 localBounds = gunInstance.transform.InverseTransformPoint(bounds.center + Vector3.forward * bounds.size.z * 0.5f);
            muzzlePoint.transform.localPosition = localBounds;
        }
        else
        {
            // Default position if no renderer found
            muzzlePoint.transform.localPosition = Vector3.forward * 0.5f;
        }
        
        muzzlePoint.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"Created muzzle point for {gunInstance.name} at local position: {muzzlePoint.transform.localPosition}");
        return muzzlePoint.transform;
    }
    
    /// <summary>
    /// Recursively find child by name
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(name.ToLower()))
            {
                return child;
            }
            
            Transform found = FindChildRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get gun information for UI display
    /// </summary>
    public string GetCurrentGunInfo()
    {
        if (currentGunData != null)
        {
            return $"{currentGunData.gunName}\n{currentGunData.description}";
        }
        return "No gun selected";
    }
    
    /// <summary>
    /// Check if a specific gun is currently selected
    /// </summary>
    public bool IsGunSelected(int gunIndex)
    {
        return currentGunIndex == gunIndex;
    }
    
    /// <summary>
    /// Check if a specific gun is currently selected by name
    /// </summary>
    public bool IsGunSelected(string gunName)
    {
        return currentGunData != null && currentGunData.gunName.Equals(gunName, System.StringComparison.OrdinalIgnoreCase);
    }
    
    private void OnDestroy()
    {
        if (currentGunInstance != null)
        {
            DestroyImmediate(currentGunInstance);
        }
    }
    
    /// <summary>
    /// Hide the controller visual model when a gun is equipped
    /// </summary>
    private void HideControllerVisual()
    {
        if (gunAttachPoint == null) return;
        
        // Look for common controller visual names in the parent hierarchy
        string[] visualNames = { "Controller Visual", "Right Controller Visual", "Left Controller Visual", 
                                  "ControllerVisual", "Model", "ControllerModel" };
        
        Transform parent = gunAttachPoint.parent;
        while (parent != null)
        {
            foreach (string name in visualNames)
            {
                Transform visual = parent.Find(name);
                if (visual != null)
                {
                    visual.gameObject.SetActive(false);
                    Debug.Log($"GunSelectionManager: Hidden controller visual: {visual.name}");
                    return;
                }
            }
            parent = parent.parent;
        }
        
        // Also check siblings of the attach point
        if (gunAttachPoint.parent != null)
        {
            foreach (Transform sibling in gunAttachPoint.parent)
            {
                foreach (string name in visualNames)
                {
                    if (sibling.name.Contains(name) || sibling.name.Contains("Visual"))
                    {
                        sibling.gameObject.SetActive(false);
                        Debug.Log($"GunSelectionManager: Hidden controller visual: {sibling.name}");
                        return;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Hide the controller line visual (ray) when a gun is equipped
    /// </summary>
    private void HideControllerLineVisual()
    {
        if (gunAttachPoint == null) return;
        
        // Look for line visual in the controller hierarchy
        // Common names: LineVisual, Line Visual, RayVisual, Ray Visual
        string[] lineVisualNames = { "LineVisual", "Line Visual", "RayVisual", "Ray Visual" };
        string[] interactorNames = { "Near-Far Interactor", "NearFar Interactor", "Ray Interactor", "XRRayInteractor" };
        
        Transform parent = gunAttachPoint.parent;
        while (parent != null)
        {
            // First try to find interactor, then look for line visual inside it
            foreach (string interactorName in interactorNames)
            {
                Transform interactor = parent.Find(interactorName);
                if (interactor != null)
                {
                    foreach (string lineName in lineVisualNames)
                    {
                        Transform lineVisual = interactor.Find(lineName);
                        if (lineVisual != null)
                        {
                            lineVisual.gameObject.SetActive(false);
                            Debug.Log($"GunSelectionManager: Hidden controller line visual: {lineVisual.name}");
                            return;
                        }
                    }
                }
            }
            
            // Also check direct children for line visual
            foreach (string lineName in lineVisualNames)
            {
                Transform lineVisual = parent.Find(lineName);
                if (lineVisual != null)
                {
                    lineVisual.gameObject.SetActive(false);
                    Debug.Log($"GunSelectionManager: Hidden controller line visual: {lineVisual.name}");
                    return;
                }
            }
            
            parent = parent.parent;
        }
        
        // Check siblings of the attach point for interactors with line visuals
        if (gunAttachPoint.parent != null)
        {
            foreach (Transform sibling in gunAttachPoint.parent)
            {
                foreach (string interactorName in interactorNames)
                {
                    if (sibling.name.Contains(interactorName) || sibling.name.Contains("Interactor"))
                    {
                        foreach (string lineName in lineVisualNames)
                        {
                            Transform lineVisual = sibling.Find(lineName);
                            if (lineVisual != null)
                            {
                                lineVisual.gameObject.SetActive(false);
                                Debug.Log($"GunSelectionManager: Hidden controller line visual: {lineVisual.name}");
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}