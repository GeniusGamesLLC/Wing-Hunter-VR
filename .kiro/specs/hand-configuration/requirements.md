# Requirements Document

## Introduction

This document specifies the requirements for a hand configuration system and settings menu in the VR Duck Hunt game. The system allows players to customize their VR hand setup, including the option to hide the left hand visuals and choose between single-handed or dual-wielding gun configurations. A new settings menu provides access to these options and serves as a foundation for future game settings.

## Glossary

- **VR Hand**: The visual representation of the player's hand in VR, typically rendered as a controller model or hand mesh
- **Hand Configuration System**: The system component that manages hand visibility and gun attachment settings
- **Dual Wielding Mode**: A gameplay mode where the player holds a gun in each hand (left and right)
- **Single Hand Mode**: A gameplay mode where the player holds a gun in only one hand (typically the dominant hand)
- **Left Hand Visual**: The 3D model or mesh representing the player's left hand/controller in VR
- **Right Hand Visual**: The 3D model or mesh representing the player's right hand/controller in VR
- **Gun Attachment Point**: The transform on a VR controller where a gun model is parented
- **Settings Menu**: A world-space UI panel that displays configurable game options
- **Settings Panel**: A visual container within the Settings Menu that groups related options
- **Gun Showcase**: A physical display (table, wall rack, or pedestal) where guns are displayed for player selection
- **Gun Pickup**: The action of grabbing a gun from the showcase to equip it
- **Controller Visual**: The default 3D model representing the VR controller when no gun is held

## Requirements

### Requirement 1

**User Story:** As a VR player, I want to hide the left hand visuals, so that I have a cleaner view and can focus on shooting with my dominant hand.

#### Acceptance Criteria

1. WHEN the player enables "hide left hand" option THEN the Hand Configuration System SHALL disable rendering of the Left Hand Visual
2. WHEN the left hand is hidden THEN the VR System SHALL continue tracking the left controller position for other interactions
3. WHEN the player disables "hide left hand" option THEN the Hand Configuration System SHALL re-enable rendering of the Left Hand Visual
4. WHEN the game starts THEN the Hand Configuration System SHALL apply the previously saved left hand visibility preference

### Requirement 2

**User Story:** As a VR player, I want to choose between dual wielding guns or using a single gun, so that I can play in my preferred style.

#### Acceptance Criteria

1. WHEN the player selects dual wielding mode THEN the Hand Configuration System SHALL attach a gun to both the left and right VR controllers
2. WHEN the player selects single hand mode THEN the Hand Configuration System SHALL attach a gun only to the right VR controller
3. WHEN in dual wielding mode THEN the Shooting Mechanic SHALL allow firing from both guns independently
4. WHEN in single hand mode THEN the Shooting Mechanic SHALL only respond to the right controller trigger
5. WHEN the gun mode changes THEN the Hand Configuration System SHALL persist the player's choice for future game sessions

### Requirement 3

**User Story:** As a VR player, I want a settings menu to access game options, so that I can customize my gameplay experience.

#### Acceptance Criteria

1. WHEN the player presses the menu button on the left controller THEN the VR System SHALL display the Settings Menu in world space
2. WHEN the Settings Menu is displayed THEN the VR System SHALL position the menu at a comfortable viewing distance in front of the player
3. WHEN the Settings Menu is open THEN the VR System SHALL pause the game if a game session is active
4. WHEN the player closes the Settings Menu THEN the VR System SHALL resume the game if it was paused
5. WHEN the Settings Menu is displayed THEN the VR System SHALL allow interaction via VR controller ray pointer

### Requirement 4

**User Story:** As a VR player, I want hand configuration options in the settings menu, so that I can adjust hand and gun settings easily.

#### Acceptance Criteria

1. WHEN the Settings Menu is displayed THEN the Settings Panel SHALL show a toggle for left hand visibility
2. WHEN the Settings Menu is displayed THEN the Settings Panel SHALL show a toggle or selector for gun mode (single/dual)
3. WHEN the player changes a setting THEN the VR System SHALL apply the change immediately
4. WHEN the player changes a setting THEN the VR System SHALL save the preference automatically



### Requirement 5

**User Story:** As a VR player, I want to pick up guns from a showcase display, so that I can physically interact with weapon selection in an immersive way.

#### Acceptance Criteria

1. WHEN the game starts THEN the VR System SHALL display available guns on a Gun Showcase in the play area
2. WHEN no gun is equipped THEN the VR System SHALL display the Controller Visual for that hand
3. WHEN the player reaches toward a gun on the showcase and presses the grip button THEN the Hand Configuration System SHALL attach that gun to the player's hand
4. WHEN a gun is picked up THEN the Hand Configuration System SHALL hide the Controller Visual for that hand
5. WHEN a gun is equipped THEN the Gun Showcase SHALL visually indicate that gun is currently in use

### Requirement 6

**User Story:** As a VR player, I want to swap my current gun for a different one on the showcase, so that I can try different weapons during gameplay.

#### Acceptance Criteria

1. WHEN the player holds a gun near the Gun Showcase and presses the grip button THEN the Hand Configuration System SHALL return the current gun to the showcase
2. WHEN a gun is returned to the showcase THEN the Gun Showcase SHALL display that gun in its original position
3. WHEN a gun is returned THEN the VR System SHALL display the Controller Visual for that hand
4. WHEN the player picks up a different gun after returning one THEN the Hand Configuration System SHALL equip the new gun
5. WHEN swapping guns THEN the Hand Configuration System SHALL update the gun preference for that hand
