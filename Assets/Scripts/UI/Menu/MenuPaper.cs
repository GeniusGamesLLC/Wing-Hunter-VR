using UnityEngine;

/// <summary>
/// Abstract base class for all menu papers on the Announcement Board.
/// Each paper represents a distinct menu section (Settings, Debug, etc.).
/// </summary>
public abstract class MenuPaper : MonoBehaviour
{
    [Header("Paper Configuration")]
    [SerializeField] protected string paperTitle;
    [SerializeField] protected Transform contentRoot;
    [SerializeField] protected bool isUnlockedByDefault = true;
    
    [Header("Animation")]
    [SerializeField] protected PaperAnimator paperAnimator;

    /// <summary>
    /// The display title of this paper.
    /// </summary>
    public string Title => paperTitle;

    /// <summary>
    /// Whether this paper is currently unlocked and visible on the board.
    /// </summary>
    public bool IsUnlocked { get; protected set; }

    /// <summary>
    /// The root transform where content UI elements are placed.
    /// </summary>
    public Transform ContentRoot => contentRoot;
    
    /// <summary>
    /// The paper animator component for focus/unfocus animations.
    /// </summary>
    public PaperAnimator Animator => paperAnimator;
    
    /// <summary>
    /// Whether Initialize has been called.
    /// </summary>
    protected bool isInitialized;

    /// <summary>
    /// Self-initialize if not already initialized by PaperManager.
    /// </summary>
    protected virtual void Start()
    {
        if (!isInitialized)
        {
            Debug.Log($"[MenuPaper] Self-initializing {paperTitle} (not initialized by PaperManager)");
            Initialize();
        }
    }

    /// <summary>
    /// Called when the paper is first created. Sets initial unlock state.
    /// </summary>
    public virtual void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;
        
        IsUnlocked = isUnlockedByDefault;
        
        // Try to get animator if not assigned
        if (paperAnimator == null)
        {
            paperAnimator = GetComponent<PaperAnimator>();
        }
        
        // Set up VR interaction on the paper background (child with collider)
        SetupInteraction();
        
        Debug.Log($"[MenuPaper] Initialized {paperTitle}, IsUnlocked={IsUnlocked}");
    }
    
    /// <summary>
    /// Sets up VR interaction for this paper.
    /// </summary>
    protected virtual void SetupInteraction()
    {
        // Find the child with a collider (typically PaperBackground)
        var collider = GetComponentInChildren<Collider>();
        if (collider != null)
        {
            var interaction = collider.GetComponent<PaperInteraction>();
            if (interaction == null)
            {
                interaction = collider.gameObject.AddComponent<PaperInteraction>();
            }
            interaction.SetMenuPaper(this);
            
            // Also set the PaperManager
            var paperManager = GetComponentInParent<PaperManager>();
            if (paperManager == null)
            {
                paperManager = FindObjectOfType<PaperManager>();
            }
            if (paperManager != null)
            {
                interaction.SetPaperManager(paperManager);
            }
            
            Debug.Log($"[MenuPaper] SetupInteraction for {paperTitle}: Collider={collider.name}, PaperManager={(paperManager != null ? "found" : "null")}");
        }
        else
        {
            Debug.LogWarning($"[MenuPaper] No collider found for {paperTitle}");
        }
    }

    /// <summary>
    /// Called when this paper becomes the focused/selected paper.
    /// Override to implement focus animations or state changes.
    /// </summary>
    public virtual void OnFocus()
    {
        // Play focus animation if animator exists
        if (paperAnimator != null)
        {
            paperAnimator.PlayFocusAnimation();
        }
    }

    /// <summary>
    /// Called when this paper loses focus to another paper.
    /// Override to implement unfocus animations or state changes.
    /// </summary>
    public virtual void OnUnfocus()
    {
        // Play unfocus animation if animator exists
        if (paperAnimator != null)
        {
            paperAnimator.PlayUnfocusAnimation();
        }
    }

    /// <summary>
    /// Refreshes the content displayed on this paper.
    /// Must be implemented by derived classes.
    /// </summary>
    public abstract void RefreshContent();

    /// <summary>
    /// Unlocks this paper, making it visible on the board.
    /// </summary>
    public void Unlock()
    {
        IsUnlocked = true;
    }
}
