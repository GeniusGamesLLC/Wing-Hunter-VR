# Credits System Documentation

## Overview

The credits system provides a comprehensive way to track and display attribution information for all assets used in the VR Duck Hunt project. This system ensures proper attribution and license compliance.

## Components

### 1. AssetCredit Struct
- **File**: `AssetCredit.cs`
- **Purpose**: Defines the structure for individual asset attribution information
- **Fields**:
  - `assetName`: Name of the asset
  - `author`: Creator or author of the asset
  - `license`: License type (e.g., "MIT", "CC BY 4.0", "Free for commercial use")
  - `source`: Where the asset was obtained (e.g., "Unity Asset Store", "GitHub")
  - `attributionText`: Full attribution text as required by the license

### 2. CreditsData ScriptableObject
- **File**: `CreditsData.cs`
- **Purpose**: Stores collection of asset credits and provides formatted output
- **Features**:
  - Array of AssetCredit structs
  - `GetFormattedCreditsText()` method for display-ready text
  - Unity Inspector integration for easy editing

### 3. CreditsManager MonoBehaviour
- **File**: `CreditsManager.cs`
- **Purpose**: Manages the credits UI and user interactions
- **Features**:
  - Show/hide credits panel
  - Load credits from CreditsData asset
  - Handle button interactions
  - Scroll position management

## Setup Instructions

### 1. Create CreditsData Asset
1. Right-click in Project window
2. Select "Create > Game > Credits Data"
3. Name the asset (e.g., "ProjectCreditsData")
4. Fill in the asset credits in the Inspector

### 2. Set Up UI (Manual Method)
1. Create a Canvas in your scene (if not already present)
2. Add the CreditsManager script to a GameObject
3. Create UI elements:
   - Credits button (to show credits)
   - Credits panel (container for credits display)
   - Scrollable text area (for credits content)
   - Close button (to hide credits)
4. Assign UI references in CreditsManager Inspector
5. Assign CreditsData asset to CreditsManager

### 3. Set Up UI (Automated Method)
1. Add `CreditsUISetup` script to a GameObject
2. Check "Create Credits UI" in Inspector
3. Play the scene to auto-generate UI structure
4. Manually assign CreditsData asset to generated CreditsManager

### 4. Integration with Game UI
- Add credits button to main menu or game over screen
- Position credits panel appropriately for VR viewing
- Ensure credits panel is initially hidden
- Test show/hide functionality

## Usage Examples

### Adding New Asset Credit
```csharp
// In CreditsData Inspector, add new element to Asset Credits array:
Asset Name: "Duck Model Pack"
Author: "3D Artist Name"
License: "CC BY 4.0"
Source: "OpenGameArt.org"
Attribution Text: "Duck models by 3D Artist Name, licensed under CC BY 4.0"
```

### Programmatic Access
```csharp
// Get formatted credits text
CreditsData creditsData = // ... load your asset
string creditsText = creditsData.GetFormattedCreditsText();

// Show credits programmatically
CreditsManager creditsManager = FindObjectOfType<CreditsManager>();
creditsManager.ShowCredits();
```

## VR Considerations

### UI Positioning
- Position credits panel 2-3 meters from player
- Use world-space Canvas for VR compatibility
- Ensure text is large enough to read in VR
- Consider comfortable viewing angles

### Interaction
- Use VR controller ray casting for button interactions
- Provide haptic feedback for button presses
- Ensure scrolling works with VR input
- Test readability with different headsets

## License Compliance

### Common License Types
- **MIT License**: Requires attribution in documentation
- **CC BY 4.0**: Requires attribution with author and license info
- **Unity Asset Store**: Follow Unity Asset Store EULA
- **Free Assets**: Check specific requirements

### Best Practices
1. Always include original author information
2. Specify exact license type and version
3. Include source URL when available
4. Update CREDITS.md file alongside ScriptableObject
5. Review license requirements before using assets
6. Keep attribution information up to date

## Maintenance

### Regular Updates
- Review and update credits when adding new assets
- Verify license compliance periodically
- Update CREDITS.md file to match ScriptableObject data
- Test credits display after UI changes

### Version Control
- Include CreditsData assets in version control
- Track changes to attribution information
- Document license requirement changes
- Maintain backup of attribution data

## Troubleshooting

### Common Issues
1. **Credits not displaying**: Check CreditsData asset assignment
2. **UI not showing**: Verify Canvas and UI element setup
3. **Text formatting issues**: Check TextMeshPro component settings
4. **Button not working**: Verify button event assignments

### Debug Tips
- Use Debug.Log in CreditsManager to trace execution
- Check Unity Console for missing component warnings
- Verify UI element active states
- Test with sample credits data first