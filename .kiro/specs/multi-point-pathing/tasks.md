# Implementation Plan

- [x] 1. Create centralized DebugSettings system
- [x] 1.1 Implement DebugSettings singleton
  - Create Assets/Scripts/Data/DebugSettings.cs
  - Implement singleton pattern with Instance property
  - Add ShowSplinePaths, ShowWaypointIndicators, ShowSpawnPointIndicators, ShowTargetPointIndicators properties
  - Add OnSettingsChanged event that fires when any property changes
  - Add SetAllVisualization(bool enabled) convenience method
  - _Requirements: 8.1, 8.2, 8.4_

- [x] 1.2 Migrate SpawnPointManager to use DebugSettings
  - Update SpawnPointManager to subscribe to DebugSettings.OnSettingsChanged
  - Replace debugShowIndicators field with DebugSettings.Instance.ShowSpawnPointIndicators
  - Update SetAllIndicatorsVisible to use centralized settings
  - _Requirements: 8.6, 8.7, 8.8_

- [ ]* 1.3 Write property test for DebugSettings events
  - **Property 14: DebugSettings events notify all subscribers**
  - **Validates: Requirements 8.8**

- [x] 2. Checkpoint - Verify DebugSettings integration
  - Ensure all tests pass, ask the user if questions arise.
  - Verify SpawnPointManager uses DebugSettings
  - Test toggling debug settings at runtime

- [x] 3. Create SplineUtility static class
- [x] 3.1 Implement Catmull-Rom spline calculations
  - Create Assets/Scripts/Utilities/SplineUtility.cs
  - Implement CatmullRomPoint(p0, p1, p2, p3, t, tension) method
  - Implement CatmullRomTangent(p0, p1, p2, p3, t, tension) method
  - Implement AddPhantomPoints(waypoints) for paths with < 4 points
  - _Requirements: 5.1, 5.2, 5.3_

- [x] 3.2 Implement arc-length parameterization
  - Implement CalculateSegmentArcLength(p0, p1, p2, p3, samples) method
  - Implement BuildArcLengthTable(waypoints, samplesPerSegment) method
  - Implement GetPointAtArcLength(waypoints, arcLengthTable, targetLength) method
  - Implement GetTangentAtArcLength(waypoints, arcLengthTable, targetLength) method
  - _Requirements: 5.4, 6.1_

- [ ]* 3.3 Write property test for Catmull-Rom interpolation
  - **Property 1: Catmull-Rom spline interpolation correctness**
  - **Validates: Requirements 5.1**

- [ ]* 3.4 Write property test for constant speed
  - **Property 9: Constant speed along curved path**
  - **Validates: Requirements 5.4, 6.1, 6.4**

- [ ]* 3.5 Write property test for phantom points
  - **Property 10: Phantom points enable short path interpolation**
  - **Validates: Requirements 5.3**

- [x] 4. Checkpoint - Verify SplineUtility
  - Ensure all tests pass, ask the user if questions arise.
  - Test spline calculations with various control points
  - Verify arc-length parameterization produces constant speed

- [x] 5. Create FlightPath data class
- [x] 5.1 Implement FlightPath class
  - Create Assets/Scripts/Data/FlightPath.cs
  - Add Waypoints, TotalArcLength, ArcLengthTable, Seed, IntermediateWaypointCount properties
  - Implement constructor that builds arc-length table from waypoints
  - Implement GetPositionAtDistance(distance) method
  - Implement GetTangentAtDistance(distance) method
  - Implement GetPositionAtTime(normalizedTime) method
  - _Requirements: 1.2, 6.3_

- [ ]* 5.2 Write property test for velocity continuity
  - **Property 2: Velocity continuity along path**
  - **Validates: Requirements 1.3**

- [x] 6. Create FlightPathConfig ScriptableObject
- [x] 6.1 Implement FlightPathConfig
  - Create Assets/Scripts/Data/FlightPathConfig.cs
  - Add spline settings (tension, arc length samples)
  - Add flight duration settings (MinFlightDuration)
  - Add flight zone settings (bounds, height constraints)
  - Add waypoint generation settings (deviation ranges)
  - Add difficulty waypoint mapping array
  - Add visualization settings (colors, widths)
  - Create instance at Assets/Data/FlightPathConfig.asset
  - _Requirements: 2.1, 2.2, 2.3, 6.5_

- [x] 7. Create IntermediatePointMarker component and prefab
- [x] 7.1 Implement IntermediatePointMarker script
  - Create Assets/Scripts/Data/IntermediatePointMarker.cs
  - Add PointIndex property
  - Add Indicator and OccluderSlot transform references
  - Implement SetIndicatorVisible(bool visible) method
  - Implement SetOccluder/ClearOccluder methods (optional occlusion support)
  - Subscribe to DebugSettings.OnSettingsChanged
  - _Requirements: 3.2, 4.1, 4.9, 4.10_

- [x] 7.2 Create IntermediateIndicatorMaterial
  - Create Assets/Materials/IntermediateIndicatorMaterial.mat
  - Set shader to Universal Render Pipeline/Lit
  - Set base color to orange (1.0, 0.6, 0.0, 1)
  - Enable emission for visibility
  - _Requirements: 4.1_

- [x] 7.3 Create IntermediatePoint prefab
  - Create IntermediatePoint GameObject with IntermediatePointMarker component
  - Add Indicator child (Sphere mesh, orange material, scale 0.25)
  - Add OccluderSlot child (empty transform)
  - Wire references in IntermediatePointMarker
  - Save as Assets/Prefabs/IntermediatePoint.prefab
  - _Requirements: 3.2, 4.1, 4.9_

- [x] 8. Checkpoint - Verify IntermediatePoint prefab
  - Ensure all tests pass, ask the user if questions arise.
  - Verify prefab structure and material
  - Test indicator visibility toggle via DebugSettings

- [x] 9. Create FlightPathGenerator
- [x] 9.1 Implement FlightPathGenerator script
  - Create Assets/Scripts/Managers/FlightPathGenerator.cs
  - Add references to intermediatePointsParent, FlightPathConfig
  - Implement GetPreplacedIntermediatePoints() to discover scene waypoints
  - Implement GenerateDynamicWaypoint(spawn, target, progress, rng) method
  - Implement GetWaypointCountForDifficulty(level) method
  - Implement GeneratePath(spawn, target, difficulty, seed) method
  - Add minimum flight duration validation and path extension logic
  - Log seed and waypoint source for debugging
  - _Requirements: 1.1, 2.1, 2.2, 2.3, 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 6.5, 6.6, 7.1, 7.2, 7.3_

- [ ]* 9.2 Write property test for waypoint count per difficulty
  - **Property 5: Waypoint count within difficulty range**
  - **Validates: Requirements 2.1, 2.2, 2.3**

- [ ]* 9.3 Write property test for path variety
  - **Property 6: Higher difficulty increases path variety**
  - **Validates: Requirements 2.5**

- [ ]* 9.4 Write property test for dynamic waypoint bounds
  - **Property 7: Dynamically generated waypoints within flight zone bounds**
  - **Validates: Requirements 3.3, 3.4**

- [ ]* 9.5 Write property test for pre-placed waypoint discovery
  - **Property 8: Pre-placed waypoints are discovered and used**
  - **Validates: Requirements 3.1, 3.2, 3.5**

- [ ]* 9.6 Write property test for seed determinism
  - **Property 11: Same seed produces identical path**
  - **Validates: Requirements 7.2**

- [ ]* 9.7 Write property test for minimum flight duration
  - **Property 15: Minimum flight duration enforced**
  - **Validates: Requirements 6.5, 6.6**

- [x] 10. Checkpoint - Verify FlightPathGenerator
  - Ensure all tests pass, ask the user if questions arise.
  - Test path generation at all difficulty levels
  - Verify minimum flight duration is enforced
  - Test with and without pre-placed IntermediatePoints

- [x] 11. Update DuckController for spline movement
- [x] 11.1 Add spline movement support to DuckController
  - Add CurrentPath property (FlightPath)
  - Add distanceTraveled tracking field
  - Add new Initialize(FlightPath path, float speed) overload
  - Implement UpdateSplineMovement() method using arc-length parameterization
  - Update duck orientation to face movement direction (tangent)
  - Integrate with existing spawn/escape animations
  - _Requirements: 1.2, 1.3, 1.4, 1.5, 6.1, 6.2_

- [x] 11.2 Add path visualization to DuckController
  - Add LineRenderer component reference
  - Implement UpdatePathVisualization() method
  - Subscribe to DebugSettings.OnSettingsChanged
  - Show/hide LineRenderer based on ShowSplinePaths setting
  - Use cyan color for path visualization
  - _Requirements: 4.3, 4.4, 4.5, 4.6, 4.7, 4.8_

- [ ]* 11.3 Write property test for duck orientation
  - **Property 3: Duck orientation matches movement direction**
  - **Validates: Requirements 1.4**

- [ ]* 11.4 Write property test for escape trigger
  - **Property 4: Escape triggers at path end**
  - **Validates: Requirements 1.5**

- [ ]* 11.5 Write property test for debug toggle
  - **Property 12: Debug toggle immediately updates all visualizations**
  - **Validates: Requirements 4.3, 8.3, 4.8**

- [ ]* 11.6 Write property test for independent path visualization
  - **Property 13: Each duck has independent path visualization**
  - **Validates: Requirements 4.7**

- [x] 12. Checkpoint - Verify DuckController spline movement
  - Ensure all tests pass, ask the user if questions arise.
  - Test duck movement along curved paths
  - Verify smooth orientation changes
  - Test path visualization toggle

- [x] 13. Update SpawnManager to use FlightPathGenerator
- [x] 13.1 Integrate FlightPathGenerator with SpawnManager
  - Add FlightPathGenerator reference to SpawnManager
  - Update SpawnDuck() to generate FlightPath using current difficulty
  - Pass FlightPath to DuckController.Initialize()
  - Ensure backward compatibility with existing spawn/target point system
  - _Requirements: 1.1, 2.4, 2.5_

- [x] 14. Add IntermediatePoints to MainScene (optional)
- [x] 14.1 Place IntermediatePoint prefabs in scene
  - Create IntermediatePoints parent GameObject
  - Place 3-5 IntermediatePoint prefabs at strategic locations in flight zone
  - Position them at varied heights and lateral positions
  - Verify they appear in FlightPathGenerator's discovered points
  - _Requirements: 3.1, 3.2_

- [ ] 15. Final checkpoint - Full system verification
  - Ensure all tests pass, ask the user if questions arise.
  - Test complete gameplay loop with multi-point pathing
  - Verify difficulty scaling affects path variety
  - Test debug visualization toggle at runtime
  - Verify minimum flight duration prevents unhittable ducks
  - Test with both pre-placed and dynamic waypoints
  - Verify smooth curved flight paths in VR

