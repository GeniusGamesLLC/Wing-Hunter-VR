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
    
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI missedDucksText;
    
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;
    
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
        
        // Hide game over panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
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
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }
    }
    
    /// <summary>
    /// Updates the missed ducks display text
    /// </summary>
    private void UpdateMissedDucksDisplay()
    {
        if (missedDucksText != null && scoreManager != null)
        {
            missedDucksText.text = $"Missed: {scoreManager.MissedDucks}/{scoreManager.MaxMissedDucks}";
        }
    }
    
    /// <summary>
    /// Updates the missed ducks display text with specific count
    /// </summary>
    /// <param name="missedCount">The new missed ducks count</param>
    private void UpdateMissedDucksDisplay(int missedCount)
    {
        if (missedDucksText != null && scoreManager != null)
        {
            missedDucksText.text = $"Missed: {missedCount}/{scoreManager.MaxMissedDucks}";
        }
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