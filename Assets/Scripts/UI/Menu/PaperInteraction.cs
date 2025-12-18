using UnityEngine;
using DuckHunt.VR;

/// <summary>
/// Handles VR interaction for menu papers.
/// Uses the standardized VRInteractable base class.
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
        base.HandleActivation();

        // Focus/unfocus this paper when activated
        if (menuPaper != null && paperManager != null)
        {
            paperManager.FocusPaper(menuPaper);
        }
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
