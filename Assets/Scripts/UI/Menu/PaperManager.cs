using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the collection of menu papers on the Announcement Board.
/// Handles paper navigation, focus state, and visibility based on unlock status.
/// </summary>
public class PaperManager : MonoBehaviour
{
    [Header("Paper Configuration")]
    [SerializeField] private List<MenuPaper> papers = new List<MenuPaper>();
    [SerializeField] private Transform paperSpawnParent;

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
        InitializeAllPapers();
    }

    /// <summary>
    /// Initializes all registered papers.
    /// </summary>
    private void InitializeAllPapers()
    {
        foreach (var paper in papers)
        {
            if (paper != null)
            {
                paper.Initialize();
            }
        }
    }

    /// <summary>
    /// Sets focus to the specified paper, or unfocuses if already focused (toggle).
    /// Unfocuses the currently focused paper if different.
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
            FocusedPaper = null;
            OnPaperFocused?.Invoke(null);
            return;
        }

        // Unfocus current paper if different
        if (FocusedPaper != null)
        {
            FocusedPaper.OnUnfocus();
        }

        // Focus new paper
        FocusedPaper = paper;
        paper.OnFocus();
        
        OnPaperFocused?.Invoke(paper);
    }

    /// <summary>
    /// Adds a new paper to the manager.
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
        
        RefreshPaperVisibility();
    }

    /// <summary>
    /// Removes a paper from the manager.
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

        papers.Remove(paper);
        RefreshPaperVisibility();
    }

    /// <summary>
    /// Refreshes the visibility of all papers based on their unlock state.
    /// Shows unlocked papers and hides locked ones.
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

        OnPaperVisibilityChanged?.Invoke();
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
