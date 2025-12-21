using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings paper for the Announcement Board menu system.
/// Displays game settings options. Currently a placeholder that can be expanded later.
/// </summary>
public class SettingsPaper : MenuPaper
{
    [Header("Settings Paper Configuration")]
    [SerializeField] private GameObject placeholderTextPrefab;
    
    private TextMeshProUGUI settingsText;
    private bool contentInitialized;

    /// <summary>
    /// Initializes the Settings paper with default unlock state.
    /// </summary>
    public override void Initialize()
    {
        // Settings paper is unlocked by default (visible from start)
        isUnlockedByDefault = true;
        base.Initialize();
        
        if (!contentInitialized)
        {
            SetupSettingsContent();
            SetupPlaceholderContentIfNeeded();
            contentInitialized = true;
        }
    }
    
    /// <summary>
    /// Sets up placeholder content if not already assigned.
    /// </summary>
    private void SetupPlaceholderContentIfNeeded()
    {
        if (placeholderContent == null)
        {
            // Create placeholder content container
            GameObject placeholderObj = new GameObject("PlaceholderContent");
            placeholderObj.transform.SetParent(transform, false);
            placeholderObj.transform.localPosition = new Vector3(0, 0, -0.005f);
            placeholderObj.layer = 5; // UI layer
            
            // Add placeholder generator
            var generator = placeholderObj.AddComponent<PlaceholderContentGenerator>();
            generator.SetTitle("Settings");
            
            placeholderContent = placeholderObj.transform;
            
            // Start in compact state (placeholder visible, content hidden)
            placeholderObj.SetActive(true);
            if (contentRoot != null)
            {
                contentRoot.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Sets up the actual settings content.
    /// </summary>
    private void SetupSettingsContent()
    {
        if (contentRoot == null)
        {
            Debug.LogWarning("[SettingsPaper] Content root is not assigned.");
            return;
        }

        // Create settings text if no prefab is assigned
        if (placeholderTextPrefab == null)
        {
            CreateDefaultSettingsText();
        }
        else
        {
            GameObject textObj = Instantiate(placeholderTextPrefab, contentRoot);
            settingsText = textObj.GetComponent<TextMeshProUGUI>();
        }

        RefreshContent();
    }

    /// <summary>
    /// Called when this paper becomes focused.
    /// </summary>
    public override void OnFocus()
    {
        base.OnFocus();
        RefreshContent();
    }

    /// <summary>
    /// Called when this paper loses focus.
    /// </summary>
    public override void OnUnfocus()
    {
        base.OnUnfocus();
    }

    /// <summary>
    /// Refreshes the settings content display.
    /// </summary>
    public override void RefreshContent()
    {
        // Placeholder - future implementation will refresh actual settings UI
        if (settingsText != null)
        {
            settingsText.text = "Settings\n\nComing Soon...";
        }
    }

    /// <summary>
    /// Creates a default settings text element when no prefab is provided.
    /// </summary>
    private void CreateDefaultSettingsText()
    {
        GameObject textObj = new GameObject("SettingsText");
        textObj.transform.SetParent(contentRoot, false);
        
        settingsText = textObj.AddComponent<TextMeshProUGUI>();
        settingsText.text = "Settings\n\nComing Soon...";
        settingsText.fontSize = 24;
        settingsText.alignment = TextAlignmentOptions.Center;
        settingsText.color = Color.black;
        
        // Set up RectTransform for proper positioning
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
