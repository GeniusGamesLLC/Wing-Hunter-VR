using UnityEngine;

namespace DuckHunt.Data
{
    /// <summary>
    /// Component attached to intermediate waypoint prefabs for multi-point pathing.
    /// Manages indicator visibility and optional occluder placement.
    /// Similar to SpawnPointMarker but for intermediate waypoints.
    /// </summary>
    public class IntermediatePointMarker : MonoBehaviour
    {
        [Header("Point Configuration")]
        [SerializeField] private int pointIndex;

        [Header("References")]
        [SerializeField] private Transform indicator;
        [SerializeField] private Transform occluderSlot;

        private GameObject occluderInstance;

        /// <summary>
        /// The index of this intermediate point
        /// </summary>
        public int PointIndex
        {
            get => pointIndex;
            set => pointIndex = value;
        }

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

        private void OnEnable()
        {
            // Subscribe to DebugSettings changes
            if (DebugSettings.Instance != null)
            {
                DebugSettings.Instance.OnSettingsChanged += OnDebugSettingsChanged;
                // Apply initial state
                OnDebugSettingsChanged();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from DebugSettings changes
            if (DebugSettings.Instance != null)
            {
                DebugSettings.Instance.OnSettingsChanged -= OnDebugSettingsChanged;
            }
        }

        /// <summary>
        /// Called when DebugSettings change to update indicator visibility
        /// </summary>
        private void OnDebugSettingsChanged()
        {
            SetIndicatorVisible(DebugSettings.Instance.ShowWaypointIndicators);
        }

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
                Debug.LogWarning($"[IntermediatePointMarker] Indicator reference is missing on {gameObject.name}");
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
                Debug.LogWarning($"[IntermediatePointMarker] Attempted to set null occluder on {gameObject.name}");
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
                Debug.LogWarning($"[IntermediatePointMarker] OccluderSlot reference is missing on {gameObject.name}, using transform position");
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
