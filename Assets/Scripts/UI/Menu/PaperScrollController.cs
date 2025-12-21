using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles scrolling for papers with content that exceeds the visible area.
/// Uses VR thumbstick input for scrolling and provides visual feedback.
/// </summary>
public class PaperScrollController : MonoBehaviour
{
    [Header("Scroll Configuration")]
    [Tooltip("The viewport RectTransform that defines the visible area")]
    [SerializeField] private RectTransform viewport;
    [Tooltip("The content RectTransform that contains all scrollable content")]
    [SerializeField] private RectTransform content;
    [Tooltip("Scroll speed multiplier for thumbstick input")]
    [SerializeField] private float scrollSpeed = 0.1f;
    [Tooltip("Deadzone for thumbstick input to prevent accidental scrolling")]
    [SerializeField] private float thumbstickDeadzone = 0.2f;
    
    [Header("Visual Feedback")]
    [Tooltip("The scroll indicator container (shown when scrolling is enabled)")]
    [SerializeField] private GameObject scrollIndicator;
    [Tooltip("The scroll handle that moves to show current position")]
    [SerializeField] private RectTransform scrollHandle;
    [Tooltip("Visual indicator shown when at the top of content")]
    [SerializeField] private GameObject topBoundaryIndicator;
    [Tooltip("Visual indicator shown when at the bottom of content")]
    [SerializeField] private GameObject bottomBoundaryIndicator;
    
    [Header("Boundary Feedback")]
    [Tooltip("Duration of boundary feedback animation")]
    [SerializeField] private float boundaryFeedbackDuration = 0.3f;
    [Tooltip("Color for boundary glow effect")]
    [SerializeField] private Color boundaryGlowColor = new Color(1f, 1f, 1f, 0.5f);
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    /// <summary>
    /// Whether scrolling is currently enabled (content exceeds viewport).
    /// </summary>
    public bool IsScrollingEnabled { get; private set; }
    
    /// <summary>
    /// Current scroll position normalized from 0 (top) to 1 (bottom).
    /// </summary>
    public float ScrollPosition { get; private set; }
    
    /// <summary>
    /// Event fired when scroll position changes.
    /// </summary>
    public event Action<float> OnScrollPositionChanged;
    
    /// <summary>
    /// Event fired when reaching a scroll boundary.
    /// </summary>
    public event Action<bool> OnBoundaryReached; // true = top, false = bottom
    
    // XR Input devices
    private UnityEngine.XR.InputDevice leftController;
    private UnityEngine.XR.InputDevice rightController;
    
    // Internal state
    private float contentHeight;
    private float viewportHeight;
    private float maxScrollOffset;
    private float currentScrollOffset;
    private bool isAtTop = true;
    private bool isAtBottom = false;
    private float boundaryFeedbackTimer = 0f;
    private bool showingBoundaryFeedback = false;
    
    // Track if this paper is currently focused (receiving input)
    private bool isReceivingInput = false;
    
    private void Start()
    {
        InitializeXRDevices();
        CalculateScrollBounds();
        UpdateScrollIndicator();
    }
    
    private void InitializeXRDevices()
    {
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);
        if (leftHandDevices.Count > 0)
        {
            leftController = leftHandDevices[0];
        }
        
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
        if (rightHandDevices.Count > 0)
        {
            rightController = rightHandDevices[0];
        }
    }
    
    private void Update()
    {
        // Try to find controllers if not valid
        if (!leftController.isValid || !rightController.isValid)
        {
            InitializeXRDevices();
        }
        
        // Only process input if scrolling is enabled and this paper is focused
        if (IsScrollingEnabled && isReceivingInput)
        {
            ProcessThumbstickInput();
        }
        
        // Update boundary feedback animation
        if (showingBoundaryFeedback)
        {
            boundaryFeedbackTimer -= Time.deltaTime;
            if (boundaryFeedbackTimer <= 0f)
            {
                HideBoundaryFeedback();
            }
        }
    }
    
    /// <summary>
    /// Enables or disables input processing for this scroll controller.
    /// Call this when the paper gains/loses focus.
    /// </summary>
    public void SetReceivingInput(bool receiving)
    {
        isReceivingInput = receiving;
        
        if (debugMode)
        {
            Debug.Log($"[PaperScrollController] SetReceivingInput: {receiving}");
        }
    }
    
    /// <summary>
    /// Calculates scroll bounds based on content and viewport sizes.
    /// Call this after content is generated or changed.
    /// </summary>
    public void CalculateScrollBounds()
    {
        if (viewport == null || content == null)
        {
            IsScrollingEnabled = false;
            return;
        }
        
        viewportHeight = viewport.rect.height;
        contentHeight = content.rect.height;
        
        // Calculate max scroll offset (how far content can scroll)
        maxScrollOffset = Mathf.Max(0f, contentHeight - viewportHeight);
        
        // Enable scrolling only if content exceeds viewport
        bool wasEnabled = IsScrollingEnabled;
        IsScrollingEnabled = maxScrollOffset > 0f;
        
        if (debugMode)
        {
            Debug.Log($"[PaperScrollController] CalculateScrollBounds: viewport={viewportHeight}, content={contentHeight}, maxOffset={maxScrollOffset}, enabled={IsScrollingEnabled}");
        }
        
        // Show/hide scroll indicator
        if (scrollIndicator != null)
        {
            scrollIndicator.SetActive(IsScrollingEnabled);
        }
        
        // Reset scroll position if scrolling was just enabled
        if (IsScrollingEnabled && !wasEnabled)
        {
            ScrollToTop();
        }
        
        UpdateScrollIndicator();
        UpdateBoundaryIndicators();
    }
    
    /// <summary>
    /// Processes thumbstick Y input for scrolling.
    /// </summary>
    private void ProcessThumbstickInput()
    {
        float thumbstickY = 0f;
        
        // Check left thumbstick
        if (leftController.isValid)
        {
            Vector2 leftThumbstick;
            if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out leftThumbstick))
            {
                if (Mathf.Abs(leftThumbstick.y) > thumbstickDeadzone)
                {
                    thumbstickY = leftThumbstick.y;
                }
            }
        }
        
        // Check right thumbstick (use if left isn't providing input)
        if (Mathf.Abs(thumbstickY) < thumbstickDeadzone && rightController.isValid)
        {
            Vector2 rightThumbstick;
            if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out rightThumbstick))
            {
                if (Mathf.Abs(rightThumbstick.y) > thumbstickDeadzone)
                {
                    thumbstickY = rightThumbstick.y;
                }
            }
        }
        
        // Apply scrolling if there's input
        if (Mathf.Abs(thumbstickY) > thumbstickDeadzone)
        {
            ScrollByThumbstick(thumbstickY);
        }
    }
    
    /// <summary>
    /// Scrolls the content based on thumbstick Y input.
    /// Positive Y = scroll up (show content above), Negative Y = scroll down (show content below).
    /// </summary>
    public void ScrollByThumbstick(float thumbstickY)
    {
        if (!IsScrollingEnabled) return;
        
        // Invert because thumbstick up should scroll content up (show lower content)
        float scrollDelta = -thumbstickY * scrollSpeed * Time.deltaTime * 100f;
        
        float previousOffset = currentScrollOffset;
        currentScrollOffset = Mathf.Clamp(currentScrollOffset + scrollDelta, 0f, maxScrollOffset);
        
        // Check if we hit a boundary
        bool hitTop = previousOffset > 0f && currentScrollOffset <= 0f;
        bool hitBottom = previousOffset < maxScrollOffset && currentScrollOffset >= maxScrollOffset;
        
        if (hitTop)
        {
            ShowBoundaryFeedback(true);
            OnBoundaryReached?.Invoke(true);
        }
        else if (hitBottom)
        {
            ShowBoundaryFeedback(false);
            OnBoundaryReached?.Invoke(false);
        }
        
        // Apply scroll to content
        ApplyScrollOffset();
        
        // Update normalized position
        ScrollPosition = maxScrollOffset > 0f ? currentScrollOffset / maxScrollOffset : 0f;
        
        UpdateScrollIndicator();
        UpdateBoundaryIndicators();
        
        OnScrollPositionChanged?.Invoke(ScrollPosition);
    }
    
    /// <summary>
    /// Scrolls to the top of the content.
    /// </summary>
    public void ScrollToTop()
    {
        currentScrollOffset = 0f;
        ScrollPosition = 0f;
        ApplyScrollOffset();
        UpdateScrollIndicator();
        UpdateBoundaryIndicators();
        OnScrollPositionChanged?.Invoke(ScrollPosition);
    }
    
    /// <summary>
    /// Scrolls to the bottom of the content.
    /// </summary>
    public void ScrollToBottom()
    {
        currentScrollOffset = maxScrollOffset;
        ScrollPosition = 1f;
        ApplyScrollOffset();
        UpdateScrollIndicator();
        UpdateBoundaryIndicators();
        OnScrollPositionChanged?.Invoke(ScrollPosition);
    }
    
    /// <summary>
    /// Scrolls to a specific normalized position (0-1).
    /// </summary>
    public void ScrollToPosition(float normalizedPosition)
    {
        normalizedPosition = Mathf.Clamp01(normalizedPosition);
        currentScrollOffset = normalizedPosition * maxScrollOffset;
        ScrollPosition = normalizedPosition;
        ApplyScrollOffset();
        UpdateScrollIndicator();
        UpdateBoundaryIndicators();
        OnScrollPositionChanged?.Invoke(ScrollPosition);
    }
    
    /// <summary>
    /// Applies the current scroll offset to the content transform.
    /// </summary>
    private void ApplyScrollOffset()
    {
        if (content == null) return;
        
        // Move content up (positive Y) to show lower content
        Vector2 anchoredPos = content.anchoredPosition;
        anchoredPos.y = currentScrollOffset;
        content.anchoredPosition = anchoredPos;
    }
    
    /// <summary>
    /// Updates the scroll indicator handle position.
    /// </summary>
    private void UpdateScrollIndicator()
    {
        if (scrollHandle == null || scrollIndicator == null) return;
        
        if (!IsScrollingEnabled)
        {
            scrollIndicator.SetActive(false);
            return;
        }
        
        scrollIndicator.SetActive(true);
        
        // Move handle based on scroll position
        // Assuming handle moves within a vertical track
        float handleY = Mathf.Lerp(0f, -1f, ScrollPosition);
        
        // Get the parent rect to calculate proper positioning
        RectTransform trackRect = scrollHandle.parent as RectTransform;
        if (trackRect != null)
        {
            float trackHeight = trackRect.rect.height;
            float handleHeight = scrollHandle.rect.height;
            float availableTravel = trackHeight - handleHeight;
            
            Vector2 handlePos = scrollHandle.anchoredPosition;
            handlePos.y = -ScrollPosition * availableTravel;
            scrollHandle.anchoredPosition = handlePos;
        }
    }
    
    /// <summary>
    /// Updates the visibility of boundary indicators.
    /// </summary>
    private void UpdateBoundaryIndicators()
    {
        isAtTop = currentScrollOffset <= 0f;
        isAtBottom = currentScrollOffset >= maxScrollOffset;
        
        // Show indicators when at boundaries (optional - can be used for persistent indicators)
        // The boundary feedback is separate and shows temporarily when hitting a boundary
    }
    
    /// <summary>
    /// Shows boundary feedback (glow/bounce effect) when hitting a scroll limit.
    /// </summary>
    private void ShowBoundaryFeedback(bool isTop)
    {
        showingBoundaryFeedback = true;
        boundaryFeedbackTimer = boundaryFeedbackDuration;
        
        if (isTop && topBoundaryIndicator != null)
        {
            topBoundaryIndicator.SetActive(true);
        }
        else if (!isTop && bottomBoundaryIndicator != null)
        {
            bottomBoundaryIndicator.SetActive(true);
        }
        
        if (debugMode)
        {
            Debug.Log($"[PaperScrollController] Boundary reached: {(isTop ? "TOP" : "BOTTOM")}");
        }
    }
    
    /// <summary>
    /// Hides the boundary feedback indicators.
    /// </summary>
    private void HideBoundaryFeedback()
    {
        showingBoundaryFeedback = false;
        
        if (topBoundaryIndicator != null)
        {
            topBoundaryIndicator.SetActive(false);
        }
        if (bottomBoundaryIndicator != null)
        {
            bottomBoundaryIndicator.SetActive(false);
        }
    }
    
    /// <summary>
    /// Enables scrolling functionality.
    /// </summary>
    public void EnableScrolling(bool enable)
    {
        if (enable)
        {
            CalculateScrollBounds();
        }
        else
        {
            IsScrollingEnabled = false;
            if (scrollIndicator != null)
            {
                scrollIndicator.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Gets whether the scroll is currently at the top.
    /// </summary>
    public bool IsAtTop => isAtTop;
    
    /// <summary>
    /// Gets whether the scroll is currently at the bottom.
    /// </summary>
    public bool IsAtBottom => isAtBottom;
    
    /// <summary>
    /// Gets the content height.
    /// </summary>
    public float ContentHeight => contentHeight;
    
    /// <summary>
    /// Gets the viewport height.
    /// </summary>
    public float ViewportHeight => viewportHeight;
    
    // ============================================
    // Configuration Methods (for setup)
    // ============================================
    
    /// <summary>
    /// Sets the viewport and content RectTransforms.
    /// </summary>
    public void SetScrollTargets(RectTransform viewportRect, RectTransform contentRect)
    {
        viewport = viewportRect;
        content = contentRect;
        CalculateScrollBounds();
    }
    
    /// <summary>
    /// Sets the scroll indicator references.
    /// </summary>
    public void SetScrollIndicator(GameObject indicator, RectTransform handle)
    {
        scrollIndicator = indicator;
        scrollHandle = handle;
        UpdateScrollIndicator();
    }
    
    /// <summary>
    /// Sets the boundary indicator references.
    /// </summary>
    public void SetBoundaryIndicators(GameObject topIndicator, GameObject bottomIndicator)
    {
        topBoundaryIndicator = topIndicator;
        bottomBoundaryIndicator = bottomIndicator;
        
        // Initially hide boundary indicators
        if (topBoundaryIndicator != null) topBoundaryIndicator.SetActive(false);
        if (bottomBoundaryIndicator != null) bottomBoundaryIndicator.SetActive(false);
    }
}
