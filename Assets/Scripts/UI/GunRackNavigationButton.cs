using UnityEngine;
using DuckHunt.VR;

/// <summary>
/// Navigation button for the gun display rack.
/// Handles VR interaction to navigate between pages.
/// Uses standardized VRInteractable base class.
/// </summary>
public class GunRackNavigationButton : VRInteractable
{
    [Tooltip("True for next/right button, false for previous/left button")]
    public bool isNextButton = true;

    private GunDisplayRack parentRack;
    private TMPro.TextMeshPro textComponent;
    private Color normalTextColor;
    private Color hoverTextColor = new Color(1f, 1f, 0.5f);

    protected override void Awake()
    {
        EnsureCollider();
        base.Awake();

        parentRack = GetComponentInParent<GunDisplayRack>();
        textComponent = GetComponent<TMPro.TextMeshPro>();
        if (textComponent != null)
        {
            normalTextColor = textComponent.color;
        }
    }

    private void EnsureCollider()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.12f, 0.15f, 0.05f);
            col.center = Vector3.zero;
        }
    }

    protected override void HandleActivation()
    {
        base.HandleActivation();
        TriggerNavigation();
    }

    protected override void HandleHoverStart()
    {
        base.HandleHoverStart();
        if (textComponent != null)
        {
            textComponent.color = hoverTextColor;
        }
    }

    protected override void HandleHoverEnd()
    {
        base.HandleHoverEnd();
        if (textComponent != null)
        {
            textComponent.color = normalTextColor;
        }
    }

    private void TriggerNavigation()
    {
        if (parentRack == null)
        {
            parentRack = GetComponentInParent<GunDisplayRack>();
        }

        if (parentRack != null)
        {
            if (isNextButton)
            {
                parentRack.NextPage();
            }
            else
            {
                parentRack.PreviousPage();
            }
        }
    }
}
