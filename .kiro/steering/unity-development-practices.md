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

### 6.1 Script Compilation Wait Times - CRITICAL
**Unity recompilation takes 15-30+ seconds. You MUST wait for it to complete before making more changes.**

**After creating or modifying ANY C# script:**
1. **STOP and WAIT** - Do NOT immediately make another MCP call or file write
2. **USE executeBash with timeout** - Run `sleep 30` to force a 30-second wait before checking compilation
3. **Check compilation status** - Use `mcp_unityMCP_read_console` to verify no errors
4. **Verify Unity is responsive** - Use `mcp_unityMCP_manage_editor` with `action="telemetry_status"`
5. **Only then proceed** - After confirming Unity has finished compiling

**MANDATORY WAIT PROCEDURE after script changes:**
```
1. Write/modify script via fsWrite or strReplace
2. IMMEDIATELY run: executeBash with command="sleep 30" (forces 30 second wait)
3. THEN check console for compilation errors with mcp_unityMCP_read_console
4. Verify telemetry_status returns successfully
5. ONLY THEN make next change or MCP call
```

**Signs Unity is still compiling:**
- Console shows "Compiling..." or script reload messages
- Unity editor shows spinning wheel
- MCP calls return errors or timeouts
- `telemetry_status` fails or returns slowly
- Console still shows OLD errors that should have been fixed

**NEVER do this:**
- Create multiple scripts in rapid succession
- Modify a script then immediately call MCP to use it
- Write script A, then immediately write script B
- Assume compilation is instant
- Check console immediately after script changes (wait 30 seconds first!)

**If you see compilation errors:**
- STOP all operations
- Fix the error in the script
- Run `sleep 30` to wait for recompilation
- Verify console is clear before continuing

### 6.2 Asset Import and Meta Files - AVOID LOOPS
- **After creating new assets (prefabs, materials, scripts) via fsWrite:**
  - Unity may not immediately generate `.meta` files or recognize the asset
  - **DO NOT loop checking for meta files** - this wastes credits and time
  - **Check ONCE**, then if the asset shows as "Unknown" or has no meta file:
    - Trigger asset refresh via MCP: `mcp_unityMCP_execute_menu_item` with `menu_path="Assets/Refresh"`
    - **WAIT after refresh** - Unity needs time to process. Do NOT immediately check again.
    - After triggering refresh, wait a moment then verify the asset is recognized
  - The asset will be properly imported after refresh completes
- **Signs that Unity needs a refresh:**
  - `assetType: "Unknown"` in get_info response
  - Missing `.meta` file after creating an asset
  - GUID is empty in asset info
- **NEVER poll/loop waiting for Unity to auto-import** - trigger refresh once and wait
- **Unity is slower than MCP calls** - always give Unity time to process after refresh commands

### 6.3 Unity MCP Known Issues (WebSocket Version)
- **`get_components` (plural) is BROKEN** - Do NOT use `action="get_components"` - it will fail
- **`get_component` (singular) CRASHES UNITY** - Do NOT use `action="get_component"` - it causes Unity to crash
- **AVOID querying component details via MCP** - Instead, read the script files directly or use Editor menu scripts to inspect components
- If you need to know what components are on a GameObject, use `action="find"` which returns `componentNames` in the result

### 7. Task Execution Strategy
- **ALWAYS verify Unity connection before starting any Unity task**
- Execute tasks one at a time, not in batches
- Wait for explicit user confirmation before proceeding to next task
- Include compilation verification steps in task workflows
- **Save scene and project after each major step**
- Stop and ask for guidance if Unity becomes unresponsive
- **Never create scripts as workarounds when Unity is disconnected**

### 8. MCP vs Editor Menu Scripts - WHEN TO USE EACH

**Use MCP commands for SMALL changes (1-5 operations):**
- Creating/modifying a single GameObject
- Adding/removing one component
- Changing a few properties
- Quick scene queries and inspections

**Use Editor Menu Scripts for LARGE/BATCH updates:**
- Modifying multiple GameObjects at once
- Bulk material assignments
- Scene-wide property changes
- Complex multi-step operations (10+ changes)
- Operations that would require many sequential MCP calls

**Editor Menu Script Pattern - CORRECT:**
```csharp
using UnityEngine;
using UnityEditor;

// IMPORTANT: Use a STATIC CLASS, NOT a class that extends Editor
public static class BatchOperationMenu
{
    [MenuItem("Tools/My Batch Operation")]
    public static void RunBatchOperation()
    {
        // Perform all operations here
        // Unity handles undo, compilation, etc.
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Batch operation complete!");
    }
}
```

**COMMON MISTAKES TO AVOID:**
```csharp
// WRONG - extending Editor breaks menu items for non-inspector scripts
public class MySetup : Editor  // DON'T DO THIS
{
    [MenuItem("Tools/Setup")]
    public static void Setup() { }
}

// WRONG - non-static class requires instance
public class MySetup  // DON'T DO THIS for menu-only scripts
{
    [MenuItem("Tools/Setup")]
    public static void Setup() { }
}

// CORRECT - static class with static methods
public static class MySetup  // DO THIS
{
    [MenuItem("Tools/Setup")]
    public static void Setup() { }
}
```

**Static Method Requirements in Static Classes:**
When using a static class, you must prefix Unity API calls with their class names:
- `Object.FindObjectOfType<T>()` instead of `FindObjectOfType<T>()`
- `Object.DestroyImmediate(obj)` instead of `DestroyImmediate(obj)`
- `Object.Instantiate(prefab)` instead of `Instantiate(prefab)`

**CRITICAL: Always clean up menu scripts after use!**
- Delete the Editor script once the task is complete
- These are one-time tools, not permanent project code
- Leaving them clutters the Tools menu and project

**Why menu scripts are better for large updates:**
- Single compilation cycle instead of many MCP round-trips
- Unity handles undo/redo properly
- More reliable than sequential MCP calls
- Faster execution for bulk operations
- Less chance of Unity becoming unresponsive

### 9. Direct Problem Solving - CRITICAL
- **ALWAYS fix root causes directly** - use MCP for small fixes, Editor scripts for large ones
- **NEVER create runtime "fixer" scripts** as band-aids for Unity issues
- **Use Unity MCP to investigate, identify, and fix problems directly**
- **Scripts that "fix" Unity scene issues at runtime often cause more problems**
- **Examples of direct fixes:**
  - Duplicate objects → Delete duplicates with Unity MCP (few) or Editor script (many)
  - Wrong materials → Apply correct materials with Unity MCP (few) or Editor script (many)
  - Missing components → Add components with Unity MCP
  - Wrong settings → Modify settings with Unity MCP
- **Avoid creating MonoBehaviour scripts that:**
  - "Fix" materials at runtime
  - "Setup" scene objects automatically on Start/Awake
  - "Correct" Unity configuration issues during gameplay
  - Work around Unity problems instead of solving them

### 10. Fix Assets at the Source - CRITICAL
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

### 11. Git Commit Efficiency
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