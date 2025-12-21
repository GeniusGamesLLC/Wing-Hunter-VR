using UnityEngine;
using TMPro;

/// <summary>
/// Generates scroll indicator visuals for PaperScrollController.
/// Creates a vertical scroll bar on the right edge of the paper with
/// a handle that moves to show current scroll position.
/// </summary>
public class ScrollIndicatorGenerator : MonoBehaviour
{
    [Header("Scroll Bar Configuration")]
    [Tooltip("Width of the scroll bar track")]
    [SerializeField] private float trackWidth = 0.008f;
    [Tooltip("Height of the scroll bar track (relative to paper height)")]
    [SerializeField] private float trackHeightRatio = 0.8f;
    [Tooltip("X offset from right edge of paper")]
    [SerializeField] private float rightEdgeOffset = 0.01f;
    
    [Header("Handle Configuration")]
    [Tooltip("Height of the scroll handle")]
    [SerializeField] private float handleHeight = 0.03f;
    [Tooltip("Width of the scroll handle")]
    [SerializeField] private float handleWidth = 0.012f;
    
    [Header("Colors")]
    [SerializeField] private Color trackColor = new Color(0.4f, 0.35f, 0.3f, 0.3f);
    [SerializeField] private Color handleColor = new Color(0.3f, 0.25f, 0.2f, 0.8f);
    [SerializeField] private Color boundaryGlowColor = new Color(1f, 0.9f, 0.7f, 0.6f);
    
    [Header("Boundary Indicators")]
    [Tooltip("Height of boundary indicator")]
    [SerializeField] private float boundaryIndicatorHeight = 0.015f;
    
    // Generated objects
    private GameObject scrollIndicatorRoot;
    private GameObject trackObject;
    private GameObject handleObject;
    private GameObject topBoundaryIndicator;
    private GameObject bottomBoundaryIndicator;
    
    // References for PaperScrollController
    private RectTransform handleRectTransform;
    
    /// <summary>
    /// Gets the scroll indicator root GameObject.
    /// </summary>
    public GameObject ScrollIndicatorRoot => scrollIndicatorRoot;
    
    /// <summary>
    /// Gets the handle RectTransform for position updates.
    /// </summary>
    public RectTransform HandleRectTransform => handleRectTransform;
    
    /// <summary>
    /// Gets the top boundary indicator GameObject.
    /// </summary>
    public GameObject TopBoundaryIndicator => topBoundaryIndicator;
    
    /// <summary>
    /// Gets the bottom boundary indicator GameObject.
    /// </summary>
    public GameObject BottomBoundaryIndicator => bottomBoundaryIndicator;
    
    /// <summary>
    /// Generates scroll indicator visuals for the given paper dimensions.
    /// </summary>
    /// <param name="paperWidth">Width of the paper in world units</param>
    /// <param name="paperHeight">Height of the paper in world units</param>
    /// <param name="parent">Parent transform to attach the indicator to</param>
    public void GenerateScrollIndicator(float paperWidth, float paperHeight, Transform parent)
    {
        // Clean up existing if any
        if (scrollIndicatorRoot != null)
        {
            DestroyImmediate(scrollIndicatorRoot);
        }
        
        // Create root container
        scrollIndicatorRoot = new GameObject("ScrollIndicator");
        scrollIndicatorRoot.transform.SetParent(parent, false);
        scrollIndicatorRoot.layer = 5; // UI layer
        
        // Position on right edge of paper
        float xPosition = (paperWidth / 2f) - rightEdgeOffset;
        scrollIndicatorRoot.transform.localPosition = new Vector3(xPosition, 0f, -0.006f);
        
        // Calculate track height
        float trackHeight = paperHeight * trackHeightRatio;
        
        // Create track (background)
        CreateTrack(trackHeight);
        
        // Create handle
        CreateHandle(trackHeight);
        
        // Create boundary indicators
        CreateBoundaryIndicators(trackHeight, paperWidth);
        
        // Initially hide the indicator
        scrollIndicatorRoot.SetActive(false);
    }
    
    /// <summary>
    /// Creates the scroll track (background bar).
    /// </summary>
    private void CreateTrack(float trackHeight)
    {
        trackObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        trackObject.name = "Track";
        trackObject.transform.SetParent(scrollIndicatorRoot.transform, false);
        trackObject.layer = 5;
        
        // Remove collider (not needed for visual)
        var collider = trackObject.GetComponent<Collider>();
        if (collider != null) DestroyImmediate(collider);
        
        // Scale to track dimensions
        trackObject.transform.localScale = new Vector3(trackWidth, trackHeight, 1f);
        trackObject.transform.localPosition = Vector3.zero;
        
        // Apply track material/color
        var renderer = trackObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // Create a simple unlit material
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = trackColor;
            renderer.material = material;
        }
    }
    
    /// <summary>
    /// Creates the scroll handle.
    /// </summary>
    private void CreateHandle(float trackHeight)
    {
        handleObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        handleObject.name = "Handle";
        handleObject.transform.SetParent(scrollIndicatorRoot.transform, false);
        handleObject.layer = 5;
        
        // Remove collider
        var collider = handleObject.GetComponent<Collider>();
        if (collider != null) DestroyImmediate(collider);
        
        // Scale to handle dimensions
        handleObject.transform.localScale = new Vector3(handleWidth, handleHeight, 1f);
        
        // Position at top of track initially
        float topY = (trackHeight / 2f) - (handleHeight / 2f);
        handleObject.transform.localPosition = new Vector3(0f, topY, -0.001f);
        
        // Add RectTransform for easier positioning updates
        // Note: For 3D world-space UI, we'll track position manually
        handleRectTransform = handleObject.AddComponent<RectTransform>();
        handleRectTransform.sizeDelta = new Vector2(handleWidth, handleHeight);
        
        // Apply handle material/color
        var renderer = handleObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = handleColor;
            renderer.material = material;
        }
    }
    
    /// <summary>
    /// Creates top and bottom boundary indicators.
    /// </summary>
    private void CreateBoundaryIndicators(float trackHeight, float paperWidth)
    {
        // Top boundary indicator
        topBoundaryIndicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
        topBoundaryIndicator.name = "TopBoundaryIndicator";
        topBoundaryIndicator.transform.SetParent(scrollIndicatorRoot.transform.parent, false);
        topBoundaryIndicator.layer = 5;
        
        var topCollider = topBoundaryIndicator.GetComponent<Collider>();
        if (topCollider != null) DestroyImmediate(topCollider);
        
        // Full width glow at top of paper
        topBoundaryIndicator.transform.localScale = new Vector3(paperWidth * 0.9f, boundaryIndicatorHeight, 1f);
        float topY = (trackHeight / 2f) + boundaryIndicatorHeight;
        topBoundaryIndicator.transform.localPosition = new Vector3(0f, topY, -0.007f);
        
        var topRenderer = topBoundaryIndicator.GetComponent<MeshRenderer>();
        if (topRenderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = boundaryGlowColor;
            topRenderer.material = material;
        }
        topBoundaryIndicator.SetActive(false);
        
        // Bottom boundary indicator
        bottomBoundaryIndicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bottomBoundaryIndicator.name = "BottomBoundaryIndicator";
        bottomBoundaryIndicator.transform.SetParent(scrollIndicatorRoot.transform.parent, false);
        bottomBoundaryIndicator.layer = 5;
        
        var bottomCollider = bottomBoundaryIndicator.GetComponent<Collider>();
        if (bottomCollider != null) DestroyImmediate(bottomCollider);
        
        // Full width glow at bottom of paper
        bottomBoundaryIndicator.transform.localScale = new Vector3(paperWidth * 0.9f, boundaryIndicatorHeight, 1f);
        float bottomY = -(trackHeight / 2f) - boundaryIndicatorHeight;
        bottomBoundaryIndicator.transform.localPosition = new Vector3(0f, bottomY, -0.007f);
        
        var bottomRenderer = bottomBoundaryIndicator.GetComponent<MeshRenderer>();
        if (bottomRenderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = boundaryGlowColor;
            bottomRenderer.material = material;
        }
        bottomBoundaryIndicator.SetActive(false);
    }
    
    /// <summary>
    /// Updates the handle position based on normalized scroll position (0-1).
    /// </summary>
    /// <param name="normalizedPosition">0 = top, 1 = bottom</param>
    public void UpdateHandlePosition(float normalizedPosition)
    {
        if (handleObject == null || trackObject == null) return;
        
        float trackHeight = trackObject.transform.localScale.y;
        float availableTravel = trackHeight - handleHeight;
        
        // Calculate Y position (top = positive, bottom = negative)
        float topY = (trackHeight / 2f) - (handleHeight / 2f);
        float targetY = topY - (normalizedPosition * availableTravel);
        
        Vector3 pos = handleObject.transform.localPosition;
        pos.y = targetY;
        handleObject.transform.localPosition = pos;
    }
    
    /// <summary>
    /// Shows or hides the scroll indicator.
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (scrollIndicatorRoot != null)
        {
            scrollIndicatorRoot.SetActive(visible);
        }
    }
    
    /// <summary>
    /// Cleans up generated objects.
    /// </summary>
    public void Cleanup()
    {
        if (scrollIndicatorRoot != null)
        {
            DestroyImmediate(scrollIndicatorRoot);
        }
        if (topBoundaryIndicator != null)
        {
            DestroyImmediate(topBoundaryIndicator);
        }
        if (bottomBoundaryIndicator != null)
        {
            DestroyImmediate(bottomBoundaryIndicator);
        }
    }
    
    private void OnDestroy()
    {
        Cleanup();
    }
}
