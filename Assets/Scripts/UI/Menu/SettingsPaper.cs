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
    
    private TextMeshProUGUI placeholderText;
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
            SetupPlaceholderContent();
            contentInitialized = true;
        }
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
        if (placeholderText != null)
        {
            placeholderText.text = "Settings\n\nComing Soon...";
        }
    }

    /// <summary>
    /// Sets up placeholder content for the settings paper.
    /// This will be replaced with actual settings UI in future implementation.
    /// </summary>
    private void SetupPlaceholderContent()
    {
        if (contentRoot == null)
        {
            Debug.LogWarning("[SettingsPaper] Content root is not assigned.");
            return;
        }

        // Create placeholder text if no prefab is assigned
        if (placeholderTextPrefab == null)
        {
            CreateDefaultPlaceholderText();
        }
        else
        {
            GameObject textObj = Instantiate(placeholderTextPrefab, contentRoot);
            placeholderText = textObj.GetComponent<TextMeshProUGUI>();
        }

        RefreshContent();
    }

    /// <summary>
    /// Creates a default placeholder text element when no prefab is provided.
    /// </summary>
    private void CreateDefaultPlaceholderText()
    {
        GameObject textObj = new GameObject("PlaceholderText");
        textObj.transform.SetParent(contentRoot, false);
        
        placeholderText = textObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Settings\n\nComing Soon...";
        placeholderText.fontSize = 24;
        placeholderText.alignment = TextAlignmentOptions.Center;
        placeholderText.color = Color.black;
        
        // Set up RectTransform for proper positioning
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
