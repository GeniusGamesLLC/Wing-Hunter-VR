using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

/// <summary>
/// Navigation button for the gun display rack.
/// Handles VR interaction to navigate between pages.
/// Uses same interaction pattern as ButtonEventWirer (hover + trigger).
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class GunRackNavigationButton : MonoBehaviour
{
    [Tooltip("True for next/right button, false for previous/left button")]
    public bool isNextButton = true;
    
    private GunDisplayRack parentRack;
    private bool isHovered = false;
    private bool wasTriggered = false;
    private float triggerCooldown = 0f;
    private TMPro.TextMeshPro textComponent;
    private XRSimpleInteractable interactable;
    private Color normalColor;
    private Color hoverColor = new Color(1f, 1f, 0.5f);
    
    private void Start()
    {
        parentRack = GetComponentInParent<GunDisplayRack>();
        
        textComponent = GetComponent<TMPro.TextMeshPro>();
        if (textComponent != null)
        {
            normalColor = textComponent.color;
        }
        
        SetupXRInteraction();
    }
    
    private void SetupXRInteraction()
    {
        // Ensure we have a collider for XR interaction
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.12f, 0.15f, 0.05f);
            col.center = Vector3.zero;
        }
        
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }
        
        // Use activated (grip+trigger) and hover events
        interactable.activated.AddListener(OnActivated);
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        
        Debug.Log($"GunRackNavigationButton: Setup complete for {(isNextButton ? "Next" : "Previous")} button");
    }
    
    private void OnActivated(ActivateEventArgs args)
    {
        // Prevent double-activation
        if (triggerCooldown > 0f) return;
        
        triggerCooldown = 0.3f;
        TriggerNavigation();
    }
    
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
        if (textComponent != null)
        {
            textComponent.color = hoverColor;
        }
        Debug.Log($"GunRackNavigationButton: Hover entered on {(isNextButton ? "Next" : "Previous")}");
    }
    
    private void OnHoverExited(HoverExitEventArgs args)
    {
        // Only set to false if no interactors are hovering
        if (interactable != null && interactable.interactorsHovering.Count == 0)
        {
            isHovered = false;
        }
        if (textComponent != null)
        {
            textComponent.color = normalColor;
        }
    }
    
    private void Update()
    {
        // Handle cooldown
        if (triggerCooldown > 0f)
        {
            triggerCooldown -= Time.deltaTime;
            return;
        }
        
        // Check for trigger press while hovering (trigger-only, no grip required)
        if (isHovered && !wasTriggered)
        {
            if (CheckTriggerPressed())
            {
                wasTriggered = true;
                triggerCooldown = 0.3f;
                TriggerNavigation();
            }
        }
        
        // Reset trigger state when released
        if (wasTriggered && !CheckTriggerPressed())
        {
            wasTriggered = false;
        }
    }
    
    private bool CheckTriggerPressed()
    {
        // Check right hand
        var rightDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightDevices);
        foreach (var device in rightDevices)
        {
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
            {
                if (triggerValue > 0.5f) return true;
            }
        }
        
        // Check left hand
        var leftDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftDevices);
        foreach (var device in leftDevices)
        {
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
            {
                if (triggerValue > 0.5f) return true;
            }
        }
        
        return false;
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
                Debug.Log("GunRackNavigationButton: Next page");
                parentRack.NextPage();
            }
            else
            {
                Debug.Log("GunRackNavigationButton: Previous page");
                parentRack.PreviousPage();
            }
        }
    }
    
    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.activated.RemoveListener(OnActivated);
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }
    }
}
