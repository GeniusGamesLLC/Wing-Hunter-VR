using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Utility script to help set up credits UI elements in the scene
/// This can be used as a reference for manual UI setup or automated creation
/// </summary>
public class CreditsUISetup : MonoBehaviour
{
    [Header("Credits UI Setup")]
    [SerializeField] private bool createCreditsUI = false;
    [SerializeField] private Transform uiParent;
    
    [Header("UI Prefab References")]
    [SerializeField] private GameObject creditsButtonPrefab;
    [SerializeField] private GameObject creditsPanelPrefab;
    
    private void Start()
    {
        if (createCreditsUI)
        {
            CreateCreditsUI();
        }
    }
    
    /// <summary>
    /// Creates a basic credits UI structure
    /// This method provides a template for setting up credits UI elements
    /// </summary>
    public void CreateCreditsUI()
    {
        // Find or create UI Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found in scene. Credits UI requires a Canvas to be present.");
            return;
        }
        
        Transform canvasTransform = canvas.transform;
        
        // Create Credits Button (this would typically be added to an existing menu)
        GameObject creditsButton = CreateCreditsButton(canvasTransform);
        
        // Create Credits Panel
        GameObject creditsPanel = CreateCreditsPanel(canvasTransform);
        
        // Set up CreditsManager
        SetupCreditsManager(creditsButton, creditsPanel);
        
        Debug.Log("Credits UI setup completed. Remember to configure CreditsData ScriptableObject.");
    }
    
    private GameObject CreateCreditsButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("CreditsButton");
        buttonObj.transform.SetParent(parent, false);
        
        // Add RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchoredPosition = new Vector2(100, 100);
        rectTransform.sizeDelta = new Vector2(120, 40);
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        
        // Add Image component for button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Credits";
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        return buttonObj;
    }
    
    private GameObject CreateCreditsPanel(Transform parent)
    {
        GameObject panelObj = new GameObject("CreditsPanel");
        panelObj.transform.SetParent(parent, false);
        
        // Add RectTransform
        RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add Image component for panel background
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        // Create scroll view for credits text
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.1f, 0.1f);
        scrollRect.anchorMax = new Vector2(0.9f, 0.8f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
        
        ScrollRect scrollComponent = scrollViewObj.AddComponent<ScrollRect>();
        
        // Create viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        // Create content area
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 1000);
        
        // Add ContentSizeFitter to auto-resize content
        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Create credits text
        GameObject textObj = new GameObject("CreditsText");
        textObj.transform.SetParent(contentObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        TextMeshProUGUI creditsText = textObj.AddComponent<TextMeshProUGUI>();
        creditsText.text = "Credits will be loaded here...";
        creditsText.fontSize = 12;
        creditsText.color = Color.white;
        creditsText.alignment = TextAlignmentOptions.TopLeft;
        
        // Configure scroll rect
        scrollComponent.content = contentRect;
        scrollComponent.viewport = viewportRect;
        scrollComponent.vertical = true;
        scrollComponent.horizontal = false;
        
        // Create close button
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform closeButtonRect = closeButtonObj.AddComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.9f, 0.9f);
        closeButtonRect.anchorMax = new Vector2(0.9f, 0.9f);
        closeButtonRect.anchoredPosition = Vector2.zero;
        closeButtonRect.sizeDelta = new Vector2(60, 30);
        
        Button closeButton = closeButtonObj.AddComponent<Button>();
        Image closeButtonImage = closeButtonObj.AddComponent<Image>();
        closeButtonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        
        // Close button text
        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);
        
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI closeButtonText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeButtonText.text = "Close";
        closeButtonText.fontSize = 10;
        closeButtonText.color = Color.white;
        closeButtonText.alignment = TextAlignmentOptions.Center;
        
        // Initially hide the panel
        panelObj.SetActive(false);
        
        return panelObj;
    }
    
    private void SetupCreditsManager(GameObject creditsButton, GameObject creditsPanel)
    {
        // Add CreditsManager to the scene
        GameObject managerObj = new GameObject("CreditsManager");
        CreditsManager creditsManager = managerObj.AddComponent<CreditsManager>();
        
        // Find UI components
        Button button = creditsButton.GetComponent<Button>();
        Button closeButton = creditsPanel.transform.Find("CloseButton").GetComponent<Button>();
        TextMeshProUGUI creditsText = creditsPanel.transform.Find("ScrollView/Viewport/Content/CreditsText").GetComponent<TextMeshProUGUI>();
        ScrollRect scrollRect = creditsPanel.transform.Find("ScrollView").GetComponent<ScrollRect>();
        
        // Use reflection to set private fields (for setup purposes)
        var creditsManagerType = typeof(CreditsManager);
        var creditsPanelField = creditsManagerType.GetField("creditsPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var creditsTextField = creditsManagerType.GetField("creditsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var creditsButtonField = creditsManagerType.GetField("creditsButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var closeCreditsButtonField = creditsManagerType.GetField("closeCreditsButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var creditsScrollRectField = creditsManagerType.GetField("creditsScrollRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        creditsPanelField?.SetValue(creditsManager, creditsPanel);
        creditsTextField?.SetValue(creditsManager, creditsText);
        creditsButtonField?.SetValue(creditsManager, button);
        closeCreditsButtonField?.SetValue(creditsManager, closeButton);
        creditsScrollRectField?.SetValue(creditsManager, scrollRect);
        
        Debug.Log("CreditsManager configured with UI references.");
    }
}