# VR Duck Hunt - Design Document

## Overview

The VR Duck Hunt game is a virtual reality adaptation of the classic arcade shooter, designed for Meta Quest devices using Unity's XR Interaction Toolkit. Players use VR controllers as guns to shoot at ducks that fly across a 3D environment. The game features progressive difficulty, score tracking, and immersive audio-visual feedback.

The implementation will create a new standalone Unity project from scratch, setting up the XR Interaction Toolkit, configuring the VR rig, and building all game systems. The Unity-StarterSamples project will serve as a reference for XR configuration patterns and Meta Quest compatibility settings.

## Architecture

The game follows a component-based architecture typical of Unity development, with clear separation between game logic, presentation, and data management.

### High-Level Components

```
┌─────────────────────────────────────────────────────────┐
│                    VR Duck Hunt Scene                    │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Game       │  │   Spawn      │  │   Score      │  │
│  │   Manager    │◄─┤   Manager    │  │   Manager    │  │
│  └──────┬───────┘  └──────┬───────┘  └──────▲───────┘  │
│         │                  │                  │          │
│         │                  ▼                  │          │
│         │          ┌──────────────┐          │          │
│         │          │   Duck       │──────────┘          │
│         │          │   Controller │                     │
│         │          └──────────────┘                     │
│         │                                                │
│         ▼                                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Shooting   │◄─┤ Gun Selection│  │   Credits    │  │
│  │   Controller │  │   Manager    │  │   Manager    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│                                                           │
│  ┌──────────────────────────────────────────────────┐  │
│  │           XR Interaction Toolkit                  │  │
│  │        (Existing Project Infrastructure)          │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Component Responsibilities

- **GameManager**: Orchestrates game state (start, playing, game over), difficulty progression, and scene initialization
- **SpawnManager**: Handles duck instantiation, spawn timing, spawn location randomization, and difficulty-based spawn rate adjustments
- **ScoreManager**: Tracks player score, missed ducks count, updates UI displays, and triggers game over conditions
- **DuckController**: Controls individual duck behavior including flight path, animation, hit detection, and destruction
- **ShootingController**: Manages player input from VR controllers, performs raycasting, triggers effects, and communicates hits
- **GunSelectionManager**: Manages multiple gun models, handles gun switching, and persists player preferences
- **CreditsManager**: Displays asset attribution information and manages credits UI

## Components and Interfaces

### GameManager

**Purpose**: Central controller for game state and flow

**Public Interface**:
```csharp
public class GameManager : MonoBehaviour
{
    public void StartGame();
    public void EndGame();
    public void RestartGame();
    public GameState CurrentState { get; }
    public int CurrentDifficulty { get; }
    public void IncreaseDifficulty();
}

public enum GameState
{
    Idle,
    Playing,
    GameOver
}
```

**Key Behaviors**:
- Initializes all manager components on scene load
- Transitions between game states
- Monitors score thresholds for difficulty increases
- Coordinates game restart

### SpawnManager

**Purpose**: Controls duck spawning logic and timing

**Public Interface**:
```csharp
public class SpawnManager : MonoBehaviour
{
    public void StartSpawning();
    public void StopSpawning();
    public void SetDifficulty(int level);
    public GameObject DuckPrefab { get; set; }
    public Transform[] SpawnPoints { get; set; }
}
```

**Key Behaviors**:
- Uses coroutines for timed spawning
- Randomizes spawn locations from predefined points
- Adjusts spawn interval based on difficulty
- Passes difficulty parameters to spawned ducks

### ScoreManager

**Purpose**: Tracks and displays game scoring

**Public Interface**:
```csharp
public class ScoreManager : MonoBehaviour
{
    public int CurrentScore { get; }
    public int MissedDucks { get; }
    public int MaxMissedDucks { get; set; }
    public void AddScore(int points);
    public void IncrementMissed();
    public void ResetScore();
    public event Action<int> OnScoreChanged;
    public event Action OnGameOver;
}
```

**Key Behaviors**:
- Maintains score and missed duck counters
- Fires events when values change
- Triggers game over when miss threshold reached
- Updates world-space UI canvas

### DuckController

**Purpose**: Controls individual duck behavior

**Public Interface**:
```csharp
public class DuckController : MonoBehaviour
{
    public float FlightSpeed { get; set; }
    public Vector3 TargetPosition { get; set; }
    public void Initialize(Vector3 start, Vector3 end, float speed);
    public void OnHit();
    public event Action<DuckController> OnDestroyed;
    public event Action<DuckController> OnEscaped;
}
```

**Key Behaviors**:
- Moves along linear path from start to end point
- Plays flying animation
- Detects when hit by raycast
- Plays destruction effects and removes self
- Notifies managers when destroyed or escaped

### ShootingController

**Purpose**: Handles VR controller shooting mechanics with gun selection integration

**Public Interface**:
```csharp
public class ShootingController : MonoBehaviour
{
    public Transform RayOrigin { get; set; }
    public float RayDistance { get; set; }
    public LayerMask TargetLayer { get; set; }
    public AudioClip HitSound { get; set; }
    public AudioClip MissSound { get; set; }
    public ParticleSystem MuzzleFlash { get; set; }
    public GunData GetCurrentGunData();
    public void SetGunSelectionManager(GunSelectionManager manager);
}
```

**Key Behaviors**:
- Listens for XR controller trigger input
- Performs raycast from controller position or gun muzzle point
- Triggers haptic feedback on controller with gun-specific intensity
- Plays gun-specific audio and visual effects
- Calls OnHit() on struck duck
- Automatically adapts to selected gun properties

### GunSelectionManager

**Purpose**: Manages multiple gun models and player selection

**Public Interface**:
```csharp
public class GunSelectionManager : MonoBehaviour
{
    public GunData CurrentGun { get; }
    public int CurrentGunIndex { get; }
    public GameObject CurrentGunInstance { get; }
    public void SelectGun(int gunIndex);
    public void SelectGun(string gunName);
    public void SelectNextGun();
    public void SelectPreviousGun();
    public event UnityEvent<GunData> OnGunChanged;
    public event UnityEvent<int> OnGunIndexChanged;
}
```

**Key Behaviors**:
- Manages collection of available gun models
- Handles gun prefab instantiation and attachment to VR controller
- Automatically detects or creates muzzle points for effects
- Persists player gun preference using PlayerPrefs
- Fires events when gun selection changes
- Validates gun collection and handles errors gracefully

### CreditsManager

**Purpose**: Displays asset attribution and license information

**Public Interface**:
```csharp
public class CreditsManager : MonoBehaviour
{
    public void ShowCredits();
    public void HideCredits();
    public void ToggleCredits();
    public void SetCreditsData(CreditsData newCreditsData);
}
```

**Key Behaviors**:
- Loads credits from CreditsData ScriptableObject
- Manages credits UI panel visibility
- Handles scrollable credits text display
- Provides proper attribution for all third-party assets
- Integrates with main game UI and menus

## Data Models

### Duck Data

```csharp
public struct DuckSpawnData
{
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public float Speed;
    public int PointValue;
}
```

### Gun Data

```csharp
[System.Serializable]
public class GunData
{
    public string gunName;
    public string description;
    public GameObject gunPrefab;
    public float fireRate;
    public float hapticIntensity;
    public float muzzleFlashScale;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public GameObject muzzleFlashPrefab;
    public Transform muzzlePoint;
    public Sprite gunIcon;
    public Texture2D gunPreview;
}
```

### Gun Collection

```csharp
[CreateAssetMenu(fileName = "GunCollection", menuName = "Game/Gun Collection")]
public class GunCollection : ScriptableObject
{
    public GunData[] AvailableGuns { get; }
    public int DefaultGunIndex { get; }
    public GunData GetGun(int index);
    public GunData GetGun(string gunName);
    public GunData GetDefaultGun();
    public string[] GetGunNames();
}
```

### Credits Data

```csharp
[System.Serializable]
public struct AssetCredit
{
    public string assetName;
    public string author;
    public string license;
    public string source;
    public string attributionText;
}

[CreateAssetMenu(fileName = "CreditsData", menuName = "Game/Credits Data")]
public class CreditsData : ScriptableObject
{
    public AssetCredit[] AssetCredits { get; }
    public string GetFormattedCreditsText();
}
```

### Difficulty Settings

```csharp
[System.Serializable]
public class DifficultySettings
{
    public int Level;
    public float SpawnInterval;
    public float DuckSpeed;
    public int ScoreThreshold;
}
```

### Game Configuration

```csharp
[CreateAssetMenu(fileName = "DuckHuntConfig", menuName = "Game/Duck Hunt Config")]
public class DuckHuntConfig : ScriptableObject
{
    public int PointsPerDuck = 10;
    public int MaxMissedDucks = 10;
    public DifficultySettings[] DifficultyLevels;
    public float RaycastDistance = 100f;
}
```


## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Trigger input initiates raycast

*For any* trigger pull event, the shooting system should perform a raycast from the controller's position in its forward direction.
**Validates: Requirements 1.1**

### Property 2: Ray intersection registers hit

*For any* duck positioned in the ray's path, the hit detection system should register a hit on that duck.
**Validates: Requirements 1.2**

### Property 3: Hit ducks are destroyed

*For any* duck that receives a hit, it should play its destruction sequence and remove itself from the scene.
**Validates: Requirements 1.3**

### Property 4: Trigger provides haptic feedback

*For any* trigger pull event, the system should send haptic feedback to the controller.
**Validates: Requirements 1.4**

### Property 5: Shots play audio

*For any* shot fired, the system should play an audio effect (either hit or miss sound).
**Validates: Requirements 1.5**

### Property 6: Ducks spawn at intervals

*For any* active game session, ducks should spawn at regular intervals defined by the current difficulty settings.
**Validates: Requirements 2.1**

### Property 7: Spawned ducks use valid positions

*For any* spawned duck, its starting position should be one of the configured spawn points.
**Validates: Requirements 2.2**

### Property 8: Active ducks move toward target

*For any* active duck, its position should continuously move toward its target position over time.
**Validates: Requirements 2.3**

### Property 9: Ducks cleanup at path end

*For any* duck that reaches its target position, it should remove itself from the scene and notify the score manager.
**Validates: Requirements 2.4**

### Property 10: Moving ducks animate

*For any* duck with velocity greater than zero, its animator should be in the flying animation state.
**Validates: Requirements 2.5**

### Property 11: Hits increase score

*For any* duck hit, the score should increase by exactly the configured point value.
**Validates: Requirements 3.1**

### Property 12: Score display synchronization

*For any* score change, the displayed score value should match the internal score value.
**Validates: Requirements 3.2**

### Property 13: Escaped ducks increment miss counter

*For any* duck that reaches the end of its path without being hit, the missed duck counter should increment by one.
**Validates: Requirements 3.4**

### Property 14: Score threshold triggers difficulty increase

*For any* score that crosses a difficulty threshold, the spawn manager should update to the next difficulty level.
**Validates: Requirements 4.1**

### Property 15: Difficulty affects duck speed

*For any* difficulty increase, newly spawned ducks should have a higher movement speed than the previous difficulty level.
**Validates: Requirements 4.2**

### Property 16: Difficulty changes show feedback

*For any* difficulty level change, a visual indicator should be displayed to the player.
**Validates: Requirements 4.4**

### Property 17: Miss threshold triggers game over

*For any* game state where missed ducks equals the maximum threshold, the game should transition to game over state.
**Validates: Requirements 5.1**

### Property 18: Game over displays score

*For any* transition to game over state, the final score display should become visible.
**Validates: Requirements 5.2**

### Property 19: Restart resets game state

*For any* restart action, all game state (score, missed count, difficulty) should return to initial values.
**Validates: Requirements 5.4**

### Property 20: Hit shots play hit sound

*For any* shot that hits a duck, the hit sound effect should play.
**Validates: Requirements 6.1**

### Property 21: Miss shots play miss sound

*For any* shot that doesn't hit a duck, the miss sound effect should play.
**Validates: Requirements 6.2**

### Property 22: Destroyed ducks spawn particles

*For any* duck destruction, particle effects should be instantiated at the duck's position.
**Validates: Requirements 6.3**

### Property 23: Trigger activates muzzle flash

*For any* trigger pull, the muzzle flash particle effect should play.
**Validates: Requirements 6.4**

### Property 24: Gun selection provides multiple options

*For any* gun selection system, at least two different gun models should be available for player choice.
**Validates: Requirements 8.1**

### Property 25: Gun attachment to controller

*For any* selected gun, the gun model should be properly attached to the VR controller transform.
**Validates: Requirements 8.2**

### Property 26: Gun preference persistence

*For any* gun selection, the player's choice should be saved and restored in future game sessions.
**Validates: Requirements 8.3**

### Property 27: Gun-specific shooting properties

*For any* gun selection change, the shooting mechanics should adapt to use gun-specific audio, haptic feedback, and visual effects.
**Validates: Requirements 8.4**

### Property 28: Gun information display

*For any* gun in the selection UI, the system should display the gun's name and description information.
**Validates: Requirements 8.5**

### Property 29: Asset attribution compliance

*For any* third-party asset used in the game, proper attribution and license information should be maintained and displayable.
**Validates: Requirements 9.5**

## Error Handling

### Input Errors

- **Missing XR Controller**: If no XR controller is detected, log an error and disable shooting functionality
- **Invalid Trigger Input**: Validate trigger input values are within expected range (0-1)

### Spawn Errors

- **Missing Spawn Points**: If no spawn points are configured, log error and prevent game start
- **Failed Instantiation**: If duck prefab fails to instantiate, log error and continue spawning
- **Null Prefab Reference**: Validate duck prefab is assigned before attempting to spawn

### State Errors

- **Invalid State Transition**: Prevent invalid game state transitions (e.g., can't restart from playing state)
- **Negative Score**: Clamp score to minimum of zero
- **Overflow Protection**: Prevent score from exceeding maximum integer value

### Scene Errors

- **Missing Manager References**: Validate all manager components exist on scene load
- **Missing UI Elements**: Log warning if UI canvas or text elements are not found
- **Audio Source Missing**: Gracefully handle missing audio sources by logging warning

## Testing Strategy

### Unit Testing Framework

The project will use Unity Test Framework (UTF) with NUnit for unit testing. Tests will be organized in an `Assets/Tests` directory structure mirroring the scripts directory.

### Property-Based Testing Framework

The project will use **Unity Test Framework with custom property-based testing utilities** built on top of NUnit. Since C# doesn't have a mature property-based testing library like QuickCheck or Hypothesis, we'll implement lightweight generators for game-specific types (positions, speeds, scores) to enable property testing.

Each property-based test will:
- Run a minimum of 100 iterations with randomized inputs
- Be tagged with a comment referencing the specific correctness property from this design document
- Use the format: `// Feature: vr-duck-hunt, Property {number}: {property_text}`

### Unit Test Coverage

Unit tests will focus on:
- **GameManager**: State transitions, difficulty progression logic, initialization
- **ScoreManager**: Score calculation, miss tracking, game over triggering
- **SpawnManager**: Spawn timing logic, difficulty parameter application
- **DuckController**: Movement calculations, hit detection, lifecycle events
- **ShootingController**: Raycast logic, input handling, effect triggering

Example unit tests:
- Test that GameManager initializes in Idle state
- Test that ScoreManager correctly calculates score after multiple hits
- Test that DuckController moves toward target position each frame
- Test that ShootingController performs raycast when trigger is pulled

### Property-Based Test Coverage

Property-based tests will verify the correctness properties defined in this document:
- Generate random duck positions and verify hit detection works correctly
- Generate random score values and verify UI synchronization
- Generate random spawn configurations and verify spawn timing
- Generate random difficulty levels and verify parameter application
- Generate random game states and verify state transitions

### Integration Testing

Integration tests will verify:
- Complete shooting flow: trigger → raycast → hit → score update
- Complete duck lifecycle: spawn → move → escape/hit → cleanup
- Difficulty progression: score increase → threshold check → difficulty update → spawn rate change
- Game over flow: miss threshold → game over state → UI display → restart

### Manual VR Testing

Manual testing in VR headset will verify:
- Controller tracking accuracy
- Haptic feedback intensity and timing
- Audio spatialization and volume levels
- UI readability and positioning
- Overall game feel and difficulty balance
- Performance and frame rate stability

## Implementation Notes

### Unity-Specific Considerations

- Create new Unity project (2021.3 LTS or newer recommended)
- Install XR Interaction Toolkit package via Package Manager
- Install XR Plugin Management and Oculus XR Plugin for Meta Quest support
- Set up XR Origin rig with ActionBasedController components
- Use Unity's XR Interaction Toolkit's `ActionBasedController` for input
- Use object pooling for duck instances to reduce garbage collection
- Implement duck movement using `Vector3.MoveTowards` for smooth interpolation
- Use Unity's Animator component for duck flying animations
- Implement UI using world-space Canvas for VR compatibility
- Configure build settings for Android platform with Oculus target

### Performance Optimization

- Limit maximum concurrent ducks to prevent performance degradation
- Use simple colliders (sphere/capsule) for duck hit detection
- Disable duck animations when far from player
- Use LOD (Level of Detail) for duck models if needed
- Pool particle effects for reuse

### VR Best Practices

- Position UI elements at comfortable viewing distance (2-3 meters)
- Ensure all interactive elements are within comfortable reach
- Provide clear visual feedback for all interactions
- Maintain 72+ FPS for smooth VR experience
- Use spatial audio for immersive sound effects
- Avoid rapid camera movements that could cause motion sickness

### Play Area and Boundary System

The game features a fully enclosed fenced play area that keeps the player in a safe shooting zone while providing an immersive infinite-ground visual effect.

**Visual Design:**
- Ground plane uses a shader/material that creates an infinite horizon effect (gradient fade to skybox color at edges)
- A rustic wooden fence completely surrounds the player area (shooting gallery aesthetic)
- The front fence is lower (~1.2m waist height) for shooting over
- Side and back fences can be taller (~1.5m) to clearly define boundaries
- Floor markers or subtle grid pattern indicate the play area bounds

**Boundary Implementation:**
- Fence colliders prevent the player from walking out of the play area
- XR Interaction Toolkit teleport areas restrict teleportation to within the fenced area
- The play area is approximately 6m x 6m to allow room for roaming and interactive elements
- Ducks fly beyond the front fence in the "duck zone"

**Fence Design:**
- Simple wooden post-and-rail fence (3-4 horizontal rails)
- Front fence: ~1.2m height (waist height for shooting over), ~6m wide
- Side fences: ~1.5m height, ~6m deep
- Back fence: ~1.5m height, ~6m wide
- Rustic/weathered wood material to match hunting theme
- Corner posts at each corner of the play area

**Play Area Layout:**
- Start Button Pedestal: Near front fence, so player faces ducks when pressing start
- Gun Display Rack: Left side of play area, along the left fence
- Scoreboard: Outside the fence in the duck zone, facing back toward player (arcade style)
- Player Start Position: Center-back of play area, facing front fence and ducks

```
Top-Down View:
                    Duck Flight Zone (beyond fence)
              [SCOREBOARD - outside fence, facing player]
    ════════════════════════════════════════════════════
    
    ┌──────────────────────────────────────────────────┐
    │ ═══════════ FRONT FENCE (low, 1.2m) ═══════════ │
    │ ║                                              ║ │
    │ ║              [START PEDESTAL]                ║ │
    │ ║                                              ║ │
    │ S                                              S │
    │ I   [GUN RACK]                                 I │
    │ D                                              D │
    │ E                                              E │
    │ ║                                              ║ │
    │ ║                   [P] Player Start           ║ │
    │ ║                                              ║ │
    │ ═══════════════ BACK FENCE ═══════════════════ │
    └──────────────────────────────────────────────────┘
    
    Play Area: ~6m x 6m
```

### Asset Requirements

- Duck 3D model with flying animation
- Multiple gun 3D models (N-ZAP 85, Nintendo Zapper Light Gun)
- Muzzle flash particle effect
- Duck destruction particle effect
- Hit sound effect
- Miss sound effect
- Gun-specific fire sounds (optional)
- Background music (optional)
- Skybox or environment assets
- UI sprites for score display, game over screen, and gun selection
- Gun preview images and icons for UI
- License files and attribution documentation for all third-party assets
- Wooden fence model or procedural fence prefab
- Infinite ground shader/material (gradient fade effect)
- Play area boundary markers (optional floor decals)
