using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

/// <summary>
/// Helper component that wires up XR button events to the StartPedestalController at runtime.
/// Supports: grip+trigger activation, and trigger-while-hovering activation.
/// </summary>
public class ButtonEventWirer : MonoBehaviour
{
    [SerializeField] private StartPedestalController pedestalController;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem buttonParticles;
    
    private XRSimpleInteractable interactable;
    private bool isHovered = false;
    private bool wasTriggered = false;
    private float triggerCooldown = 0f;
    
    // Track which controllers are hovering
    private HashSet<UnityEngine.XR.InputDevice> hoveringControllers = new HashSet<UnityEngine.XR.InputDevice>();
    
    private void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        
        // Auto-find particle system if not assigned
        if (buttonParticles == null)
        {
            buttonParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        if (interactable != null)
        {
            // Wire up activated event (grip + trigger)
            interactable.activated.AddListener(OnActivated);
            
            // Wire up hover events to track when ray is pointing at button
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            
            Debug.Log("ButtonEventWirer: Events wired (activate + hover tracking)");
        }
        else
        {
            Debug.LogWarning("ButtonEventWirer: No XRSimpleInteractable found on this GameObject");
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
        
        // Check for trigger press while hovering (without grip)
        if (isHovered && !wasTriggered)
        {
            // Check both controllers for trigger input
            if (CheckTriggerOnHoveringControllers())
            {
                wasTriggered = true;
                triggerCooldown = 0.3f; // Prevent rapid re-triggering
                TriggerButton();
            }
        }
        
        // Reset trigger state when released
        if (wasTriggered && !IsTriggerPressed())
        {
            wasTriggered = false;
        }
    }
    
    private bool CheckTriggerOnHoveringControllers()
    {
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
        
        return false;
    }
    
    private bool IsTriggerPressed()
    {
        var leftDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftDevices);
        foreach (var device in leftDevices)
        {
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
            {
                if (triggerValue > 0.5f) return true;
            }
        }
        
        var rightDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightDevices);
        foreach (var device in rightDevices)
        {
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
            {
                if (triggerValue > 0.5f) return true;
            }
        }
        
        return false;
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
    
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
        Debug.Log($"ButtonEventWirer: Hover entered from {args.interactorObject}");
    }
    
    private void OnHoverExited(HoverExitEventArgs args)
    {
        // Only set to false if no interactors are hovering
        if (interactable != null && interactable.interactorsHovering.Count == 0)
        {
            isHovered = false;
        }
        Debug.Log($"ButtonEventWirer: Hover exited, still hovered: {isHovered}");
    }
    
    private void OnActivated(ActivateEventArgs args)
    {
        // Prevent double-activation if we just triggered via hover
        if (triggerCooldown > 0f) return;
        
        triggerCooldown = 0.3f;
        TriggerButton();
    }
    
    private void TriggerButton()
    {
        if (pedestalController != null)
        {
            pedestalController.OnStartButtonPressed();
        }
        
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        
        if (buttonParticles != null)
        {
            buttonParticles.Play();
        }
    }
}
