# Hand Configuration System - Design Document

## Overview

The Hand Configuration System provides players with control over their VR hand setup and gun selection experience. It includes a settings menu for toggling options, a physical gun showcase for immersive weapon selection, and support for both single-handed and dual-wielding gameplay modes.

Key features:
- Toggle left hand visibility while maintaining controller tracking
- Switch between single-hand and dual-wielding gun modes
- World-space settings menu accessible via left menu button
- Physical gun showcase for picking up and swapping weapons
- Controller visuals shown when no gun is equipped
- Automatic preference persistence

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Hand Configuration System                 │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────┐     ┌──────────────────┐             │
│  │  Settings Menu   │     │  Gun Showcase    │             │
│  │    Manager       │     │    Controller    │             │
│  └────────┬─────────┘     └────────┬─────────┘             │
│           │                         │                       │
│           ▼                         ▼                       │
│  ┌──────────────────────────────────────────────┐          │
│  │         Hand Configuration Manager            │          │
│  │  - Left hand visibility                       │          │
│  │  - Gun mode (single/dual)                     │          │
│  │  - Controller visual state                    │          │
│  │  - Preference persistence                     │          │
│  └──────────────────────────────────────────────┘          │
│           │                         │                       │
│           ▼                         ▼                       │
│  ┌──────────────┐          ┌──────────────┐                │
│  │  Left Hand   │          │  Right Hand  │                │
│  │  Controller  │          │  Controller  │                │
│  └──────────────┘          └──────────────┘                │
│                                                              │
│  ┌──────────────────────────────────────────────┐          │
│  │         Shooting Controller (Modified)        │          │
│  │  - Dual trigger support                       │          │
│  │  - Per-hand gun references                    │          │
│  └──────────────────────────────────────────────┘          │
└─────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### HandConfigurationManager

**Purpose**: Central manager for hand visibility and gun mode settings

**Public Interface**:
```csharp
public class HandConfigurationManager : MonoBehaviour
{
    public bool LeftHandVisible { get; set; }
    public GunMode CurrentGunMode { get; set; }
    
    public void SetLeftHandVisibility(bool visible);
    public void SetGunMode(GunMode mode);
    public void EquipGun(Hand hand, GunData gun);
    public void UnequipGun(Hand hand);
    public GunData GetEquippedGun(Hand hand);
    public bool IsGunEquipped(Hand hand);
    
    public event Action<bool> OnLeftHandVisibilityChanged;
    public event Action<GunMode> OnGunModeChanged;
    public event Action<Hand, GunData> OnGunEquipped;
    public event Action<Hand> OnGunUnequipped;
}

public enum GunMode
{
    SingleHand,
    DualWield
}

public enum Hand
{
    Left,
    Right
}
```

### SettingsMenuManager

**Purpose**: Controls the world-space settings menu display and interaction

**Public Interface**:
```csharp
public class SettingsMenuManager : MonoBehaviour
{
    public bool IsMenuOpen { get; }
    
    public void OpenMenu();
    public void CloseMenu();
    public void ToggleMenu();
    
    public event Action OnMenuOpened;
    public event Action OnMenuClosed;
}
```

### GunShowcaseController

**Purpose**: Manages the physical gun display and pickup/return interactions

**Public Interface**:
```csharp
public class GunShowcaseController : MonoBehaviour
{
    public GunData[] AvailableGuns { get; }
    
    public void ReturnGun(GunData gun);
    public bool IsGunAvailable(GunData gun);
    public void SetGunInUse(GunData gun, bool inUse);
    
    public event Action<GunData> OnGunPickedUp;
    public event Action<GunData> OnGunReturned;
}
```

### GunPickupInteractable

**Purpose**: Individual gun on the showcase that can be picked up

**Public Interface**:
```csharp
public class GunPickupInteractable : MonoBehaviour
{
    public GunData GunData { get; }
    public bool IsAvailable { get; }
    
    public void SetAvailable(bool available);
    public void OnGrabbed(Hand hand);
}
```

### HandVisualController

**Purpose**: Controls the visibility of controller/hand visuals per hand

**Public Interface**:
```csharp
public class HandVisualController : MonoBehaviour
{
    public Hand Hand { get; }
    public bool IsControllerVisible { get; }
    public bool IsGunEquipped { get; }
    
    public void ShowControllerVisual();
    public void HideControllerVisual();
    public void OnGunEquipped();
    public void OnGunUnequipped();
}
```

## Data Models

### HandConfigurationSettings

```csharp
[System.Serializable]
public class HandConfigurationSettings
{
    public bool leftHandVisible = true;
    public GunMode gunMode = GunMode.SingleHand;
    public string leftGunId = "";
    public string rightGunId = "";
}
```

### SettingsData (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "SettingsData", menuName = "Game/Settings Data")]
public class SettingsData : ScriptableObject
{
    public float menuDistanceFromPlayer = 2f;
    public float menuHeight = 1.5f;
    public Vector3 showcasePosition;
    public Vector3 showcaseRotation;
}
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Left hand visibility toggle round-trip

*For any* initial visibility state, toggling left hand visibility on then off (or off then on) should return the hand to its original visibility state.
**Validates: Requirements 1.1, 1.3**

### Property 2: Hidden hand maintains tracking

*For any* hidden left hand, the controller transform should continue to update its position and rotation based on physical controller movement.
**Validates: Requirements 1.2**

### Property 3: Left hand visibility preference persistence

*For any* left hand visibility setting, saving and reloading the game should restore the same visibility state.
**Validates: Requirements 1.4**

### Property 4: Dual wielding attaches guns to both hands

*For any* transition to dual wielding mode, both left and right controller attachment points should have a gun instance attached.
**Validates: Requirements 2.1**

### Property 5: Single hand mode attaches gun only to right hand

*For any* transition to single hand mode, only the right controller should have a gun attached, and the left should have no gun.
**Validates: Requirements 2.2**

### Property 6: Dual wielding enables independent firing

*For any* dual wielding state, pulling either the left or right trigger should fire only the corresponding gun.
**Validates: Requirements 2.3**

### Property 7: Single hand mode ignores left trigger

*For any* single hand mode state, pulling the left trigger should not fire any gun or produce shooting effects.
**Validates: Requirements 2.4**

### Property 8: Gun mode preference persistence

*For any* gun mode setting, saving and reloading the game should restore the same gun mode.
**Validates: Requirements 2.5**

### Property 9: Menu button toggles settings menu

*For any* menu state, pressing the left menu button should toggle the settings menu visibility (open if closed, close if open).
**Validates: Requirements 3.1**

### Property 10: Settings menu positioned in front of player

*For any* opened settings menu, the menu position should be within 1.5-3 meters in front of the player's head position.
**Validates: Requirements 3.2**

### Property 11: Settings menu pause/resume round-trip

*For any* active game session, opening the settings menu should pause the game, and closing it should resume the game to its previous state.
**Validates: Requirements 3.3, 3.4**

### Property 12: Settings changes apply immediately

*For any* setting change in the menu, the corresponding system state should update within the same frame without requiring menu close or game restart.
**Validates: Requirements 4.3**

### Property 13: Controller visual state matches gun equipped state

*For any* hand, the controller visual should be visible if and only if no gun is equipped on that hand.
**Validates: Requirements 5.2, 5.4, 6.3**

### Property 14: Gun pickup attaches gun to hand

*For any* available gun on the showcase, gripping it should result in that gun being attached to the gripping hand.
**Validates: Requirements 5.3, 6.4**

### Property 15: Showcase indicates gun in-use state

*For any* gun on the showcase, its visual indicator should reflect whether it is currently equipped by the player.
**Validates: Requirements 5.5**

### Property 16: Gun return restores showcase state

*For any* equipped gun, returning it to the showcase should make it available for pickup again and display it in its original position.
**Validates: Requirements 6.1, 6.2**

### Property 17: Gun swap updates preference

*For any* gun swap (return old, pickup new), the saved gun preference for that hand should match the newly equipped gun.
**Validates: Requirements 6.5**

## Error Handling

### Input Errors
- **Missing Controller**: If a VR controller is not detected, disable gun pickup for that hand and log a warning
- **Invalid Gun Reference**: If a gun prefab is missing, skip that gun in the showcase and log an error

### State Errors
- **Null Gun on Unequip**: If attempting to unequip when no gun is equipped, ignore the request gracefully
- **Duplicate Pickup**: If attempting to pick up a gun already in use, reject the pickup and provide haptic feedback

### Persistence Errors
- **Corrupted Preferences**: If saved preferences fail to load, reset to defaults and log a warning
- **Missing Gun ID**: If a saved gun ID doesn't match any available gun, use the default gun

### UI Errors
- **Menu Already Open**: If menu button pressed while menu is open, close the menu instead of opening another
- **Missing UI References**: If UI elements are not assigned, log error and disable settings functionality

## Testing Strategy

### Property-Based Testing Framework

The project will use Unity Test Framework with custom property-based testing utilities. Each property test will run a minimum of 100 iterations with randomized inputs.

### Unit Test Coverage

- **HandConfigurationManager**: Visibility toggle, gun mode switching, gun equip/unequip
- **SettingsMenuManager**: Open/close state, pause/resume integration
- **GunShowcaseController**: Gun availability tracking, pickup/return logic
- **HandVisualController**: Controller visual state management

### Property-Based Test Coverage

Property tests will verify:
- Toggle operations are reversible (round-trip properties)
- Persistence correctly saves and restores all settings
- Gun mode correctly affects which hands have guns
- Controller visuals correctly reflect gun equipped state
- Menu positioning stays within comfortable VR range

### Integration Testing

- Complete gun pickup flow: approach showcase → grip → gun attaches → controller hides
- Complete gun swap flow: grip near showcase → gun returns → grip new gun → new gun attaches
- Settings menu flow: button press → menu opens → change setting → effect applies → close menu
- Mode switching: single → dual (left gun appears) → single (left gun disappears)

### Manual VR Testing

- Verify menu is comfortable to read and interact with
- Test gun pickup feels natural and responsive
- Verify controller visuals transition smoothly
- Test dual wielding feels balanced and fun
- Verify settings persist across game sessions
