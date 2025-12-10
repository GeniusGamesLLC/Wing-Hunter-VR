using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunSelectionUI : MonoBehaviour
{
    [Header("Gun Selection Manager")]
    [SerializeField] private GunSelectionManager gunSelectionManager;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject gunSelectionPanel;
    [SerializeField] private Button showSelectionButton;
    [SerializeField] private Button hideSelectionButton;
    [SerializeField] private Button previousGunButton;
    [SerializeField] private Button nextGunButton;
    
    [Header("Gun Display")]
    [SerializeField] private TextMeshProUGUI gunNameText;
    [SerializeField] private TextMeshProUGUI gunDescriptionText;
    [SerializeField] private Image gunPreviewImage;
    [SerializeField] private Image gunIconImage;
    
    [Header("Gun List")]
    [SerializeField] private Transform gunListParent;
    [SerializeField] private GameObject gunListItemPrefab;
    
    private Button[] gunListButtons;
    
    private void Start()
    {
        InitializeUI();
        SetupGunSelection();
    }
    
    private void InitializeUI()
    {
        // Hide selection panel initially
        if (gunSelectionPanel != null)
        {
            gunSelectionPanel.SetActive(false);
        }
        
        // Wire up button events
        if (showSelectionButton != null)
        {
            showSelectionButton.onClick.AddListener(ShowGunSelection);
        }
        
        if (hideSelectionButton != null)
        {
            hideSelectionButton.onClick.AddListener(HideGunSelection);
        }
        
        if (previousGunButton != null)
        {
            previousGunButton.onClick.AddListener(SelectPreviousGun);
        }
        
        if (nextGunButton != null)
        {
            nextGunButton.onClick.AddListener(SelectNextGun);
        }
    }
    
    private void SetupGunSelection()
    {
        if (gunSelectionManager == null)
        {
            gunSelectionManager = FindObjectOfType<GunSelectionManager>();
        }
        
        if (gunSelectionManager != null)
        {
            // Subscribe to gun change events
            gunSelectionManager.OnGunChanged.AddListener(OnGunChanged);
            gunSelectionManager.OnGunIndexChanged.AddListener(OnGunIndexChanged);
            
            // Create gun list UI
            CreateGunListUI();
            
            // Update display with current gun
            if (gunSelectionManager.CurrentGun != null)
            {
                OnGunChanged(gunSelectionManager.CurrentGun);
            }
        }
        else
        {
            Debug.LogWarning("GunSelectionUI: No GunSelectionManager found!");
        }
    }
    
    private void CreateGunListUI()
    {
        if (gunListParent == null || gunSelectionManager.GunCollection == null)
        {
            return;
        }
        
        // Clear existing buttons
        foreach (Transform child in gunListParent)
        {
            Destroy(child.gameObject);
        }
        
        var guns = gunSelectionManager.GunCollection.AvailableGuns;
        gunListButtons = new Button[guns.Length];
        
        for (int i = 0; i < guns.Length; i++)
        {
            GameObject listItem = CreateGunListItem(guns[i], i);
            if (listItem != null)
            {
                gunListButtons[i] = listItem.GetComponent<Button>();
            }
        }
        
        UpdateGunListSelection();
    }
    
    private GameObject CreateGunListItem(GunData gunData, int index)
    {
        GameObject listItem;
        
        if (gunListItemPrefab != null)
        {
            listItem = Instantiate(gunListItemPrefab, gunListParent);
        }
        else
        {
            // Create simple button if no prefab provided
            listItem = new GameObject($"GunButton_{index}");
            listItem.transform.SetParent(gunListParent, false);
            
            // Add RectTransform
            RectTransform rect = listItem.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 40);
            
            // Add Button
            Button button = listItem.AddComponent<Button>();
            
            // Add Image for button background
            Image buttonImage = listItem.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(listItem.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = gunData.gunName;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
        }
        
        // Set up button click event
        Button listButton = listItem.GetComponent<Button>();
        if (listButton != null)
        {
            int gunIndex = index; // Capture for closure
            listButton.onClick.AddListener(() => SelectGun(gunIndex));
        }
        
        // Update text if TextMeshPro component exists
        TextMeshProUGUI buttonText = listItem.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = gunData.gunName;
        }
        
        return listItem;
    }
    
    private void OnGunChanged(GunData newGunData)
    {
        UpdateGunDisplay(newGunData);
    }
    
    private void OnGunIndexChanged(int newIndex)
    {
        UpdateGunListSelection();
    }
    
    private void UpdateGunDisplay(GunData gunData)
    {
        if (gunData == null) return;
        
        // Update gun name
        if (gunNameText != null)
        {
            gunNameText.text = gunData.gunName;
        }
        
        // Update gun description
        if (gunDescriptionText != null)
        {
            gunDescriptionText.text = gunData.description;
        }
        
        // Update gun preview image
        if (gunPreviewImage != null && gunData.gunPreview != null)
        {
            Sprite previewSprite = Sprite.Create(gunData.gunPreview, 
                new Rect(0, 0, gunData.gunPreview.width, gunData.gunPreview.height), 
                new Vector2(0.5f, 0.5f));
            gunPreviewImage.sprite = previewSprite;
            gunPreviewImage.gameObject.SetActive(true);
        }
        else if (gunPreviewImage != null)
        {
            gunPreviewImage.gameObject.SetActive(false);
        }
        
        // Update gun icon
        if (gunIconImage != null && gunData.gunIcon != null)
        {
            gunIconImage.sprite = gunData.gunIcon;
            gunIconImage.gameObject.SetActive(true);
        }
        else if (gunIconImage != null)
        {
            gunIconImage.gameObject.SetActive(false);
        }
    }
    
    private void UpdateGunListSelection()
    {
        if (gunListButtons == null || gunSelectionManager == null) return;
        
        int currentIndex = gunSelectionManager.CurrentGunIndex;
        
        for (int i = 0; i < gunListButtons.Length; i++)
        {
            if (gunListButtons[i] != null)
            {
                // Highlight selected gun button
                Image buttonImage = gunListButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = (i == currentIndex) ? 
                        new Color(0.4f, 0.6f, 0.8f, 0.8f) : // Selected color
                        new Color(0.2f, 0.2f, 0.2f, 0.8f);  // Default color
                }
            }
        }
    }
    
    /// <summary>
    /// Show the gun selection panel
    /// </summary>
    public void ShowGunSelection()
    {
        if (gunSelectionPanel != null)
        {
            gunSelectionPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the gun selection panel
    /// </summary>
    public void HideGunSelection()
    {
        if (gunSelectionPanel != null)
        {
            gunSelectionPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Toggle gun selection panel visibility
    /// </summary>
    public void ToggleGunSelection()
    {
        if (gunSelectionPanel != null)
        {
            gunSelectionPanel.SetActive(!gunSelectionPanel.activeInHierarchy);
        }
    }
    
    /// <summary>
    /// Select a gun by index
    /// </summary>
    public void SelectGun(int gunIndex)
    {
        if (gunSelectionManager != null)
        {
            gunSelectionManager.SelectGun(gunIndex);
        }
    }
    
    /// <summary>
    /// Select the next gun in the collection
    /// </summary>
    public void SelectNextGun()
    {
        if (gunSelectionManager != null)
        {
            gunSelectionManager.SelectNextGun();
        }
    }
    
    /// <summary>
    /// Select the previous gun in the collection
    /// </summary>
    public void SelectPreviousGun()
    {
        if (gunSelectionManager != null)
        {
            gunSelectionManager.SelectPreviousGun();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunChanged.RemoveListener(OnGunChanged);
            gunSelectionManager.OnGunIndexChanged.RemoveListener(OnGunIndexChanged);
        }
        
        // Clean up button listeners
        if (showSelectionButton != null)
        {
            showSelectionButton.onClick.RemoveListener(ShowGunSelection);
        }
        
        if (hideSelectionButton != null)
        {
            hideSelectionButton.onClick.RemoveListener(HideGunSelection);
        }
        
        if (previousGunButton != null)
        {
            previousGunButton.onClick.RemoveListener(SelectPreviousGun);
        }
        
        if (nextGunButton != null)
        {
            nextGunButton.onClick.RemoveListener(SelectNextGun);
        }
    }
}