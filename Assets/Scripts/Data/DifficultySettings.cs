using UnityEngine;

namespace DuckHunt.Data
{
    /// <summary>
    /// Configuration settings for a specific difficulty level
    /// </summary>
    [System.Serializable]
    public class DifficultySettings
    {
        [Header("Difficulty Configuration")]
        [Tooltip("The difficulty level number (1, 2, 3, etc.)")]
        public int Level = 1;
        
        [Header("Spawn Settings")]
        [Tooltip("Time interval between duck spawns in seconds")]
        [Range(0.5f, 5.0f)]
        public float SpawnInterval = 2.0f;
        
        [Header("Movement Settings")]
        [Tooltip("Speed at which ducks move across the scene")]
        [Range(1.0f, 20.0f)]
        public float DuckSpeed = 5.0f;
        
        [Header("Progression")]
        [Tooltip("Score threshold required to reach this difficulty level")]
        public int ScoreThreshold = 0;
        
        /// <summary>
        /// Creates a new DifficultySettings with default values
        /// </summary>
        public DifficultySettings()
        {
            Level = 1;
            SpawnInterval = 2.0f;
            DuckSpeed = 5.0f;
            ScoreThreshold = 0;
        }
        
        /// <summary>
        /// Creates a new DifficultySettings with specified parameters
        /// </summary>
        public DifficultySettings(int level, float spawnInterval, float duckSpeed, int scoreThreshold)
        {
            Level = level;
            SpawnInterval = spawnInterval;
            DuckSpeed = duckSpeed;
            ScoreThreshold = scoreThreshold;
        }
    }
}