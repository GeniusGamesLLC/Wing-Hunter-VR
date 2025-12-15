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

### 6. MCP Server Interaction - CRITICAL
- **NEVER make parallel/simultaneous MCP calls** - Always wait for one call to complete before making another
- **ONE MCP call at a time** - Do not batch multiple Unity operations in the same turn
- **STOP ON FIRST ERROR** - If any MCP call fails or returns an error, STOP immediately and ask user to check Unity
- **NO RETRIES** - Do not retry failed MCP calls; ask user to verify Unity status first
- Allow Unity time to process changes between MCP calls (wait 2-3 seconds mentally)
- Check Unity's responsiveness with telemetry status calls before any operation
- Use batch operations sparingly to avoid overwhelming Unity
- Monitor Unity process stability during automated operations

### 6.1 Unity MCP Known Issues (localhost:8080 HTTP Server Version)
- **`get_components` (plural) is BROKEN** - Do NOT use `action="get_components"` - it will fail
- **`get_component` (singular) WORKS** - Use `action="get_component"` with `component_name` parameter instead
- When you need component info, query one component at a time using `get_component`

### 7. Task Execution Strategy
- **ALWAYS verify Unity connection before starting any Unity task**
- Execute tasks one at a time, not in batches
- Wait for explicit user confirmation before proceeding to next task
- Include compilation verification steps in task workflows
- **Save scene and project after each major step**
- Stop and ask for guidance if Unity becomes unresponsive
- **Never create scripts as workarounds when Unity is disconnected**

### 8. Direct Problem Solving - CRITICAL
- **ALWAYS fix root causes directly using Unity MCP commands**
- **NEVER create "fixer" scripts as band-aids for Unity issues**
- **Use Unity MCP to investigate, identify, and fix problems directly**
- **Scripts that "fix" Unity scene issues often cause more problems**
- **Examples of direct fixes:**
  - Duplicate objects → Delete duplicates with Unity MCP
  - Wrong materials → Apply correct materials with Unity MCP  
  - Missing components → Add components with Unity MCP
  - Wrong settings → Modify settings with Unity MCP
- **Avoid creating scripts that:**
  - "Fix" materials at runtime
  - "Setup" scene objects automatically
  - "Correct" Unity configuration issues
  - Work around Unity problems instead of solving them

### 9. Fix Assets at the Source - CRITICAL
- **ALWAYS fix prefabs, models, and assets directly** instead of adding code workarounds
- **When something is oriented wrong** (rotated, flipped, backwards):
  - Fix the prefab's transform rotation directly
  - Fix the model import settings
  - Do NOT add "invert" toggles or rotation compensation in code
- **When a muzzle point, spawn point, or reference point is wrong:**
  - Fix the position/rotation in the prefab itself
  - Do NOT add offset calculations in scripts
- **Examples of proper fixes:**
  - Gun pointing backwards → Rotate the prefab 180° on Y axis
  - Muzzle forward is inverted → Fix muzzle point rotation in prefab
  - Model imported sideways → Fix import rotation settings or prefab root
- **Code workarounds to avoid:**
  - `invertDirection` or `flipAxis` booleans
  - Runtime rotation compensation (`Quaternion.Euler(0, 180, 0)`)
  - Negative direction multipliers (`-transform.forward`)
  - Offset values that compensate for wrong asset orientation
- **Why this matters:** Workarounds create technical debt, confuse future developers, and often break when assets are updated or reused

### 10. Git Commit Efficiency
- **Keep commit responses concise** - Don't repeat the commit message details in the response
- The commit message already contains all necessary information
- Simply confirm "Changes committed successfully" or similar brief acknowledgment
- Avoid verbose summaries that duplicate what's already in the commit message

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

## Unity Connection Verification - CRITICAL

**BEFORE ANY Unity MCP operation, ALWAYS verify connection first:**

1. **MANDATORY**: Start every Unity task by running `mcp_unityMCP_manage_editor` with `action="telemetry_status"`
2. **MANDATORY**: Follow up with a test command like `mcp_unityMCP_manage_scene` with `action="get_active"`
3. **If EITHER command fails**: STOP immediately and ask user to fix Unity connection
4. **NEVER proceed** with Unity operations if connection verification fails
5. **NEVER create scripts as workarounds** when Unity is disconnected

## Unity Connection Issues - ABSOLUTE RULES

If Unity MCP commands fail with "No Unity plugins are currently connected" OR any Unity command returns an error OR returns "Python error":
1. **STOP IMMEDIATELY** - Do not make any more MCP calls
2. **Do not retry** - Even one retry can crash Unity
3. **Ask the user to check Unity status**: "Unity seems to be having issues. Please check if Unity is running and responsive, then let me know when it's ready to continue."
4. **Wait for explicit user confirmation** ("Unity is ready" or similar) before resuming
5. **Do not attempt to continue** with Unity-related tasks until user confirms
6. **Do not create scripts as fallback solutions** - fix the connection first
7. **After user confirms ready**: Start fresh with telemetry_status check before any operation

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