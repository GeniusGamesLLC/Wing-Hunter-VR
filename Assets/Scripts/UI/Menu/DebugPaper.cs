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
/// Supports scrolling when content exceeds the visible area.
/// </summary>
public class DebugPaper : MenuPaper
{
    [Header("Debug Paper Configuration")]
    [SerializeField] private GameObject togglePrefab;
    [SerializeField] private GameObject categoryHeaderPrefab;
    [Tooltip("Vertical spacing between toggles (in world units, e.g., 0.035 = 3.5cm)")]
    [SerializeField] private float toggleSpacing = 0.035f;
    [Tooltip("Extra vertical spacing after each category (in world units)")]
    [SerializeField] private float categorySpacing = 0.02f;
    [Tooltip("Height of category headers (in world units)")]
    [SerializeField] private float headerHeight = 0.04f;
    
    [Header("Layout Configuration")]
    [Tooltip("Left margin from paper edge (in world units, e.g., 0.02 = 2cm)")]
    [SerializeField] private float leftMargin = 0.02f;
    [Tooltip("Indentation for category headers from left margin (in world units)")]
    [SerializeField] private float categoryIndent = 0f;
    [Tooltip("Indentation for toggles under category headers (in world units, e.g., 0.03 = 3cm)")]
    [SerializeField] private float toggleIndent = 0.03f;
    
    [Header("Scrolling")]
    [Tooltip("Reference to the scroll controller (auto-created if null)")]
    [SerializeField] private PaperScrollController scrollController;
    [Tooltip("Paper width for scroll indicator positioning (in world units)")]
    [SerializeField] private float paperWidth = 0.25f;
    [Tooltip("Paper height / viewport height for scroll calculations (in world units)")]
    [SerializeField] private float paperHeight = 0.35f;
    
    // Actual spacing values used at runtime (auto-corrected if serialized values are too large)
    private float _toggleSpacing;
    private float _categorySpacing;
    private float _headerHeight;
    
    // Scroll indicator generator
    private ScrollIndicatorGenerator scrollIndicatorGenerator;
    
    // Total content height after generating toggles
    private float totalContentHeight;

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
        SetupPlaceholderContentIfNeeded();
        SetupScrolling();
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
            generator.SetTitle("Debug");
            
            placeholderContent = placeholderObj.transform;
            
            // Start in compact state (placeholder visible, content hidden)
            placeholderObj.SetActive(true);
            if (contentRoot != null)
            {
                contentRoot.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromSettingsChanges();
        
        // Clean up scroll indicator
        if (scrollIndicatorGenerator != null)
        {
            scrollIndicatorGenerator.Cleanup();
        }
    }
    
    /// <summary>
    /// Sets up scrolling functionality after toggle generation.
    /// </summary>
    private void SetupScrolling()
    {
        // Create scroll controller if not assigned
        if (scrollController == null)
        {
            scrollController = gameObject.AddComponent<PaperScrollController>();
        }
        
        // Create scroll indicator generator
        scrollIndicatorGenerator = gameObject.AddComponent<ScrollIndicatorGenerator>();
        scrollIndicatorGenerator.GenerateScrollIndicator(paperWidth, paperHeight, transform);
        
        // Configure scroll controller with the generated indicators
        if (scrollIndicatorGenerator.ScrollIndicatorRoot != null)
        {
            scrollController.SetScrollIndicator(
                scrollIndicatorGenerator.ScrollIndicatorRoot,
                scrollIndicatorGenerator.HandleRectTransform
            );
        }
        
        if (scrollIndicatorGenerator.TopBoundaryIndicator != null && 
            scrollIndicatorGenerator.BottomBoundaryIndicator != null)
        {
            scrollController.SetBoundaryIndicators(
                scrollIndicatorGenerator.TopBoundaryIndicator,
                scrollIndicatorGenerator.BottomBoundaryIndicator
            );
        }
        
        // Subscribe to scroll position changes to update the indicator
        scrollController.OnScrollPositionChanged += OnScrollPositionChanged;
        
        // Configure scroll bounds based on content
        ConfigureScrollBounds();
        
        Debug.Log($"[DebugPaper] Scrolling setup complete. Content height: {totalContentHeight}, Paper height: {paperHeight}");
    }
    
    /// <summary>
    /// Configures scroll bounds based on generated content height.
    /// </summary>
    private void ConfigureScrollBounds()
    {
        if (scrollController == null || contentRoot == null) return;
        
        // Get or create RectTransforms for viewport and content
        RectTransform viewportRect = GetOrCreateViewportRect();
        RectTransform contentRect = GetOrCreateContentRect();
        
        if (viewportRect != null && contentRect != null)
        {
            // Set content height based on generated toggles
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalContentHeight);
            
            scrollController.SetScrollTargets(viewportRect, contentRect);
            scrollController.CalculateScrollBounds();
        }
    }
    
    /// <summary>
    /// Gets or creates a viewport RectTransform for scrolling.
    /// </summary>
    private RectTransform GetOrCreateViewportRect()
    {
        // Use the paper itself as the viewport
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = gameObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(paperWidth, paperHeight);
        }
        return rect;
    }
    
    /// <summary>
    /// Gets or creates a content RectTransform for scrolling.
    /// </summary>
    private RectTransform GetOrCreateContentRect()
    {
        if (contentRoot == null) return null;
        
        RectTransform rect = contentRoot.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = contentRoot.gameObject.AddComponent<RectTransform>();
        }
        return rect;
    }
    
    /// <summary>
    /// Called when scroll position changes.
    /// </summary>
    private void OnScrollPositionChanged(float normalizedPosition)
    {
        // Update scroll indicator handle position
        if (scrollIndicatorGenerator != null)
        {
            scrollIndicatorGenerator.UpdateHandlePosition(normalizedPosition);
        }
    }
    
    /// <summary>
    /// Gets the scroll controller (for testing purposes).
    /// </summary>
    public PaperScrollController ScrollController => scrollController;

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

        // Auto-correct spacing values if they seem to be in wrong units (> 1 means likely pixels instead of meters)
        _toggleSpacing = toggleSpacing > 1f ? toggleSpacing * 0.001f : toggleSpacing;
        _categorySpacing = categorySpacing > 1f ? categorySpacing * 0.001f : categorySpacing;
        _headerHeight = headerHeight > 1f ? headerHeight * 0.001f : headerHeight;
        
        Debug.Log($"[DebugPaper] Using spacing values: toggle={_toggleSpacing}, category={_categorySpacing}, header={_headerHeight}");

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

        // Generate UI for each category with manual Y positioning (world-space units)
        // Y starts at 0 and goes negative (down) since prefabs are anchored to top
        float currentY = 0f;
        
        Debug.Log($"[DebugPaper] Generating UI for {groupedProperties.Count} categories");
        foreach (var kvp in groupedProperties.OrderBy(k => k.Key))
        {
            string category = kvp.Key;
            var categoryProperties = kvp.Value;

            Debug.Log($"[DebugPaper] Creating category '{category}' with {categoryProperties.Count} properties at Y={currentY}");
            CreateCategorySection(category, categoryProperties.ToArray(), ref currentY);
        }

        Debug.Log($"[DebugPaper] Finished generating UI. Final Y position: {currentY}");
        
        // Store total content height (absolute value since Y goes negative)
        totalContentHeight = Mathf.Abs(currentY);
        
        // Initial sync from settings
        SyncAllTogglesFromSettings();
        
        // Reconfigure scroll bounds after content generation
        ConfigureScrollBounds();
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
        
        // Add extra spacing after category
        currentY -= _categorySpacing;
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
            headerObj.name = $"Header_{category}";
            
            // Calculate X position with left margin and category indent
            float xPosition = leftMargin + categoryIndent;
            
            // Position the header using localPosition (works for both Transform and RectTransform)
            // For 3D world-space UI, we use localPosition directly
            headerObj.transform.localPosition = new Vector3(xPosition, currentY, 0);
            
            // Also set RectTransform if available
            var rectTransform = headerObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(xPosition, currentY);
            }
            
            Debug.Log($"[DebugPaper] Header '{category}' positioned at X={xPosition}, Y={currentY}");
            
            // Try TextMeshProUGUI first (Canvas UI), then TextMeshPro (3D world space)
            var labelTextUGUI = headerObj.GetComponentInChildren<TextMeshProUGUI>();
            if (labelTextUGUI != null)
            {
                labelTextUGUI.text = category;
                // Ensure left alignment
                labelTextUGUI.alignment = TextAlignmentOptions.Left;
            }
            else
            {
                // Try 3D TextMeshPro component
                var labelText3D = headerObj.GetComponentInChildren<TMPro.TextMeshPro>();
                if (labelText3D != null)
                {
                    labelText3D.text = category;
                    // Ensure left alignment
                    labelText3D.alignment = TextAlignmentOptions.Left;
                }
            }

            var button = headerObj.GetComponentInChildren<Button>();
            if (button != null)
            {
                categoryToggleAllButtons[category] = button;
                string cat = category;
                button.onClick.AddListener(() => OnToggleAllPressed(cat));
                
                // Add VR interaction component for hover + trigger interaction
                // This prevents paper unfocus when clicking Toggle All buttons
                if (button.GetComponent<ToggleAllButtonInteraction>() == null)
                {
                    button.gameObject.AddComponent<ToggleAllButtonInteraction>();
                }
            }
            
            // Move Y down by header height
            currentY -= _headerHeight;
        }
        else
        {
            // Create a simple header if no prefab
            headerObj = new GameObject($"Header_{category}");
            headerObj.transform.SetParent(contentRoot, false);

            // Calculate X position with left margin and category indent
            float xPosition = leftMargin + categoryIndent;

            // Add layout components
            var rectTransform = headerObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0, 1); // Left-aligned pivot
            rectTransform.anchoredPosition = new Vector2(xPosition, currentY);
            rectTransform.sizeDelta = new Vector2(0, _headerHeight);

            // Add horizontal layout
            var layout = headerObj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 10f;
            layout.padding = new RectOffset(0, 5, 0, 0); // No left padding since we use margin

            // Create label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(headerObj.transform, false);
            var labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = category;
            labelText.fontSize = 16;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Left; // Left alignment
            labelText.color = new Color(0.15f, 0.1f, 0.05f, 1f);
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(150, _headerHeight);

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
            
            // Move Y down by header height
            currentY -= _headerHeight;
        }
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
            toggleObj.name = $"Toggle_{property.Name}";
            
            // Calculate X position with left margin and toggle indent (under category headers)
            float xPosition = leftMargin + toggleIndent;
            
            // Position the toggle using localPosition (works for both Transform and RectTransform)
            // For 3D world-space UI, we use localPosition directly
            toggleObj.transform.localPosition = new Vector3(xPosition, currentY, 0);
            
            // Also set RectTransform if available
            var rectTransform = toggleObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(xPosition, currentY);
            }
            
            Debug.Log($"[DebugPaper] Toggle '{property.Name}' positioned at X={xPosition}, Y={currentY}");
            
            // Configure label - Try TextMeshProUGUI first (Canvas UI), then TextMeshPro (3D world space)
            var labelTextUGUI = toggleObj.GetComponentInChildren<TextMeshProUGUI>();
            if (labelTextUGUI != null)
            {
                labelTextUGUI.text = displayName;
                // Ensure left alignment
                labelTextUGUI.alignment = TextAlignmentOptions.Left;
            }
            else
            {
                // Try 3D TextMeshPro component
                var labelText3D = toggleObj.GetComponentInChildren<TMPro.TextMeshPro>();
                if (labelText3D != null)
                {
                    labelText3D.text = displayName;
                    // Ensure left alignment
                    labelText3D.alignment = TextAlignmentOptions.Left;
                }
            }
            
            // Move Y down by toggle spacing
            currentY -= _toggleSpacing;
        }
        else
        {
            // Create a simple toggle if no prefab
            toggleObj = new GameObject($"Toggle_{property.Name}");
            toggleObj.transform.SetParent(contentRoot, false);

            // Calculate X position with left margin and toggle indent
            float xPosition = leftMargin + toggleIndent;

            var rectTransform = toggleObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0, 1); // Left-aligned pivot
            rectTransform.anchoredPosition = new Vector2(xPosition, currentY);
            rectTransform.sizeDelta = new Vector2(0, _toggleSpacing);

            // Add horizontal layout
            var layout = toggleObj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 10f;
            layout.padding = new RectOffset(0, 5, 0, 0); // No left padding since we use indent

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
            labelText.alignment = TextAlignmentOptions.Left; // Left alignment
            labelText.color = new Color(0.15f, 0.1f, 0.05f, 1f);
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(200, _toggleSpacing);
            
            // Move Y down by toggle spacing
            currentY -= _toggleSpacing;
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
        
        // Enable scroll input when paper is focused
        if (scrollController != null)
        {
            scrollController.SetReceivingInput(true);
        }
    }
    
    public override void OnUnfocus()
    {
        base.OnUnfocus();
        
        // Disable scroll input when paper loses focus
        if (scrollController != null)
        {
            scrollController.SetReceivingInput(false);
        }
    }
}
