# Credits System Integration Guide

## Overview

This guide explains how to integrate the credits system with the existing VR Duck Hunt game systems, particularly with the UI and game management components.

## Integration Points

### 1. GameManager Integration

The credits system can be integrated with the GameManager to show credits during game over or from the main menu.

```csharp
// In GameManager.cs, add reference to CreditsManager
[SerializeField] private CreditsManager creditsManager;

// Method to show credits from game over state
public void ShowCreditsFromGameOver()
{
    if (creditsManager != null)
    {
        creditsManager.ShowCredits();
    }
}
```

### 2. UI Canvas Integration

Since tasks 11.1 and 11.2 (UI canvas creation) haven't been completed yet, the credits system is designed to work with any UI setup:

#### Option A: World-Space Canvas (Recommended for VR)
```csharp
// Create world-space canvas positioned 2-3 meters from player
Canvas canvas = // ... your world-space canvas
canvas.renderMode = RenderMode.WorldSpace;
canvas.transform.position = new Vector3(0, 1.5f, 3f); // 3 meters in front, 1.5m high
canvas.transform.localScale = Vector3.one * 0.01f; // Scale for VR viewing
```

#### Option B: Screen Space Canvas
```csharp
// For testing or non-VR scenarios
Canvas canvas = // ... your screen-space canvas
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
```

### 3. Menu System Integration

Add credits button to existing menu systems:

```csharp
// Example: Adding to a main menu
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private CreditsManager creditsManager;
    [SerializeField] private Button creditsButton;
    
    private void Start()
    {
        if (creditsButton != null && creditsManager != null)
        {
            creditsButton.onClick.AddListener(creditsManager.ShowCredits);
        }
    }
}
```

### 4. Game Over Screen Integration

Show credits option when game ends:

```csharp
// In your game over UI script
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private CreditsManager creditsManager;
    [SerializeField] private Button viewCreditsButton;
    
    public void ShowGameOverScreen()
    {
        // ... existing game over logic
        
        // Enable credits button
        if (viewCreditsButton != null)
        {
            viewCreditsButton.gameObject.SetActive(true);
        }
    }
}
```

## Setup Workflow

### Step 1: Create CreditsData Asset
1. In Unity Editor: Right-click in Project → Create → Game → Credits Data
2. Name it "VRDuckHuntCredits" 
3. Fill in asset information (see sample data below)

### Step 2: Set Up UI Structure
Choose one of these approaches:

#### Approach A: Manual Setup
1. Create Canvas (World Space for VR)
2. Add CreditsManager script to a GameObject
3. Create UI elements manually
4. Assign references in CreditsManager Inspector

#### Approach B: Automated Setup
1. Add CreditsUISetup script to any GameObject
2. Check "Create Credits UI" in Inspector
3. Play scene to generate UI
4. Assign CreditsData asset to generated CreditsManager

### Step 3: Integration with Existing Systems
1. Add credits button to main menu or game over screen
2. Wire button to CreditsManager.ShowCredits()
3. Position credits panel appropriately for VR
4. Test functionality

## Sample Credits Data

Here's sample data to populate your CreditsData asset:

```
Asset Name: "Weapons of Choice FREE"
Author: "Komposite Sound"
License: "Free for commercial and non-commercial use"
Source: "Unity Asset Store"
Attribution: "Gun sound effects provided by Komposite Sound"

Asset Name: "XR Interaction Toolkit"
Author: "Unity Technologies"
License: "Unity Package License"
Source: "Unity Package Manager"
Attribution: "VR functionality powered by Unity XR Interaction Toolkit"

Asset Name: "Duck 3D Model"
Author: "[Your 3D Artist]"
License: "[License Type]"
Source: "[Source Location]"
Attribution: "[Required Attribution Text]"
```

## VR-Specific Considerations

### Positioning
- Place credits panel 2-3 meters from player
- Use comfortable viewing angle (slightly below eye level)
- Ensure text is large enough to read in VR headset

### Interaction
- Use VR controller ray casting for button interactions
- Provide haptic feedback for button presses
- Test with actual VR headset for usability

### Performance
- Keep credits text reasonable length to avoid performance issues
- Use object pooling if credits panel is shown/hidden frequently
- Consider LOD for text rendering if needed

## Testing Checklist

- [ ] CreditsData asset created and populated
- [ ] CreditsManager added to scene
- [ ] UI elements created and assigned
- [ ] Credits button shows credits panel
- [ ] Close button hides credits panel
- [ ] Scroll functionality works (if content is long)
- [ ] Credits text displays correctly
- [ ] Integration with game flow works
- [ ] VR interaction tested (if applicable)
- [ ] Performance is acceptable

## Troubleshooting

### Common Issues

1. **Credits not showing**: Check CreditsData asset assignment
2. **UI elements missing**: Verify Canvas and UI setup
3. **Button not responding**: Check button event assignments
4. **Text not readable in VR**: Adjust font size and panel distance
5. **Performance issues**: Reduce text length or optimize UI

### Debug Steps

1. Check Unity Console for errors
2. Verify all UI references are assigned
3. Test with sample credits data first
4. Use Debug.Log in CreditsManager methods
5. Check Canvas render mode and settings

## Future Enhancements

Potential improvements for the credits system:

1. **Animated transitions**: Fade in/out effects for credits panel
2. **Categorized credits**: Group credits by type (Audio, Models, Code, etc.)
3. **Clickable links**: Make source URLs clickable (where applicable)
4. **Localization**: Support for multiple languages
5. **Dynamic loading**: Load credits from external files
6. **Version tracking**: Track which assets are used in which game versions