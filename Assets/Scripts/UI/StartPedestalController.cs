using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Controls the game start pedestal with a physical button and state-based UI.
/// Handles game start/restart functionality and displays game state information.
/// </summary>
public class StartPedestalController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScoreManager scoreManager;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI instructionText;
    
    [Header("Button Settings")]
    [SerializeField] private UnityEvent onButtonPressed;
    
    [Header("Pedestal Animation")]
    [SerializeField] private float raisedHeight = -0.05f;
    [SerializeField] private float loweredHeight = -1.2f;
    [SerializeField] private float animationDuration = 0.8f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // Animation state
    private float targetHeight;
    private float animationStartHeight;
    private float animationProgress = 1f;
    private bool isAnimating = false;
    
    // High score tracking
    private int highScore = 0;
    private const string HIGH_SCORE_KEY = "DuckHunt_HighScore";
    
    private void Awake()
    {
        // Load high score from PlayerPrefs
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }
    
    private void Start()
    {
        // Find managers if not assigned
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<ScoreManager>();
        }
        
        // Subscribe to game state changes
        if (gameManager != null)
        {
            gameManager.OnStateChanged += OnGameStateChanged;
        }
        
        // Subscribe to score changes for high score tracking
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += OnScoreChanged;
        }
        
        // Initialize UI
        UpdateUI(gameManager != null ? gameManager.CurrentState : GameState.Idle);
        UpdateHighScoreDisplay();
        
        // Set initial height based on current state
        targetHeight = raisedHeight;
    }
    
    private void Update()
    {
        // Handle pedestal animation
        if (isAnimating)
        {
            animationProgress += Time.deltaTime / animationDuration;
            
            if (animationProgress >= 1f)
            {
                animationProgress = 1f;
                isAnimating = false;
            }
            
            float curveValue = animationCurve.Evaluate(animationProgress);
            float newY = Mathf.Lerp(animationStartHeight, targetHeight, curveValue);
            
            Vector3 pos = transform.localPosition;
            pos.y = newY;
            transform.localPosition = pos;
        }
    }

    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.OnStateChanged -= OnGameStateChanged;
        }
        
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= OnScoreChanged;
        }
    }
    
    /// <summary>
    /// Called when the physical button is pressed (wired via Unity Events)
    /// </summary>
    public void OnStartButtonPressed()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("StartPedestalController: GameManager not found!");
            return;
        }
        
        switch (gameManager.CurrentState)
        {
            case GameState.Idle:
                Debug.Log("StartPedestalController: Starting game");
                gameManager.StartGame();
                break;
                
            case GameState.GameOver:
                Debug.Log("StartPedestalController: Restarting game");
                gameManager.RestartGame();
                break;
                
            case GameState.Playing:
                Debug.Log("StartPedestalController: Game already in progress");
                break;
        }
        
        onButtonPressed?.Invoke();
    }
    
    /// <summary>
    /// Handles game state changes
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        UpdateUI(newState);
        
        // Animate pedestal based on game state
        if (newState == GameState.Playing)
        {
            LowerPedestal();
        }
        else if (newState == GameState.GameOver || newState == GameState.Idle)
        {
            RaisePedestal();
        }
        
        // Check for new high score when game ends
        if (newState == GameState.GameOver && scoreManager != null)
        {
            CheckHighScore(scoreManager.CurrentScore);
        }
    }
    
    /// <summary>
    /// Lowers the pedestal into the ground
    /// </summary>
    private void LowerPedestal()
    {
        animationStartHeight = transform.localPosition.y;
        targetHeight = loweredHeight;
        animationProgress = 0f;
        isAnimating = true;
    }
    
    /// <summary>
    /// Raises the pedestal back up
    /// </summary>
    private void RaisePedestal()
    {
        animationStartHeight = transform.localPosition.y;
        targetHeight = raisedHeight;
        animationProgress = 0f;
        isAnimating = true;
    }
    
    /// <summary>
    /// Tracks score changes for high score
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        // Update high score in real-time if exceeded
        if (newScore > highScore)
        {
            highScore = newScore;
            UpdateHighScoreDisplay();
        }
    }
    
    /// <summary>
    /// Updates the UI based on current game state
    /// </summary>
    private void UpdateUI(GameState state)
    {
        switch (state)
        {
            case GameState.Idle:
                SetStatusText("READY TO PLAY");
                SetInstructionText("Press Button to Start");
                break;
                
            case GameState.Playing:
                SetStatusText("GAME IN PROGRESS");
                SetInstructionText("Shoot the ducks!");
                break;
                
            case GameState.GameOver:
                int finalScore = scoreManager != null ? scoreManager.CurrentScore : 0;
                SetStatusText($"GAME OVER\nScore: {finalScore}");
                SetInstructionText("Press Button to Restart");
                break;
        }
    }
    
    /// <summary>
    /// Checks and saves high score
    /// </summary>
    private void CheckHighScore(int score)
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
            Debug.Log($"StartPedestalController: New high score: {highScore}");
        }
        UpdateHighScoreDisplay();
    }
    
    private void SetStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
    }
    
    private void SetInstructionText(string text)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
        }
    }
    
    private void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }
    
    /// <summary>
    /// Resets the high score (for testing/debug)
    /// </summary>
    public void ResetHighScore()
    {
        highScore = 0;
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, 0);
        PlayerPrefs.Save();
        UpdateHighScoreDisplay();
        Debug.Log("StartPedestalController: High score reset");
    }
}
