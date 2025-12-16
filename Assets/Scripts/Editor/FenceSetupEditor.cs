using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create the wooden fence play area boundary.
/// Run from menu: Tools > VR Duck Hunt > Create Play Area Fence
/// </summary>
public class FenceSetupEditor : Editor
{
    // Fence dimensions
    private const float PlayAreaWidth = 6f;
    private const float PlayAreaDepth = 6f;
    private const float FenceHeight = 1.0f; // Uniform waist-height fence on all sides
    
    // Post settings
    private const float PostRadius = 0.06f;
    private const int PostsPerSide = 4;
    
    // Rail settings
    private const float RailHeight = 0.08f;
    private const float RailDepth = 0.04f;
    private const int NumberOfRails = 3;
    
    // Center position (player start position)
    private static readonly Vector3 CenterOffset = new Vector3(0, 0, -23f);
    
    [MenuItem("Tools/VR Duck Hunt/Create Play Area Fence")]
    public static void CreateFence()
    {
        // Load wood material
        Material woodMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WoodFenceMaterial.mat");
        if (woodMaterial == null)
        {
            Debug.LogWarning("[FenceSetup] WoodFenceMaterial not found, using default material");
        }
        
        // Clean up existing fence
        CleanupFence();
        
        // Create parent object
        GameObject fenceParent = new GameObject("PlayAreaFence");
        fenceParent.transform.position = CenterOffset;
        
        // Find Environment parent
        GameObject envParent = GameObject.Find("--- Environment ---");
        if (envParent != null)
        {
            fenceParent.transform.SetParent(envParent.transform);
        }
        
        // Calculate half dimensions
        float halfWidth = PlayAreaWidth / 2f;
        float halfDepth = PlayAreaDepth / 2f;
        
        // Create front fence (at +Z from player)
        CreateFenceSide(fenceParent.transform, "FrontFence", 
            new Vector3(-halfWidth, 0, halfDepth), 
            new Vector3(halfWidth, 0, halfDepth), 
            FenceHeight, true, woodMaterial);
        
        // Create back fence (behind player)
        CreateFenceSide(fenceParent.transform, "BackFence", 
            new Vector3(-halfWidth, 0, -halfDepth), 
            new Vector3(halfWidth, 0, -halfDepth), 
            FenceHeight, true, woodMaterial);
        
        // Create left fence
        CreateFenceSide(fenceParent.transform, "LeftFence", 
            new Vector3(-halfWidth, 0, -halfDepth), 
            new Vector3(-halfWidth, 0, halfDepth), 
            FenceHeight, false, woodMaterial);
        
        // Create right fence
        CreateFenceSide(fenceParent.transform, "RightFence", 
            new Vector3(halfWidth, 0, -halfDepth), 
            new Vector3(halfWidth, 0, halfDepth), 
            FenceHeight, false, woodMaterial);
        
        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log($"[FenceSetup] Created play area fence at {CenterOffset}");
    }
    
    [MenuItem("Tools/VR Duck Hunt/Remove Play Area Fence")]
    public static void CleanupFence()
    {
        GameObject existing = GameObject.Find("PlayAreaFence");
        if (existing != null)
        {
            DestroyImmediate(existing);
            Debug.Log("[FenceSetup] Removed existing fence");
        }
    }
    
    private static void CreateFenceSide(Transform parent, string name, Vector3 start, Vector3 end, float height, bool isHorizontal, Material material)
    {
        GameObject side = new GameObject(name);
        side.transform.SetParent(parent);
        side.transform.localPosition = Vector3.zero;
        
        float length = Vector3.Distance(start, end);
        Vector3 direction = (end - start).normalized;
        
        // Create posts
        for (int i = 0; i < PostsPerSide; i++)
        {
            float t = (float)i / (PostsPerSide - 1);
            Vector3 postPos = Vector3.Lerp(start, end, t);
            CreatePost(side.transform, postPos, height, $"Post_{i}", material);
        }
        
        // Create rails
        float railSpacing = height / (NumberOfRails + 1);
        for (int i = 1; i <= NumberOfRails; i++)
        {
            float railY = railSpacing * i;
            Vector3 railCenter = (start + end) / 2f + Vector3.up * railY;
            CreateRail(side.transform, railCenter, length, direction, isHorizontal, $"Rail_{i}", material);
        }
        
        // Add a box collider for the entire fence side
        AddFenceCollider(side, start, end, height, isHorizontal);
    }
    
    private static void CreatePost(Transform parent, Vector3 localPos, float height, string name, Material material)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = name;
        post.transform.SetParent(parent);
        post.transform.localPosition = localPos + Vector3.up * (height / 2f);
        post.transform.localScale = new Vector3(PostRadius * 2f, height / 2f, PostRadius * 2f);
        
        // Apply material
        if (material != null)
        {
            post.GetComponent<Renderer>().sharedMaterial = material;
        }
        
        // Remove individual collider (we use one big collider per side)
        DestroyImmediate(post.GetComponent<Collider>());
    }
    
    private static void CreateRail(Transform parent, Vector3 center, float length, Vector3 direction, bool isHorizontal, string name, Material material)
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = name;
        rail.transform.SetParent(parent);
        rail.transform.localPosition = center;
        
        // Scale based on orientation
        if (isHorizontal)
        {
            // Rail runs along X axis
            rail.transform.localScale = new Vector3(length - PostRadius * 2f, RailHeight, RailDepth);
        }
        else
        {
            // Rail runs along Z axis
            rail.transform.localScale = new Vector3(RailDepth, RailHeight, length - PostRadius * 2f);
        }
        
        // Apply material
        if (material != null)
        {
            rail.GetComponent<Renderer>().sharedMaterial = material;
        }
        
        // Remove individual collider
        DestroyImmediate(rail.GetComponent<Collider>());
    }
    
    private static void AddFenceCollider(GameObject side, Vector3 start, Vector3 end, float height, bool isHorizontal)
    {
        BoxCollider collider = side.AddComponent<BoxCollider>();
        
        Vector3 center = (start + end) / 2f + Vector3.up * (height / 2f);
        float length = Vector3.Distance(start, end);
        
        collider.center = center;
        
        if (isHorizontal)
        {
            // Fence runs along X axis
            collider.size = new Vector3(length, height, RailDepth * 2f);
        }
        else
        {
            // Fence runs along Z axis
            collider.size = new Vector3(RailDepth * 2f, height, length);
        }
    }
}
