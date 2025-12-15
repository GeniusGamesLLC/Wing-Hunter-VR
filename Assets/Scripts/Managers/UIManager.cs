using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the world-space UI canvas and all UI elements for the VR Duck Hunt game
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Canvas")]
    [SerializeField] private Canvas worldSpaceCanvas;
    
    [Header("Score Display - World Space (TextMeshPro)")]
    [SerializeField] private TextMeshPro scoreText3D;
    [SerializeField] private TextMeshPro missedDucksText3D;
    [SerializeField] private TextMeshPro levelText3D;
    
    [Header("Score Display - Canvas (TextMeshProUGUI) - Legacy")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI missedDucksText;
    
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;
    
    [Header("Difficulty Display")]
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private GameObject difficultyFeedbackPanel;
    [SerializeField] private TextMeshProUGUI difficultyFeedbackText;
    [SerializeField] private DifficultyFeedbackEffect difficultyEffect;
    [SerializeField] private float feedbackDisplayDuration = 2f;
    
    [Header("Configuration")]
    [SerializeField] private Vector3 canvasPosition = new Vector3(0, 2, 3);
    [SerializeField] private Vector3 canvasRotation = new Vector3(0, 0, 0);
    [SerializeField] private float canvasScale = 0.01f;
    
    // Manager references
    private GameManager gameManager;
    private ScoreManager scoreManager;
    
    private void Awake()
    {
        // Find manager references
        gameManager = FindObjectOfType<GameManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        
        if (gameManager == null)
        {
            Debug.LogError("UIManager: GameManager not found in scene!");
        }
        
        if (scoreManager == null)
        {
            Debug.LogError("UIManager: ScoreManager not found in scene!");
        }
        
        // Auto-discover UI elements if not assigned
        AutoDiscoverUIElements();
    }
    
    /// <summary>
    /// Auto-discovers UI elements by name if they are not manually assigned
    /// </summary>
    private void AutoDiscoverUIElements()
    {
        // First try to find world-space scoreboard (preferred for VR)
        AutoDiscoverWorldSpaceScoreboard();
        
        // Find Canvas if not assigned (legacy/fallback)
        if (worldSpaceCanvas == null)
        {
            worldSpaceCanvas = FindObjectOfType<Canvas>();
            if (worldSpaceCanvas != null)
            {
                Debug.Log("UIManager: Auto-discovered Canvas");
            }
        }
        
        if (worldSpaceCanvas == null && scoreText3D == null)
        {
            Debug.LogWarning("UIManager: No Canvas or WorldScoreboard found in scene!");
            return;
        }
        
        // Find UI elements within the canvas (if canvas exists)
        if (worldSpaceCanvas == null) return;
        Transform canvasTransform = worldSpaceCanvas.transform;
        
        // Score display elements
        if (scoreText == null)
        {
            scoreText = FindUIElement<TextMeshProUGUI>(canvasTransform, "ScoreText");
        }
        
        if (missedDucksText == null)
        {
            missedDucksText = FindUIElement<TextMeshProUGUI>(canvasTransform, "MissedDucksText");
        }
        
        if (difficultyText == null)
        {
            difficultyText = FindUIElement<TextMeshProUGUI>(canvasTransform, "DifficultyText");
        }
        
        // Game over panel elements
        if (gameOverPanel == null)
        {
            Transform panel = FindChildRecursive(canvasTransform, "GameOverPanel");
            if (panel != null)
            {
                gameOverPanel = panel.gameObject;
                Debug.Log("UIManager: Auto-discovered GameOverPanel");
            }
        }
        
        if (finalScoreText == null && gameOverPanel != null)
        {
            finalScoreText = FindUIElement<TextMeshProUGUI>(gameOverPanel.transform, "FinalScoreText");
        }
        
        if (restartButton == null && gameOverPanel != null)
        {
            // Try to find by name RestartButton or just Button
            restartButton = FindUIElement<Button>(gameOverPanel.transform, "RestartButton");
            if (restartButton == null)
            {
                restartButton = FindUIElement<Button>(gameOverPanel.transform, "Button");
            }
        }
        
        // Difficulty feedback panel elements
        if (difficultyFeedbackPanel == null)
        {
            Transform panel = FindChildRecursive(canvasTransform, "DifficultyFeedbackPanel");
            if (panel != null)
            {
                difficultyFeedbackPanel = panel.gameObject;
                Debug.Log("UIManager: Auto-discovered DifficultyFeedbackPanel");
            }
        }
        
        if (difficultyFeedbackText == null && difficultyFeedbackPanel != null)
        {
            difficultyFeedbackText = FindUIElement<TextMeshProUGUI>(difficultyFeedbackPanel.transform, "DifficultyFeedbackText");
        }
        
        // Log discovery results
        LogDiscoveryResults();
    }
    
    /// <summary>
    /// Auto-discovers world-space scoreboard TextMeshPro elements
    /// </summary>
    private void AutoDiscoverWorldSpaceScoreboard()
    {
        GameObject scoreboard = GameObject.Find("WorldScoreboard");
        if (scoreboard == null) return;
        
        Debug.Log("UIManager: Found WorldScoreboard, discovering text elements...");
        
        if (scoreText3D == null)
        {
            Transform t = FindChildRecursive(scoreboard.transform, "ScoreText");
            if (t != null) scoreText3D = t.GetComponent<TextMeshPro>();
        }
        
        if (missedDucksText3D == null)
        {
            Transform t = FindChildRecursive(scoreboard.transform, "MissedText");
            if (t != null) missedDucksText3D = t.GetComponent<TextMeshPro>();
        }
        
        if (levelText3D == null)
        {
            Transform t = FindChildRecursive(scoreboard.transform, "LevelText");
            if (t != null) levelText3D = t.GetComponent<TextMeshPro>();
        }
        
        Debug.Log($"UIManager: WorldScoreboard - Score:{scoreText3D != null}, Missed:{missedDucksText3D != null}, Level:{levelText3D != null}");
    }
    
    /// <summary>
    /// Finds a UI element of type T by name within a parent transform
    /// </summary>
    private T FindUIElement<T>(Transform parent, string elementName) where T : Component
    {
        Transform found = FindChildRecursive(parent, elementName);
        if (found != null)
        {
            T component = found.GetComponent<T>();
            if (component != null)
            {
                Debug.Log($"UIManager: Auto-discovered {elementName}");
                return component;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Recursively finds a child transform by name
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        // Check direct children first
        Transform found = parent.Find(childName);
        if (found != null)
        {
            return found;
        }
        
        // Search recursively in all children
        foreach (Transform child in parent)
        {
            found = FindChildRecursive(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Logs the results of UI element discovery
    /// </summary>
    private void LogDiscoveryResults()
    {
        int found = 0;
        int missing = 0;
        
        if (worldSpaceCanvas != null) found++; else missing++;
        if (scoreText != null) found++; else missing++;
        if (missedDucksText != null) found++; else missing++;
        if (difficultyText != null) found++; else missing++;
        if (gameOverPanel != null) found++; else missing++;
        if (finalScoreText != null) found++; else missing++;
        if (restartButton != null) found++; else missing++;
        if (difficultyFeedbackPanel != null) found++; else missing++;
        if (difficultyFeedbackText != null) found++; else missing++;
        
        Debug.Log($"UIManager: UI element discovery complete. Found: {found}, Missing: {missing}");
        
        if (missing > 0)
        {
            if (scoreText == null) Debug.LogWarning("UIManager: ScoreText not found");
            if (missedDucksText == null) Debug.LogWarning("UIManager: MissedDucksText not found");
            if (difficultyText == null) Debug.LogWarning("UIManager: DifficultyText not found");
            if (gameOverPanel == null) Debug.LogWarning("UIManager: GameOverPanel not found");
            if (finalScoreText == null) Debug.LogWarning("UIManager: FinalScoreText not found");
            if (restartButton == null) Debug.LogWarning("UIManager: RestartButton not found");
            if (difficultyFeedbackPanel == null) Debug.LogWarning("UIManager: DifficultyFeedbackPanel not found");
            if (difficultyFeedbackText == null) Debug.LogWarning("UIManager: DifficultyFeedbackText not found");
        }
    }
    
    private void Start()
    {
        // Subscribe to manager events
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += UpdateScoreDisplay;
            scoreManager.OnMissedDucksChanged += UpdateMissedDucksDisplay;
        }
        
        if (gameManager != null)
        {
            gameManager.OnStateChanged += OnGameStateChanged;
            gameManager.OnDifficultyChanged += OnDifficultyChanged;
        }
        
        // Wire up restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        
        // Initialize UI state
        InitializeUI();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= UpdateScoreDisplay;
            scoreManager.OnMissedDucksChanged -= UpdateMissedDucksDisplay;
        }
        
        if (gameManager != null)
        {
            gameManager.OnStateChanged -= OnGameStateChanged;
            gameManager.OnDifficultyChanged -= OnDifficultyChanged;
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }
    }
    
    /// <summary>
    /// Initializes the UI elements and canvas setup
    /// </summary>
    private void InitializeUI()
    {
        // Set up world space canvas if not already configured
        if (worldSpaceCanvas != null)
        {
            SetupWorldSpaceCanvas();
        }
        
        // Initialize score display
        UpdateScoreDisplay(0);
        UpdateMissedDucksDisplay();
        
        // Initialize difficulty display
        UpdateDifficultyDisplay(1);
        
        // Hide panels initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (difficultyFeedbackPanel != null)
        {
            difficultyFeedbackPanel.SetActive(false);
        }
        
        Debug.Log("UIManager: UI initialized");
    }
    
    /// <summary>
    /// Sets up the world space canvas with proper positioning and scale
    /// </summary>
    private void SetupWorldSpaceCanvas()
    {
        // Set render mode to World Space
        worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
        
        // Position the canvas in front of the player
        worldSpaceCanvas.transform.position = canvasPosition;
        worldSpaceCanvas.transform.rotation = Quaternion.Euler(canvasRotation);
        worldSpaceCanvas.transform.localScale = Vector3.one * canvasScale;
        
        // Configure canvas properties for VR
        worldSpaceCanvas.sortingOrder = 1;
        
        Debug.Log($"UIManager: World space canvas configured at position {canvasPosition}");
    }
    
    /// <summary>
    /// Updates the score display text
    /// </summary>
    /// <param name="newScore">The new score value</param>
    private void UpdateScoreDisplay(int newScore)
    {
        // World-space 3D text (preferred)
        if (scoreText3D != null)
            scoreText3D.text = $"Score: {newScore}";
        // Legacy canvas text
        if (scoreText != null)
            scoreText.text = $"Score: {newScore}";
    }
    
    /// <summary>
    /// Updates the missed ducks display text
    /// </summary>
    private void UpdateMissedDucksDisplay()
    {
        if (scoreManager == null) return;
        string text = $"Missed: {scoreManager.MissedDucks}/{scoreManager.MaxMissedDucks}";
        
        if (missedDucksText3D != null)
            missedDucksText3D.text = text;
        if (missedDucksText != null)
            missedDucksText.text = text;
    }
    
    /// <summary>
    /// Updates the missed ducks display text with specific count
    /// </summary>
    /// <param name="missedCount">The new missed ducks count</param>
    private void UpdateMissedDucksDisplay(int missedCount)
    {
        if (scoreManager == null) return;
        string text = $"Missed: {missedCount}/{scoreManager.MaxMissedDucks}";
        
        if (missedDucksText3D != null)
            missedDucksText3D.text = text;
        if (missedDucksText != null)
            missedDucksText.text = text;
    }
    
    /// <summary>
    /// Handles game state changes
    /// </summary>
    /// <param name="newState">The new game state</param>
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Idle:
                ShowIdleUI();
                break;
            case GameState.Playing:
                ShowPlayingUI();
                break;
            case GameState.GameOver:
                ShowGameOverUI();
                break;
        }
    }
    
    /// <summary>
    /// Shows UI elements for idle state
    /// </summary>
    private void ShowIdleUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Update displays
        UpdateScoreDisplay(0);
        UpdateMissedDucksDisplay();
    }
    
    /// <summary>
    /// Shows UI elements for playing state
    /// </summary>
    private void ShowPlayingUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Update displays
        UpdateMissedDucksDisplay();
    }
    
    /// <summary>
    /// Shows UI elements for game over state
    /// </summary>
    private void ShowGameOverUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Update final score display
        if (finalScoreText != null && scoreManager != null)
        {
            finalScoreText.text = $"Final Score: {scoreManager.CurrentScore}";
        }
        
        Debug.Log("UIManager: Game over UI displayed");
    }
    
    /// <summary>
    /// Handles restart button click
    /// </summary>
    private void OnRestartButtonClicked()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
            Debug.Log("UIManager: Restart button clicked");
        }
    }
    
    /// <summary>
    /// Sets the canvas position (useful for runtime adjustments)
    /// </summary>
    /// <param name="position">New canvas position</param>
    public void SetCanvasPosition(Vector3 position)
    {
        canvasPosition = position;
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.position = position;
        }
    }
    
    /// <summary>
    /// Sets the canvas scale (useful for runtime adjustments)
    /// </summary>
    /// <param name="scale">New canvas scale</param>
    public void SetCanvasScale(float scale)
    {
        canvasScale = scale;
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.localScale = Vector3.one * scale;
        }
    }
    
    /// <summary>
    /// Updates the difficulty display text
    /// </summary>
    /// <param name="difficultyLevel">The current difficulty level</param>
    private void UpdateDifficultyDisplay(int difficultyLevel)
    {
        string text = $"Level: {difficultyLevel}";
        
        if (levelText3D != null)
            levelText3D.text = text;
        if (difficultyText != null)
            difficultyText.text = text;
    }
    
    /// <summary>
    /// Handles difficulty level changes and shows visual feedback
    /// </summary>
    /// <param name="newDifficultyLevel">The new difficulty level</param>
    private void OnDifficultyChanged(int newDifficultyLevel)
    {
        // Update the persistent difficulty display
        UpdateDifficultyDisplay(newDifficultyLevel);
        
        // Show temporary feedback notification
        ShowDifficultyFeedback(newDifficultyLevel);
        
        Debug.Log($"UIManager: Difficulty changed to level {newDifficultyLevel}");
    }
    
    /// <summary>
    /// Shows a temporary visual feedback for difficulty changes
    /// </summary>
    /// <param name="difficultyLevel">The new difficulty level</param>
    private void ShowDifficultyFeedback(int difficultyLevel)
    {
        // Show text feedback if available
        if (difficultyFeedbackPanel != null && difficultyFeedbackText != null)
        {
            // Update feedback text
            difficultyFeedbackText.text = $"DIFFICULTY INCREASED!\nLevel {difficultyLevel}";
            
            // Show the feedback panel
            difficultyFeedbackPanel.SetActive(true);
            
            // Hide the panel after the specified duration
            StartCoroutine(HideDifficultyFeedbackAfterDelay());
        }
        
        // Trigger particle effect if available
        if (difficultyEffect != null)
        {
            difficultyEffect.TriggerDifficultyEffect(difficultyLevel);
        }
        
        Debug.Log($"UIManager: Showing difficulty feedback for level {difficultyLevel}");
    }
    
    /// <summary>
    /// Coroutine to hide the difficulty feedback panel after a delay
    /// </summary>
    private System.Collections.IEnumerator HideDifficultyFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDisplayDuration);
        
        if (difficultyFeedbackPanel != null)
        {
            difficultyFeedbackPanel.SetActive(false);
        }
    }
    
    #if UNITY_EDITOR
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        // Ensure canvas scale is positive
        if (canvasScale <= 0)
        {
            canvasScale = 0.01f;
        }
    }
    #endif
}