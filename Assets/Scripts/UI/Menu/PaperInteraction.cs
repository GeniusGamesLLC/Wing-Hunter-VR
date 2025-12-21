using UnityEngine;
using DuckHunt.VR;

/// <summary>
/// Handles VR interaction for menu papers.
/// Uses the standardized VRInteractable base class.
/// Ignores clicks on interactive elements (toggles, buttons) to preserve focus.
/// Requirements: 12.3
/// </summary>
public class PaperInteraction : VRInteractable
{
    [Header("Paper References")]
    [SerializeField] private MenuPaper menuPaper;
    [SerializeField] private PaperManager paperManager;

    protected override void Awake()
    {
        base.Awake();

        // Auto-find references if not assigned
        if (menuPaper == null)
        {
            menuPaper = GetComponent<MenuPaper>() ?? GetComponentInParent<MenuPaper>();
        }

        if (paperManager == null)
        {
            paperManager = FindObjectOfType<PaperManager>();
        }
    }

    protected override void HandleActivation()
    {
        // Check if an interactive element (toggle or button) is currently being interacted with
        // If so, ignore this paper activation to preserve focus
        if (IsInteractiveElementActive())
        {
            return;
        }
        
        base.HandleActivation();

        // Focus/unfocus this paper when activated
        if (menuPaper != null && paperManager != null)
        {
            paperManager.FocusPaper(menuPaper);
        }
    }
    
    /// <summary>
    /// Checks if any interactive element (toggle or button) is currently being interacted with.
    /// This prevents paper unfocus when clicking on toggles or buttons.
    /// </summary>
    /// <returns>True if an interactive element is active, false otherwise.</returns>
    private bool IsInteractiveElementActive()
    {
        // Check if a debug toggle interaction is in progress
        if (DebugToggleInteraction.IsInteractionInProgress)
        {
            return true;
        }
        
        // Check if a toggle all button interaction is in progress
        if (ToggleAllButtonInteraction.IsInteractionInProgress)
        {
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Sets the menu paper reference.
    /// </summary>
    public void SetMenuPaper(MenuPaper paper)
    {
        menuPaper = paper;
    }

    /// <summary>
    /// Sets the paper manager reference.
    /// </summary>
    public void SetPaperManager(PaperManager manager)
    {
        paperManager = manager;
    }
}
