---
inclusion: always
---

# Wing Hunter VR - Project Notes

## Gun System

### Current Focus
- **Do NOT modify both guns at once** - each gun has different settings and needs individual tuning
- When fixing one gun, don't assume the same fix applies to others

### Muzzle Points - Setup Convention
Each gun prefab MUST have a `MuzzlePoint` child GameObject for shooting to work correctly.

**IMPORTANT: Use the MuzzlePoint Prefab as a Nested Prefab**
- The MuzzlePoint MUST be added as a **nested prefab** (not just copied content)
- Location: `Assets/Prefabs/MuzzlePoint.prefab`
- The prefab contains the DirectionIndicator arrow for visual debugging

**Setup Steps for New Guns (in Unity Editor):**
1. Open the gun prefab in Prefab Mode (double-click the prefab)
2. In Project window, find `Assets/Prefabs/MuzzlePoint.prefab`
3. **Drag** the MuzzlePoint prefab into the gun's hierarchy (as a child of the root)
4. Position it at the barrel tip (where bullets exit)
5. Rotate it so its local +Z axis points OUT of the barrel
6. Save the prefab (Ctrl+S)

**Why Nested Prefab Matters:**
- Changes to MuzzlePoint.prefab automatically propagate to all guns
- Maintains consistency across all weapons
- The blue prefab icon in hierarchy confirms proper nesting

**MCP/Kiro Limitation - CRITICAL:**
- MCP commands like `manage_gameobject` with `save_as_prefab` do NOT persist nested prefab children
- Opening prefab stage, adding children, and saving does NOT work - children are lost on close
- **WORKAROUND:** Write the prefab YAML file directly using `fsWrite` instead of MCP commands
- Reference existing gun prefabs (e.g., `N-ZAP_85_Gun.prefab`) as templates for the YAML structure
- Key elements in the YAML:
  - Root GameObject with Transform listing children
  - `PrefabInstance` blocks for each nested prefab (Model, MuzzlePoint)
  - `stripped Transform` references linking to the nested prefab instances
  - GUIDs reference the source prefabs (model import prefab, MuzzlePoint.prefab)
- Always verify in Unity that MuzzlePoint shows the blue prefab icon
- If not nested properly, delete and re-add by dragging from Project window

**How to Verify Muzzle Direction:**
- Select the MuzzlePoint in the scene
- In Scene view toolbar, switch from "Global" to "Local" pivot mode
- The blue arrow (Z-axis) should point out the barrel
- OR: Enter Play mode and select ShootingController - the red Gizmo ray shows actual shoot direction
- OR: Use `GunRayVisualizer` component for runtime debug visualization

**Important:**
- Do NOT modify existing muzzle bones (breaks mesh/textures)
- Create a NEW child `MuzzlePoint` with correct rotation instead
- Do NOT use code workarounds (`-transform.forward`, `invertDirection`, etc.)

### Creating New Gun Prefabs (via Kiro/MCP)

**DO NOT use MCP commands to create gun prefabs with nested children. Use direct file writing instead.**

**Steps:**
1. Copy an existing gun prefab YAML as template (e.g., `N-ZAP_85_Gun.prefab`)
2. Update the following in the YAML:
   - Root GameObject name (e.g., `Golden_Gun`)
   - Model prefab GUID (find in the imported model's `.prefab` file or via asset search)
   - Model scale (varies by import - N-ZAP uses 10, Golden Gun uses 0.01)
   - MuzzlePoint position (y and z values for barrel tip)
3. Write the file using `fsWrite` to `Assets/Prefabs/Guns/{GunName}.prefab`
4. Unity will auto-import and the nested prefabs will be properly linked

**Key GUIDs to know:**
- MuzzlePoint prefab: `8f199b0543db74e71828b8fdc5446b2c`
- Find model GUIDs via: `mcp_unityMCP_manage_asset` with `action="get_info"`

### Material/Shader Compatibility - CRITICAL

**Pink materials = shader not compatible with URP (Universal Render Pipeline)**

**When importing new gun models, ALWAYS check materials:**
1. Use `mcp_unityMCP_manage_material` with `action="get_material_info"` to check shader
2. If shader is "Standard", "Standard (Specular setup)", or any non-URP shader → FIX IT
3. Convert to `Universal Render Pipeline/Lit` shader

**How to fix pink materials:**
- Edit the `.mat` file directly using `fsWrite`
- Change `m_Shader` to URP Lit: `{fileID: 4800000, guid: 933532a4fcc9baf4fa0491de14d08ed7, type: 3}`
- Update keywords: replace `_SPECGLOSSMAP` with `_SPECULAR_SETUP`
- Add `_BaseMap` texture entry (copy from `_MainTex`)
- Add `_BaseColor` (copy from `_Color`)
- Add URP-specific floats: `_Surface: 0`, `_WorkflowMode: 0`, `_ReceiveShadows: 1`

**URP Lit shader GUID:** `933532a4fcc9baf4fa0491de14d08ed7`

**Checklist for new gun imports:**
- [ ] Check material shader compatibility
- [ ] Convert materials to URP if needed
- [ ] Verify gun renders correctly (not pink) in Scene view
- [ ] Set up gun prefab with nested Model and MuzzlePoint

## VR Setup

### Controller Mapping
- Right trigger: Shoot
- Right B button: Switch gun (debug feature)

### Key Components
- `ShootingController`: Handles shooting input and raycasting
- `GunSelectionManager`: Manages gun instantiation and switching
- `GunRayVisualizer`: Debug ray visualization from muzzle
