using UnityEngine;

namespace DuckHunt.Data
{
    /// <summary>
    /// Data structure containing all information needed to spawn a duck
    /// </summary>
    [System.Serializable]
    public struct DuckSpawnData
    {
        [Header("Position Settings")]
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        
        [Header("Movement Settings")]
        public float Speed;
        
        [Header("Scoring")]
        public int PointValue;
        
        /// <summary>
        /// Creates a new DuckSpawnData with the specified parameters
        /// </summary>
        public DuckSpawnData(Vector3 startPosition, Vector3 endPosition, float speed, int pointValue)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Speed = speed;
            PointValue = pointValue;
        }
    }
}