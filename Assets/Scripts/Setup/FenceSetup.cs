using UnityEngine;

/// <summary>
/// Sets up the wooden fence play area boundary.
/// Creates a rustic post-and-rail fence around the player area.
/// </summary>
public class FenceSetup : MonoBehaviour
{
    [Header("Fence Dimensions")]
    [Tooltip("Width of the play area (X axis)")]
    public float playAreaWidth = 6f;
    
    [Tooltip("Depth of the play area (Z axis)")]
    public float playAreaDepth = 6f;
    
    [Tooltip("Height of the front fence (lower for shooting over)")]
    public float frontFenceHeight = 1.2f;
    
    [Tooltip("Height of side and back fences")]
    public float sideFenceHeight = 1.5f;
    
    [Header("Post Settings")]
    [Tooltip("Radius of fence posts")]
    public float postRadius = 0.06f;
    
    [Tooltip("Number of posts per side (including corners)")]
    public int postsPerSide = 4;
    
    [Header("Rail Settings")]
    [Tooltip("Height of rails (thickness)")]
    public float railHeight = 0.08f;
    
    [Tooltip("Depth of rails (thickness)")]
    public float railDepth = 0.04f;
    
    [Tooltip("Number of horizontal rails")]
    public int numberOfRails = 3;
    
    [Header("Material")]
    public Material woodMaterial;
    
    [Header("Center Offset")]
    [Tooltip("Center position of the play area")]
    public Vector3 centerOffset = new Vector3(0, 0, -23f);
    
    private GameObject fenceParent;
    
    void Start()
    {
        // Auto-setup if material is assigned
        if (woodMaterial != null)
        {
            SetupFence();
        }
    }
    
    [ContextMenu("Setup Fence")]
    public void SetupFence()
    {
        // Clean up existing fence
        CleanupFence();
        
        // Create parent object
        fenceParent = new GameObject("PlayAreaFence");
        fenceParent.transform.position = centerOffset;
        
        // Calculate half dimensions
        float halfWidth = playAreaWidth / 2f;
        float halfDepth = playAreaDepth / 2f;
        
        // Create front fence (low, facing +Z direction from player)
        CreateFenceSide("FrontFence", 
            new Vector3(-halfWidth, 0, halfDepth), 
            new Vector3(halfWidth, 0, halfDepth), 
            frontFenceHeight, true);
        
        // Create back fence (tall, behind player)
        CreateFenceSide("BackFence", 
            new Vector3(-halfWidth, 0, -halfDepth), 
            new Vector3(halfWidth, 0, -halfDepth), 
            sideFenceHeight, true);
        
        // Create left fence (tall)
        CreateFenceSide("LeftFence", 
            new Vector3(-halfWidth, 0, -halfDepth), 
            new Vector3(-halfWidth, 0, halfDepth), 
            sideFenceHeight, false);
        
        // Create right fence (tall)
        CreateFenceSide("RightFence", 
            new Vector3(halfWidth, 0, -halfDepth), 
            new Vector3(halfWidth, 0, halfDepth), 
            sideFenceHeight, false);
        
        Debug.Log($"[FenceSetup] Created play area fence at {centerOffset}");
    }
    
    private void CreateFenceSide(string name, Vector3 start, Vector3 end, float height, bool isHorizontal)
    {
        GameObject side = new GameObject(name);
        side.transform.SetParent(fenceParent.transform);
        side.transform.localPosition = Vector3.zero;
        
        float length = Vector3.Distance(start, end);
        Vector3 direction = (end - start).normalized;
        
        // Create posts
        for (int i = 0; i < postsPerSide; i++)
        {
            float t = (float)i / (postsPerSide - 1);
            Vector3 postPos = Vector3.Lerp(start, end, t);
            CreatePost(side.transform, postPos, height, $"Post_{i}");
        }
        
        // Create rails
        float railSpacing = height / (numberOfRails + 1);
        for (int i = 1; i <= numberOfRails; i++)
        {
            float railY = railSpacing * i;
            Vector3 railCenter = (start + end) / 2f + Vector3.up * railY;
            CreateRail(side.transform, railCenter, length, direction, $"Rail_{i}");
        }
        
        // Add a box collider for the entire fence side
        AddFenceCollider(side, start, end, height, isHorizontal);
    }
    
    private void CreatePost(Transform parent, Vector3 localPos, float height, string name)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = name;
        post.transform.SetParent(parent);
        post.transform.localPosition = localPos + Vector3.up * (height / 2f);
        post.transform.localScale = new Vector3(postRadius * 2f, height / 2f, postRadius * 2f);
        
        // Apply material
        if (woodMaterial != null)
        {
            post.GetComponent<Renderer>().material = woodMaterial;
        }
        
        // Remove individual collider (we use one big collider per side)
        Destroy(post.GetComponent<Collider>());
    }
    
    private void CreateRail(Transform parent, Vector3 center, float length, Vector3 direction, string name)
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = name;
        rail.transform.SetParent(parent);
        rail.transform.localPosition = center;
        
        // Calculate rotation to align with fence direction
        if (direction != Vector3.zero)
        {
            rail.transform.localRotation = Quaternion.LookRotation(direction);
        }
        
        // Scale: length along Z (forward), height on Y, depth on X
        rail.transform.localScale = new Vector3(railDepth, railHeight, length - postRadius * 2f);
        
        // Apply material
        if (woodMaterial != null)
        {
            rail.GetComponent<Renderer>().material = woodMaterial;
        }
        
        // Remove individual collider
        Destroy(rail.GetComponent<Collider>());
    }
    
    private void AddFenceCollider(GameObject side, Vector3 start, Vector3 end, float height, bool isHorizontal)
    {
        BoxCollider collider = side.AddComponent<BoxCollider>();
        
        Vector3 center = (start + end) / 2f + Vector3.up * (height / 2f);
        float length = Vector3.Distance(start, end);
        
        collider.center = center;
        
        if (isHorizontal)
        {
            // Fence runs along X axis
            collider.size = new Vector3(length, height, railDepth * 2f);
        }
        else
        {
            // Fence runs along Z axis
            collider.size = new Vector3(railDepth * 2f, height, length);
        }
    }
    
    [ContextMenu("Cleanup Fence")]
    public void CleanupFence()
    {
        if (fenceParent != null)
        {
            if (Application.isPlaying)
            {
                Destroy(fenceParent);
            }
            else
            {
                DestroyImmediate(fenceParent);
            }
        }
        
        // Also find and destroy any existing fence
        GameObject existing = GameObject.Find("PlayAreaFence");
        if (existing != null)
        {
            if (Application.isPlaying)
            {
                Destroy(existing);
            }
            else
            {
                DestroyImmediate(existing);
            }
        }
    }
}
