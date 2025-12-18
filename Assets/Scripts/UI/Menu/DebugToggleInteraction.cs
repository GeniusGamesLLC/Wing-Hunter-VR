using UnityEngine;
using UnityEngine.UI;
using DuckHunt.VR;

/// <summary>
/// Handles VR interaction for debug toggles on the announcement board.
/// Uses the standardized VRInteractable base class.
/// </summary>
[RequireComponent(typeof(Toggle))]
public class DebugToggleInteraction : VRInteractable
{
    private Toggle toggle;

    protected override void Awake()
    {
        toggle = GetComponent<Toggle>();
        base.Awake();
    }

    protected override void HandleActivation()
    {
        base.HandleActivation();

        if (toggle != null)
        {
            toggle.isOn = !toggle.isOn;
        }
    }
}
