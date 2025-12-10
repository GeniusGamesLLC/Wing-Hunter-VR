using UnityEngine;
using UnityEngine.Events;

public class GunSelectionManager : MonoBehaviour
{
    [Header("Gun Configuration")]
    [SerializeField] private GunCollection gunCollection;
    [SerializeField] private Transform gunAttachPoint;
    
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
        
        // Destroy current gun if exists
        if (currentGunInstance != null)
        {
            DestroyImmediate(currentGunInstance);
        }
        
        // Instantiate new gun
        if (gunAttachPoint != null && newGunData.gunPrefab != null)
        {
            currentGunInstance = Instantiate(newGunData.gunPrefab, gunAttachPoint);
            currentGunInstance.transform.localPosition = Vector3.zero;
            currentGunInstance.transform.localRotation = Quaternion.identity;
            
            // Find muzzle point in the gun prefab
            Transform muzzlePoint = FindMuzzlePoint(currentGunInstance);
            if (muzzlePoint != null)
            {
                newGunData.muzzlePoint = muzzlePoint;
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
    /// Select next gun in the collection
    /// </summary>
    public void SelectNextGun()
    {
        int nextIndex = (currentGunIndex + 1) % gunCollection.AvailableGuns.Length;
        SelectGun(nextIndex);
    }
    
    /// <summary>
    /// Select previous gun in the collection
    /// </summary>
    public void SelectPreviousGun()
    {
        int prevIndex = (currentGunIndex - 1 + gunCollection.AvailableGuns.Length) % gunCollection.AvailableGuns.Length;
        SelectGun(prevIndex);
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
}