using UnityEngine;
using TMPro;
using DuckHunt.VR;

/// <summary>
/// Individual slot in the gun display rack.
/// Shows a rotating gun preview with name label.
/// Handles selection interaction via VR controller.
/// Uses standardized VRInteractable base class.
/// </summary>
public class GunDisplaySlot : VRInteractable
{
    private GunDisplayRack parentRack;
    private int slotIndex;
    private int currentGunIndex = -1;

    private GameObject gunPreviewInstance;
    private TextMeshPro nameLabel;
    private GameObject highlightIndicator;

    private float previewScale;
    private float labelYOffset;
    private float rotationSpeed;
    private bool isSelected;

    protected override void Awake()
    {
        // Don't call base.Awake() yet - we need to set up collider first in Initialize()
    }

    public void Initialize(GunDisplayRack rack, int index, float scale, float labelOffset, float rotSpeed)
    {
        parentRack = rack;
        slotIndex = index;
        previewScale = scale;
        labelYOffset = labelOffset;
        rotationSpeed = rotSpeed;

        CreateNameLabel();
        CreateHighlightIndicator();
        EnsureInteractionCollider();

        // Now call base.Awake() to set up the interactable after collider exists
        base.Awake();

        // Re-register with XR Interaction Manager after a frame
        StartCoroutine(ReregisterInteractable());
    }

    private System.Collections.IEnumerator ReregisterInteractable()
    {
        yield return null;
        yield return null;
        yield return null;

        if (interactable != null)
        {
            var manager = Object.FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
            if (manager != null)
            {
                interactable.enabled = false;
                yield return null;
                interactable.enabled = true;
            }
        }
    }

    protected override void HandleActivation()
    {
        base.HandleActivation();
        OnSelect();
    }

    protected override void HandleHoverStart()
    {
        base.HandleHoverStart();
        // Scale up slightly on hover
        if (gunPreviewInstance != null)
        {
            gunPreviewInstance.transform.localScale = Vector3.one * previewScale * 1.15f;
        }
    }

    protected override void HandleHoverEnd()
    {
        base.HandleHoverEnd();
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
        col.size = new Vector3(0.2f, 0.25f, 0.2f);
        col.center = new Vector3(0, 0.05f, 0);
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

    protected override void Update()
    {
        base.Update();

        // Rotate gun preview
        if (gunPreviewInstance != null && rotationSpeed > 0)
        {
            gunPreviewInstance.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    /// <summary>
    /// Called when player selects this slot
    /// </summary>
    public void OnSelect()
    {
        if (currentGunIndex >= 0 && parentRack != null)
        {
            parentRack.SelectGun(currentGunIndex);
        }
    }
}
