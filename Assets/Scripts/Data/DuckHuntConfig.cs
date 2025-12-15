using UnityEngine;
    /// <summary>
    /// ScriptableObject containing all configuration settings for the Duck Hunt game
    /// </summary>
    [CreateAssetMenu(fileName = "DuckHuntConfig", menuName = "Duck Hunt/Game Configuration")]
    public class DuckHuntConfig : ScriptableObject
    {
        [Header("Scoring Configuration")]
        [Tooltip("Points awarded for hitting a duck")]
        [Range(1, 100)]
        public int PointsPerDuck = 10;
        
        [Tooltip("Maximum number of ducks that can be missed before game over")]
        [Range(1, 20)]
        public int MaxMissedDucks = 10;
        
        [Header("Shooting Configuration")]
        [Tooltip("Maximum distance for raycast shooting")]
        [Range(10f, 200f)]
        public float RaycastDistance = 100f;
        
        [Header("Difficulty Progression")]
        [Tooltip("Array of difficulty settings for progressive gameplay")]
        public DifficultySettings[] DifficultyLevels = new DifficultySettings[]
        {
            new DifficultySettings(1, 3.0f, 4.0f, 0),      // Level 1: Easy start
            new DifficultySettings(2, 2.5f, 5.0f, 50),     // Level 2: Slightly faster
            new DifficultySettings(3, 2.0f, 6.0f, 120),    // Level 3: More challenging
            new DifficultySettings(4, 1.5f, 7.5f, 200),    // Level 4: Fast paced
            new DifficultySettings(5, 1.0f, 9.0f, 300)     // Level 5: Expert level
        };
        
        [Header("Audio Configuration")]
        [Tooltip("Volume level for sound effects")]
        [Range(0f, 1f)]
        public float SoundEffectVolume = 0.8f;
        
        [Tooltip("Intensity of haptic feedback")]
        [Range(0f, 1f)]
        public float HapticIntensity = 0.6f;
        
        [Header("Performance Configuration")]
        [Tooltip("Maximum number of ducks that can be active at once")]
        [Range(3, 20)]
        public int MaxConcurrentDucks = 10;
        
        [Tooltip("Target frame rate for VR (72 for Quest)")]
        [Range(60, 120)]
        public int TargetFrameRate = 72;
        
        /// <summary>
        /// Gets the difficulty settings for a specific level
        /// </summary>
        /// <param name="level">The difficulty level to retrieve</param>
        /// <returns>DifficultySettings for the specified level, or the highest level if out of range</returns>
        public DifficultySettings GetDifficultySettings(int level)
        {
            if (DifficultyLevels == null || DifficultyLevels.Length == 0)
            {
                Debug.LogWarning("No difficulty levels configured, returning default settings");
                return new DifficultySettings();
            }
            
            // Clamp level to available difficulty settings
            int clampedLevel = Mathf.Clamp(level - 1, 0, DifficultyLevels.Length - 1);
            return DifficultyLevels[clampedLevel];
        }
        
        /// <summary>
        /// Gets the maximum difficulty level available
        /// </summary>
        public int MaxDifficultyLevel => DifficultyLevels?.Length ?? 1;
        
        /// <summary>
        /// Validates the configuration settings
        /// </summary>
        private void OnValidate()
        {
            // Ensure we have at least one difficulty level
            if (DifficultyLevels == null || DifficultyLevels.Length == 0)
            {
                DifficultyLevels = new DifficultySettings[] { new DifficultySettings() };
            }
            
            // Ensure difficulty levels are properly ordered
            for (int i = 0; i < DifficultyLevels.Length; i++)
            {
                if (DifficultyLevels[i] != null)
                {
                    DifficultyLevels[i].Level = i + 1;
                }
            }
        }
    }