# Implementation Plan

- [x] 1. Create URP-compatible indicator materials
- [x] 1.1 Create SpawnIndicatorMaterial
  - Create new material at Assets/Materials/SpawnIndicatorMaterial.mat
  - Set shader to Universal Render Pipeline/Lit
  - Set base color to green (0, 0.8, 0, 1)
  - Enable emission with green color for visibility
  - Verify material renders correctly (not pink) in Scene view
  - _Requirements: 3.1, 3.3, 3.5_

- [x] 1.2 Create TargetIndicatorMaterial
  - Create new material at Assets/Materials/TargetIndicatorMaterial.mat
  - Set shader to Universal Render Pipeline/Lit
  - Set base color to red (0.8, 0, 0, 1)
  - Enable emission with red color for visibility
  - Verify material renders correctly (not pink) in Scene view
  - _Requirements: 3.1, 3.4, 3.5_

- [-] 2. Create SpawnPointMarker component
- [x] 2.1 Implement SpawnPointMarker script
  - Create Assets/Scripts/Data/SpawnPointMarker.cs
  - Add SpawnPointType enum (Spawn, Target)
  - Add PointIndex property with serialized field
  - Add PointType property (read-only, set in prefab)
  - Add Indicator transform reference
  - Add OccluderSlot transform reference
  - Implement SetIndicatorVisible(bool visible) method
  - Implement SetOccluder(GameObject prefab) method
  - Implement ClearOccluder() method
  - _Requirements: 2.1, 2.2, 4.1, 4.2, 5.1, 5.2_

- [ ]* 2.2 Write property test for indicator visibility
  - **Property 7: Indicator toggle affects all points**
  - **Validates: Requirements 4.4**

- [ ]* 2.3 Write property test for occluder positioning
  - **Property 8: Occluder position matches spawn point**
  - **Validates: Requirements 5.2**

- [-] 3. Create spawn point and target point prefabs
- [x] 3.1 Create SpawnPoint prefab
  - Create empty GameObject named "SpawnPoint"
  - Add SpawnPointMarker component with PointType = Spawn
  - Create child "Indicator" with Sphere mesh (scale 0.3)
  - Apply SpawnIndicatorMaterial to Indicator
  - Create child "OccluderSlot" as empty transform
  - Wire Indicator and OccluderSlot references in SpawnPointMarker
  - Save as prefab at Assets/Prefabs/SpawnPoint.prefab
  - _Requirements: 2.1, 2.4, 3.3_

- [x] 3.2 Create TargetPoint prefab
  - Create empty GameObject named "TargetPoint"
  - Add SpawnPointMarker component with PointType = Target
  - Create child "Indicator" with Sphere mesh (scale 0.3)
  - Apply TargetIndicatorMaterial to Indicator
  - Create child "OccluderSlot" as empty transform
  - Wire Indicator and OccluderSlot references in SpawnPointMarker
  - Save as prefab at Assets/Prefabs/TargetPoint.prefab
  - _Requirements: 2.2, 2.5, 3.4_

- [ ]* 3.3 Write property test for prefab indicator child
  - **Property 3: Point prefabs include Indicator child**
  - **Validates: Requirements 2.4, 2.5**

- [ ]* 3.4 Write property test for indicator materials
  - **Property 4: Indicator materials use URP Lit shader**
  - **Validates: Requirements 3.1, 3.5**

- [ ]* 3.5 Write property test for spawn indicator color
  - **Property 5: Spawn indicators use green color**
  - **Validates: Requirements 3.3**

- [ ]* 3.6 Write property test for target indicator color
  - **Property 6: Target indicators use red color**
  - **Validates: Requirements 3.4**

- [x] 4. Checkpoint - Verify prefabs and materials
  - Ensure all tests pass, ask the user if questions arise.
  - Verify materials render correctly in Scene view (not pink)
  - Verify prefabs have correct structure and references

- [-] 5. Create SpawnPointManager component
- [x] 5.1 Implement SpawnPointManager script
  - Create Assets/Scripts/Managers/SpawnPointManager.cs
  - Add SpawnPoints and TargetPoints arrays (auto-populated)
  - Add ShowIndicators property with backing field
  - Implement RefreshPointLists() to find all points in hierarchy
  - Implement GetSpawnPoint(int index) method
  - Implement GetTargetPoint(int index) method
  - Implement GetPointPair(int index) method
  - Implement SetAllIndicatorsVisible(bool visible) method
  - Call RefreshPointLists() in Awake()
  - Set indicators hidden by default in Start() if in play mode
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [ ]* 5.2 Write property test for point organization
  - **Property 2: Points are organized under correct parent containers**
  - **Validates: Requirements 1.4, 1.5**

- [-] 6. Create SpawnPointMigrationTool editor script
- [x] 6.1 Implement migration tool
  - Create Assets/Scripts/Editor/SpawnPointMigrationTool.cs
  - Add MenuItem "Tools/Spawn Points/Migrate to Prefab System"
  - Implement FindExistingSpawnPoints() to locate all spawn/target points
  - Implement MigratePoints() with prefab replacement logic
  - Preserve world position and rotation during migration
  - Rename points to standardized format (SpawnPoint_XX, TargetPoint_XX)
  - Parent points to correct containers (SpawnPoints, TargetPoints)
  - Register all changes with Undo system
  - Display results dialog with migration statistics
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [ ]* 6.2 Write property test for naming convention
  - **Property 1: Spawn and target points follow naming convention with matching indices**
  - **Validates: Requirements 1.1, 1.2, 1.3**

- [ ]* 6.3 Write property test for migration position preservation
  - **Property 9: Migration preserves positions and applies naming convention**
  - **Validates: Requirements 6.2, 6.3, 6.4**

- [ ]* 6.4 Write property test for migration count
  - **Property 10: Migration reports accurate count**
  - **Validates: Requirements 6.5**

- [x] 7. Checkpoint - Verify migration tool
  - Ensure all tests pass, ask the user if questions arise.
  - Test migration tool on current scene
  - Verify all points are renamed correctly
  - Verify positions are preserved

- [x] 8. Run migration on MainScene
- [x] 8.1 Execute migration tool
  - Open MainScene in Unity Editor
  - Run Tools > Spawn Points > Migrate to Prefab System
  - Verify migration completes successfully
  - Check all spawn points renamed to SpawnPoint_00 through SpawnPoint_XX
  - Check all target points renamed to TargetPoint_00 through TargetPoint_XX
  - Verify indicators are green (spawn) and red (target)
  - Save scene after migration
  - _Requirements: 1.1, 1.2, 1.3, 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 8.2 Verify SpawnManager compatibility
  - Confirm existing SpawnManager still discovers points correctly
  - Test duck spawning works with migrated points
  - Verify ducks fly from spawn points to target points
  - _Requirements: 2.1, 2.2_

- [x] 9. Add debug visibility toggle
- [x] 9.1 Add debug controls to SpawnPointManager
  - Added [SerializeField] bool debugShowIndicators field
  - Added public DebugShowIndicators property to toggle debug mode
  - Property can be called from settings menu in VR (no keyboard in VR)
  - Added SpawnPointManager GameObject to MainScene (was missing - caused indicators to show at runtime)
  - _Requirements: 4.3_

- [x] 10. Create SpawnPointConfig ScriptableObject
- [x] 10.1 Implement configuration asset
  - Create Assets/Scripts/Data/SpawnPointConfig.cs
  - Add indicator material references
  - Add indicator scale setting
  - Add visibility settings (editor, play mode, debug)
  - Add occluder settings (default prefab, offset, scale)
  - Create instance at Assets/Data/SpawnPointConfig.asset
  - Wire config to SpawnPointManager
  - _Requirements: 4.1, 4.2, 4.3, 5.1_

- [ ] 11. Final checkpoint - Full system verification
  - Ensure all tests pass, ask the user if questions arise.
  - Verify indicators show in editor, hide in play mode
  - Verify debug toggle shows indicators in play mode
  - Verify materials render correctly on Quest (no pink)
  - Test full gameplay loop with migrated spawn points

