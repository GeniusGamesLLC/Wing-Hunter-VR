# Implementation Plan

- [x] 1. Create GitHub repository and Unity project
- [x] 1.1 Create new GitHub repository
  - Create new repository on GitHub (public or private)
  - Initialize with README.md describing the VR Duck Hunt project
  - Add Unity .gitignore template
  - Add MIT or appropriate license file
  - Clone repository locally
  - _Requirements: 8.2_

- [x] 1.2 Create Unity project and configure XR setup
  - Create new Unity 2021.3 LTS project in the cloned repository
  - Install XR Interaction Toolkit via Package Manager
  - Install XR Plugin Management and Oculus XR Plugin
  - Configure project settings for Android/Meta Quest build target
  - Set up XR Origin rig with camera and controllers
  - Create basic scene with ground plane and skybox
  - Commit initial Unity project structure to Git
  - _Requirements: 8.1_

- [x] 2. Implement core data structures and configuration
  - Create DuckHuntConfig ScriptableObject for game settings
  - Define DifficultySettings class with spawn interval and speed parameters
  - Define DuckSpawnData struct for spawn information
  - Create folder structure (Scripts/Managers, Scripts/Controllers, Scripts/Data)
  - _Requirements: 8.2, 8.4_

- [x] 3. Implement ScoreManager
- [x] 3.1 Create ScoreManager script with score tracking
  - Write ScoreManager MonoBehaviour with score and missed duck counters
  - Implement AddScore, IncrementMissed, and ResetScore methods
  - Add events for OnScoreChanged and OnGameOver
  - Implement game over trigger when max missed threshold reached
  - _Requirements: 3.1, 3.4, 5.1_

- [ ]* 3.2 Write property test for score increment
  - **Property 11: Hits increase score**
  - **Validates: Requirements 3.1**

- [ ]* 3.3 Write property test for miss counter
  - **Property 13: Escaped ducks increment miss counter**
  - **Validates: Requirements 3.4**

- [ ]* 3.4 Write property test for game over trigger
  - **Property 17: Miss threshold triggers game over**
  - **Validates: Requirements 5.1**

- [-] 4. Implement GameManager and state management
- [x] 4.1 Create GameManager script with state machine
  - Write GameManager MonoBehaviour with GameState enum (Idle, Playing, GameOver)
  - Implement StartGame, EndGame, and RestartGame methods
  - Add difficulty tracking and IncreaseDifficulty method
  - Wire up references to ScoreManager and SpawnManager
  - Implement score threshold monitoring for difficulty progression
  - _Requirements: 4.1, 5.1, 5.4_

- [ ]* 4.2 Write property test for difficulty progression
  - **Property 14: Score threshold triggers difficulty increase**
  - **Validates: Requirements 4.1**

- [ ]* 4.3 Write property test for restart state reset
  - **Property 19: Restart resets game state**
  - **Validates: Requirements 5.4**

- [x] 5. Create duck prefab and DuckController
- [x] 5.1 Create duck 3D model and prefab
  - Create or import simple duck 3D model with texture
  - Set up Animator component with flying animation
  - Add collider for hit detection
  - Create duck prefab in Prefabs folder
  - _Requirements: 2.5, 7.2_

- [x] 5.1.5 Wait for Unity compilation
  - Allow Unity to compile and process changes before continuing
  - Check Unity console for any compilation errors
  - Verify all assets are properly imported

- [x] 5.2 Implement DuckController script
  - Write DuckController MonoBehaviour with movement logic
  - Implement Initialize method to set start/end positions and speed
  - Add Update method to move duck using Vector3.MoveTowards
  - Implement OnHit method for destruction sequence
  - Add events for OnDestroyed and OnEscaped
  - Detect when duck reaches target position and trigger escape
  - _Requirements: 2.3, 2.4, 1.3_

- [ ]* 5.3 Write property test for duck movement
  - **Property 8: Active ducks move toward target**
  - **Validates: Requirements 2.3**

- [ ]* 5.4 Write property test for duck cleanup
  - **Property 9: Ducks cleanup at path end**
  - **Validates: Requirements 2.4**

- [ ]* 5.5 Write property test for duck destruction
  - **Property 3: Hit ducks are destroyed**
  - **Validates: Requirements 1.3**

- [x] 5.9 Wait for Unity compilation
  - Allow Unity to compile DuckController changes
  - Check Unity console for any compilation errors
  - Verify DuckController script compiles without errors

- [x] 6. Implement SpawnManager
- [x] 6.1 Create SpawnManager script with spawning logic
  - Write SpawnManager MonoBehaviour with coroutine-based spawning
  - Implement StartSpawning and StopSpawning methods
  - Create array of spawn point transforms
  - Implement SetDifficulty method to adjust spawn interval and duck speed
  - Add random spawn point selection logic
  - Instantiate ducks with randomized start/end positions
  - _Requirements: 2.1, 2.2, 4.1, 4.2_

- [ ]* 6.2 Write property test for spawn intervals
  - **Property 6: Ducks spawn at intervals**
  - **Validates: Requirements 2.1**

- [ ]* 6.3 Write property test for spawn positions
  - **Property 7: Spawned ducks use valid positions**
  - **Validates: Requirements 2.2**

- [ ]* 6.4 Write property test for difficulty affecting speed
  - **Property 15: Difficulty affects duck speed**
  - **Validates: Requirements 4.2**

- [x] 6.9 Wait for Unity compilation
  - Allow Unity to compile SpawnManager changes
  - Check Unity console for any compilation errors
  - Verify SpawnManager script compiles without errors

- [-] 7. Implement ShootingController
- [x] 7.1 Create ShootingController script with raycast shooting
  - Write ShootingController MonoBehaviour
  - Subscribe to XR controller trigger input action
  - Implement raycast from controller position in forward direction
  - Detect hits on duck layer using LayerMask
  - Call OnHit on struck DuckController
  - Trigger haptic feedback using XR controller SendHapticImpulse
  - _Requirements: 1.1, 1.2, 1.4_

- [ ]* 7.2 Write property test for raycast on trigger
  - **Property 1: Trigger input initiates raycast**
  - **Validates: Requirements 1.1**

- [ ]* 7.3 Write property test for hit detection
  - **Property 2: Ray intersection registers hit**
  - **Validates: Requirements 1.2**

- [ ]* 7.4 Write property test for haptic feedback
  - **Property 4: Trigger provides haptic feedback**
  - **Validates: Requirements 1.4**

- [x] 8. Add audio system
- [x] 8.1 Implement audio feedback for shooting
  - Add AudioSource component to ShootingController
  - Import or create hit and miss sound effects
  - Play hit sound when raycast hits duck
  - Play miss sound when raycast hits nothing
  - _Requirements: 1.5, 6.1, 6.2_

- [ ]* 8.2 Write property test for shot audio
  - **Property 5: Shots play audio**
  - **Validates: Requirements 1.5**

- [ ]* 8.3 Write property test for hit sound
  - **Property 20: Hit shots play hit sound**
  - **Validates: Requirements 6.1**

- [ ]* 8.4 Write property test for miss sound
  - **Property 21: Miss shots play miss sound**
  - **Validates: Requirements 6.2**

- [x] 9. Implement gun selection system
- [x] 9.1 Create gun data structures and collection system
  - Create GunData class for individual gun properties
  - Create GunCollection ScriptableObject for managing multiple guns
  - Define gun properties: name, description, prefab, audio, effects
  - Set up gun validation and lookup methods
  - _Requirements: 8.1, 8.2, 8.5_

- [x] 9.2 Implement GunSelectionManager
  - Write GunSelectionManager MonoBehaviour for gun switching
  - Implement gun prefab instantiation and attachment to VR controller
  - Add automatic muzzle point detection and creation
  - Implement player preference persistence using PlayerPrefs
  - Add event system for gun change notifications
  - _Requirements: 8.2, 8.3, 8.4_

- [x] 9.3 Enhance ShootingController with gun integration
  - Modify ShootingController to work with GunSelectionManager
  - Update raycast origin to use gun muzzle point dynamically
  - Implement gun-specific audio, haptic feedback, and effects
  - Add automatic configuration updates when gun changes
  - _Requirements: 1.1, 6.4, 8.4_

- [x] 9.4 Create gun selection UI system
  - Implement GunSelectionUI for user interface
  - Create gun selection panel with show/hide functionality
  - Add gun information display (name, description, preview)
  - Implement next/previous gun navigation
  - Add visual selection feedback and highlighting
  - _Requirements: 8.5_

- [x] 9.5 Set up gun assets and attribution
  - Configure N-ZAP 85 and Nintendo Zapper Light Gun prefabs
  - Create gun collection asset with both gun models
  - Update CREDITS.md with proper gun asset attribution
  - Integrate gun credits with CreditsData ScriptableObject
  - Ensure CC-BY-4.0 license compliance for both guns
  - _Requirements: 9.5_

- [x] 9.6 Create gun collection setup utilities
  - Write GunCollectionSetup utility for automated asset creation
  - Add gun prefab validation and muzzle point detection
  - Create sample gun collection with proper configuration
  - Add comprehensive documentation and integration guides
  - _Requirements: 8.1, 8.2_

- [ ]* 9.7 Write property tests for gun selection system
  - **Property 24: Gun selection provides multiple options**
  - **Validates: Requirements 8.1**
  - **Property 25: Gun attachment to controller**
  - **Validates: Requirements 8.2**
  - **Property 26: Gun preference persistence**
  - **Validates: Requirements 8.3**
  - **Property 27: Gun-specific shooting properties**
  - **Validates: Requirements 8.4**
  - **Property 28: Gun information display**
  - **Validates: Requirements 8.5**

- [x] 9.9 Wait for Unity compilation
  - Allow Unity to compile gun selection system changes
  - Check Unity console for any compilation errors
  - Verify all gun selection scripts compile without errors

- [-] 10. Add visual effects
- [x] 10.1 Create and integrate particle effects
  - Create muzzle flash particle system for gun barrel
  - Create destruction particle system for duck hits
  - Attach muzzle flash to gun's attachment point in GunController
  - Trigger muzzle flash on trigger pull via ShootingController
  - Instantiate destruction particles in DuckController.OnHit
  - _Requirements: 6.3, 6.4_

- [ ]* 10.2 Write property test for muzzle flash
  - **Property 23: Trigger activates muzzle flash**
  - **Validates: Requirements 6.4**

- [ ]* 10.3 Write property test for destruction particles
  - **Property 22: Destroyed ducks spawn particles**
  - **Validates: Requirements 6.3**

- [x] 11. Implement UI system
- [x] 11.1 Create world-space UI canvas
  - Create Canvas with World Space render mode
  - Position canvas 2-3 meters in front of player
  - Add TextMeshPro text for score display
  - Add TextMeshPro text for missed ducks counter
  - Create game over panel with final score and restart button
  - _Requirements: 3.2, 5.2, 5.3, 7.4_

- [x] 11.2 Wire up UI to game systems
  - Subscribe ScoreManager to update score text on OnScoreChanged
  - Update missed ducks text when IncrementMissed is called
  - Show game over panel when GameManager enters GameOver state
  - Wire restart button to GameManager.RestartGame
  - _Requirements: 3.2, 5.2, 5.3_

- [x] 11.3 Create credits tracking and attribution system
  - Create CreditsData ScriptableObject to store attribution information
  - Define AssetCredit struct with fields: assetName, author, license, source, attributionText
  - Create CREDITS.md file in project root to document all asset attributions
  - Add credits panel to UI canvas with scrollable text area
  - Create CreditsManager script to load and display credits from ScriptableObject
  - Add credits button to main menu or game over screen
  - Implement credits panel show/hide functionality
  - Include gun asset attributions for N-ZAP 85 and Nintendo Zapper Light Gun
  - _Requirements: 7.4, 9.5_

- [ ]* 11.4 Write property test for score display sync
  - **Property 12: Score display synchronization**
  - **Validates: Requirements 3.2**

- [ ]* 11.5 Write property test for game over display
  - **Property 18: Game over displays score**
  - **Validates: Requirements 5.2**

- [ ]* 11.6 Write property test for asset attribution compliance
  - **Property 29: Asset attribution compliance**
  - **Validates: Requirements 9.5**

- [-] 12. Add difficulty feedback system
- [x] 12.1 Implement visual difficulty indicators
  - Create UI element or particle effect for difficulty change
  - Trigger visual feedback in GameManager.IncreaseDifficulty
  - Display difficulty level number to player
  - _Requirements: 4.4_

- [ ]* 12.2 Write property test for difficulty feedback
  - **Property 16: Difficulty changes show feedback**
  - **Validates: Requirements 4.4**

- [x] 13. Configure spawn points and environment
- [x] 13.1 Set up spawn point system
  - Create empty GameObjects as spawn points around player
  - Position spawn points in arc formation at varying heights
  - Assign spawn points to SpawnManager
  - Create corresponding target points on opposite side
  - _Requirements: 2.2_

- [x] 13.2 Polish scene environment
  - Add or configure skybox for outdoor environment
  - Add ground plane with appropriate material
  - Add ambient lighting
  - Position UI canvas for comfortable viewing
  - _Requirements: 7.1, 7.4_

- [x] 14. Implement object pooling for ducks
- [x] 14.1 Create simple object pool system
  - Create ObjectPool utility class for duck reuse
  - Modify SpawnManager to use pool instead of Instantiate
  - Modify DuckController to return to pool instead of Destroy
  - Set pool size based on max concurrent ducks
  - _Requirements: 2.1_

- [x] 15. Create DuckHuntConfig asset and wire up settings
- [x] 15.1 Configure game balance parameters
  - Create DuckHuntConfig ScriptableObject asset
  - Set points per duck (e.g., 10 points)
  - Set max missed ducks (e.g., 10 misses)
  - Configure difficulty levels with spawn intervals and speeds
  - Set raycast distance
  - Wire config asset to GameManager
  - _Requirements: 3.1, 5.1, 4.1, 4.2_

- [x] 16. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.
  - Core game systems verified and working:
    - ✅ Shooting system with audio and haptic feedback
    - ✅ Duck spawning and movement system
    - ✅ Score tracking and UI display
    - ✅ Game state management
    - ✅ Gun selection system with multiple weapons
    - ✅ Particle effects for muzzle flash and duck destruction
    - ✅ VR controller integration
    - ✅ Scene setup with spawn points and environment

- [ ] 17. Build and test on Meta Quest
- [x] 17.1 Configure build settings
  - Set build target to Android
  - Configure XR settings for Oculus
  - Set minimum API level for Quest compatibility
  - Configure graphics settings for mobile VR
  - _Requirements: 8.3_

- [x] 17.2 Test VR functionality
  - Build and deploy to Meta Quest device
  - Verify controller tracking and input
  - Test shooting mechanics with physical controllers
  - Verify haptic feedback intensity
  - Check audio spatialization
  - Validate UI readability and positioning
  - Test complete gameplay loop
  - _Requirements: 1.1, 1.2, 1.4, 6.1, 6.2, 7.4_

- [-] 18. Final polish and optimization
- [x] 18.1 Performance optimization
  - Profile frame rate and identify bottlenecks
  - Optimize duck count and spawn rates for stable 72 FPS
  - Add LOD to duck models if needed
  - Optimize particle effects
  - _Requirements: 7.3_

- [ ] 18.2 Gameplay balancing
  - Playtest and adjust difficulty curve
  - Balance spawn rates and duck speeds
  - Tune score thresholds for difficulty progression
  - Adjust audio volumes
  - Fine-tune haptic feedback intensity
  - _Requirements: 4.1, 4.2_

- [ ] 19. Fix muzzle flash particle effects for Quest VR
- [ ] 19.1 Investigate and fix pink particle shader issue
  - Research Quest-compatible particle shaders for URP
  - Create or find Quest-compatible muzzle flash material
  - Test with Vulkan and OpenGLES3 renderers
  - Verify depth texture and soft particles work correctly
  - Update GunCollection to use fixed muzzle flash prefab
  - Re-enable muzzle flash in ShootingController
  - _Requirements: 6.4_

- [ ] 20. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Bug Fixes - Scene Configuration Issues (Found during task verification)

- [x] 21. Fix SpawnManager missing DuckPool reference
- [x] 21.1 Create DuckPool GameObject and configure
  - Create new GameObject named "DuckPool" in scene
  - Add DuckPool component to the GameObject
  - Assign Duck prefab (Assets/Prefabs/Duck.prefab) to duckPrefab field
  - Set initialPoolSize to 5 and maxPoolSize to 20
  - Assign DuckPool reference to SpawnManager.duckPool field
  - Save scene after configuration
  - _Requirements: 2.1_

- [x] 22. Fix SpawnManager missing spawn and target points
- [x] 22.1 Create spawn points in arc formation
  - Create parent GameObject "SpawnPoints" to organize spawn points
  - Create 5 spawn point GameObjects positioned in arc around player:
    - SpawnPoint_Left: (-8, 2, 5)
    - SpawnPoint_LeftCenter: (-4, 3, 7)
    - SpawnPoint_Center: (0, 4, 8)
    - SpawnPoint_RightCenter: (4, 3, 7)
    - SpawnPoint_Right: (8, 2, 5)
  - Assign all spawn points to SpawnManager.SpawnPoints array
  - _Requirements: 2.2_

- [x] 22.2 Create target points on opposite side
  - Create parent GameObject "TargetPoints" to organize target points
  - Create 5 target point GameObjects on opposite side:
    - TargetPoint_Left: (8, 2, -5)
    - TargetPoint_LeftCenter: (4, 3, -7)
    - TargetPoint_Center: (0, 4, -8)
    - TargetPoint_RightCenter: (-4, 3, -7)
    - TargetPoint_Right: (-8, 2, -5)
  - ✅ Updated SpawnManager to auto-discover points (no manual array wiring needed)
  - Save scene after configuration
  - _Requirements: 2.2_

- [x] 23. Fix ScoreManager missing gameConfig reference
- [x] 23.1 Wire ScoreManager to DuckHuntConfig
  - Find ScoreManager GameObject in scene
  - Assign DuckHuntConfig asset (Assets/Data/DuckHuntConfig.asset) to gameConfig field
  - Verify maxMissedDucks updates to 10 (from config)
  - Save scene after configuration
  - _Requirements: 3.4, 5.1_

- [x] 24. Fix UIManager missing UI element references
- [x] 24.1 Create or verify UI elements in Canvas
  - Verify Canvas exists with World Space render mode
  - Create/find ScoreText (TextMeshProUGUI) for score display
  - Create/find MissedDucksText (TextMeshProUGUI) for missed counter
  - Create/find DifficultyText (TextMeshProUGUI) for level display
  - _Requirements: 3.2_

- [x] 24.2 Create GameOver panel UI elements
  - Create GameOverPanel GameObject (initially inactive)
  - Add FinalScoreText (TextMeshProUGUI) inside panel
  - Add RestartButton (Button) with "Restart" text
  - Style panel for VR readability (large text, high contrast)
  - _Requirements: 5.2, 5.3_

- [x] 24.3 Create difficulty feedback panel
  - Create DifficultyFeedbackPanel GameObject (initially inactive)
  - Add DifficultyFeedbackText (TextMeshProUGUI) for "LEVEL UP!" message
  - Position panel prominently in player view
  - _Requirements: 4.4_

- [x] 24.4 Wire UIManager to all UI elements
  - Assign worldSpaceCanvas reference to Canvas
  - Assign scoreText, missedDucksText, difficultyText references
  - Assign gameOverPanel, finalScoreText, restartButton references
  - Assign difficultyFeedbackPanel, difficultyFeedbackText references
  - Save scene after all wiring complete
  - _Requirements: 3.2, 5.2, 5.3, 4.4_

- [x] 26. Verify all scene wiring and test gameplay
- [x] 26.1 Verify component references
  - Check GameManager has ScoreManager and SpawnManager references
  - Check SpawnManager has DuckPool and spawn/target points
  - Check ScoreManager has DuckHuntConfig
  - Check UIManager has all UI element references
  - Check ShootingController has GunSelectionManager reference
  - _Requirements: All_

- [x] 26.2 Test gameplay loop in editor
  - Enter Play mode in Unity Editor
  - Verify game starts and ducks spawn
  - Verify shooting works and score updates
  - Verify missed ducks increment counter
  - Verify game over triggers at threshold
  - Verify restart button works
  - _Requirements: All_


## In Progress - World Space Scoreboard UI

- [x] 28. Replace HUD with world-space scoreboard signs
- [x] 28.1 Create WorldSpaceScoreboard setup script
  - Create temporary setup script to build scoreboard UI
  - Position wooden sign posts to the left of player (~3m away, angled toward player)
  - Add Score, Missed, and Level text displays on signs
  - Wire up to existing ScoreManager and GameManager events
  - After setup is verified working, convert to prefab and delete setup script
  - _Requirements: 3.2, 4.4, 5.2_

- [x] 28.2 Update UIManager to use world-space scoreboard
  - Modify UIManager to find and update world-space text elements
  - Remove camera-attached canvas references
  - Keep game over panel functionality (can be world-space too)
  - _Requirements: 3.2, 5.2, 5.3_

## Future Improvements (Low Priority)

- [x] 27. Fix VR headset initial orientation
- [x] 27.1 Investigate and fix player facing backwards at game start
  - Created VRRecenter script (Assets/Scripts/VR/VRRecenter.cs)
  - Solution implemented:
    - Auto-recenter on game start (1.5 second delay to allow headset tracking to stabilize)
    - Manual recenter via left menu button or Y button on Quest controller
    - Rotates XR Origin to align player's forward direction with game's forward (positive Z)
  - Features:
    - Automatic XR Origin detection at runtime
    - Configurable target forward direction
    - Debug logging for troubleshooting
    - Works with any VR controller that has menu/secondary button
  - Added VRRecenter GameObject to MainScene
  - _Requirements: 7.1_


## Game Polish - Gameplay Area Design

- [ ] 29. Restrict duck spawn area to front of player
- [ ] 29.1 Reconfigure spawn points to front hemisphere only
  - Remove spawn points behind the player (negative Z or behind XR Origin)
  - Ensure all spawn points are in a 180° arc in front of player
  - Adjust target points to fly across the front area (left-to-right or right-to-left)
  - Ducks should never spawn or fly behind the player
  - _Requirements: 2.2_

- [ ] 30. Create player boundary/play area
- [ ] 30.1 Add invisible boundary to prevent player walking into duck area
  - Create boundary colliders or teleport blockers around play area
  - Player should stay in a designated "shooting gallery" zone
  - Consider using XR Interaction Toolkit's teleport area restrictions
  - Add visual floor markers to indicate play area bounds
  - _Requirements: 7.1_

- [ ] 31. Create game start pedestal/table
- [ ] 31.1 Design and create start game pedestal
  - Create a table or pedestal GameObject in front of player
  - Add a large "START GAME" button on the pedestal
  - Button should be interactable with VR controller (poke or ray)
  - Wire button to GameManager.StartGame()
  - Remove auto-start behavior (VRGameAutoStart)
  - Game should wait in Idle state until player presses start
  - _Requirements: 5.4_

- [ ] 31.2 Add game state UI to pedestal
  - Show "Press to Start" text when game is idle
  - Show "Game Over - Press to Restart" when game ends
  - Display high score on pedestal
  - _Requirements: 5.2, 5.3_

- [ ] 32. Create gun display rack
- [ ] 32.1 Design and create gun display table/rack
  - Create a display table or wall rack to showcase all available guns
  - Position guns on the rack with proper spacing
  - Each gun slot should show the gun model
  - Add name labels under each gun
  - _Requirements: 8.5_

- [ ] 32.2 Implement gun selection from display rack
  - Player can point at a gun and press trigger to select it
  - Selected gun highlights or has visual feedback
  - Gun appears in player's hand when selected
  - Current selection is visually indicated on the rack
  - _Requirements: 8.1, 8.2_

## Known Bugs and Issues

- [ ] 33. Fix score not updating when shooting ducks
- [ ] 33.1 Verify SpawnManager has gameConfig wired up
  - SpawnManager now uses serialized field instead of Resources.Load
  - Ensure gameConfig is assigned to Assets/Data/DuckHuntConfig.asset in scene
  - Verify OnDuckDestroyed calls scoreManager.AddScore(gameConfig.PointsPerDuck)
  - Test that score increments when duck is hit
  - _Requirements: 3.1_

- [ ] 34. Clean up unused utility scripts
- [x] 34.1 Remove FixVRIssues.cs
  - Script was not attached to any scene object
  - Deleted: Assets/Scripts/FixVRIssues.cs
  
- [ ] 34.2 Simplify VRGameAutoStart.cs
  - Remove FixGroundMaterial() workaround - ground should be set up correctly in scene
  - Remove FixUIForVR() workaround - UI should be positioned correctly in scene
  - Keep only the game auto-start functionality (or remove entirely if using start button)
  - _Requirements: 7.1_

- [ ] 35. Fix muzzle flash pink shader on Quest
  - Particle effects showing pink due to shader incompatibility
  - Need Quest-compatible URP particle shader
  - Currently disabled in ShootingController
  - _Requirements: 6.4_



- [ ] 36. Fix duck destruction visual feedback
- [ ] 36.1 Fix particle effect spawning at wrong position
  - Particle effect spawns at player's face instead of duck's position
  - Effect should spawn at duck's world position when hit
  - Check DuckController.PlayDestructionEffects() for position bug
  - _Requirements: 6.3_

- [ ] 36.2 Fix duck freezing instead of playing death animation
  - Ducks freeze in place when hit instead of falling/exploding
  - Should have satisfying destruction animation or immediate disappear with particles
  - Consider adding a brief fall animation before returning to pool
  - _Requirements: 6.3_

- [ ] 36.3 Create proper duck destruction particle effect
  - Current effect is just pink sparks (shader issue)
  - Need a proper feather explosion or poof effect
  - Must use Quest-compatible URP shaders
  - Should be visually satisfying and readable in VR
  - _Requirements: 6.3_



## Scene Cleanup and Organization

- [ ] 37. Clean up and organize scene hierarchy
- [ ] 37.1 Audit scene for unused GameObjects
  - Review all root-level GameObjects in scene
  - Remove any test objects, duplicates, or unused items
  - Remove the standalone "Duck" object if it's not needed (ducks come from pool)
  - Remove "DuckExplosionEffect" if not being used properly
  - _Requirements: 7.1_

- [ ] 37.2 Organize scene hierarchy with parent containers
  - Group related objects under empty parent GameObjects:
    - "--- Environment ---" (Ground, Skybox, Lighting)
    - "--- Managers ---" (GameManager, ScoreManager, SpawnManager, UIManager, etc.)
    - "--- VR Rig ---" (XR Origin and related)
    - "--- Spawn System ---" (SpawnPoints, TargetPoints, DuckPool)
    - "--- UI ---" (WorldScoreboard, any canvases)
  - Use "---" prefix for organizational parents (Unity convention)
  - _Requirements: 7.1_

- [ ] 37.3 Verify all component references are properly wired
  - Check each manager has its required references assigned
  - Remove any broken or missing script references
  - Ensure no "Missing Script" components exist
  - _Requirements: All_



- [ ] 37.4 Resolve WorldScoreboard prefab overrides
  - Scene instance of WorldScoreboard has overrides from prefab
  - Investigate what properties were changed on the instance
  - Decide whether to:
    - Apply overrides to prefab (if changes are intentional improvements)
    - Revert instance to prefab (if changes were accidental)
  - Ensure prefab and instance are in sync
  - _Requirements: 3.2_

