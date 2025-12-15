# Implementation Plan

- [ ] 1. Create hand configuration data structures
- [ ] 1.1 Create HandConfigurationSettings class
  - Define serializable class with leftHandVisible, gunMode, leftGunId, rightGunId fields
  - Add default values (leftHandVisible=true, gunMode=SingleHand)
  - Create Hand and GunMode enums
  - _Requirements: 1.4, 2.5_

- [ ] 1.2 Create SettingsData ScriptableObject
  - Define menu positioning parameters (distance, height)
  - Define showcase position and rotation defaults
  - Create asset instance in Assets/Data folder
  - _Requirements: 3.2_

- [ ] 2. Implement HandConfigurationManager
- [ ] 2.1 Create HandConfigurationManager script
  - Implement LeftHandVisible property with setter that updates visuals
  - Implement CurrentGunMode property with setter that updates gun attachments
  - Add events: OnLeftHandVisibilityChanged, OnGunModeChanged, OnGunEquipped, OnGunUnequipped
  - Implement EquipGun, UnequipGun, GetEquippedGun, IsGunEquipped methods
  - _Requirements: 1.1, 1.3, 2.1, 2.2_

- [ ] 2.2 Add preference persistence to HandConfigurationManager
  - Save settings to PlayerPrefs on change
  - Load settings from PlayerPrefs on Awake
  - Apply loaded settings on Start
  - _Requirements: 1.4, 2.5_

- [ ]* 2.3 Write property test for left hand visibility toggle
  - **Property 1: Left hand visibility toggle round-trip**
  - **Validates: Requirements 1.1, 1.3**

- [ ]* 2.4 Write property test for visibility preference persistence
  - **Property 3: Left hand visibility preference persistence**
  - **Validates: Requirements 1.4**

- [ ]* 2.5 Write property test for gun mode preference persistence
  - **Property 8: Gun mode preference persistence**
  - **Validates: Requirements 2.5**

- [ ] 3. Implement HandVisualController
- [ ] 3.1 Create HandVisualController script
  - Add Hand enum field to identify left/right
  - Cache references to controller model renderers
  - Implement ShowControllerVisual and HideControllerVisual methods
  - Implement OnGunEquipped and OnGunUnequipped callbacks
  - Subscribe to HandConfigurationManager events
  - _Requirements: 5.2, 5.4, 6.3_

- [ ] 3.2 Integrate HandVisualController with XR controllers
  - Find and reference controller model GameObjects in XR Origin
  - Wire up to existing controller setup
  - Test controller visuals toggle correctly
  - _Requirements: 5.2_

- [ ]* 3.3 Write property test for controller visual state
  - **Property 13: Controller visual state matches gun equipped state**
  - **Validates: Requirements 5.2, 5.4, 6.3**

- [ ] 4. Modify ShootingController for dual wielding
- [ ] 4.1 Refactor ShootingController for per-hand shooting
  - Add support for left and right gun references
  - Subscribe to both left and right trigger input actions
  - Implement per-hand raycast from respective gun muzzle points
  - Add gun mode check to enable/disable left hand shooting
  - _Requirements: 2.3, 2.4_

- [ ] 4.2 Integrate ShootingController with HandConfigurationManager
  - Subscribe to OnGunModeChanged event
  - Update active guns when mode changes
  - Subscribe to OnGunEquipped/OnGunUnequipped for dynamic gun changes
  - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [ ]* 4.3 Write property test for dual wielding firing
  - **Property 6: Dual wielding enables independent firing**
  - **Validates: Requirements 2.3**

- [ ]* 4.4 Write property test for single hand mode
  - **Property 7: Single hand mode ignores left trigger**
  - **Validates: Requirements 2.4**

- [ ]* 4.5 Write property test for dual wielding gun attachment
  - **Property 4: Dual wielding attaches guns to both hands**
  - **Validates: Requirements 2.1**

- [ ]* 4.6 Write property test for single hand gun attachment
  - **Property 5: Single hand mode attaches gun only to right hand**
  - **Validates: Requirements 2.2**

- [ ] 5. Checkpoint - Ensure core hand configuration works
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 6. Implement SettingsMenuManager
- [ ] 6.1 Create SettingsMenuManager script
  - Implement IsMenuOpen property
  - Implement OpenMenu, CloseMenu, ToggleMenu methods
  - Add OnMenuOpened and OnMenuClosed events
  - Position menu in front of player using SettingsData parameters
  - _Requirements: 3.1, 3.2_

- [ ] 6.2 Implement game pause/resume on menu
  - Store previous Time.timeScale on menu open
  - Set Time.timeScale to 0 when menu opens
  - Restore Time.timeScale when menu closes
  - Handle edge case of menu open when game not running
  - _Requirements: 3.3, 3.4_

- [ ] 6.3 Wire menu button input
  - Subscribe to left controller menu button input action
  - Call ToggleMenu on button press
  - _Requirements: 3.1_

- [ ]* 6.4 Write property test for menu toggle
  - **Property 9: Menu button toggles settings menu**
  - **Validates: Requirements 3.1**

- [ ]* 6.5 Write property test for menu positioning
  - **Property 10: Settings menu positioned in front of player**
  - **Validates: Requirements 3.2**

- [ ]* 6.6 Write property test for pause/resume
  - **Property 11: Settings menu pause/resume round-trip**
  - **Validates: Requirements 3.3, 3.4**

- [ ] 7. Create Settings Menu UI
- [ ] 7.1 Create world-space settings canvas
  - Create Canvas with World Space render mode
  - Add panel background with VR-readable styling
  - Position using SettingsData parameters
  - Add XR UI interaction components for ray pointer
  - _Requirements: 3.2, 3.5_

- [ ] 7.2 Create hand configuration UI elements
  - Add toggle for left hand visibility
  - Add toggle or dropdown for gun mode (Single/Dual)
  - Add close button
  - Style for VR readability (large text, high contrast)
  - _Requirements: 4.1, 4.2_

- [ ] 7.3 Wire UI to HandConfigurationManager
  - Connect left hand toggle to SetLeftHandVisibility
  - Connect gun mode selector to SetGunMode
  - Update UI state when settings change externally
  - _Requirements: 4.3, 4.4_

- [ ]* 7.4 Write property test for immediate settings application
  - **Property 12: Settings changes apply immediately**
  - **Validates: Requirements 4.3**

- [ ] 8. Checkpoint - Ensure settings menu works
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 9. Implement GunShowcaseController
- [ ] 9.1 Create GunShowcaseController script
  - Define AvailableGuns array from GunCollection
  - Implement ReturnGun method to restore gun to showcase
  - Implement IsGunAvailable and SetGunInUse methods
  - Add OnGunPickedUp and OnGunReturned events
  - _Requirements: 5.1, 5.5, 6.1, 6.2_

- [ ] 9.2 Create showcase display structure
  - Create parent GameObject for showcase (table/rack)
  - Create gun slot positions for each available gun
  - Add visual indicators for in-use state (e.g., outline, dimming)
  - Position showcase in play area using SettingsData
  - _Requirements: 5.1, 5.5_

- [ ]* 9.3 Write property test for showcase in-use indicator
  - **Property 15: Showcase indicates gun in-use state**
  - **Validates: Requirements 5.5**

- [ ]* 9.4 Write property test for gun return
  - **Property 16: Gun return restores showcase state**
  - **Validates: Requirements 6.1, 6.2**

- [ ] 10. Implement GunPickupInteractable
- [ ] 10.1 Create GunPickupInteractable script
  - Add GunData reference field
  - Implement IsAvailable property
  - Implement SetAvailable method to show/hide gun on showcase
  - Add XR Grab Interactable component for grip interaction
  - _Requirements: 5.3_

- [ ] 10.2 Implement pickup logic
  - Detect grip button press when hand is near gun
  - Call HandConfigurationManager.EquipGun on pickup
  - Notify GunShowcaseController of pickup
  - _Requirements: 5.3_

- [ ] 10.3 Implement return logic
  - Detect grip button press when holding gun near showcase
  - Call HandConfigurationManager.UnequipGun on return
  - Notify GunShowcaseController of return
  - _Requirements: 6.1_

- [ ]* 10.4 Write property test for gun pickup
  - **Property 14: Gun pickup attaches gun to hand**
  - **Validates: Requirements 5.3, 6.4**

- [ ]* 10.5 Write property test for gun swap preference
  - **Property 17: Gun swap updates preference**
  - **Validates: Requirements 6.5**

- [ ] 11. Integrate with existing GunSelectionManager
- [ ] 11.1 Refactor GunSelectionManager for showcase integration
  - Remove automatic gun attachment on start
  - Add method to get gun by ID for showcase
  - Ensure GunCollection is accessible to showcase
  - _Requirements: 5.1_

- [ ] 11.2 Update gun attachment flow
  - HandConfigurationManager calls GunSelectionManager for gun instantiation
  - Ensure muzzle points are correctly set up for both hands
  - Handle gun-specific effects (audio, haptics) per hand
  - _Requirements: 2.1, 2.2_

- [ ] 12. Add left hand tracking verification
- [ ] 12.1 Ensure hidden hand maintains tracking
  - Verify controller transform updates when visuals hidden
  - Test that hiding visuals doesn't disable XR tracking
  - _Requirements: 1.2_

- [ ]* 12.2 Write property test for hidden hand tracking
  - **Property 2: Hidden hand maintains tracking**
  - **Validates: Requirements 1.2**

- [ ] 13. Checkpoint - Ensure gun showcase works
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 14. Scene setup and integration
- [ ] 14.1 Add HandConfigurationManager to scene
  - Create GameObject with HandConfigurationManager component
  - Wire references to left and right hand controllers
  - Configure default settings
  - _Requirements: All_

- [ ] 14.2 Add HandVisualController to each hand
  - Add component to left controller GameObject
  - Add component to right controller GameObject
  - Wire renderer references
  - _Requirements: 5.2_

- [ ] 14.3 Create and position gun showcase in scene
  - Instantiate showcase prefab or create in scene
  - Position in front of player start position
  - Populate with available guns from GunCollection
  - _Requirements: 5.1_

- [ ] 14.4 Create and position settings menu in scene
  - Create settings canvas (initially hidden)
  - Wire to SettingsMenuManager
  - Test menu button opens/closes menu
  - _Requirements: 3.1, 3.2_

- [ ] 15. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.
