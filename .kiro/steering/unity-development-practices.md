---
inclusion: always
---

# Unity Development Best Practices

## Compilation Safety Guidelines

When working with Unity projects, follow these practices to prevent crashes and compilation issues:

### 1. Paced Script Creation
- **Create one script at a time** - Never create multiple scripts in rapid succession
- **Wait for compilation** after each script creation or modification
- **Check Unity console** for errors before proceeding to next task
- **Verify spinning wheel stops** in Unity editor before continuing

### 2. Compilation Checkpoints
- Use explicit compilation checkpoint tasks between major implementations
- Always check `EditorApplication.isCompiling` status when possible
- Force refresh with `AssetDatabase.Refresh()` if Unity seems unresponsive
- Monitor Unity console output for compilation errors

### 3. Script Organization
- **Avoid namespaces** unless absolutely necessary (can cause compilation issues)
- Keep scripts simple and focused on single responsibilities
- Use clear, descriptive class and method names
- Follow Unity naming conventions (PascalCase for public members)

### 4. Scene Safety Practices
- **Save scene after each modification** - Use Ctrl+S or File > Save Scene frequently
- **Save project after major changes** - Use Ctrl+Shift+S or File > Save Project
- **Create scene backups** before major modifications
- **Test scene changes incrementally** - Don't make multiple complex changes at once
- **Verify scene integrity** after each GameObject addition/modification

### 5. Error Handling
- Always check Unity console after script changes
- Address compilation errors immediately before proceeding
- Use Unity's built-in logging (`Debug.Log`, `Debug.LogWarning`, `Debug.LogError`)
- Test scripts in isolation when possible

### 6. MCP Server Interaction
- Allow Unity time to process changes between MCP calls
- Check Unity's responsiveness with telemetry status calls
- Use batch operations sparingly to avoid overwhelming Unity
- Monitor Unity process stability during automated operations

### 7. Task Execution Strategy
- Execute tasks one at a time, not in batches
- Wait for explicit user confirmation before proceeding to next task
- Include compilation verification steps in task workflows
- **Save scene and project after each major step**
- Stop and ask for guidance if Unity becomes unresponsive

## Unity-Specific Code Patterns

### MonoBehaviour Scripts
```csharp
using UnityEngine;

public class ExampleController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    
    private void Start()
    {
        // Initialization code
    }
    
    private void Update()
    {
        // Frame update code
    }
}
```

### ScriptableObject Configuration
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "New Config", menuName = "Game/Config")]
public class GameConfig : ScriptableObject
{
    [Header("Game Settings")]
    public float gameSpeed = 1f;
    public int maxLives = 3;
}
```

### Event-Driven Architecture
```csharp
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class GameEvent : UnityEvent<int> { }
    
    public GameEvent OnScoreChanged;
    
    private void TriggerScoreChange(int newScore)
    {
        OnScoreChanged?.Invoke(newScore);
    }
}
```

## Scene Management Commands

Use these Unity MCP commands for safe scene operations:

```
# Save current scene
mcp_unityMCP_manage_scene with action="save"

# Check scene status
mcp_unityMCP_manage_scene with action="get_active"

# Create GameObject safely
mcp_unityMCP_manage_gameobject with action="create"

# Modify GameObject properties incrementally
mcp_unityMCP_manage_gameobject with action="modify"
```

## Compilation Verification Commands

Use these Unity MCP commands to verify compilation status:

```
# Check console for errors
mcp_unityMCP_read_console

# Verify Unity responsiveness  
mcp_unityMCP_manage_editor with telemetry_status

# Force asset refresh
AssetDatabase.Refresh() via script execution
```

## Scene Modification Workflow

When modifying scenes, follow this safe workflow:

1. **Before Changes**: Save current scene state
2. **Make Change**: Perform single modification (add GameObject, change property, etc.)
3. **Verify**: Check that change applied correctly
4. **Save**: Save scene immediately after verification
5. **Test**: Test the change works as expected
6. **Repeat**: Only then proceed to next modification

## Emergency Recovery

If Unity becomes unresponsive:
1. Stop all automated operations immediately
2. Check Unity console for error messages
3. Save any unsaved work manually (scene and project)
4. Consider restarting Unity if necessary
5. Resume operations only after stability is confirmed
6. **Check for scene corruption** - verify scene loads correctly after restart

## Critical Save Points

Always save at these points:
- After creating new GameObjects
- After modifying GameObject properties
- After adding/removing components
- After changing scene hierarchy
- Before running any tests
- After completing each task step

Remember: **Stability over speed** - It's better to work slowly and maintain a stable Unity environment than to rush and cause crashes that require recovery time.