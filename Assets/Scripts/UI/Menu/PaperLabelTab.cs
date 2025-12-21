using UnityEngine;
using TMPro;

/// <summary>
/// Small label/tag displayed above each paper showing its title.
/// Stays fixed on the board when the paper expands (doesn't move with paper).
/// Requirements: 13.1, 13.4
/// </summary>
public class PaperLabelTab : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshPro titleText;
    [SerializeField] private Transform tabVisual;
    
    [Header("Configuration")]
    [Tooltip("The paper this label tab belongs to")]
    [SerializeField] private MenuPaper associatedPaper;
    
    /// <summary>
    /// The fixed position on the board where this tab is pinned.
    /// This position doesn't change when the paper expands.
    /// </summary>
    private Vector3 pinnedPosition;
    
    /// <summary>
    /// Whether the pinned position has been set.
    /// </summary>
    private bool hasPinnedPosition;
    
    /// <summary>
    /// The title displayed on this label tab.
    /// </summary>
    public string Title => titleText != null ? titleText.text : string.Empty;
    
    /// <summary>
    /// The paper associated with this label tab.
    /// </summary>
    public MenuPaper AssociatedPaper => associatedPaper;
    
    /// <summary>
    /// The pinned position on the board.
    /// </summary>
    public Vector3 PinnedPosition => pinnedPosition;

    private void Awake()
    {
        // Try to find TextMeshPro if not assigned
        if (titleText == null)
        {
            titleText = GetComponentInChildren<TextMeshPro>();
        }
        
        // Use this transform as tab visual if not assigned
        if (tabVisual == null)
        {
            tabVisual = transform;
        }
    }

    /// <summary>
    /// Sets the title text displayed on the label tab.
    /// </summary>
    /// <param name="title">The title to display.</param>
    public void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }
    
    /// <summary>
    /// Sets the associated paper for this label tab.
    /// </summary>
    /// <param name="paper">The paper this tab belongs to.</param>
    public void SetAssociatedPaper(MenuPaper paper)
    {
        associatedPaper = paper;
        
        if (paper != null && titleText != null)
        {
            titleText.text = paper.Title;
        }
    }
    
    /// <summary>
    /// Sets the pinned position on the board.
    /// The tab will stay at this position even when the paper expands.
    /// </summary>
    /// <param name="position">The world or local position to pin to.</param>
    public void SetPinnedPosition(Vector3 position)
    {
        pinnedPosition = position;
        hasPinnedPosition = true;
        transform.localPosition = position;
    }
    
    /// <summary>
    /// Called when the associated paper gains focus.
    /// The tab stays in place (doesn't move with the paper).
    /// </summary>
    public void OnPaperFocused()
    {
        // Tab stays at pinned position - no movement
        // This ensures the label remains visible and readable
        // while the paper expands toward the user
        if (hasPinnedPosition)
        {
            transform.localPosition = pinnedPosition;
        }
    }
    
    /// <summary>
    /// Called when the associated paper loses focus.
    /// The tab stays in place (doesn't move with the paper).
    /// </summary>
    public void OnPaperUnfocused()
    {
        // Tab stays at pinned position - no movement
        if (hasPinnedPosition)
        {
            transform.localPosition = pinnedPosition;
        }
    }
    
    /// <summary>
    /// Sets the visibility of this label tab.
    /// </summary>
    /// <param name="visible">Whether the tab should be visible.</param>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    /// <summary>
    /// Updates the tab visibility based on the associated paper's unlock state.
    /// </summary>
    public void UpdateVisibilityFromPaper()
    {
        if (associatedPaper != null)
        {
            SetVisible(associatedPaper.IsUnlocked);
        }
    }
}
