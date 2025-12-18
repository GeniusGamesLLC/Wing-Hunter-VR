using System;
using UnityEngine;

namespace DuckHunt.Data
{
    /// <summary>
    /// Centralized runtime-accessible debug settings that can be toggled during gameplay
    /// and integrated with a future settings menu.
    /// Implements singleton pattern for global access.
    /// </summary>
    public class DebugSettings : MonoBehaviour
    {
        private static DebugSettings _instance;
        
        /// <summary>
        /// Singleton instance. Creates a new GameObject if none exists.
        /// </summary>
        public static DebugSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DebugSettings>();
                    
                    if (_instance == null)
                    {
                        Debug.Log("[DebugSettings] No instance found in scene, creating one.");
                        GameObject go = new GameObject("DebugSettings");
                        _instance = go.AddComponent<DebugSettings>();
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Event fired when any debug setting changes.
        /// Subscribe to this to update visualization state.
        /// </summary>
        public event Action OnSettingsChanged;
        
        [Header("Path Visualization")]
        [Tooltip("Show spline paths for duck flight trajectories")]
        [SerializeField] private bool _showSplinePaths = false;
        
        [Tooltip("Show intermediate waypoint indicators")]
        [SerializeField] private bool _showWaypointIndicators = false;
        
        [Header("Point Indicators")]
        [Tooltip("Show spawn point indicators")]
        [SerializeField] private bool _showSpawnPointIndicators = false;
        
        [Tooltip("Show target point indicators")]
        [SerializeField] private bool _showTargetPointIndicators = false;
        
        [Header("UI Debug")]
        [Tooltip("Show interaction hitboxes with state colors (normal/hover/activated)")]
        [SerializeField] private bool _showInteractionHitboxes = false;
        
        [Tooltip("Material used for debug hitbox visualization (transparent, color-tintable)")]
        [SerializeField] private Material _hitboxMaterial;
        
        [Tooltip("Hitbox color when not hovered")]
        [SerializeField] private Color _hitboxNormalColor = new Color(0f, 1f, 0f, 0.3f);
        
        [Tooltip("Hitbox color when hovered")]
        [SerializeField] private Color _hitboxHoverColor = new Color(1f, 1f, 0f, 0.5f);
        
        [Tooltip("Hitbox color when activated (trigger pressed)")]
        [SerializeField] private Color _hitboxActivatedColor = new Color(1f, 0f, 0f, 0.7f);
        
        /// <summary>
        /// Whether to show spline paths for duck flight trajectories
        /// </summary>
        [DebugCategory("Path Visualization")]
        public bool ShowSplinePaths
        {
            get => _showSplinePaths;
            set
            {
                if (_showSplinePaths != value)
                {
                    _showSplinePaths = value;
                    NotifySettingsChanged();
                }
            }
        }
        
        /// <summary>
        /// Whether to show intermediate waypoint indicators
        /// </summary>
        [DebugCategory("Path Visualization")]
        public bool ShowWaypointIndicators
        {
            get => _showWaypointIndicators;
            set
            {
                if (_showWaypointIndicators != value)
                {
                    _showWaypointIndicators = value;
                    NotifySettingsChanged();
                }
            }
        }
        
        /// <summary>
        /// Whether to show spawn point indicators
        /// </summary>
        [DebugCategory("Point Indicators")]
        public bool ShowSpawnPointIndicators
        {
            get => _showSpawnPointIndicators;
            set
            {
                if (_showSpawnPointIndicators != value)
                {
                    _showSpawnPointIndicators = value;
                    NotifySettingsChanged();
                }
            }
        }
        
        /// <summary>
        /// Whether to show target point indicators
        /// </summary>
        [DebugCategory("Point Indicators")]
        public bool ShowTargetPointIndicators
        {
            get => _showTargetPointIndicators;
            set
            {
                if (_showTargetPointIndicators != value)
                {
                    _showTargetPointIndicators = value;
                    NotifySettingsChanged();
                }
            }
        }
        
        /// <summary>
        /// Whether to show interaction hitboxes with state colors
        /// </summary>
        [DebugCategory("UI Debug")]
        public bool ShowInteractionHitboxes
        {
            get => _showInteractionHitboxes;
            set
            {
                if (_showInteractionHitboxes != value)
                {
                    _showInteractionHitboxes = value;
                    NotifySettingsChanged();
                }
            }
        }
        
        /// <summary>
        /// Shared material for debug hitbox visualization.
        /// Assign once in the scene's DebugSettings component.
        /// </summary>
        public Material HitboxMaterial => _hitboxMaterial;
        
        /// <summary>Hitbox color when not hovered.</summary>
        public Color HitboxNormalColor => _hitboxNormalColor;
        
        /// <summary>Hitbox color when hovered.</summary>
        public Color HitboxHoverColor => _hitboxHoverColor;
        
        /// <summary>Hitbox color when activated.</summary>
        public Color HitboxActivatedColor => _hitboxActivatedColor;
        
        private void Awake()
        {
            // Ensure singleton pattern
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[DebugSettings] Duplicate instance found, destroying this one.");
                Destroy(this);
                return;
            }
            
            _instance = this;
            Debug.Log("[DebugSettings] Instance initialized.");
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Called when Inspector values change. Fires the settings changed event.
        /// </summary>
        private void OnValidate()
        {
            // Only notify during play mode to avoid editor-time issues
            if (Application.isPlaying)
            {
                // Use DelayCall to avoid issues with OnValidate timing
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        NotifySettingsChanged();
                    }
                };
            }
        }
#endif
        
        /// <summary>
        /// Sets all visualization options to the specified state
        /// </summary>
        /// <param name="enabled">True to enable all visualizations, false to disable</param>
        public void SetAllVisualization(bool enabled)
        {
            bool changed = false;
            
            if (_showSplinePaths != enabled)
            {
                _showSplinePaths = enabled;
                changed = true;
            }
            
            if (_showWaypointIndicators != enabled)
            {
                _showWaypointIndicators = enabled;
                changed = true;
            }
            
            if (_showSpawnPointIndicators != enabled)
            {
                _showSpawnPointIndicators = enabled;
                changed = true;
            }
            
            if (_showTargetPointIndicators != enabled)
            {
                _showTargetPointIndicators = enabled;
                changed = true;
            }
            
            if (_showInteractionHitboxes != enabled)
            {
                _showInteractionHitboxes = enabled;
                changed = true;
            }
            
            if (changed)
            {
                NotifySettingsChanged();
            }
        }
        
        /// <summary>
        /// Notifies all subscribers that settings have changed
        /// </summary>
        public void NotifySettingsChanged()
        {
            OnSettingsChanged?.Invoke();
            Debug.Log($"[DebugSettings] Settings changed - SplinePaths: {_showSplinePaths}, Waypoints: {_showWaypointIndicators}, SpawnPoints: {_showSpawnPointIndicators}, TargetPoints: {_showTargetPointIndicators}");
        }
    }
}
