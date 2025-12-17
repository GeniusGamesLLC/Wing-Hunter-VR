using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// World-space gun display rack that shows gun models for selection.
/// Reads guns dynamically from GunCollection ScriptableObject.
/// Supports pagination when there are more guns than display slots.
/// </summary>
public class GunDisplayRack : MonoBehaviour
{
    [Header("Gun Data")]
    [SerializeField] private GunCollection gunCollection;
    [SerializeField] private GunSelectionManager gunSelectionManager;
    
    [Header("Display Settings")]
    [SerializeField] private int slotsPerPage = 3;
    [SerializeField] private float slotSpacing = 0.4f;
    [SerializeField] private float gunPreviewScale = 0.3f;
    [SerializeField] private float gunRotationSpeed = 30f;
    
    [Header("Layout")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private float labelOffset = -0.05f;
    
    [Header("Navigation")]
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;
    [SerializeField] private TextMeshPro pageIndicatorText;
    
    [Header("Selection Highlight")]
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color normalColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    
    // Runtime state
    private int currentPage = 0;
    private int totalPages = 1;
    private GunDisplaySlot[] displaySlots;
    private int selectedGunIndex = -1;
    
    // Events
    public UnityEvent<int> OnGunSelected;
    
    private void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        // Find GunSelectionManager if not assigned
        if (gunSelectionManager == null)
        {
            gunSelectionManager = FindObjectOfType<GunSelectionManager>();
        }
        
        // Get GunCollection from manager if not assigned
        if (gunCollection == null && gunSelectionManager != null)
        {
            gunCollection = gunSelectionManager.GunCollection;
        }
        
        if (gunCollection == null)
        {
            Debug.LogError("GunDisplayRack: No GunCollection assigned!");
            return;
        }
        
        // Calculate total pages
        int gunCount = GetEnabledGunCount();
        totalPages = Mathf.CeilToInt((float)gunCount / slotsPerPage);
        if (totalPages < 1) totalPages = 1;
        
        // Create display slots
        CreateDisplaySlots();
        
        // Subscribe to gun selection changes
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunIndexChanged.AddListener(OnGunSelectionChanged);
            selectedGunIndex = gunSelectionManager.CurrentGunIndex;
        }
        
        // Show first page
        ShowPage(0);
    }
    
    private int GetEnabledGunCount()
    {
        int count = 0;
        foreach (var gun in gunCollection.AvailableGuns)
        {
            if (gun != null && gun.isEnabled) count++;
        }
        return count;
    }
    
    private void CreateDisplaySlots()
    {
        // Create slots parent if not assigned
        if (slotsParent == null)
        {
            GameObject slotsObj = new GameObject("GunSlots");
            slotsObj.transform.SetParent(transform);
            slotsObj.transform.localPosition = new Vector3(0, 0.05f, -0.1f); // Slightly raised
            slotsObj.transform.localRotation = Quaternion.identity;
            slotsParent = slotsObj.transform;
        }
        
        displaySlots = new GunDisplaySlot[slotsPerPage];
        
        // Calculate starting X position to center the slots
        float totalWidth = (slotsPerPage - 1) * slotSpacing;
        float startX = -totalWidth / 2f;
        
        for (int i = 0; i < slotsPerPage; i++)
        {
            GameObject slotObj = new GameObject($"Slot_{i}");
            slotObj.transform.SetParent(slotsParent);
            slotObj.transform.localPosition = new Vector3(startX + i * slotSpacing, 0, 0);
            slotObj.transform.localRotation = Quaternion.identity;
            
            GunDisplaySlot slot = slotObj.AddComponent<GunDisplaySlot>();
            slot.Initialize(this, i, gunPreviewScale, labelOffset, gunRotationSpeed);
            displaySlots[i] = slot;
        }
    }
    
    public void ShowPage(int pageIndex)
    {
        if (gunCollection == null) return;
        
        currentPage = Mathf.Clamp(pageIndex, 0, totalPages - 1);
        
        // Get enabled guns
        var enabledGuns = GetEnabledGuns();
        int startIndex = currentPage * slotsPerPage;
        
        // Update each slot
        for (int i = 0; i < slotsPerPage; i++)
        {
            int gunIndex = startIndex + i;
            
            if (gunIndex < enabledGuns.Length)
            {
                var gunData = enabledGuns[gunIndex];
                int actualIndex = GetActualGunIndex(gunData);
                bool isSelected = (actualIndex == selectedGunIndex);
                displaySlots[i].ShowGun(gunData, actualIndex, isSelected, selectedColor, normalColor);
            }
            else
            {
                displaySlots[i].Clear();
            }
        }
        
        // Update navigation arrows
        UpdateNavigationUI();
    }
    
    private GunData[] GetEnabledGuns()
    {
        var guns = gunCollection.AvailableGuns;
        int count = GetEnabledGunCount();
        GunData[] enabled = new GunData[count];
        
        int idx = 0;
        foreach (var gun in guns)
        {
            if (gun != null && gun.isEnabled)
            {
                enabled[idx++] = gun;
            }
        }
        return enabled;
    }
    
    private int GetActualGunIndex(GunData gunData)
    {
        var guns = gunCollection.AvailableGuns;
        for (int i = 0; i < guns.Length; i++)
        {
            if (guns[i] == gunData) return i;
        }
        return -1;
    }
    
    private void UpdateNavigationUI()
    {
        // Show/hide arrows based on pagination
        if (leftArrow != null)
        {
            leftArrow.SetActive(currentPage > 0);
        }
        
        if (rightArrow != null)
        {
            rightArrow.SetActive(currentPage < totalPages - 1);
        }
        
        // Update page indicator
        if (pageIndicatorText != null)
        {
            if (totalPages > 1)
            {
                pageIndicatorText.text = $"{currentPage + 1}/{totalPages}";
                pageIndicatorText.gameObject.SetActive(true);
            }
            else
            {
                pageIndicatorText.gameObject.SetActive(false);
            }
        }
    }
    
    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            ShowPage(currentPage + 1);
        }
    }
    
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            ShowPage(currentPage - 1);
        }
    }
    
    public void SelectGun(int gunIndex)
    {
        if (gunSelectionManager != null)
        {
            gunSelectionManager.SelectGun(gunIndex);
        }
        
        OnGunSelected?.Invoke(gunIndex);
    }
    
    private void OnGunSelectionChanged(int newIndex)
    {
        selectedGunIndex = newIndex;
        
        // Refresh current page to update selection highlight
        ShowPage(currentPage);
        
        // Navigate to page containing selected gun if not visible
        NavigateToGun(newIndex);
    }
    
    private void NavigateToGun(int gunIndex)
    {
        // Find which page this gun is on
        var enabledGuns = GetEnabledGuns();
        int enabledIndex = -1;
        
        for (int i = 0; i < enabledGuns.Length; i++)
        {
            if (GetActualGunIndex(enabledGuns[i]) == gunIndex)
            {
                enabledIndex = i;
                break;
            }
        }
        
        if (enabledIndex >= 0)
        {
            int targetPage = enabledIndex / slotsPerPage;
            if (targetPage != currentPage)
            {
                ShowPage(targetPage);
            }
        }
    }
    
    private void OnDestroy()
    {
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunIndexChanged.RemoveListener(OnGunSelectionChanged);
        }
    }
}
