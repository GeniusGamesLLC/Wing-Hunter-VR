using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Utility script to help set up difficulty feedback UI elements in the scene
/// </summary>
public class DifficultyUISetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private Vector3 difficultyTextPosition = new Vector3(-2f, 1.5f, 0f);
    [SerializeField] private Vector3 feedbackPanelPosition = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector2 feedbackPanelSize = new Vector2(400f, 150f);
    
    /// <summary>
    /// Sets up difficulty UI elements automatically if enabled
    /// </summary>
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupDifficultyUI();
        }
    }
    
    /// <summary>
    /// Creates and configures difficulty UI elements
    /// </summary>
    [ContextMenu("Setup Difficulty UI")]
    public void SetupDifficultyUI()
    {
        // Find the UIManager in the scene
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("DifficultyUISetup: No UIManager found in scene. Please add a UIManager first.");
            return;
        }
        
        // Find or create the main canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("DifficultyUISetup: No Canvas found in scene. Please add a Canvas first.");
            return;
        }
        
        // Create difficulty text if it doesn't exist
        CreateDifficultyText(canvas.transform);
        
        // Create difficulty feedback panel if it doesn't exist
        CreateDifficultyFeedbackPanel(canvas.transform);
        
        // Create difficulty effect component
        CreateDifficultyEffect(uiManager.transform);
        
        Debug.Log("DifficultyUISetup: Difficulty UI elements created successfully");
    }
    
    /// <summary>
    /// Creates the difficulty level display text
    /// </summary>
    /// <param name="canvasTransform">The canvas transform to parent the text to</param>
    private void CreateDifficultyText(Transform canvasTransform)
    {
        // Check if difficulty text already exists
        TextMeshProUGUI existingText = canvasTransform.GetComponentInChildren<TextMeshProUGUI>();
        if (existingText != null && existingText.name.Contains("Difficulty"))
        {
            Debug.Log("DifficultyUISetup: Difficulty text already exists");
            return;
        }
        
        // Create difficulty text GameObject
        GameObject difficultyTextObj = new GameObject("DifficultyText");
        difficultyTextObj.transform.SetParent(canvasTransform, false);
        
        // Add and configure TextMeshProUGUI component
        TextMeshProUGUI difficultyText = difficultyTextObj.AddComponent<TextMeshProUGUI>();
        difficultyText.text = "Level: 1";
        difficultyText.fontSize = 24f;
        difficultyText.color = Color.white;
        difficultyText.alignment = TextAlignmentOptions.Center;
        
        // Position the text
        RectTransform rectTransform = difficultyText.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = difficultyTextPosition;
        rectTransform.sizeDelta = new Vector2(200f, 50f);
        
        Debug.Log("DifficultyUISetup: Created difficulty text");
    }
    
    /// <summary>
    /// Creates the difficulty feedback panel
    /// </summary>
    /// <param name="canvasTransform">The canvas transform to parent the panel to</param>
    private void CreateDifficultyFeedbackPanel(Transform canvasTransform)
    {
        // Check if feedback panel already exists
        Transform existingPanel = canvasTransform.Find("DifficultyFeedbackPanel");
        if (existingPanel != null)
        {
            Debug.Log("DifficultyUISetup: Difficulty feedback panel already exists");
            return;
        }
        
        // Create feedback panel GameObject
        GameObject feedbackPanel = new GameObject("DifficultyFeedbackPanel");
        feedbackPanel.transform.SetParent(canvasTransform, false);
        
        // Add and configure Image component for background
        Image panelImage = feedbackPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.8f); // Semi-transparent black
        
        // Position and size the panel
        RectTransform panelRect = feedbackPanel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = feedbackPanelPosition;
        panelRect.sizeDelta = feedbackPanelSize;
        
        // Create feedback text child
        GameObject feedbackTextObj = new GameObject("FeedbackText");
        feedbackTextObj.transform.SetParent(feedbackPanel.transform, false);
        
        // Add and configure TextMeshProUGUI component
        TextMeshProUGUI feedbackText = feedbackTextObj.AddComponent<TextMeshProUGUI>();
        feedbackText.text = "DIFFICULTY INCREASED!\nLevel 2";
        feedbackText.fontSize = 20f;
        feedbackText.color = Color.yellow;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.fontStyle = FontStyles.Bold;
        
        // Position the text to fill the panel
        RectTransform textRect = feedbackText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Hide the panel initially
        feedbackPanel.SetActive(false);
        
        Debug.Log("DifficultyUISetup: Created difficulty feedback panel");
    }
    
    /// <summary>
    /// Creates the difficulty particle effect component
    /// </summary>
    /// <param name="uiManagerTransform">The UIManager transform to attach the effect to</param>
    private void CreateDifficultyEffect(Transform uiManagerTransform)
    {
        // Check if difficulty effect already exists
        DifficultyFeedbackEffect existingEffect = uiManagerTransform.GetComponentInChildren<DifficultyFeedbackEffect>();
        if (existingEffect != null)
        {
            Debug.Log("DifficultyUISetup: Difficulty effect already exists");
            return;
        }
        
        // Create difficulty effect GameObject
        GameObject effectObj = new GameObject("DifficultyEffect");
        effectObj.transform.SetParent(uiManagerTransform, false);
        
        // Position the effect near the UI
        effectObj.transform.localPosition = new Vector3(0f, 1f, -0.5f);
        
        // Add the DifficultyFeedbackEffect component
        DifficultyFeedbackEffect effect = effectObj.AddComponent<DifficultyFeedbackEffect>();
        
        Debug.Log("DifficultyUISetup: Created difficulty effect component");
    }
}