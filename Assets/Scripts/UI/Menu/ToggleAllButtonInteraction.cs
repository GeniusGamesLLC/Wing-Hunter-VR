using UnityEngine;
using UnityEngine.UI;
using DuckHunt.VR;

/// <summary>
/// Handles VR interaction for Toggle All buttons on the debug paper.
/// Uses the standardized VRInteractable base class.
/// Prevents paper unfocus when interacting with buttons.
/// Requirements: 12.2
/// </summary>
[RequireComponent(typeof(Button))]
public class ToggleAllButtonInteraction : VRInteractable
{
    private Button button;
    private MenuPaper parentPaper;
    
    /// <summary>
    /// Flag indicating this interaction is currently being processed.
    /// Used to block raycast propagation to paper background.
    /// </summary>
    public static bool IsInteractionInProgress { get; private set; }

    protected override void Awake()
    {
        button = GetComponent<Button>();
        
        // Find parent paper for focus preservation
        parentPaper = GetComponentInParent<MenuPaper>();
        
        base.Awake();
    }

    protected override void HandleActivation()
    {
        // Set flag to block paper interaction during button activation
        IsInteractionInProgress = true;
        
        try
        {
            // Invoke the button click without triggering paper unfocus
            if (button != null)
            {
                button.onClick.Invoke();
            }
            
            // Call base to fire the OnActivated event
            base.HandleActivation();
        }
        finally
        {
            // Reset flag after activation
            IsInteractionInProgress = false;
        }
    }
    
    /// <summary>
    /// Override hover start to ensure paper stays focused.
    /// </summary>
    protected override void HandleHoverStart()
    {
        base.HandleHoverStart();
        
        // Ensure parent paper remains focused when hovering over button
        if (parentPaper != null && !parentPaper.IsFocused)
        {
            var paperManager = FindObjectOfType<PaperManager>();
            if (paperManager != null)
            {
                paperManager.FocusPaper(parentPaper);
            }
        }
    }
}
