using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DuckHunt.Data;

/// <summary>
/// Debug paper that displays all debug options from DebugSettings.
/// Uses reflection to auto-generate toggles grouped by DebugCategoryAttribute.
/// Hidden by default, unlocked via Konami code.
/// </summary>
public class DebugPaper : MenuPaper
{
    [Header("Debug Paper Configuration")]
    [SerializeField] private GameObject togglePrefab;
    [SerializeField] private GameObject categoryHeaderPrefab;
    [SerializeField] private float toggleSpacing = 30f;
    [SerializeField] private float categorySpacing = 40f;
    [SerializeField] private float headerHeight = 35f;

    /// <summary>
    /// Dictionary mapping category names to their toggle bindings.
    /// </summary>
    private Dictionary<string, List<DebugToggleBinding>> categoryToggles = new Dictionary<string, List<DebugToggleBinding>>();

    /// <summary>
    /// Dictionary mapping category names to their Toggle All buttons.
    /// </summary>
    private Dictionary<string, Button> categoryToggleAllButtons = new Dictionary<string, Button>();

    /// <summary>
    /// All toggle bindings for quick access.
    /// </summary>
    private List<DebugToggleBinding> allBindings = new List<DebugToggleBinding>();

    /// <summary>
    /// Flag to prevent feedback loops during sync.
    /// </summary>
    private bool isSyncing = false;

    /// <summary>
    /// Returns all toggle bindings (for testing purposes).
    /// </summary>
    public IReadOnlyList<DebugToggleBinding> AllBindings => allBindings.AsReadOnly();

    /// <summary>
    /// Returns the category-to-toggles mapping (for testing purposes).
    /// </summary>
    public IReadOnlyDictionary<string, List<DebugToggleBinding>> CategoryToggles => categoryToggles;

    /// <summary>
    /// Returns the category Toggle All buttons (for testing purposes).
    /// </summary>
    public IReadOnlyDictionary<string, Button> CategoryToggleAllButtons => categoryToggleAllButtons;

    public override void Initialize()
    {
        // Debug paper is hidden by default - must be unlocked via Konami code
        isUnlockedByDefault = false;
        base.Initialize();

        GenerateTogglesFromDebugSettings();
        SubscribeToSettingsChanges();
    }

    private void OnDestroy()
    {
        UnsubscribeFromSettingsChanges();
    }

    /// <summary>
    /// Subscribes to DebugSettings.OnSettingsChanged for external sync.
    /// </summary>
    private void SubscribeToSettingsChanges()
    {
        var settings = DebugSettings.Instance;
        if (settings != null)
        {
            settings.OnSettingsChanged += OnDebugSettingsChanged;
        }
    }

    /// <summary>
    /// Unsubscribes from DebugSettings events.
    /// </summary>
    private void UnsubscribeFromSettingsChanges()
    {
        var settings = DebugSettings.Instance;
        if (settings != null)
        {
            settings.OnSettingsChanged -= OnDebugSettingsChanged;
        }
    }

    /// <summary>
    /// Called when DebugSettings changes externally.
    /// Syncs all toggles to reflect current values.
    /// </summary>
    private void OnDebugSettingsChanged()
    {
        if (isSyncing) return;
        SyncAllTogglesFromSettings();
    }

    /// <summary>
    /// Uses reflection to discover all boolean properties in DebugSettings
    /// that have the [DebugCategory] attribute and generates UI toggles grouped by category.
    /// Only boolean properties with the attribute are included.
    /// </summary>
    public void GenerateTogglesFromDebugSettings()
    {
        Debug.Log($"[DebugPaper] GenerateTogglesFromDebugSettings called. ContentRoot: {(contentRoot != null ? contentRoot.name : "NULL")}");
        
        if (contentRoot == null)
        {
            Debug.LogError("[DebugPaper] ContentRoot is null! Cannot generate toggles.");
            return;
        }

        // Clear existing
        ClearGeneratedUI();

        var settingsType = typeof(DebugSettings);
        // Only include boolean properties that have the [DebugCategory] attribute
        // This filters out inherited MonoBehaviour properties like enabled, useGUILayout, etc.
        var properties = settingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(bool) && 
                        p.CanRead && 
                        p.CanWrite &&
                        p.GetCustomAttribute<DebugCategoryAttribute>() != null)
            .ToList();
        
        Debug.Log($"[DebugPaper] Found {properties.Count} boolean properties with [DebugCategory] in DebugSettings");

        // Group properties by category
        var groupedProperties = new Dictionary<string, List<PropertyInfo>>();

        foreach (var prop in properties)
        {
            var categoryAttr = prop.GetCustomAttribute<DebugCategoryAttribute>();
            string category = categoryAttr.Category; // Safe - we already filtered for non-null

            if (!groupedProperties.ContainsKey(category))
            {
                groupedProperties[category] = new List<PropertyInfo>();
            }
            groupedProperties[category].Add(prop);
        }

        // Generate UI for each category
        float currentY = 0f;
        foreach (var kvp in groupedProperties.OrderBy(k => k.Key))
        {
            string category = kvp.Key;
            var categoryProperties = kvp.Value;

            CreateCategorySection(category, categoryProperties.ToArray(), ref currentY);
        }

        // Initial sync from settings
        SyncAllTogglesFromSettings();
    }

    /// <summary>
    /// Creates a category section with header, Toggle All button, and toggles.
    /// </summary>
    private void CreateCategorySection(string category, PropertyInfo[] properties, ref float currentY)
    {
        if (contentRoot == null)
        {
            Debug.LogWarning("[DebugPaper] ContentRoot is null, cannot create category section");
            return;
        }

        categoryToggles[category] = new List<DebugToggleBinding>();

        // Create category header with Toggle All button
        CreateCategoryHeader(category, ref currentY);

        // Create toggles for each property
        foreach (var prop in properties)
        {
            CreateToggleForProperty(prop, category, ref currentY);
        }

        // Add spacing after category
        currentY -= categorySpacing;
    }

    /// <summary>
    /// Creates a category header with label and Toggle All button.
    /// </summary>
    private void CreateCategoryHeader(string category, ref float currentY)
    {
        GameObject headerObj;

        if (categoryHeaderPrefab != null)
        {
            headerObj = Instantiate(categoryHeaderPrefab, contentRoot);
        }
        else
        {
            // Create a simple header if no prefab
            headerObj = new GameObject($"Header_{category}");
            headerObj.transform.SetParent(contentRoot, false);

            // Add layout components
            var rectTransform = headerObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, currentY);
            rectTransform.sizeDelta = new Vector2(0, headerHeight);

            // Add horizontal layout
            var layout = headerObj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 10f;
            layout.padding = new RectOffset(5, 5, 0, 0);

            // Create label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(headerObj.transform, false);
            var labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = category;
            labelText.fontSize = 16;
            labelText.fontStyle = FontStyles.Bold;
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(150, headerHeight);

            // Create Toggle All button
            var buttonObj = new GameObject("ToggleAllButton");
            buttonObj.transform.SetParent(headerObj.transform, false);
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(80, 25);

            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            var buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            var buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Toggle All";
            buttonText.fontSize = 12;
            buttonText.alignment = TextAlignmentOptions.Center;
            var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;

            // Store button reference
            categoryToggleAllButtons[category] = button;

            // Wire up button click
            string cat = category; // Capture for closure
            button.onClick.AddListener(() => OnToggleAllPressed(cat));
        }

        // If using prefab, find and configure components
        if (categoryHeaderPrefab != null)
        {
            var labelText = headerObj.GetComponentInChildren<TextMeshProUGUI>();
            if (labelText != null)
            {
                labelText.text = category;
            }

            var button = headerObj.GetComponentInChildren<Button>();
            if (button != null)
            {
                categoryToggleAllButtons[category] = button;
                string cat = category;
                button.onClick.AddListener(() => OnToggleAllPressed(cat));
            }

            var rectTransform = headerObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, currentY);
            }
        }

        currentY -= headerHeight;
    }

    /// <summary>
    /// Creates a toggle UI element for a debug property.
    /// </summary>
    private void CreateToggleForProperty(PropertyInfo property, string category, ref float currentY)
    {
        // Get display name (convert PascalCase to spaces)
        string displayName = AddSpacesToPascalCase(property.Name);

        // Get tooltip from attribute
        var tooltipAttr = property.GetCustomAttribute<TooltipAttribute>();
        string tooltip = tooltipAttr?.tooltip;

        GameObject toggleObj;

        if (togglePrefab != null)
        {
            toggleObj = Instantiate(togglePrefab, contentRoot);
        }
        else
        {
            // Create a simple toggle if no prefab
            toggleObj = new GameObject($"Toggle_{property.Name}");
            toggleObj.transform.SetParent(contentRoot, false);

            var rectTransform = toggleObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, currentY);
            rectTransform.sizeDelta = new Vector2(0, toggleSpacing);

            // Add horizontal layout
            var layout = toggleObj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 10f;
            layout.padding = new RectOffset(20, 5, 0, 0);

            // Create toggle background
            var toggleBgObj = new GameObject("Background");
            toggleBgObj.transform.SetParent(toggleObj.transform, false);
            var bgRect = toggleBgObj.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(20, 20);
            var bgImage = toggleBgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Create checkmark
            var checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(toggleBgObj.transform, false);
            var checkRect = checkmarkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.1f, 0.1f);
            checkRect.anchorMax = new Vector2(0.9f, 0.9f);
            checkRect.sizeDelta = Vector2.zero;
            var checkImage = checkmarkObj.AddComponent<Image>();
            checkImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);

            // Create toggle component
            var toggle = toggleObj.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;

            // Create label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);
            var labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = displayName;
            labelText.fontSize = 14;
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(200, toggleSpacing);
        }

        // Get or configure toggle component
        var toggleComponent = toggleObj.GetComponent<Toggle>();
        if (toggleComponent == null)
        {
            toggleComponent = toggleObj.GetComponentInChildren<Toggle>();
        }

        if (toggleComponent == null)
        {
            Debug.LogError($"[DebugPaper] No Toggle component found for property {property.Name}");
            return;
        }

        // Configure label if using prefab
        if (togglePrefab != null)
        {
            var labelText = toggleObj.GetComponentInChildren<TextMeshProUGUI>();
            if (labelText != null)
            {
                labelText.text = displayName;
            }

            var rectTransform = toggleObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, currentY);
            }
        }

        // Create binding
        var binding = new DebugToggleBinding(property, toggleComponent, category, displayName, tooltip);
        allBindings.Add(binding);
        categoryToggles[category].Add(binding);

        // Wire up toggle change event
        toggleComponent.onValueChanged.AddListener((value) => OnToggleChanged(binding));

        // Add VR interaction component for hover + trigger interaction
        if (toggleObj.GetComponent<DebugToggleInteraction>() == null)
        {
            toggleObj.AddComponent<DebugToggleInteraction>();
        }

        currentY -= toggleSpacing;
    }

    /// <summary>
    /// Called when a toggle value changes via UI interaction.
    /// </summary>
    private void OnToggleChanged(DebugToggleBinding binding)
    {
        if (isSyncing) return;

        isSyncing = true;
        try
        {
            binding.SyncToSettings();
        }
        finally
        {
            isSyncing = false;
        }
    }

    /// <summary>
    /// Called when a Toggle All button is pressed.
    /// Sets all toggles in the category to the inverse of the majority state.
    /// </summary>
    public void OnToggleAllPressed(string category)
    {
        if (!categoryToggles.ContainsKey(category))
        {
            Debug.LogWarning($"[DebugPaper] Category '{category}' not found");
            return;
        }

        var toggles = categoryToggles[category];
        if (toggles.Count == 0) return;

        // Count how many are currently on
        int onCount = toggles.Count(t => t.GetCurrentValue());
        int total = toggles.Count;

        // If majority are on (more than half), turn all off; otherwise turn all on
        bool newValue = onCount <= total / 2;

        isSyncing = true;
        try
        {
            foreach (var binding in toggles)
            {
                binding.SetValue(newValue);
                binding.UIToggle.SetIsOnWithoutNotify(newValue);
            }

            // Notify settings changed once after all updates
            DebugSettings.Instance?.NotifySettingsChanged();
        }
        finally
        {
            isSyncing = false;
        }
    }

    /// <summary>
    /// Syncs all toggles from DebugSettings values.
    /// </summary>
    public void SyncAllTogglesFromSettings()
    {
        isSyncing = true;
        try
        {
            foreach (var binding in allBindings)
            {
                binding.SyncFromSettings();
            }
        }
        finally
        {
            isSyncing = false;
        }
    }

    /// <summary>
    /// Clears all generated UI elements.
    /// </summary>
    private void ClearGeneratedUI()
    {
        // Remove listeners
        foreach (var binding in allBindings)
        {
            if (binding.UIToggle != null)
            {
                binding.UIToggle.onValueChanged.RemoveAllListeners();
            }
        }

        foreach (var button in categoryToggleAllButtons.Values)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        // Destroy generated objects
        if (contentRoot != null)
        {
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(contentRoot.GetChild(i).gameObject);
            }
        }

        // Clear collections
        allBindings.Clear();
        categoryToggles.Clear();
        categoryToggleAllButtons.Clear();
    }

    /// <summary>
    /// Converts PascalCase to a string with spaces.
    /// e.g., "ShowSplinePaths" -> "Show Spline Paths"
    /// </summary>
    private string AddSpacesToPascalCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var result = new System.Text.StringBuilder();
        result.Append(text[0]);

        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
            {
                result.Append(' ');
            }
            result.Append(text[i]);
        }

        return result.ToString();
    }

    public override void RefreshContent()
    {
        SyncAllTogglesFromSettings();
    }

    public override void OnFocus()
    {
        base.OnFocus();
        RefreshContent();
    }
}
