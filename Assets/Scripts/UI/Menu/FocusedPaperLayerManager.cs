using UnityEngine;

/// <summary>
/// Simplified paper layer manager - no longer uses camera stacking.
/// 
/// The previous approach used overlay cameras to render papers on top of world geometry,
/// but this caused issues with rays rendering on top of everything.
/// 
/// New approach: Rely on natural depth sorting by positioning the focused paper
/// close enough to the player that it's naturally in front of world geometry.
/// Controllers, guns, and rays will naturally render on top of the paper at the correct depth.
/// </summary>
public class FocusedPaperLayerManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    /// <summary>
    /// Whether the system is initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes the layer manager (now a no-op since we use natural depth).
    /// </summary>
    public void Initialize()
    {
        if (IsInitialized) return;
        
        IsInitialized = true;
        
        if (debugMode)
        {
            Debug.Log("[FocusedPaperLayerManager] Initialized - using natural depth sorting (no camera stacking)");
        }
    }

    /// <summary>
    /// Called when a paper gains focus. No longer changes layers.
    /// </summary>
    public void SetPaperFocused(Transform paper)
    {
        if (!IsInitialized || paper == null) return;
        
        if (debugMode)
        {
            Debug.Log($"[FocusedPaperLayerManager] Paper '{paper.name}' focused - using natural depth sorting");
        }
    }

    /// <summary>
    /// Called when a paper loses focus. No longer changes layers.
    /// </summary>
    public void SetPaperUnfocused(Transform paper)
    {
        if (!IsInitialized || paper == null) return;
        
        if (debugMode)
        {
            Debug.Log($"[FocusedPaperLayerManager] Paper '{paper.name}' unfocused");
        }
    }
}
