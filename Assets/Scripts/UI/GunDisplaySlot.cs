using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Individual slot in the gun display rack.
/// Shows a rotating gun preview with name label.
/// Handles selection interaction via VR controller.
/// Uses same interaction pattern as ButtonEventWirer (hover + trigger).
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class GunDisplaySlot : MonoBehaviour
{
    private GunDisplayRack parentRack;
    private int slotIndex;
    private int currentGunIndex = -1;
    
    private GameObject gunPreviewInstance;
    private TextMeshPro nameLabel;
    private GameObject highlightIndicator;
    private XRSimpleInteractable interactable;
    
    private float previewScale;
    private float labelYOffset;
    private float rotationSpeed;
    private bool isSelected;
    private bool isHovered;
    private bool wasTriggered;
    private float triggerCooldown;
    
    public void Initialize(GunDisplayRack rack, int index, float scale, float labelOffset, float rotSpeed)
    {
        parentRack = rack;
        slotIndex = index;
        previewScale = scale;
        labelYOffset = labelOffset;
        rotationSpeed = rotSpeed;
        
        CreateNameLabel();
        CreateHighlightIndicator();
        SetupXRInteraction();
        
        // Re-register with XR Interaction Manager after a frame to ensure proper setup
        StartCoroutine(ReregisterInteractable());
    }
    
    private System.Collections.IEnumerator ReregisterInteractable()
    {
        // Wait a few frames to ensure XR system is fully initialized
        yield return null;
        yield return null;
        yield return null;
        
        if (interactable != null)
        {
            // Find the interaction manager in the scene
            var manager = Object.FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
            if (manager != null)
            {
                // Force re-registration by toggling enabled state
                interactable.enabled = false;
                yield return null;
                interactable.enabled = true;
            }
        }
    }
    
    private void SetupXRInteraction()
    {
        // Ensure collider exists first (XRSimpleInteractable needs it)
        EnsureInteractionCollider();
        
        // Get or add XRSimpleInteractable
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }
        
        // Make sure the interactable knows about our collider
        var col = GetComponent<BoxCollider>();
        if (col != null && interactable.colliders != null && !interactable.colliders.Contains(col))
        {
            interactable.colliders.Add(col);
        }
        
        // Subscribe to interaction events - use activated (grip+trigger) and hover
        interactable.activated.AddListener(OnActivated);
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }
    
    private void OnActivated(ActivateEventArgs args)
    {
        // Prevent double-activation
        if (triggerCooldown > 0f) return;
        
        triggerCooldown = 0.3f;
        OnSelect();
    }
    
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
        Debug.Log($"GunDisplaySlot: Hover entered on slot {slotIndex} (gun index {currentGunIndex})");
        // Scale up slightly on hover
        if (gunPreviewInstance != null)
        {
            gunPreviewInstance.transform.localScale = Vector3.one * previewScale * 1.15f;
        }

    }
    
    private void OnHoverExited(HoverExitEventArgs args)
    {
        // Only set to false if no interactors are hovering
        if (interactable != null && interactable.interactorsHovering.Count == 0)
        {
            isHovered = false;
        }
        // Return to normal scale
        if (gunPreviewInstance != null)
        {
            gunPreviewInstance.transform.localScale = Vector3.one * previewScale;
        }
    }
    
    private void CreateNameLabel()
    {
        GameObject labelObj = new GameObject("NameLabel");
        labelObj.transform.SetParent(transform);
        labelObj.transform.localPosition = new Vector3(0, labelYOffset, 0);
        labelObj.transform.localRotation = Quaternion.identity;
        
        nameLabel = labelObj.AddComponent<TextMeshPro>();
        nameLabel.fontSize = 0.4f;
        nameLabel.alignment = TextAlignmentOptions.Center;
        nameLabel.color = Color.white;
        
        RectTransform rect = nameLabel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0.4f, 0.08f);
    }
    
    private void CreateHighlightIndicator()
    {
        highlightIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        highlightIndicator.name = "SelectionBase";
        highlightIndicator.transform.SetParent(transform);
        highlightIndicator.transform.localPosition = new Vector3(0, -0.02f, 0);
        highlightIndicator.transform.localScale = new Vector3(0.18f, 0.02f, 0.18f);
        
        var collider = highlightIndicator.GetComponent<Collider>();
        if (collider != null) DestroyImmediate(collider);
        
        var renderer = highlightIndicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }
    
    public void ShowGun(GunData gunData, int gunIndex, bool selected, Color selectedColor, Color normalColor)
    {
        currentGunIndex = gunIndex;
        isSelected = selected;
        
        if (gunPreviewInstance != null)
        {
            DestroyImmediate(gunPreviewInstance);
        }
        
        if (gunData.gunPrefab != null)
        {
            gunPreviewInstance = Instantiate(gunData.gunPrefab, transform);
            gunPreviewInstance.name = "GunPreview";
            gunPreviewInstance.transform.localPosition = new Vector3(0, 0.05f, 0);
            gunPreviewInstance.transform.localRotation = Quaternion.Euler(0, 90, 0);
            gunPreviewInstance.transform.localScale = Vector3.one * previewScale;
            
            DisableScripts(gunPreviewInstance);
            SetLayerRecursive(gunPreviewInstance, 0);
        }
        
        if (nameLabel != null)
        {
            nameLabel.text = gunData.gunName;
            nameLabel.gameObject.SetActive(true);
        }
        
        UpdateHighlight(selected, selectedColor, normalColor);
        gameObject.SetActive(true);
    }
    
    public void Clear()
    {
        currentGunIndex = -1;
        
        if (gunPreviewInstance != null)
        {
            DestroyImmediate(gunPreviewInstance);
            gunPreviewInstance = null;
        }
        
        if (nameLabel != null)
        {
            nameLabel.gameObject.SetActive(false);
        }
        
        if (highlightIndicator != null)
        {
            highlightIndicator.SetActive(false);
        }
        
        gameObject.SetActive(false);
    }
    
    private void UpdateHighlight(bool selected, Color selectedColor, Color normalColor)
    {
        if (highlightIndicator != null)
        {
            highlightIndicator.SetActive(true);
            var renderer = highlightIndicator.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = selected ? selectedColor : normalColor;
            }
        }
    }
    
    private void EnsureInteractionCollider()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
        // Smaller collider to avoid overlapping with arrows (slots are 0.4m apart)
        col.size = new Vector3(0.2f, 0.25f, 0.2f);
        col.center = new Vector3(0, 0.05f, 0); // Centered on gun preview position
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
    
    private void DisableScripts(GameObject obj)
    {
        var behaviours = obj.GetComponentsInChildren<MonoBehaviour>();
        foreach (var b in behaviours)
        {
            if (b != null && !(b is Renderer) && !(b is MeshFilter))
            {
                b.enabled = false;
            }
        }
    }
    
    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
    
    private void Update()
    {
        // Handle cooldown
        if (triggerCooldown > 0f)
        {
            triggerCooldown -= Time.deltaTime;
        }
        
        // Check for trigger press while hovering (trigger-only, no grip required)
        if (isHovered && !wasTriggered && triggerCooldown <= 0f)
        {
            if (CheckTriggerPressed())
            {
                wasTriggered = true;
                triggerCooldown = 0.3f;
                OnSelect();
            }
        }
        
        // Reset trigger state when released
        if (wasTriggered && !CheckTriggerPressed())
        {
            wasTriggered = false;
        }
        
        // Rotate gun preview
        if (gunPreviewInstance != null && rotationSpeed > 0)
        {
            gunPreviewInstance.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
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
    
    /// <summary>
    /// Called when player selects this slot
    /// </summary>
    public void OnSelect()
    {
        if (currentGunIndex >= 0 && parentRack != null)
        {
            Debug.Log($"GunDisplaySlot: Selected gun index {currentGunIndex}");
            parentRack.SelectGun(currentGunIndex);
        }
    }
}
