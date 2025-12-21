# Requirements Document

## Introduction

This document specifies the requirements for implementing a VR-accessible settings menu system styled as a physical "Announcement Board" with pinned papers representing different menu sections. The board includes a Settings paper, a hidden Debug paper (unlocked via Konami code), and is extensible for future papers like announcements or release notes. The design matches the existing gun rack aesthetic for visual consistency.

## Glossary

- **Announcement_Board**: A physical world-space object (cork board/wooden board style) that holds all menu papers; can be summoned to appear in front of the player
- **Menu_Paper**: A pinned note/paper on the board representing a distinct menu section (e.g., Settings, Debug, Announcements)
- **Settings_Paper**: The default menu paper containing game settings options
- **Debug_Paper**: A hidden menu paper dedicated to debug visualization toggles, auto-generated from DebugSettings; appears only after Konami code unlock
- **DebugSettings**: The centralized singleton MonoBehaviour that holds all debug visualization flags as boolean properties
- **Category**: A logical grouping of related debug options (e.g., Spawning, Pathing, UI) defined via C# attributes
- **Auto-generation**: The process of using C# reflection to automatically create UI toggle elements from DebugSettings properties
- **Toggle**: A UI element (Unity Toggle component) that represents a single boolean option
- **VR_Konami_Code**: The classic Konami code adapted for VR: Either thumbstick (Up, Up, Down, Down, Left, Right, Left, Right), then B button, A button
- **Compact_State**: The unfocused state of a Menu_Paper showing a small preview with placeholder content
- **Expanded_State**: The focused state of a Menu_Paper showing full-size content with interactive elements
- **Scroll_Indicator**: A visual element showing the current scroll position within scrollable content
- **Label_Tab**: A small paper or tag element positioned above a Menu_Paper displaying its title/category

## Requirements

### Requirement 1

**User Story:** As a VR user, I want to summon an announcement board menu that appears in front of me, so that I can access game settings without leaving VR.

#### Acceptance Criteria

1. WHEN the user presses the Left Primary Button (X button on Quest), THE Announcement_Board SHALL toggle between visible and hidden states
2. WHILE the Announcement_Board is hidden, THE Announcement_Board SHALL remain inactive and not obstruct the player's view
3. WHEN the Announcement_Board becomes visible, THE Announcement_Board SHALL appear within 1-2 meters in front of the player camera at eye level, facing the player
4. WHEN the Announcement_Board is summoned, THE Announcement_Board SHALL display the Settings_Paper as the default focused paper

### Requirement 2

**User Story:** As a VR user, I want the announcement board to display multiple papers I can select, so that I can navigate between different menu sections.

#### Acceptance Criteria

1. WHEN the Announcement_Board is displayed, THE Announcement_Board SHALL show all unlocked Menu_Papers pinned to the board
2. WHEN the user points at and selects a Menu_Paper, THE Announcement_Board SHALL bring that paper into focus and display its content
3. WHEN a Menu_Paper is in focus, THE Announcement_Board SHALL visually highlight it and dim other papers
4. WHEN the Debug_Paper is locked, THE Announcement_Board SHALL not display the Debug_Paper on the board

### Requirement 3

**User Story:** As a developer, I want a Debug paper that displays all debug options, so that I can toggle visualizations without leaving VR or using the Unity Inspector.

#### Acceptance Criteria

1. WHEN the Debug_Paper is in focus, THE Debug_Paper SHALL show all debug toggles from DebugSettings
2. WHEN a user changes a toggle value, THE Debug_Paper SHALL update the corresponding DebugSettings property within the same frame
3. WHEN DebugSettings properties change externally, THE Debug_Paper SHALL synchronize toggle states to reflect the current values

### Requirement 4

**User Story:** As a developer, I want debug options organized by category, so that I can quickly find related settings.

#### Acceptance Criteria

1. WHEN displaying debug options, THE Debug_Paper SHALL group toggles by their assigned category (Spawning, Pathing, UI, etc.)
2. WHEN a category contains one or more options, THE Debug_Paper SHALL display a visible header label for that category
3. WHEN new debug options are added to DebugSettings with a category attribute, THE Debug_Paper SHALL automatically include them under the appropriate category header

### Requirement 5

**User Story:** As a developer, I want the Debug paper UI to be auto-generated from DebugSettings, so that I don't need to manually update the UI when adding new debug options.

#### Acceptance Criteria

1. WHEN the Debug_Paper initializes, THE Debug_Paper SHALL use C# reflection to discover all public boolean properties in DebugSettings
2. WHEN a debug property has a DebugCategory attribute, THE Debug_Paper SHALL place the corresponding toggle under that category section
3. WHEN a debug property has a Tooltip attribute, THE Debug_Paper SHALL display the tooltip text when the user hovers over that toggle

### Requirement 6

**User Story:** As a developer, I want a "Toggle All" option per category, so that I can quickly enable or disable all options in a group.

#### Acceptance Criteria

1. WHEN a category header is displayed, THE Debug_Paper SHALL include a "Toggle All" button adjacent to the category label
2. WHEN the user presses a "Toggle All" button, THE Debug_Paper SHALL set all toggles in that category to the inverse of the majority state (if most are on, turn all off; if most are off, turn all on)

### Requirement 7

**User Story:** As a VR user, I want a close/dismiss option on the announcement board, so that I can hide it without using the controller button.

#### Acceptance Criteria

1. WHEN the Announcement_Board is displayed, THE Announcement_Board SHALL show a close button or dismissible area
2. WHEN the user activates the close option, THE Announcement_Board SHALL hide and return focus to gameplay

### Requirement 8

**User Story:** As a developer, I want the Debug paper to be hidden by default and unlocked via a secret VR input sequence, so that end users don't accidentally access debug features.

#### Acceptance Criteria

1. WHEN the game starts, THE Debug_Paper SHALL be locked and hidden from the Announcement_Board
2. WHEN the user performs the VR Konami Code sequence (Either thumbstick: Up, Up, Down, Down, Left, Right, Left, Right; then B button, A button), THE Announcement_Board SHALL unlock the Debug_Paper and pin it to the board
3. WHEN the Debug_Paper is unlocked, THE Announcement_Board SHALL play a confirmation sound and animate the Debug_Paper appearing/pinning to the board
4. WHILE the Debug_Paper is unlocked, THE Announcement_Board SHALL persist the unlocked state for the current session only

### Requirement 9

**User Story:** As a VR user, I want the announcement board to match the game's visual style, so that it feels like part of the game world.

#### Acceptance Criteria

1. WHEN the Announcement_Board is displayed, THE Announcement_Board SHALL use a cork board or wooden board visual style consistent with the gun rack aesthetic
2. WHEN Menu_Papers are displayed, THE Announcement_Board SHALL render them as pinned notes/papers with visible pins or tacks
3. WHEN the Debug_Paper is unlocked, THE Announcement_Board SHALL animate it appearing as if being pinned to the board

### Requirement 10

**User Story:** As a VR user, I want papers to expand when focused so that I can see more content and interact with settings comfortably.

#### Acceptance Criteria

1. WHEN a Menu_Paper is unfocused, THE Menu_Paper SHALL display at a compact size with placeholder/preview content indicating its purpose
2. WHEN a Menu_Paper becomes focused, THE Menu_Paper SHALL animate to a larger expanded size (similar to Quest Settings app panels)
3. WHEN a Menu_Paper is expanded, THE Menu_Paper SHALL display full content with readable text and interactive elements
4. WHEN a Menu_Paper loses focus, THE Menu_Paper SHALL animate back to the compact unfocused state

### Requirement 11

**User Story:** As a VR user, I want debug options to be left-aligned and properly formatted, so that I can easily scan and read the settings list.

#### Acceptance Criteria

1. WHEN displaying debug toggles, THE Debug_Paper SHALL left-align all toggle labels and category headers
2. WHEN displaying debug toggles, THE Debug_Paper SHALL use consistent indentation for toggles under category headers
3. WHEN displaying category headers, THE Debug_Paper SHALL visually distinguish headers from toggle items using font weight or size

### Requirement 12

**User Story:** As a VR user, I want to interact with toggles using the VR trigger, so that I can change settings without the paper losing focus.

#### Acceptance Criteria

1. WHEN the user points at a toggle and presses the trigger, THE Debug_Paper SHALL toggle the setting value without unfocusing the paper
2. WHEN the user points at a Toggle All button and presses the trigger, THE Debug_Paper SHALL toggle all items in that category without unfocusing the paper
3. WHEN the user points at empty space on a focused paper and presses the trigger, THE Debug_Paper SHALL maintain focus and not dismiss

### Requirement 13

**User Story:** As a VR user, I want each paper to have a visible label/tab above it, so that I can quickly identify what each paper contains.

#### Acceptance Criteria

1. WHEN a Menu_Paper is displayed on the board, THE Announcement_Board SHALL show a label tab above the paper indicating its title (e.g., "Settings", "Debug", "News")
2. WHEN displaying the label tab, THE Announcement_Board SHALL style it as a smaller paper or tag pinned above the main paper
3. WHEN a Menu_Paper is in the compact unfocused state, THE Announcement_Board SHALL keep the label tab visible and readable
4. WHEN a Menu_Paper expands to focused state, THE Announcement_Board SHALL keep the label tab pinned to the board (not moving with the paper)

### Requirement 14

**User Story:** As a VR user, I want to scroll through debug options when there are many settings, so that I can access all options without the paper becoming too large.

#### Acceptance Criteria

1. WHEN the Debug_Paper content exceeds the visible area, THE Debug_Paper SHALL enable vertical scrolling
2. WHEN scrolling is enabled, THE Debug_Paper SHALL respond to thumbstick up/down input for scrolling
3. WHEN scrolling is enabled, THE Debug_Paper SHALL display a visual scroll indicator showing current position
4. WHEN the user reaches the top or bottom of scrollable content, THE Debug_Paper SHALL provide visual feedback indicating the boundary

### Requirement 15

**User Story:** As a VR user, I want the focused paper to be positioned at a comfortable reading distance with adequate size, so that I can easily read and interact with all content.

#### Acceptance Criteria

1. WHEN a Menu_Paper becomes focused, THE Menu_Paper SHALL position itself approximately 1.2-1.5 meters in front of the player (further than current ~1m distance)
2. WHEN a Menu_Paper is expanded, THE Menu_Paper SHALL be large enough to display content with comfortable margins and readable text
3. WHEN displaying debug toggles, THE Debug_Paper SHALL have sufficient width to show toggle labels and switches without crowding
4. WHEN the paper is focused, THE Menu_Paper SHALL maintain a comfortable viewing angle relative to the player's eye level

### Requirement 16

**User Story:** As a VR user, I want toggle switches to have a clear visual design, so that I can easily see the current state and interact with them.

#### Acceptance Criteria

1. WHEN displaying a toggle, THE Debug_Paper SHALL render it as a visual switch (iOS-style) with a sliding knob and track
2. WHEN a toggle is OFF, THE Debug_Paper SHALL display the knob on the left side with a gray track
3. WHEN a toggle is ON, THE Debug_Paper SHALL display the knob on the right side with a colored (green/blue) track
4. WHEN a toggle state changes, THE Debug_Paper SHALL animate the knob sliding to its new position
5. WHEN displaying toggle layout, THE Debug_Paper SHALL position the label on the left and the toggle switch on the right with consistent spacing
6. WHEN displaying a Toggle All button, THE Debug_Paper SHALL render it with a distinct button style (not just text) with visual feedback on interaction
