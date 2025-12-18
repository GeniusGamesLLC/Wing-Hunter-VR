# Multi-Point Pathing System - Design Document

## Overview

The Multi-Point Pathing System enhances duck flight behavior in the VR Duck Hunt game by enabling ducks to follow curved, multi-waypoint paths instead of simple straight lines. This creates more challenging and visually interesting gameplay while maintaining smooth, natural-looking flight motion.

The system introduces:
- Catmull-Rom spline interpolation for smooth curved paths
- Difficulty-based path complexity (more waypoints at higher difficulties)
- Hybrid waypoint system: pre-placed IntermediatePoint prefabs AND dynamic generation
- Arc-length parameterization for consistent flight speed along curves
- Runtime-toggleable debug visualization for paths and waypoints (orange for intermediates)
- Centralized debug settings system for future settings menu integration
- Optional occlusion support on pre-placed intermediate points (none by default)

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Multi-Point Pathing System                        │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                     DebugSettings (Singleton)                 │  │
│  │  - ShowSplinePaths: bool                                      │  │
│  │  - ShowWaypointIndicators: bool                               │  │
│  │  - OnSettingsChanged: event                                   │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                              │                                       │
│              ┌───────────────┼───────────────┐                      │
│              ▼               ▼               ▼                      │
│  ┌──────────────────┐ ┌──────────────┐ ┌──────────────────┐        │
│  │ FlightPathGenerator│ │SpawnPointMgr│ │  DuckController  │        │
│  │                    │ │  (updated)  │ │    (updated)     │        │
│  │ - GeneratePath()   │ │             │ │                  │        │
│  │ - GetCandidates()  │ │ Uses Debug  │ │ - FlightPath     │        │
│  │ - SelectWaypoints()│ │ Settings    │ │ - SplineFollower │        │
│  └────────┬───────────┘ └──────────────┘ │ - PathVisualizer│        │
│           │                               └────────┬─────────┘        │
│           ▼                                        │                  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                      SplineUtility (Static)                   │  │
│  │  - CatmullRomPoint(): Vector3                                 │  │
│  │  - CatmullRomTangent(): Vector3                               │  │
│  │  - CalculateArcLength(): float                                │  │
│  │  - GetPointAtArcLength(): Vector3                             │  │
│  │  - BuildArcLengthTable(): float[]                             │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                        FlightPath (Data)                      │  │
│  │  - Waypoints: Vector3[]                                       │  │
│  │  - TotalArcLength: float                                      │  │
│  │  - ArcLengthTable: float[]                                    │  │
│  │  - Seed: int                                                  │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### DebugSettings (Singleton)

**Purpose**: Centralized runtime-accessible debug settings that can be toggled during gameplay and integrated with a future settings menu.

**Public Interface**:
```csharp
public class DebugSettings : MonoBehaviour
{
    public static DebugSettings Instance { get; }
    
    public bool ShowSplinePaths { get; set; }
    public bool ShowWaypointIndicators { get; set; }
    public bool ShowSpawnPointIndicators { get; set; }
    public bool ShowTargetPointIndicators { get; set; }
    
    public event Action OnSettingsChanged;
    
    public void SetAllVisualization(bool enabled);
    public void NotifySettingsChanged();
}
```

**Key Behaviors**:
- Singleton pattern for global access
- Fires events when any setting changes
- Persists settings via PlayerPrefs (optional)
- Replaces individual debug flags in other components

### FlightPath (Data Class)

**Purpose**: Immutable data structure representing a complete flight path with precomputed arc-length data.

**Public Interface**:
```csharp
public class FlightPath
{
    public Vector3[] Waypoints { get; }
    public float TotalArcLength { get; }
    public float[] ArcLengthTable { get; }
    public int Seed { get; }
    public int IntermediateWaypointCount { get; }
    
    public Vector3 GetPositionAtDistance(float distance);
    public Vector3 GetTangentAtDistance(float distance);
    public Vector3 GetPositionAtTime(float normalizedTime);
    public float GetDistanceAtTime(float normalizedTime);
}
```

**Key Behaviors**:
- Stores waypoints array (spawn + intermediates + target)
- Precomputes arc-length lookup table for constant-speed traversal
- Provides position/tangent queries by distance or normalized time
- Immutable after construction

### SplineUtility (Static Class)

**Purpose**: Mathematical utilities for Catmull-Rom spline calculations.

**Public Interface**:
```csharp
public static class SplineUtility
{
    public static Vector3 CatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension = 0.5f);
    public static Vector3 CatmullRomTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension = 0.5f);
    public static float CalculateSegmentArcLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int samples = 20);
    public static float[] BuildArcLengthTable(Vector3[] waypoints, int samplesPerSegment = 20);
    public static Vector3 GetPointAtArcLength(Vector3[] waypoints, float[] arcLengthTable, float targetLength);
    public static Vector3 GetTangentAtArcLength(Vector3[] waypoints, float[] arcLengthTable, float targetLength);
    public static Vector3[] AddPhantomPoints(Vector3[] waypoints);
}
```

**Key Behaviors**:
- Catmull-Rom interpolation with configurable tension
- Arc-length parameterization for constant-speed movement
- Phantom point generation for paths with < 4 control points
- Tangent calculation for duck orientation

### FlightPathGenerator

**Purpose**: Generates flight paths using a hybrid approach - pre-placed IntermediatePoint prefabs when available, with dynamic generation as fallback.

**Public Interface**:
```csharp
public class FlightPathGenerator : MonoBehaviour
{
    public FlightPath GeneratePath(Vector3 spawnPoint, Vector3 targetPoint, int difficultyLevel, int? seed = null);
    public Vector3[] GetPreplacedIntermediatePoints();
    public Vector3 GenerateDynamicWaypoint(Vector3 spawnPoint, Vector3 targetPoint, float progressAlongPath, System.Random rng);
    public int GetWaypointCountForDifficulty(int difficultyLevel);
    
    [Header("Pre-placed Waypoints")]
    [SerializeField] private Transform intermediatePointsParent;
    [SerializeField] private bool preferPreplacedWaypoints = true;
    
    [Header("Dynamic Generation - Flight Zone")]
    [SerializeField] private Bounds flightZone;
    [SerializeField] private float minHeightAboveGround = 1.5f;
    [SerializeField] private float maxHeightAboveGround = 6f;
    
    [Header("Dynamic Generation - Deviation")]
    [SerializeField] private float lateralDeviationRange = 3f;
    [SerializeField] private float verticalDeviationRange = 1.5f;
    [SerializeField] private float minDistanceFromEndpoints = 2f;
    
    [Header("Spline Settings")]
    [SerializeField] private float splineTension = 0.5f;
}
```

**Key Behaviors**:
- Discovers pre-placed IntermediatePoint prefabs from scene hierarchy
- Prefers pre-placed waypoints when available and configured
- Falls back to dynamic generation when no pre-placed points exist
- Dynamic generation creates waypoints within flight zone bounds
- Supports deterministic generation via seed parameter
- Logs seed and waypoint source (pre-placed vs dynamic) for debugging

### IntermediatePointMarker

**Purpose**: Component for pre-placed intermediate waypoint prefabs (similar to SpawnPointMarker).

**Public Interface**:
```csharp
public class IntermediatePointMarker : MonoBehaviour
{
    public int PointIndex { get; set; }
    public Transform Indicator { get; }
    public Transform OccluderSlot { get; }
    public GameObject OccluderInstance { get; }
    
    public void SetIndicatorVisible(bool visible);
    public void SetOccluder(GameObject occluderPrefab);
    public void ClearOccluder();
}
```

**Key Behaviors**:
- Similar structure to SpawnPointMarker but for intermediate waypoints
- Orange indicator color (distinct from green spawn, red target)
- Optional occlusion slot (can be left empty for open-sky curves)
- Integrates with DebugSettings for visibility control

### DuckController (Updated)

**Purpose**: Extended to support spline-based movement along FlightPath.

**Additional Interface**:
```csharp
public partial class DuckController
{
    public FlightPath CurrentPath { get; private set; }
    
    public void Initialize(FlightPath path, float speed);
    
    private float distanceTraveled;
    private LineRenderer pathLineRenderer;
    
    private void UpdateSplineMovement();
    private void UpdatePathVisualization();
    private void OnDebugSettingsChanged();
}
```

**Key Behaviors**:
- Tracks distance traveled along spline
- Updates position using arc-length parameterization
- Orients duck to face movement direction (tangent)
- Manages LineRenderer for path visualization
- Subscribes to DebugSettings changes

### SpawnPointManager (Updated)

**Purpose**: Updated to use centralized DebugSettings.

**Changes**:
```csharp
public partial class SpawnPointManager
{
    // Remove: private bool debugShowIndicators;
    // Add: Reference to DebugSettings.Instance
    
    private void OnDebugSettingsChanged();
    private void UpdateIndicatorVisibility();
}
```

## Data Models

### FlightPathConfig

```csharp
[CreateAssetMenu(fileName = "FlightPathConfig", menuName = "Duck Hunt/Flight Path Config")]
public class FlightPathConfig : ScriptableObject
{
    [Header("Spline Settings")]
    [Range(0f, 1f)]
    public float SplineTension = 0.5f;
    
    [Range(10, 50)]
    public int ArcLengthSamples = 20;
    
    [Header("Flight Duration")]
    [Tooltip("Minimum time a duck must be in flight (seconds). Paths will be extended if needed.")]
    [Range(1f, 5f)]
    public float MinFlightDuration = 2f;
    
    [Header("Flight Zone")]
    public Bounds FlightZone = new Bounds(Vector3.zero, new Vector3(20f, 8f, 20f));
    public float MinHeightAboveGround = 1.5f;
    public float MaxHeightAboveGround = 6f;
    
    [Header("Waypoint Generation")]
    public float LateralDeviationRange = 3f;
    public float VerticalDeviationRange = 1.5f;
    public float MinDistanceFromEndpoints = 2f;
    
    [Header("Difficulty Waypoint Mapping")]
    public DifficultyWaypointSettings[] WaypointsByDifficulty;
    
    [Header("Visualization")]
    public Color SplinePathColor = Color.cyan;
    public Color IntermediateWaypointColor = new Color(1f, 0.6f, 0f); // Orange
    public float SplinePathWidth = 0.05f;
    public float WaypointIndicatorScale = 0.25f;
    public int SplineVisualizationSamples = 50;
}

[System.Serializable]
public class DifficultyWaypointSettings
{
    public int DifficultyLevel;
    public int MinWaypoints;
    public int MaxWaypoints;
    [Range(0f, 1f)]
    public float ChanceOfMaxWaypoints;
}
```

### Default Waypoint Mapping

Waypoint count provides path variety, while duck speed (from DifficultySettings) is the primary difficulty driver.

| Difficulty | Min Waypoints | Max Waypoints | Distribution | Flight Behavior |
|------------|---------------|---------------|--------------|-----------------|
| 1          | 1             | 2             | Weighted toward max | Gentle curves, slow speed |
| 2          | 1             | 2             | Even distribution | Moderate curves |
| 3          | 1             | 3             | Even distribution | Varied paths |
| 4          | 0             | 3             | Even distribution | High variety, fast speed |
| 5          | 0             | 3             | Weighted toward min | Unpredictable, fastest speed |

**Design Rationale**: 
- Duck speed (from existing DifficultySettings) is the primary difficulty driver
- Waypoint count adds variety and unpredictability rather than directly scaling difficulty
- Higher difficulties have MORE variety (0-3 waypoints) making paths less predictable
- At difficulty 5, weighting toward 0 waypoints creates fast straight-line ducks mixed with occasional long curves
- The combination of speed + path unpredictability creates the challenge at higher levels

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Catmull-Rom spline interpolation correctness

*For any* four control points and parameter t in [0,1], the calculated spline position should match the Catmull-Rom formula within floating-point tolerance.
**Validates: Requirements 5.1**

### Property 2: Velocity continuity along path

*For any* two adjacent sample points on a spline path, the angle between their velocity vectors should be below a continuity threshold (no abrupt direction changes).
**Validates: Requirements 1.3**

### Property 3: Duck orientation matches movement direction

*For any* duck position along a spline path, the duck's forward vector should align with the path tangent at that position within a small angular tolerance.
**Validates: Requirements 1.4**

### Property 4: Escape triggers at path end

*For any* duck that reaches the end of its flight path (distance traveled >= total arc length), the escape event should be triggered exactly once.
**Validates: Requirements 1.5**

### Property 5: Waypoint count within difficulty range

*For any* generated flight path, the number of intermediate waypoints should be within the configured range for the current difficulty level (1-2 for levels 1-2, 1-3 for level 3, 0-3 for levels 4-5).
**Validates: Requirements 2.1, 2.2, 2.3**

### Property 6: Higher difficulty increases path variety

*For any* large sample of generated paths, the variance in waypoint count at difficulty N+1 should be greater than or equal to the variance at difficulty N.
**Validates: Requirements 2.5**

### Property 7: Dynamically generated waypoints within flight zone bounds

*For any* dynamically generated intermediate waypoint, its position should be within the defined flight zone boundaries and between the minimum and maximum height constraints.
**Validates: Requirements 3.3, 3.4**

### Property 8: Pre-placed waypoints are discovered and used

*For any* path generation when pre-placed IntermediatePoint prefabs exist and preferPreplacedWaypoints is true, the generated path should include waypoints from the pre-placed set.
**Validates: Requirements 3.1, 3.2, 3.5**

### Property 9: Constant speed along curved path

*For any* duck moving along a spline path, the distance traveled per unit time should remain constant (within tolerance) regardless of path curvature.
**Validates: Requirements 5.4, 6.1, 6.4**

### Property 15: Minimum flight duration enforced

*For any* generated flight path, the total flight duration (arc length / speed) should be at least the configured minimum flight duration.
**Validates: Requirements 6.5, 6.6**

### Property 10: Phantom points enable short path interpolation

*For any* path with fewer than 4 control points, the system should add phantom points such that valid Catmull-Rom interpolation can be performed.
**Validates: Requirements 5.3**

### Property 11: Same seed produces identical path

*For any* spawn point, target point, difficulty level, and seed value, calling GeneratePath with the same parameters should produce identical waypoint selections.
**Validates: Requirements 7.2**

### Property 12: Debug toggle immediately updates all visualizations

*For any* change to DebugSettings properties, all subscribed components (existing ducks, spawn point indicators) should update their visualization state within the same frame.
**Validates: Requirements 4.3, 8.3, 4.8**

### Property 13: Each duck has independent path visualization

*For any* set of active ducks with debug visualization enabled, each duck should have its own LineRenderer displaying its unique flight path.
**Validates: Requirements 4.7**

### Property 14: DebugSettings events notify all subscribers

*For any* change to DebugSettings, the OnSettingsChanged event should fire and all registered listeners should receive the notification.
**Validates: Requirements 8.8**

## Error Handling

### Path Generation Errors

- **No Candidate Waypoints**: If no valid intermediate waypoints exist (all points are spawn or target), generate a straight-line path with 0 intermediates
- **Invalid Difficulty Level**: Clamp difficulty to valid range (1-5) and log warning
- **Null Spawn/Target Points**: Log error and return null path; SpawnManager should handle gracefully

### Spline Calculation Errors

- **Degenerate Path**: If all waypoints are coincident, return spawn position for all queries
- **NaN/Infinity Results**: Clamp results to valid ranges and log warning
- **Arc Length Table Corruption**: Rebuild table if queries return invalid results

### Visualization Errors

- **Missing LineRenderer**: Create LineRenderer component dynamically if missing
- **DebugSettings Not Found**: Create default instance with all visualization disabled
- **Material Not Assigned**: Use default unlit material for path visualization

## Testing Strategy

### Unit Testing Framework

The project uses Unity Test Framework (UTF) with NUnit. Tests are located in `Assets/Tests/EditMode/` for editor tests and `Assets/Tests/PlayMode/` for runtime tests.

### Property-Based Testing Framework

Property-based tests use custom generators built on NUnit with randomized inputs. Each property test runs a minimum of 100 iterations.

Each property-based test will be tagged with:
`// Feature: multi-point-pathing, Property {number}: {property_text}`

### Unit Test Coverage

- **SplineUtility**: Catmull-Rom point/tangent calculation, arc-length computation, phantom point generation
- **FlightPath**: Position/tangent queries, arc-length table lookup, boundary conditions
- **FlightPathGenerator**: Waypoint selection, difficulty mapping, seed determinism
- **DebugSettings**: Event firing, property changes, singleton behavior

### Property-Based Test Coverage

- Generate random control points and verify spline calculations
- Generate random paths and verify velocity continuity
- Generate paths at all difficulty levels and verify waypoint counts
- Generate paths with same seed and verify determinism
- Toggle debug settings and verify all subscribers update

### Integration Testing

- Full path generation → duck movement → escape trigger flow
- Debug visualization toggle during active gameplay
- SpawnManager integration with FlightPathGenerator
- DebugSettings integration with all visualization components

### Manual VR Testing

- Visual verification of smooth curved flight paths
- Verify duck orientation looks natural along curves
- Test debug visualization toggle in VR (future settings menu)
- Performance testing with multiple curved-path ducks

## Implementation Notes

### Catmull-Rom Spline Formula

The Catmull-Rom spline interpolates between p1 and p2 using control points p0 and p3:

```
P(t) = 0.5 * ((2*p1) + 
              (-p0 + p2) * t + 
              (2*p0 - 5*p1 + 4*p2 - p3) * t² + 
              (-p0 + 3*p1 - 3*p2 + p3) * t³)
```

With tension parameter τ (default 0.5):
```
P(t) = τ * ((-t³ + 2t² - t) * p0 + 
            (3t³ - 5t² + 2) * p1 + 
            (-3t³ + 4t² + t) * p2 + 
            (t³ - t²) * p3)
```

### Arc-Length Parameterization

To achieve constant speed along the spline:
1. Build a lookup table mapping parameter t → cumulative arc length
2. For a desired distance d, binary search the table to find t
3. Use t to calculate position on spline

### Phantom Point Generation

For paths with < 4 points:
- 2 points (start, end): Add phantom before start and after end by extrapolation
- 3 points (start, mid, end): Add phantom before start by extrapolation

Extrapolation formula: `phantom = 2 * endpoint - adjacent`

### LineRenderer Configuration

```csharp
lineRenderer.positionCount = sampleCount;
lineRenderer.startWidth = 0.05f;
lineRenderer.endWidth = 0.05f;
lineRenderer.material = unlitMaterial;
lineRenderer.startColor = Color.cyan;
lineRenderer.endColor = Color.cyan;
lineRenderer.useWorldSpace = true;
```

### Performance Considerations

- Arc-length table is computed once per path, not per frame
- LineRenderer positions are set once when path is assigned
- DebugSettings uses events to avoid polling
- Spline calculations use cached waypoint arrays

### Waypoint Selection Algorithm (Hybrid Approach)

1. **Check for pre-placed waypoints**: Look for IntermediatePoint prefabs in scene
2. **If pre-placed waypoints exist and preferred**:
   - Select waypoints that create reasonable paths between spawn and target
   - Prefer waypoints that are roughly between spawn and target positions
3. **If no pre-placed waypoints or dynamic generation preferred**:
   - Generate waypoints dynamically using the algorithm below
4. **Build final path**: [spawn] + [intermediates] + [target]
5. **Validate minimum duration**: Calculate arc length / speed
   - If duration < minFlightDuration, add additional waypoints to extend path
   - Or reduce effective speed for this duck to meet minimum duration

### Dynamic Waypoint Generation Algorithm

```csharp
Vector3 GenerateDynamicWaypoint(Vector3 spawn, Vector3 target, float progress, System.Random rng)
{
    Vector3 directPath = target - spawn;
    Vector3 basePosition = spawn + directPath * progress;
    
    // Calculate perpendicular direction for lateral deviation
    Vector3 perpendicular = Vector3.Cross(directPath.normalized, Vector3.up).normalized;
    if (perpendicular == Vector3.zero)
        perpendicular = Vector3.right;
    
    // Apply random deviations
    float lateralOffset = (float)(rng.NextDouble() * 2 - 1) * lateralDeviationRange;
    float verticalOffset = (float)(rng.NextDouble() * 2 - 1) * verticalDeviationRange;
    
    Vector3 waypoint = basePosition + perpendicular * lateralOffset + Vector3.up * verticalOffset;
    
    // Clamp to flight zone and height constraints
    waypoint.y = Mathf.Clamp(waypoint.y, minHeightAboveGround, maxHeightAboveGround);
    waypoint = ClampToFlightZone(waypoint);
    
    return waypoint;
}
```

### IntermediatePoint Prefab Structure

```
IntermediatePoint (IntermediatePointMarker component)
├── Indicator (Sphere mesh, orange URP material, scale 0.25)
└── OccluderSlot (Empty transform for optional occluder parenting)
```

- Orange indicator color: RGB(1.0, 0.6, 0.0)
- Smaller scale than spawn/target indicators (0.25 vs 0.3)
- OccluderSlot is optional - can be left empty for open-sky curves

### Migration from Existing System

The existing `DuckController.Initialize(Vector3 start, Vector3 end, float speed)` will be preserved for backward compatibility. A new overload `Initialize(FlightPath path, float speed)` will be added for spline-based movement. The SpawnManager will be updated to use FlightPathGenerator and the new Initialize overload.

