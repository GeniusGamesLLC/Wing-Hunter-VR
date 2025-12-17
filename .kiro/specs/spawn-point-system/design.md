# Spawn Point System - Design Document

## Overview

The Spawn Point System standardizes how spawn points and target points are created, named, and managed in the VR Duck Hunt game. This system replaces the current ad-hoc approach (mixed naming conventions, inconsistent indicators, pink materials) with a prefab-based architecture that ensures consistency, maintainability, and future extensibility.

The system introduces:
- Standardized prefabs for spawn and target points
- URP-compatible indicator materials (green for spawn, red for target)
- Optional occluder support for visual effects (clouds, bushes)
- Debug visibility controls
- Migration tooling for existing scene points

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Spawn Point System                        │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │  SpawnPoint      │      │  TargetPoint     │            │
│  │  Prefab          │      │  Prefab          │            │
│  │  ┌────────────┐  │      │  ┌────────────┐  │            │
│  │  │ Indicator  │  │      │  │ Indicator  │  │            │
│  │  │ (Green)    │  │      │  │ (Red)      │  │            │
│  │  └────────────┘  │      │  └────────────┘  │            │
│  │  ┌────────────┐  │      │  ┌────────────┐  │            │
│  │  │ Occluder   │  │      │  │ Occluder   │  │            │
│  │  │ (Optional) │  │      │  │ (Optional) │  │            │
│  │  └────────────┘  │      │  └────────────┘  │            │
│  └──────────────────┘      └──────────────────┘            │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              SpawnPointManager                        │  │
│  │  - Manages all spawn/target points                    │  │
│  │  - Controls indicator visibility                      │  │
│  │  - Provides point lookup by index                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         SpawnPointMigrationTool (Editor)              │  │
│  │  - Converts existing points to prefab instances       │  │
│  │  - Standardizes naming conventions                    │  │
│  │  - Preserves positions and rotations                  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### SpawnPointMarker

**Purpose**: Component attached to spawn point prefabs to identify and configure them.

**Public Interface**:
```csharp
public class SpawnPointMarker : MonoBehaviour
{
    public int PointIndex { get; set; }
    public SpawnPointType PointType { get; }
    public Transform Indicator { get; }
    public Transform OccluderSlot { get; }
    public GameObject OccluderInstance { get; }
    
    public void SetIndicatorVisible(bool visible);
    public void SetOccluder(GameObject occluderPrefab);
    public void ClearOccluder();
}

public enum SpawnPointType
{
    Spawn,
    Target
}
```

**Key Behaviors**:
- Stores point index for pairing spawn/target points
- Manages indicator visibility
- Handles occluder instantiation and positioning
- Provides type identification for spawn vs target

### SpawnPointManager

**Purpose**: Central manager for all spawn and target points in the scene.

**Public Interface**:
```csharp
public class SpawnPointManager : MonoBehaviour
{
    public SpawnPointMarker[] SpawnPoints { get; }
    public SpawnPointMarker[] TargetPoints { get; }
    public bool ShowIndicators { get; set; }
    
    public SpawnPointMarker GetSpawnPoint(int index);
    public SpawnPointMarker GetTargetPoint(int index);
    public (SpawnPointMarker spawn, SpawnPointMarker target) GetPointPair(int index);
    public void SetAllIndicatorsVisible(bool visible);
    public void RefreshPointLists();
}
```

**Key Behaviors**:
- Auto-discovers spawn and target points in scene hierarchy
- Provides indexed access to point pairs
- Controls global indicator visibility
- Validates point naming and pairing

### SpawnPointMigrationTool (Editor Only)

**Purpose**: Editor utility to migrate existing spawn points to the new prefab system.

**Public Interface**:
```csharp
public static class SpawnPointMigrationTool
{
    [MenuItem("Tools/Spawn Points/Migrate to Prefab System")]
    public static void MigrateSpawnPoints();
    
    public static MigrationResult MigratePoints(
        Transform spawnPointsParent,
        Transform targetPointsParent,
        GameObject spawnPointPrefab,
        GameObject targetPointPrefab
    );
}

public struct MigrationResult
{
    public int SpawnPointsMigrated;
    public int TargetPointsMigrated;
    public int PointsRenamed;
    public string[] Warnings;
}
```

**Key Behaviors**:
- Finds all existing spawn/target points by name pattern
- Replaces GameObjects with prefab instances
- Preserves world position and rotation
- Renames points to standardized convention
- Reports migration statistics

## Data Models

### SpawnPointConfig

```csharp
[CreateAssetMenu(fileName = "SpawnPointConfig", menuName = "Game/Spawn Point Config")]
public class SpawnPointConfig : ScriptableObject
{
    [Header("Indicator Settings")]
    public Material SpawnIndicatorMaterial;
    public Material TargetIndicatorMaterial;
    public float IndicatorScale = 0.3f;
    
    [Header("Visibility")]
    public bool ShowIndicatorsInEditor = true;
    public bool ShowIndicatorsInPlayMode = false;
    public bool DebugShowIndicators = false;
    
    [Header("Occluder")]
    public GameObject DefaultOccluderPrefab;
    public Vector3 OccluderOffset = Vector3.zero;
    public float OccluderScale = 1f;
}
```

### Naming Convention

```
SpawnPoints/
├── SpawnPoint_00
├── SpawnPoint_01
├── SpawnPoint_02
└── ...

TargetPoints/
├── TargetPoint_00
├── TargetPoint_01
├── TargetPoint_02
└── ...
```

- Index is zero-padded to 2 digits (00-99)
- SpawnPoint_XX pairs with TargetPoint_XX
- Points are children of their respective parent containers

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Spawn and target points follow naming convention with matching indices

*For any* spawn point in the scene, its name should match the pattern "SpawnPoint_XX" where XX is a zero-padded two-digit index, and there should exist a corresponding "TargetPoint_XX" with the same index.
**Validates: Requirements 1.1, 1.2, 1.3**

### Property 2: Points are organized under correct parent containers

*For any* spawn point, its parent should be named "SpawnPoints", and for any target point, its parent should be named "TargetPoints".
**Validates: Requirements 1.4, 1.5**

### Property 3: Point prefabs include Indicator child

*For any* spawn point or target point prefab instance, it should have a child GameObject named "Indicator".
**Validates: Requirements 2.4, 2.5**

### Property 4: Indicator materials use URP Lit shader

*For any* indicator's material, the shader should be "Universal Render Pipeline/Lit" to ensure Quest compatibility.
**Validates: Requirements 3.1, 3.5**

### Property 5: Spawn indicators use green color

*For any* spawn point indicator, its material's base color should be green (RGB approximately 0, 1, 0).
**Validates: Requirements 3.3**

### Property 6: Target indicators use red color

*For any* target point indicator, its material's base color should be red (RGB approximately 1, 0, 0).
**Validates: Requirements 3.4**

### Property 7: Indicator toggle affects all points

*For any* call to SetAllIndicatorsVisible, all spawn and target point indicators should have matching visibility state.
**Validates: Requirements 4.4**

### Property 8: Occluder position matches spawn point

*For any* spawn point with an assigned occluder, the occluder's world position should equal the spawn point's world position plus any configured offset.
**Validates: Requirements 5.2**

### Property 9: Migration preserves positions and applies naming convention

*For any* set of spawn points before migration, after migration each point should have the same world position and rotation, and all names should match the standardized naming pattern.
**Validates: Requirements 6.2, 6.3, 6.4**

### Property 10: Migration reports accurate count

*For any* migration operation, the reported count of migrated points should equal the actual number of points processed.
**Validates: Requirements 6.5**

## Error Handling

### Missing References

- **Missing Indicator**: If indicator child is missing, log warning and skip visibility operations
- **Missing Parent Container**: If SpawnPoints/TargetPoints parent doesn't exist, create it automatically
- **Missing Prefab**: If prefab reference is null during migration, abort with error message

### Invalid State

- **Duplicate Indices**: If two points have the same index, log error and skip the duplicate
- **Orphaned Points**: If a spawn point has no matching target point, log warning but continue
- **Invalid Names**: If a point name doesn't match expected pattern, include in migration candidates

### Migration Errors

- **Prefab Not Found**: Display error dialog and abort migration
- **Scene Not Saved**: Prompt user to save scene before migration
- **Undo Support**: Register all changes with Undo system for rollback

## Testing Strategy

### Unit Testing Framework

The project uses Unity Test Framework (UTF) with NUnit. Tests are located in `Assets/Tests/EditMode/` for editor tests and `Assets/Tests/PlayMode/` for runtime tests.

### Property-Based Testing Framework

Property-based tests use custom generators built on NUnit with randomized inputs. Each property test runs a minimum of 100 iterations.

Each property-based test will be tagged with:
`// Feature: spawn-point-system, Property {number}: {property_text}`

### Unit Test Coverage

- **SpawnPointMarker**: Indicator visibility toggle, occluder assignment/clearing
- **SpawnPointManager**: Point discovery, indexed access, visibility control
- **SpawnPointMigrationTool**: Name parsing, position preservation, prefab replacement

### Property-Based Test Coverage

- Generate random point configurations and verify naming conventions
- Generate random positions and verify migration preserves them
- Generate random visibility states and verify toggle affects all points
- Generate random occluder assignments and verify positioning

### Integration Testing

- Full migration workflow: create legacy points → run migration → verify results
- Indicator visibility: toggle in editor → enter play mode → verify hidden → enable debug → verify shown
- Occluder system: assign occluder → verify position → swap occluder → verify update

### Manual Testing

- Visual verification of indicator colors in VR headset
- Confirm no pink materials on Quest device
- Test occluder visual effect (ducks emerging from clouds)

## Implementation Notes

### Prefab Structure

**SpawnPoint.prefab**:
```
SpawnPoint (SpawnPointMarker component)
├── Indicator (Sphere mesh, green URP material, scale 0.3)
└── OccluderSlot (Empty transform for occluder parenting)
```

**TargetPoint.prefab**:
```
TargetPoint (SpawnPointMarker component)
├── Indicator (Sphere mesh, red URP material, scale 0.3)
└── OccluderSlot (Empty transform for occluder parenting)
```

### Material Setup

Create two URP Lit materials:
- `SpawnIndicatorMaterial.mat`: Base color green (0, 0.8, 0, 1), emission enabled for visibility
- `TargetIndicatorMaterial.mat`: Base color red (0.8, 0, 0, 1), emission enabled for visibility

### Migration Algorithm

1. Find all GameObjects matching "SpawnPoint*" or "TargetPoint*" patterns
2. Sort by current name to establish index order
3. For each point:
   a. Record world position and rotation
   b. Instantiate appropriate prefab
   c. Set position and rotation
   d. Rename to standardized format
   e. Parent to correct container
   f. Delete original GameObject
4. Return migration statistics

### Compatibility with Existing SpawnManager

The existing `SpawnManager` script auto-discovers points by finding children of "SpawnPoints" and "TargetPoints" containers. The new system maintains this structure, so no changes to `SpawnManager` are required.

### Future Occluder Ideas

- Cloud prefab: Fluffy cloud that ducks fly out of
- Bush prefab: Foliage that ducks emerge from
- Portal prefab: Magical portal effect
- Barn door prefab: Ducks fly out of barn opening

