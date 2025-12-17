using UnityEngine;

namespace DuckHunt.Data
{
    /// <summary>
    /// Defines the type of spawn point
    /// </summary>
    public enum SpawnPointType
    {
        Spawn,
        Target
    }

    /// <summary>
    /// Component attached to spawn point prefabs to identify and configure them.
    /// Manages indicator visibility and optional occluder placement.
    /// </summary>
    public class SpawnPointMarker : MonoBehaviour
    {
        [Header("Point Configuration")]
        [SerializeField] private int pointIndex;
        [SerializeField] private SpawnPointType pointType;

        [Header("References")]
        [SerializeField] private Transform indicator;
        [SerializeField] private Transform occluderSlot;

        private GameObject occluderInstance;

        /// <summary>
        /// The index of this point, used for pairing spawn/target points
        /// </summary>
        public int PointIndex
        {
            get => pointIndex;
            set => pointIndex = value;
        }

        /// <summary>
        /// The type of this point (Spawn or Target)
        /// </summary>
        public SpawnPointType PointType => pointType;

        /// <summary>
        /// Reference to the indicator child transform
        /// </summary>
        public Transform Indicator => indicator;

        /// <summary>
        /// Reference to the occluder slot transform
        /// </summary>
        public Transform OccluderSlot => occluderSlot;

        /// <summary>
        /// The currently instantiated occluder, if any
        /// </summary>
        public GameObject OccluderInstance => occluderInstance;

        /// <summary>
        /// Shows or hides the indicator visual
        /// </summary>
        /// <param name="visible">True to show, false to hide</param>
        public void SetIndicatorVisible(bool visible)
        {
            if (indicator != null)
            {
                indicator.gameObject.SetActive(visible);
            }
            else
            {
                Debug.LogWarning($"[SpawnPointMarker] Indicator reference is missing on {gameObject.name}");
            }
        }

        /// <summary>
        /// Instantiates an occluder prefab at the occluder slot position
        /// </summary>
        /// <param name="occluderPrefab">The prefab to instantiate</param>
        public void SetOccluder(GameObject occluderPrefab)
        {
            if (occluderPrefab == null)
            {
                Debug.LogWarning($"[SpawnPointMarker] Attempted to set null occluder on {gameObject.name}");
                return;
            }

            // Clear existing occluder first
            ClearOccluder();

            if (occluderSlot != null)
            {
                occluderInstance = Instantiate(occluderPrefab, occluderSlot.position, occluderSlot.rotation, occluderSlot);
            }
            else
            {
                // Fall back to spawning at this transform's position
                occluderInstance = Instantiate(occluderPrefab, transform.position, transform.rotation, transform);
                Debug.LogWarning($"[SpawnPointMarker] OccluderSlot reference is missing on {gameObject.name}, using transform position");
            }
        }

        /// <summary>
        /// Destroys the current occluder instance if one exists
        /// </summary>
        public void ClearOccluder()
        {
            if (occluderInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(occluderInstance);
                }
                else
                {
                    DestroyImmediate(occluderInstance);
                }
                occluderInstance = null;
            }
        }
    }
}
