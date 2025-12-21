using UnityEngine;
using TMPro;

/// <summary>
/// Generates placeholder content for menu papers when in compact (unfocused) state.
/// Creates wavy lines or blurred text to suggest content without showing details.
/// Requirements: 10.1 - Display compact size with placeholder/preview content
/// </summary>
public class PlaceholderContentGenerator : MonoBehaviour
{
    [Header("Placeholder Configuration")]
    [Tooltip("Number of placeholder lines to generate")]
    [SerializeField] private int numberOfLines = 5;
    [Tooltip("Width of each line (in world units)")]
    [SerializeField] private float lineWidth = 0.18f;
    [Tooltip("Height of each line (in world units)")]
    [SerializeField] private float lineHeight = 0.008f;
    [Tooltip("Vertical spacing between lines (in world units)")]
    [SerializeField] private float lineSpacing = 0.025f;
    [Tooltip("Starting Y position from top (in world units)")]
    [SerializeField] private float startY = 0.12f;
    [Tooltip("Color of placeholder lines")]
    [SerializeField] private Color lineColor = new Color(0.6f, 0.55f, 0.5f, 0.5f);
    
    [Header("Line Variation")]
    [Tooltip("Minimum width multiplier for line variation")]
    [SerializeField] private float minWidthMultiplier = 0.4f;
    [Tooltip("Maximum width multiplier for line variation")]
    [SerializeField] private float maxWidthMultiplier = 1.0f;
    
    [Header("Title")]
    [Tooltip("Show a title at the top of placeholder")]
    [SerializeField] private bool showTitle = true;
    [Tooltip("Title text to display")]
    [SerializeField] private string titleText = "...";
    [Tooltip("Title font size")]
    [SerializeField] private float titleFontSize = 0.03f;
    
    private bool isGenerated = false;

    private void Awake()
    {
        // Generate placeholder content on awake if not already done
        if (!isGenerated)
        {
            GeneratePlaceholderContent();
        }
    }

    /// <summary>
    /// Generates the placeholder content (wavy lines).
    /// </summary>
    public void GeneratePlaceholderContent()
    {
        if (isGenerated) return;
        
        // Clear any existing children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        float currentY = startY;
        
        // Create title if enabled
        if (showTitle && !string.IsNullOrEmpty(titleText))
        {
            CreateTitleText(currentY);
            currentY -= lineSpacing * 1.5f;
        }
        
        // Create placeholder lines
        for (int i = 0; i < numberOfLines; i++)
        {
            // Vary line width for more natural look
            float widthMultiplier = Random.Range(minWidthMultiplier, maxWidthMultiplier);
            CreatePlaceholderLine(i, currentY, widthMultiplier);
            currentY -= lineSpacing;
        }
        
        isGenerated = true;
    }
    
    /// <summary>
    /// Creates a title text element.
    /// </summary>
    private void CreateTitleText(float yPosition)
    {
        GameObject titleObj = new GameObject("PlaceholderTitle");
        titleObj.transform.SetParent(transform, false);
        titleObj.transform.localPosition = new Vector3(0, yPosition, -0.001f);
        titleObj.transform.localRotation = Quaternion.identity;
        titleObj.transform.localScale = Vector3.one;
        
        // Add TextMeshPro component for 3D world space
        TextMeshPro tmp = titleObj.AddComponent<TextMeshPro>();
        tmp.text = titleText;
        tmp.fontSize = titleFontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(lineColor.r * 0.8f, lineColor.g * 0.8f, lineColor.b * 0.8f, lineColor.a);
        
        // Configure RectTransform
        RectTransform rect = titleObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(lineWidth, titleFontSize * 1.5f);
    }
    
    /// <summary>
    /// Creates a single placeholder line (quad mesh).
    /// </summary>
    private void CreatePlaceholderLine(int index, float yPosition, float widthMultiplier)
    {
        GameObject lineObj = new GameObject($"PlaceholderLine_{index}");
        lineObj.transform.SetParent(transform, false);
        
        // Position the line
        float actualWidth = lineWidth * widthMultiplier;
        // Offset X slightly for left-aligned look with some variation
        float xOffset = (lineWidth - actualWidth) * -0.4f;
        lineObj.transform.localPosition = new Vector3(xOffset, yPosition, -0.001f);
        lineObj.transform.localRotation = Quaternion.identity;
        lineObj.transform.localScale = new Vector3(actualWidth, lineHeight, 0.001f);
        
        // Add mesh components
        MeshFilter meshFilter = lineObj.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateQuadMesh();
        
        MeshRenderer meshRenderer = lineObj.AddComponent<MeshRenderer>();
        
        // Create a simple unlit material for the line
        Material lineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        if (lineMaterial != null)
        {
            lineMaterial.color = lineColor;
            // Enable transparency
            lineMaterial.SetFloat("_Surface", 1); // Transparent
            lineMaterial.SetFloat("_Blend", 0); // Alpha
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.renderQueue = 3000;
        }
        meshRenderer.material = lineMaterial;
        
        // Set layer to UI
        lineObj.layer = 5;
    }
    
    /// <summary>
    /// Creates a simple quad mesh.
    /// </summary>
    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "PlaceholderLineQuad";
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };
        
        int[] triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Sets the title text.
    /// </summary>
    public void SetTitle(string title)
    {
        titleText = title;
        
        // Update existing title if already generated
        if (isGenerated)
        {
            Transform titleTransform = transform.Find("PlaceholderTitle");
            if (titleTransform != null)
            {
                TextMeshPro tmp = titleTransform.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text = title;
                }
            }
        }
    }
    
    /// <summary>
    /// Regenerates the placeholder content.
    /// </summary>
    public void Regenerate()
    {
        isGenerated = false;
        GeneratePlaceholderContent();
    }
}
