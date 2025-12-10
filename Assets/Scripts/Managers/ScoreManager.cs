using System;
using UnityEngine;
using DuckHunt.Data;

namespace DuckHunt.Managers
{
    /// <summary>
    /// Manages the player's score, missed duck count, and game over conditions
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DuckHuntConfig gameConfig;
        
        [Header("Debug Info")]
        [SerializeField, ReadOnly] private int currentScore;
        [SerializeField, ReadOnly] private int missedDucks;
        [SerializeField, ReadOnly] private int maxMissedDucks;
        
        // Public properties
        public int CurrentScore => currentScore;
        public int MissedDucks => missedDucks;
        public int MaxMissedDucks 
        { 
            get => maxMissedDucks; 
            set => maxMissedDucks = Mathf.Max(0, value); 
        }
        
        // Events
        public event Action<int> OnScoreChanged;
        public event Action OnGameOver;
        
        private void Awake()
        {
            // Initialize with config values if available
            if (gameConfig != null)
            {
                maxMissedDucks = gameConfig.MaxMissedDucks;
            }
            else
            {
                // Default fallback value
                maxMissedDucks = 10;
                Debug.LogWarning("ScoreManager: No DuckHuntConfig assigned, using default max missed ducks value");
            }
        }
        
        private void Start()
        {
            // Initialize score tracking
            ResetScore();
        }
        
        /// <summary>
        /// Adds points to the current score
        /// </summary>
        /// <param name="points">Points to add (must be positive)</param>
        public void AddScore(int points)
        {
            if (points < 0)
            {
                Debug.LogWarning($"ScoreManager: Attempted to add negative points ({points}). Ignoring.");
                return;
            }
            
            // Prevent integer overflow
            if (currentScore > int.MaxValue - points)
            {
                currentScore = int.MaxValue;
                Debug.LogWarning("ScoreManager: Score reached maximum value");
            }
            else
            {
                currentScore += points;
            }
            
            // Notify listeners of score change
            OnScoreChanged?.Invoke(currentScore);
            
            Debug.Log($"ScoreManager: Score increased by {points}. Total score: {currentScore}");
        }
        
        /// <summary>
        /// Increments the missed duck counter and checks for game over condition
        /// </summary>
        public void IncrementMissed()
        {
            missedDucks++;
            
            Debug.Log($"ScoreManager: Duck missed. Total missed: {missedDucks}/{maxMissedDucks}");
            
            // Check for game over condition
            if (missedDucks >= maxMissedDucks)
            {
                TriggerGameOver();
            }
        }
        
        /// <summary>
        /// Resets the score and missed duck count to initial values
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            missedDucks = 0;
            
            // Notify listeners of score reset
            OnScoreChanged?.Invoke(currentScore);
            
            Debug.Log("ScoreManager: Score and missed duck count reset to zero");
        }
        
        /// <summary>
        /// Triggers the game over condition
        /// </summary>
        private void TriggerGameOver()
        {
            Debug.Log($"ScoreManager: Game Over triggered! Final score: {currentScore}, Missed ducks: {missedDucks}");
            OnGameOver?.Invoke();
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
                maxMissedDucks = config.MaxMissedDucks;
                Debug.Log($"ScoreManager: Configuration updated. Max missed ducks: {maxMissedDucks}");
            }
            else
            {
                Debug.LogWarning("ScoreManager: Attempted to set null configuration");
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
            // Ensure max missed ducks is positive
            if (maxMissedDucks <= 0)
            {
                maxMissedDucks = 1;
            }
            
            // Ensure current values are non-negative
            currentScore = Mathf.Max(0, currentScore);
            missedDucks = Mathf.Max(0, missedDucks);
        }
        #endif
    }
    
    /// <summary>
    /// Attribute to make fields read-only in the inspector
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}