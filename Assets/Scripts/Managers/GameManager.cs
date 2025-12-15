using System;
using UnityEngine;
    /// <summary>
    /// Central controller for game state and flow management
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DuckHuntConfig gameConfig;
        
        [Header("Manager References")]
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private SpawnManager spawnManager;
        
        [Header("Debug Info")]
        [SerializeField, ReadOnly] private GameState currentState = GameState.Idle;
        [SerializeField, ReadOnly] private int currentDifficulty = 1;
        
        // Public properties
        public GameState CurrentState => currentState;
        public int CurrentDifficulty => currentDifficulty;
        
        // Events
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnDifficultyChanged;
        
        // Public setters for setup scripts
        public void SetScoreManager(ScoreManager manager) => scoreManager = manager;
        public void SetSpawnManager(SpawnManager manager) => spawnManager = manager;
        
        private void Awake()
        {
            // Validate configuration
            if (gameConfig == null)
            {
                Debug.LogError("GameManager: No DuckHuntConfig assigned! Game may not function properly.");
            }
            
            // Find manager references if not assigned
            if (scoreManager == null)
            {
                scoreManager = FindObjectOfType<ScoreManager>();
                if (scoreManager == null)
                {
                    Debug.LogError("GameManager: ScoreManager not found in scene!");
                }
            }
            
            if (spawnManager == null)
            {
                spawnManager = FindObjectOfType<SpawnManager>();
                if (spawnManager == null)
                {
                    Debug.LogError("GameManager: SpawnManager not found in scene!");
                }
            }
        }
        
        private void Start()
        {
            // Subscribe to score manager events
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += OnScoreChanged;
                scoreManager.OnGameOver += OnGameOverTriggered;
                
                // Set the game config on the score manager
                scoreManager.SetGameConfig(gameConfig);
            }
            
            // Initialize game state
            SetState(GameState.Idle);
            ResetDifficulty();
            
            Debug.Log("GameManager: Initialized and ready");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= OnScoreChanged;
                scoreManager.OnGameOver -= OnGameOverTriggered;
            }
        }
        
        /// <summary>
        /// Starts a new game session
        /// </summary>
        public void StartGame()
        {
            if (currentState != GameState.Idle)
            {
                Debug.LogWarning($"GameManager: Cannot start game from {currentState} state");
                return;
            }
            
            Debug.Log("GameManager: Starting new game");
            
            // Reset all game systems
            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }
            
            ResetDifficulty();
            
            // Start spawning ducks
            if (spawnManager != null)
            {
                ApplyDifficultySettings();
                spawnManager.StartSpawning();
            }
            
            // Change to playing state
            SetState(GameState.Playing);
        }
        
        /// <summary>
        /// Ends the current game session
        /// </summary>
        public void EndGame()
        {
            if (currentState != GameState.Playing)
            {
                Debug.LogWarning($"GameManager: Cannot end game from {currentState} state");
                return;
            }
            
            Debug.Log("GameManager: Ending game");
            
            // Stop spawning ducks
            if (spawnManager != null)
            {
                spawnManager.StopSpawning();
            }
            
            // Change to game over state
            SetState(GameState.GameOver);
        }
        
        /// <summary>
        /// Restarts the game from game over state
        /// </summary>
        public void RestartGame()
        {
            if (currentState != GameState.GameOver)
            {
                Debug.LogWarning($"GameManager: Cannot restart game from {currentState} state");
                return;
            }
            
            Debug.Log("GameManager: Restarting game");
            
            // Reset to idle state first
            SetState(GameState.Idle);
            
            // Start a new game
            StartGame();
        }
        
        /// <summary>
        /// Increases the difficulty level and applies new settings
        /// </summary>
        public void IncreaseDifficulty()
        {
            if (gameConfig == null)
            {
                Debug.LogWarning("GameManager: Cannot increase difficulty - no game config assigned");
                return;
            }
            
            int maxLevel = gameConfig.MaxDifficultyLevel;
            if (currentDifficulty >= maxLevel)
            {
                Debug.Log($"GameManager: Already at maximum difficulty level ({maxLevel})");
                return;
            }
            
            currentDifficulty++;
            Debug.Log($"GameManager: Difficulty increased to level {currentDifficulty}");
            
            // Apply new difficulty settings
            ApplyDifficultySettings();
            
            // Notify listeners
            OnDifficultyChanged?.Invoke(currentDifficulty);
        }
        
        /// <summary>
        /// Resets difficulty to level 1
        /// </summary>
        private void ResetDifficulty()
        {
            currentDifficulty = 1;
            Debug.Log("GameManager: Difficulty reset to level 1");
            OnDifficultyChanged?.Invoke(currentDifficulty);
        }
        
        /// <summary>
        /// Applies current difficulty settings to the spawn manager
        /// </summary>
        private void ApplyDifficultySettings()
        {
            if (gameConfig == null || spawnManager == null)
            {
                return;
            }
            
            DifficultySettings settings = gameConfig.GetDifficultySettings(currentDifficulty);
            spawnManager.SetDifficulty(currentDifficulty);
            
            Debug.Log($"GameManager: Applied difficulty level {currentDifficulty} - " +
                     $"Spawn Interval: {settings.SpawnInterval}s, Duck Speed: {settings.DuckSpeed}");
        }
        
        /// <summary>
        /// Sets the current game state and notifies listeners
        /// </summary>
        /// <param name="newState">The new game state</param>
        private void SetState(GameState newState)
        {
            if (currentState == newState)
            {
                return;
            }
            
            GameState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"GameManager: State changed from {previousState} to {newState}");
            OnStateChanged?.Invoke(newState);
        }
        
        /// <summary>
        /// Handles score changes and checks for difficulty progression
        /// </summary>
        /// <param name="newScore">The updated score</param>
        private void OnScoreChanged(int newScore)
        {
            if (currentState != GameState.Playing || gameConfig == null)
            {
                return;
            }
            
            // Check if we should increase difficulty
            DifficultySettings nextDifficultySettings = gameConfig.GetDifficultySettings(currentDifficulty + 1);
            
            // Only increase if we haven't reached max difficulty and score threshold is met
            if (currentDifficulty < gameConfig.MaxDifficultyLevel && 
                newScore >= nextDifficultySettings.ScoreThreshold)
            {
                IncreaseDifficulty();
            }
        }
        
        /// <summary>
        /// Handles game over event from score manager
        /// </summary>
        private void OnGameOverTriggered()
        {
            if (currentState == GameState.Playing)
            {
                EndGame();
            }
        }
        
        /// <summary>
        /// Sets the game configuration (useful for runtime configuration changes)
        /// </summary>
        /// <param name="config">The new configuration to use</param>
        public void SetGameConfig(DuckHuntConfig config)
        {
            if (config != null)
            {
                gameConfig = config;
                
                // Update score manager with new config
                if (scoreManager != null)
                {
                    scoreManager.SetGameConfig(config);
                }
                
                Debug.Log("GameManager: Configuration updated");
            }
            else
            {
                Debug.LogWarning("GameManager: Attempted to set null configuration");
            }
        }
        
        /// <summary>
        /// Gets the current game configuration
        /// </summary>
        public DuckHuntConfig GetGameConfig()
        {
            return gameConfig;
        }
        
        #if UNITY_EDITOR
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            // Ensure difficulty is at least 1
            if (currentDifficulty < 1)
            {
                currentDifficulty = 1;
            }
        }
        #endif
    }
    
    /// <summary>
    /// Enumeration of possible game states
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Game is not active, waiting to start
        /// </summary>
        Idle,
        
        /// <summary>
        /// Game is actively running
        /// </summary>
        Playing,
        
        /// <summary>
        /// Game has ended, showing results
        /// </summary>
        GameOver
    }