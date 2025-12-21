using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the collection of menu papers on the Announcement Board.
/// Handles paper navigation, focus state, visibility based on unlock status,
/// label tabs for each paper, and paper-to-player positioning.
/// Requirements: 1.3, 13.1, 13.3
/// </summary>
public class PaperManager : MonoBehaviour
{
    [Header("Paper Configuration")]
    [SerializeField] private List<MenuPaper> papers = new List<MenuPaper>();
    [SerializeField] private Transform paperSpawnParent;
    
    [Header("Label Tab Configuration")]
    [Tooltip("Prefab for the label tab displayed above each paper")]
    [SerializeField] private GameObject labelTabPrefab;
    [Tooltip("Y offset for label tabs above papers (in local space). Paper is 0.35 tall, so half is 0.175. Add gap for separation.")]
    [SerializeField] private float labelTabYOffset = 0.22f;
    
    [Header("Paper-to-Player Configuration")]
    [Tooltip("Whether focused papers should follow the player if they move")]
    [SerializeField] private bool paperFollowsPlayer = false;
    [Tooltip("How quickly the paper follows the player (if enabled)")]
    [SerializeField] private float paperFollowSpeed = 5f;
    
    [Header("Rendering")]
    [Tooltip("Layer manager for rendering focused papers on top of world geometry")]
    [SerializeField] private FocusedPaperLayerManager layerManager;
    
    /// <summary>
    /// Dictionary mapping papers to their label tabs.
    /// </summary>
    private Dictionary<MenuPaper, PaperLabelTab> paperLabelTabs = new Dictionary<MenuPaper, PaperLabelTab>();
    
    /// <summary>
    /// Dictionary storing each paper's home position on the board (world space).
    /// </summary>
    private Dictionary<MenuPaper, Vector3> paperHomePositions = new Dictionary<MenuPaper, Vector3>();
    
    /// <summary>
    /// Dictionary storing each paper's home rotation on the board (world space).
    /// </summary>
    private Dictionary<MenuPaper, Quaternion> paperHomeRotations = new Dictionary<MenuPaper, Quaternion>();

    /// <summary>
    /// The currently focused paper, or null if none is focused.
    /// </summary>
    public MenuPaper FocusedPaper { get; private set; }

    /// <summary>
    /// Returns all papers that are currently unlocked.
    /// </summary>
    public IReadOnlyList<MenuPaper> UnlockedPapers => 
        papers.Where(p => p != null && p.IsUnlocked).ToList();

    /// <summary>
    /// Returns all registered papers regardless of unlock state.
    /// </summary>
    public IReadOnlyList<MenuPaper> AllPapers => papers.AsReadOnly();

    /// <summary>
    /// Event fired when a paper gains focus.
    /// </summary>
    public event Action<MenuPaper> OnPaperFocused;

    /// <summary>
    /// Event fired when paper visibility changes (unlock/lock).
    /// </summary>
    public event Action OnPaperVisibilityChanged;

    private void Awake()
    {
        // Initialize layer manager if not assigned
        if (layerManager == null)
        {
            layerManager = GetComponent<FocusedPaperLayerManager>();
            if (layerManager == null)
            {
                layerManager = gameObject.AddComponent<FocusedPaperLayerManager>();
            }
        }
        
        InitializeAllPapers();
    }
    
    private void Update()
    {
        // If a paper is focused and follow mode is enabled, keep it in front of the player
        if (paperFollowsPlayer && FocusedPaper != null && FocusedPaper.MoveToPlayerOnFocus)
        {
            UpdateFocusedPaperPosition();
        }
    }
    
    /// <summary>
    /// Updates the focused paper's position to stay in front of the player.
    /// Only called if paperFollowsPlayer is enabled.
    /// </summary>
    private void UpdateFocusedPaperPosition()
    {
        if (FocusedPaper == null || FocusedPaper.PlayerCamera == null) return;
        
        var animator = FocusedPaper.Animator;
        if (animator != null && animator.IsAnimating) return; // Don't interrupt animations
        
        // Calculate target position in front of player
        Transform playerCamera = FocusedPaper.PlayerCamera;
        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        Vector3 targetPosition = playerCamera.position + cameraForward * FocusedPaper.FocusDistanceFromPlayer;
        targetPosition.y = playerCamera.position.y + FocusedPaper.FocusHeightOffset;
        
        // Calculate target rotation (facing player)
        Vector3 directionToPlayer = playerCamera.position - targetPosition;
        directionToPlayer.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer.normalized, Vector3.up);
        
        // Smoothly move paper to target position
        FocusedPaper.transform.position = Vector3.Lerp(
            FocusedPaper.transform.position, 
            targetPosition, 
            Time.deltaTime * paperFollowSpeed);
        FocusedPaper.transform.rotation = Quaternion.Slerp(
            FocusedPaper.transform.rotation, 
            targetRotation, 
            Time.deltaTime * paperFollowSpeed);
    }

    /// <summary>
    /// Initializes all registered papers and creates their label tabs.
    /// Also stores their home positions on the board.
    /// </summary>
    private void InitializeAllPapers()
    {
        foreach (var paper in papers)
        {
            if (paper != null)
            {
                paper.Initialize();
                CreateLabelTabForPaper(paper);
                StorePaperHomePosition(paper);
            }
        }
    }
    
    /// <summary>
    /// Stores the paper's current world position and rotation as its home position.
    /// </summary>
    private void StorePaperHomePosition(MenuPaper paper)
    {
        if (paper == null) return;
        
        paperHomePositions[paper] = paper.transform.position;
        paperHomeRotations[paper] = paper.transform.rotation;
    }
    
    /// <summary>
    /// Gets the stored home position for a paper.
    /// </summary>
    public Vector3 GetPaperHomePosition(MenuPaper paper)
    {
        if (paper != null && paperHomePositions.TryGetValue(paper, out Vector3 pos))
        {
            return pos;
        }
        return paper != null ? paper.transform.position : Vector3.zero;
    }
    
    /// <summary>
    /// Gets the stored home rotation for a paper.
    /// </summary>
    public Quaternion GetPaperHomeRotation(MenuPaper paper)
    {
        if (paper != null && paperHomeRotations.TryGetValue(paper, out Quaternion rot))
        {
            return rot;
        }
        return paper != null ? paper.transform.rotation : Quaternion.identity;
    }
    
    /// <summary>
    /// Creates a label tab for the specified paper.
    /// </summary>
    /// <param name="paper">The paper to create a label tab for.</param>
    private void CreateLabelTabForPaper(MenuPaper paper)
    {
        if (paper == null || labelTabPrefab == null)
        {
            return;
        }
        
        // Don't create duplicate tabs
        if (paperLabelTabs.ContainsKey(paper))
        {
            return;
        }
        
        // Instantiate the label tab
        GameObject tabObj = Instantiate(labelTabPrefab, transform);
        PaperLabelTab labelTab = tabObj.GetComponent<PaperLabelTab>();
        
        if (labelTab == null)
        {
            Debug.LogWarning($"PaperManager: LabelTab prefab missing PaperLabelTab component");
            Destroy(tabObj);
            return;
        }
        
        // Configure the label tab
        labelTab.SetAssociatedPaper(paper);
        labelTab.SetTitle(paper.Title);
        
        // Position the tab above the paper
        Vector3 paperLocalPos = paper.transform.localPosition;
        Vector3 tabPosition = new Vector3(paperLocalPos.x, paperLocalPos.y + labelTabYOffset, paperLocalPos.z);
        labelTab.SetPinnedPosition(tabPosition);
        
        // Set initial visibility based on paper unlock state
        labelTab.SetVisible(paper.IsUnlocked);
        
        // Store the mapping
        paperLabelTabs[paper] = labelTab;
        
        Debug.Log($"[PaperManager] Created label tab for '{paper.Title}' at position {tabPosition}");
    }

    /// <summary>
    /// Sets focus to the specified paper, or unfocuses if already focused (toggle).
    /// Unfocuses the currently focused paper if different.
    /// Also notifies label tabs of focus changes.
    /// </summary>
    /// <param name="paper">The paper to focus/unfocus. Must be unlocked.</param>
    public void FocusPaper(MenuPaper paper)
    {
        if (paper == null)
        {
            Debug.LogWarning("PaperManager: Cannot focus null paper.");
            return;
        }

        if (!paper.IsUnlocked)
        {
            Debug.LogWarning($"PaperManager: Cannot focus locked paper '{paper.Title}'.");
            return;
        }

        if (!papers.Contains(paper))
        {
            Debug.LogWarning($"PaperManager: Paper '{paper.Title}' is not registered.");
            return;
        }

        // Toggle: if already focused, unfocus it
        if (FocusedPaper == paper)
        {
            FocusedPaper.OnUnfocus();
            NotifyLabelTabUnfocused(FocusedPaper);
            
            // Move paper back to default layer for normal rendering
            if (layerManager != null)
            {
                layerManager.SetPaperUnfocused(FocusedPaper.transform);
            }
            
            FocusedPaper = null;
            OnPaperFocused?.Invoke(null);
            return;
        }

        // Unfocus current paper if different
        if (FocusedPaper != null)
        {
            FocusedPaper.OnUnfocus();
            NotifyLabelTabUnfocused(FocusedPaper);
            
            // Move previous paper back to default layer
            if (layerManager != null)
            {
                layerManager.SetPaperUnfocused(FocusedPaper.transform);
            }
        }

        // Focus new paper
        FocusedPaper = paper;
        paper.OnFocus();
        NotifyLabelTabFocused(paper);
        
        // Move paper to focused layer for rendering on top
        if (layerManager != null)
        {
            layerManager.SetPaperFocused(paper.transform);
        }
        
        OnPaperFocused?.Invoke(paper);
    }
    
    /// <summary>
    /// Notifies the label tab that its paper has been focused.
    /// </summary>
    private void NotifyLabelTabFocused(MenuPaper paper)
    {
        if (paper != null && paperLabelTabs.TryGetValue(paper, out PaperLabelTab tab))
        {
            tab.OnPaperFocused();
        }
    }
    
    /// <summary>
    /// Notifies the label tab that its paper has been unfocused.
    /// </summary>
    private void NotifyLabelTabUnfocused(MenuPaper paper)
    {
        if (paper != null && paperLabelTabs.TryGetValue(paper, out PaperLabelTab tab))
        {
            tab.OnPaperUnfocused();
        }
    }

    /// <summary>
    /// Adds a new paper to the manager and creates its label tab.
    /// </summary>
    /// <param name="paper">The paper to add.</param>
    public void AddPaper(MenuPaper paper)
    {
        if (paper == null)
        {
            Debug.LogWarning("PaperManager: Cannot add null paper.");
            return;
        }

        if (papers.Contains(paper))
        {
            Debug.LogWarning($"PaperManager: Paper '{paper.Title}' is already registered.");
            return;
        }

        papers.Add(paper);
        paper.Initialize();
        CreateLabelTabForPaper(paper);
        StorePaperHomePosition(paper);
        
        RefreshPaperVisibility();
    }

    /// <summary>
    /// Removes a paper from the manager and destroys its label tab.
    /// </summary>
    /// <param name="paper">The paper to remove.</param>
    public void RemovePaper(MenuPaper paper)
    {
        if (paper == null) return;

        if (FocusedPaper == paper)
        {
            FocusedPaper.OnUnfocus();
            FocusedPaper = null;
        }

        // Remove and destroy the label tab
        RemoveLabelTabForPaper(paper);
        
        // Remove home position tracking
        paperHomePositions.Remove(paper);
        paperHomeRotations.Remove(paper);

        papers.Remove(paper);
        RefreshPaperVisibility();
    }
    
    /// <summary>
    /// Removes and destroys the label tab for the specified paper.
    /// </summary>
    private void RemoveLabelTabForPaper(MenuPaper paper)
    {
        if (paper != null && paperLabelTabs.TryGetValue(paper, out PaperLabelTab tab))
        {
            if (tab != null)
            {
                Destroy(tab.gameObject);
            }
            paperLabelTabs.Remove(paper);
        }
    }

    /// <summary>
    /// Refreshes the visibility of all papers and their label tabs based on unlock state.
    /// Shows unlocked papers/tabs and hides locked ones.
    /// </summary>
    public void RefreshPaperVisibility()
    {
        foreach (var paper in papers)
        {
            if (paper != null)
            {
                paper.gameObject.SetActive(paper.IsUnlocked);
            }
        }
        
        // Update label tab visibility
        UpdateLabelTabVisibility();

        OnPaperVisibilityChanged?.Invoke();
    }
    
    /// <summary>
    /// Updates the visibility of all label tabs based on their associated paper's unlock state.
    /// </summary>
    private void UpdateLabelTabVisibility()
    {
        foreach (var kvp in paperLabelTabs)
        {
            MenuPaper paper = kvp.Key;
            PaperLabelTab tab = kvp.Value;
            
            if (tab != null && paper != null)
            {
                tab.SetVisible(paper.IsUnlocked);
            }
        }
    }

    /// <summary>
    /// Gets the first unlocked paper, useful for setting default focus.
    /// </summary>
    /// <returns>The first unlocked paper, or null if none are unlocked.</returns>
    public MenuPaper GetFirstUnlockedPaper()
    {
        return papers.FirstOrDefault(p => p != null && p.IsUnlocked);
    }

    /// <summary>
    /// Finds a paper by its title.
    /// </summary>
    /// <param name="title">The title to search for.</param>
    /// <returns>The paper with the matching title, or null if not found.</returns>
    public MenuPaper GetPaperByTitle(string title)
    {
        return papers.FirstOrDefault(p => p != null && p.Title == title);
    }
}
