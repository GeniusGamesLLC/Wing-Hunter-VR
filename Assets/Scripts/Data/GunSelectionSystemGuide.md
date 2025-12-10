# Gun Selection System Guide

## Overview

The gun selection system allows players to choose between different gun models in the VR Duck Hunt game. The system is designed to be extensible, allowing easy addition of new guns in the future.

## System Components

### 1. GunData Class
- **File**: `GunData.cs`
- **Purpose**: Defines properties and settings for individual guns
- **Key Fields**:
  - `gunName`: Display name of the gun
  - `description`: Description for UI display
  - `gunPrefab`: 3D model prefab reference
  - `fireRate`: Rate of fire multiplier
  - `hapticIntensity`: Controller vibration strength
  - `muzzleFlashScale`: Scale factor for muzzle flash effects
  - `fireSound`: Gun-specific firing sound
  - `gunIcon`: UI icon for gun selection
  - `gunPreview`: Preview image for gun selection UI

### 2. GunCollection ScriptableObject
- **File**: `GunCollection.cs`
- **Purpose**: Manages collection of available guns
- **Features**:
  - Array of GunData configurations
  - Default gun selection
  - Gun lookup by name or index
  - Collection validation
  - Gun name extraction for UI

### 3. GunSelectionManager
- **File**: `GunSelectionManager.cs`
- **Purpose**: Handles gun switching and instantiation
- **Key Features**:
  - Gun prefab instantiation and attachment
  - Automatic muzzle point detection
  - Player preference persistence
  - Event system for gun changes
  - Next/previous gun navigation

### 4. Enhanced ShootingController
- **File**: `ShootingController.cs` (updated)
- **Purpose**: Integrates shooting mechanics with gun selection
- **New Features**:
  - Gun-specific audio and effects
  - Dynamic muzzle point updates
  - Gun-specific haptic feedback
  - Automatic configuration updates

### 5. GunSelectionUI
- **File**: `GunSelectionUI.cs`
- **Purpose**: Provides user interface for gun selection
- **Features**:
  - Gun list display with buttons
  - Gun information display (name, description, preview)
  - Next/previous navigation
  - Visual selection feedback

## Current Gun Assets

### N-ZAP 85
- **Model**: Futuristic water gun design
- **Author**: ThePinguFan2006
- **License**: CC-BY-4.0
- **Prefab Path**: `Assets/Import/N-ZAP_85/N-ZAP_85.prefab`
- **Characteristics**: Modern styling, higher fire rate

### Nintendo Zapper Light Gun
- **Model**: Classic Nintendo light gun
- **Author**: Matt Wilson
- **License**: CC-BY-4.0
- **Prefab Path**: `Assets/Import/Nintendo_Zapper_Light_Gun/Nintendo_Zapper_Light_Gun.prefab`
- **Characteristics**: Nostalgic design, classic feel

## Setup Instructions

### 1. Create Gun Collection Asset
1. Use `GunCollectionSetup` utility script:
   - Add script to any GameObject
   - Check "Create Gun Collection" in Inspector
   - Assign gun prefab references if needed
   - Play scene to generate asset

2. Manual creation:
   - Right-click in Project → Create → Game → Gun Collection
   - Configure gun data for each available gun
   - Set default gun index

### 2. Set Up Gun Selection Manager
1. Create GameObject named "GunSelectionManager"
2. Add `GunSelectionManager` component
3. Assign gun collection asset
4. Set gun attach point (usually VR controller transform)

### 3. Update Shooting Controller
1. Assign `GunSelectionManager` reference in `ShootingController`
2. The controller will automatically update when guns change

### 4. Set Up Gun Selection UI
1. Create UI Canvas (world-space for VR)
2. Add `GunSelectionUI` component to UI GameObject
3. Create UI elements:
   - Gun selection panel
   - Gun name/description text
   - Previous/next buttons
   - Gun list (optional)
4. Assign UI references in `GunSelectionUI`

## Integration with Existing Systems

### VR Controller Integration
```csharp
// In your VR setup script
GunSelectionManager gunManager = FindObjectOfType<GunSelectionManager>();
if (gunManager != null)
{
    // Set the controller as the gun attach point
    gunManager.gunAttachPoint = vrController.transform;
}
```

### Game Manager Integration
```csharp
// In GameManager, add gun selection controls
public class GameManager : MonoBehaviour
{
    [SerializeField] private GunSelectionManager gunSelectionManager;
    
    private void Update()
    {
        // Example: Change guns with controller buttons
        if (Input.GetButtonDown("NextGun"))
        {
            gunSelectionManager.SelectNextGun();
        }
        
        if (Input.GetButtonDown("PreviousGun"))
        {
            gunSelectionManager.SelectPreviousGun();
        }
    }
}
```

### UI Menu Integration
```csharp
// Add gun selection to main menu
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GunSelectionUI gunSelectionUI;
    
    public void ShowGunSelection()
    {
        gunSelectionUI.ShowGunSelection();
    }
}
```

## Adding New Guns

### 1. Import Gun Model
1. Import 3D model and textures
2. Create prefab with proper scale and orientation
3. Ensure gun points forward (positive Z-axis)
4. Add colliders if needed for interaction

### 2. Update Gun Collection
1. Open gun collection asset in Inspector
2. Increase "Available Guns" array size
3. Configure new gun data:
   - Assign gun prefab
   - Set name and description
   - Configure audio and effects
   - Set gameplay parameters

### 3. Update Credits
1. Add attribution to `CREDITS.md`
2. Update `CreditsData` ScriptableObject
3. Include license information

### 4. Test Integration
1. Verify gun appears in selection UI
2. Test gun switching functionality
3. Verify muzzle point detection
4. Test audio and effects

## Muzzle Point Detection

The system automatically detects muzzle points using common naming conventions:
- "MuzzlePoint"
- "Muzzle" 
- "BarrelEnd"
- "FirePoint"
- "Barrel_End"

If no muzzle point is found, one is created automatically at the gun's forward tip.

### Manual Muzzle Point Setup
For better control, add an empty GameObject named "MuzzlePoint" to your gun prefab:
1. Position it at the barrel end
2. Orient it forward (positive Z-axis)
3. The system will automatically detect and use it

## VR Considerations

### Gun Positioning
- Guns should be oriented with barrel pointing forward (+Z)
- Scale appropriately for VR hand size
- Consider grip positioning for natural holding

### UI Positioning
- Place gun selection UI 2-3 meters from player
- Use world-space Canvas for VR compatibility
- Ensure buttons are large enough for VR interaction

### Performance
- Use LOD for gun models if needed
- Optimize textures for VR rendering
- Consider object pooling for effects

## Troubleshooting

### Common Issues

1. **Gun not appearing**: Check gun prefab assignment in collection
2. **Wrong orientation**: Ensure gun prefab faces forward (+Z axis)
3. **Muzzle effects in wrong position**: Add manual MuzzlePoint to prefab
4. **Audio not playing**: Check gun-specific audio clip assignments
5. **UI not responding**: Verify GunSelectionManager reference in UI

### Debug Tips
- Use Debug.Log in GunSelectionManager to trace gun changes
- Check Unity Console for muzzle point detection messages
- Verify gun collection validation passes
- Test with simple gun switching buttons first

## Future Enhancements

Potential improvements for the gun selection system:

1. **Gun Unlocking**: Progression system to unlock new guns
2. **Gun Customization**: Color schemes, attachments, modifications
3. **Gun Statistics**: Accuracy, damage, fire rate displays
4. **Gun Animations**: Reload animations, firing animations
5. **Gun Physics**: Recoil simulation, weight differences
6. **Gun Sounds**: Unique audio for each gun type
7. **Gun Effects**: Unique muzzle flashes, shell ejection
8. **Gun Persistence**: Save/load gun configurations

## Performance Notes

- Gun switching is optimized with object destruction/instantiation
- Player preferences are saved using PlayerPrefs
- Muzzle point detection is cached after first calculation
- UI updates are event-driven to minimize overhead
- Gun collection validation occurs only at startup