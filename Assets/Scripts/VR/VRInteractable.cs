using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using DuckHunt.Data;

namespace DuckHunt.VR
{
    /// <summary>
    /// Reusable base class for VR interactable objects.
    /// Handles hover + trigger interaction pattern with proper per-controller tracking.
    /// Only responds to trigger from the controller that is actually hovering.
    /// Includes debug hitbox visualization controlled by DebugSettings.ShowInteractionHitboxes.
    /// </summary>
    public class VRInteractable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] protected float triggerThreshold = 0.5f;
        [SerializeField] protected float activationCooldown = 0.3f;



        protected XRSimpleInteractable interactable;
        protected HashSet<IXRHoverInteractor> hoveringInteractors = new HashSet<IXRHoverInteractor>();
        protected float cooldownTimer;
        protected bool wasActivated;

        // Debug hitbox visualization
        protected GameObject hitboxVisual;
        protected MeshRenderer hitboxRenderer;
        protected Material hitboxMaterial;
        protected bool lastShowHitboxes;

        /// <summary>
        /// True if any interactor is currently hovering over this object.
        /// </summary>
        public bool IsHovered => hoveringInteractors.Count > 0;

        /// <summary>
        /// True if currently in activated state (trigger held while hovering).
        /// </summary>
        public bool IsActivated => wasActivated;

        /// <summary>
        /// Event fired when the interactable is activated (trigger pressed while hovering).
        /// </summary>
        public event Action OnActivated;

        /// <summary>
        /// Event fired when hover begins.
        /// </summary>
        public event Action OnHoverStart;

        /// <summary>
        /// Event fired when hover ends.
        /// </summary>
        public event Action OnHoverEnd;

        protected virtual void Awake()
        {
            SetupInteractable();
            CreateHitboxVisual();
        }

        protected virtual void OnDestroy()
        {
            CleanupInteractable();

            if (hitboxMaterial != null)
            {
                Destroy(hitboxMaterial);
            }
            if (hitboxVisual != null)
            {
                Destroy(hitboxVisual);
            }
        }

        protected virtual void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            // Check trigger only for hovering interactors
            if (hoveringInteractors.Count > 0 && !wasActivated && cooldownTimer <= 0f)
            {
                if (CheckHoveringInteractorTrigger())
                {
                    wasActivated = true;
                    cooldownTimer = activationCooldown;
                    HandleActivation();
                }
            }

            // Reset activation when trigger released
            if (wasActivated && !CheckHoveringInteractorTrigger())
            {
                wasActivated = false;
            }

            UpdateHitboxVisual();
        }

        /// <summary>
        /// Sets up the XR interactable component and event listeners.
        /// </summary>
        protected virtual void SetupInteractable()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            if (interactable == null)
            {
                interactable = gameObject.AddComponent<XRSimpleInteractable>();
            }

            // Register colliders
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                if (!interactable.colliders.Contains(col))
                {
                    interactable.colliders.Add(col);
                }
            }

            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }

        /// <summary>
        /// Cleans up event listeners.
        /// </summary>
        protected virtual void CleanupInteractable()
        {
            if (interactable != null)
            {
                interactable.hoverEntered.RemoveListener(OnHoverEntered);
                interactable.hoverExited.RemoveListener(OnHoverExited);
            }
        }

        protected virtual void OnHoverEntered(HoverEnterEventArgs args)
        {
            bool wasEmpty = hoveringInteractors.Count == 0;
            hoveringInteractors.Add(args.interactorObject);

            if (wasEmpty)
            {
                OnHoverStart?.Invoke();
                HandleHoverStart();
            }
        }

        protected virtual void OnHoverExited(HoverExitEventArgs args)
        {
            hoveringInteractors.Remove(args.interactorObject);

            if (hoveringInteractors.Count == 0)
            {
                OnHoverEnd?.Invoke();
                HandleHoverEnd();
                wasActivated = false;
            }
        }

        /// <summary>
        /// Checks if any hovering interactor's controller has trigger pressed.
        /// Only checks the specific controllers that are actually hovering.
        /// </summary>
        protected bool CheckHoveringInteractorTrigger()
        {
            foreach (var interactor in hoveringInteractors)
            {
                if (interactor == null) continue;

                var node = GetInteractorNode(interactor);
                if (node == XRNode.LeftHand || node == XRNode.RightHand)
                {
                    if (GetTriggerValue(node) > triggerThreshold)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the XRNode (hand) for an interactor.
        /// </summary>
        protected XRNode GetInteractorNode(IXRHoverInteractor interactor)
        {
            var interactorTransform = (interactor as MonoBehaviour)?.transform;
            if (interactorTransform == null) return XRNode.Head;

            // Check parent hierarchy for controller identification
            var name = interactorTransform.name.ToLower();
            var parentName = interactorTransform.parent?.name.ToLower() ?? "";

            if (name.Contains("left") || parentName.Contains("left"))
            {
                return XRNode.LeftHand;
            }
            if (name.Contains("right") || parentName.Contains("right"))
            {
                return XRNode.RightHand;
            }

            // Try to get from XRController component
            var controller = interactorTransform.GetComponentInParent<ActionBasedController>();
            if (controller != null)
            {
                var controllerName = controller.name.ToLower();
                if (controllerName.Contains("left")) return XRNode.LeftHand;
                if (controllerName.Contains("right")) return XRNode.RightHand;
            }

            return XRNode.Head;
        }

        /// <summary>
        /// Gets the trigger value for a specific hand.
        /// </summary>
        protected float GetTriggerValue(XRNode node)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(node, devices);

            foreach (var device in devices)
            {
                if (device.TryGetFeatureValue(CommonUsages.trigger, out float value))
                {
                    return value;
                }
            }
            return 0f;
        }

        /// <summary>
        /// Override to handle activation (trigger pressed while hovering).
        /// </summary>
        protected virtual void HandleActivation()
        {
            OnActivated?.Invoke();
        }

        /// <summary>
        /// Override to handle hover start.
        /// </summary>
        protected virtual void HandleHoverStart()
        {
        }

        /// <summary>
        /// Override to handle hover end.
        /// </summary>
        protected virtual void HandleHoverEnd()
        {
        }

        #region Debug Hitbox Visualization

        /// <summary>
        /// Creates the debug hitbox visualization mesh.
        /// </summary>
        protected virtual void CreateHitboxVisual()
        {
            // Guard: don't create if already exists
            if (hitboxVisual != null) return;
            
            // Also check for existing HitboxVisual child (from previous runs)
            var existingVisual = transform.Find("HitboxVisual");
            if (existingVisual != null)
            {
                Destroy(existingVisual.gameObject);
            }
            
            // First try to get collider on this object, then children
            var col = GetComponent<BoxCollider>();
            if (col == null)
            {
                col = GetComponentInChildren<BoxCollider>();
            }
            if (col == null) return;
            
            // Also clean up any existing HitboxVisual on the collider's transform
            var existingOnCollider = col.transform.Find("HitboxVisual");
            if (existingOnCollider != null)
            {
                Destroy(existingOnCollider.gameObject);
            }

            hitboxVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hitboxVisual.name = "HitboxVisual";
            
            // Parent to the collider's GameObject
            hitboxVisual.transform.SetParent(col.transform);
            hitboxVisual.transform.localPosition = col.center;
            hitboxVisual.transform.localRotation = Quaternion.identity;
            hitboxVisual.transform.localScale = col.size;

            // Remove collider from visual (we don't want double colliders)
            var visualCollider = hitboxVisual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Destroy(visualCollider);
            }

            // Set up the hitbox material
            hitboxRenderer = hitboxVisual.GetComponent<MeshRenderer>();
            
            // Get material from centralized DebugSettings
            var baseMaterial = DebugSettings.Instance?.HitboxMaterial;
            if (baseMaterial != null)
            {
                // Use a copy so we can change colors per-instance
                hitboxMaterial = new Material(baseMaterial);
                Debug.Log($"[VRInteractable] {gameObject.name}: Using DebugSettings hitbox material");
            }
            else
            {
                // Fallback: use URP Unlit shader which works on Quest
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Sprites/Default");
                }
                if (shader != null)
                {
                    hitboxMaterial = new Material(shader);
                    // Configure for transparency
                    hitboxMaterial.SetFloat("_Surface", 1); // Transparent
                    hitboxMaterial.SetFloat("_Blend", 0); // Alpha
                    hitboxMaterial.SetFloat("_SrcBlend", 5); // SrcAlpha
                    hitboxMaterial.SetFloat("_DstBlend", 10); // OneMinusSrcAlpha
                    hitboxMaterial.SetFloat("_ZWrite", 0);
                    hitboxMaterial.renderQueue = 3000;
                    hitboxMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    Debug.LogWarning($"[VRInteractable] {gameObject.name}: No hitbox material in DebugSettings, using fallback shader");
                }
                else
                {
                    // Last resort: modify the primitive's default material
                    hitboxMaterial = hitboxRenderer.material;
                    Debug.LogError($"[VRInteractable] {gameObject.name}: Could not find any suitable shader!");
                }
            }
            
            hitboxMaterial.color = DebugSettings.Instance?.HitboxNormalColor ?? new Color(0f, 1f, 0f, 0.3f);
            hitboxRenderer.material = hitboxMaterial;

            hitboxVisual.SetActive(false);
        }

        /// <summary>
        /// Updates hitbox visibility and color based on state.
        /// </summary>
        protected virtual void UpdateHitboxVisual()
        {
            bool showHitboxes = DebugSettings.Instance != null && DebugSettings.Instance.ShowInteractionHitboxes;

            if (showHitboxes != lastShowHitboxes)
            {
                lastShowHitboxes = showHitboxes;
                if (hitboxVisual != null)
                {
                    hitboxVisual.SetActive(showHitboxes);
                }
            }

            if (showHitboxes && hitboxMaterial != null)
            {
                var settings = DebugSettings.Instance;
                Color targetColor;
                if (wasActivated)
                {
                    targetColor = settings?.HitboxActivatedColor ?? new Color(1f, 0f, 0f, 0.7f);
                }
                else if (IsHovered)
                {
                    targetColor = settings?.HitboxHoverColor ?? new Color(1f, 1f, 0f, 0.5f);
                }
                else
                {
                    targetColor = settings?.HitboxNormalColor ?? new Color(0f, 1f, 0f, 0.3f);
                }
                hitboxMaterial.color = targetColor;
            }
        }

        #endregion
    }
}
