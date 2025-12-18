# Requirements Document

## Introduction

This document specifies the requirements for implementing a multi-point pathing system for ducks in the VR Duck Hunt game. Currently, ducks fly in a straight line from a spawn point to a target point. This feature will enable ducks to follow curved, multi-waypoint paths that create more natural and challenging flight patterns. The system will use intermediate waypoints (which can include existing spawn and target points) to create varied flight paths, with smooth Catmull-Rom or Bezier spline interpolation for fluid motion. Path complexity will scale with the game's difficulty level.

## Glossary

- **Flight Path**: The complete trajectory a duck follows from spawn to final destination
- **Waypoint**: A 3D position that defines a point along the duck's flight path
- **Intermediate Point**: A waypoint between the spawn point and target point that the duck passes through
- **Spawn Point**: The starting position where a duck begins its flight (existing system)
- **Target Point**: The final destination where a duck ends its flight (existing system)
- **Spline Interpolation**: A mathematical method for creating smooth curves through a set of control points
- **Catmull-Rom Spline**: A type of interpolating spline that passes through all control points with smooth transitions
- **Path Segment**: The portion of a flight path between two consecutive waypoints
- **Intermediate Indicator**: A visual marker showing the location of an intermediate waypoint in the editor
- **Difficulty Level**: The current game difficulty (1-5) that affects duck speed, spawn rate, and path complexity
- **Path Complexity**: The number of intermediate waypoints in a flight path, which increases with difficulty

## Requirements

### Requirement 1

**User Story:** As a player, I want ducks to fly along curved, multi-point paths, so that the gameplay is more challenging and visually interesting.

#### Acceptance Criteria

1. WHEN a duck spawns THEN the Flight Path System SHALL generate a path with zero or more intermediate waypoints between spawn and target
2. WHEN a duck follows a multi-point path THEN the Duck Controller SHALL move the duck smoothly through all waypoints using spline interpolation
3. WHEN a duck transitions between path segments THEN the Duck Controller SHALL maintain continuous velocity direction without abrupt changes
4. WHEN a duck is moving along a spline path THEN the Duck Controller SHALL orient the duck to face its current movement direction
5. WHEN a duck reaches the final waypoint THEN the Duck Controller SHALL trigger the escape sequence as in the current system

### Requirement 2

**User Story:** As a game designer, I want path variety to increase with difficulty level, so that higher difficulties have more unpredictable flight patterns combined with faster duck speed.

#### Acceptance Criteria

1. WHEN the difficulty level is 1-2 THEN the Flight Path System SHALL generate paths with one to two intermediate waypoints (consistent gentle curves)
2. WHEN the difficulty level is 3 THEN the Flight Path System SHALL generate paths with one to three intermediate waypoints (moderate variety)
3. WHEN the difficulty level is 4-5 THEN the Flight Path System SHALL generate paths with zero to three intermediate waypoints (maximum variety)
4. WHEN generating intermediate waypoints THEN the Flight Path System SHALL select positions that create reasonable flight arcs
5. WHEN the difficulty increases THEN the Flight Path System SHALL increase path variety while duck speed provides the primary difficulty scaling

### Requirement 3

**User Story:** As a game designer, I want intermediate waypoints to be either pre-placed in the scene or dynamically generated, so that I have control over specific paths while also enabling variety through procedural generation.

#### Acceptance Criteria

1. WHEN intermediate waypoints are needed THEN the Flight Path System SHALL support both pre-placed IntermediatePoint prefabs and dynamic generation
2. WHEN pre-placed intermediate waypoints exist THEN the Flight Path System SHALL consider them as candidates for path generation
3. WHEN dynamically generating waypoints THEN the Flight Path System SHALL create positions within the defined flight zone boundaries
4. WHEN dynamically generating waypoints THEN the Flight Path System SHALL position them at reasonable heights for flight paths
5. WHEN selecting waypoints THEN the Flight Path System SHALL prefer pre-placed waypoints when available, falling back to dynamic generation
6. WHEN generating paths THEN the Flight Path System SHALL ensure positions create smooth, non-self-intersecting trajectories

### Requirement 4

**User Story:** As a developer, I want intermediate waypoints and spline paths to have visual indicators that can be toggled at runtime, so that I can debug flight paths during gameplay and integrate with a future settings menu.

#### Acceptance Criteria

1. WHEN an intermediate waypoint exists (pre-placed or generated) THEN the Flight Path System SHALL display a distinct indicator color (yellow or orange) separate from spawn (green) and target (red) points
2. WHEN the game is in play mode THEN the Flight Path System SHALL hide intermediate waypoint indicators by default
3. WHEN debug visualization is toggled at runtime THEN the Flight Path System SHALL immediately show or hide all path visualizations
4. WHEN a duck is spawned with debug visualization enabled THEN the Flight Path System SHALL draw the complete spline path as a visible line
5. WHEN drawing the spline path THEN the Flight Path System SHALL use a distinct color (cyan or magenta) to differentiate from other scene elements
6. WHEN the spline path is visualized THEN the Flight Path System SHALL show the path as a smooth curve with sufficient sample points for visual clarity
7. WHEN multiple ducks are active with debug visualization THEN the Flight Path System SHALL draw each duck's spline path independently
8. WHEN debug settings are changed THEN the Flight Path System SHALL apply changes to both existing and newly spawned ducks
9. WHEN a pre-placed IntermediatePoint prefab is created THEN the Flight Path System SHALL support an optional occlusion slot (like spawn/target points)
10. WHEN an IntermediatePoint has no occlusion assigned THEN the Flight Path System SHALL function normally with the duck curving through open sky

### Requirement 8

**User Story:** As a developer, I want all debug visualization options (including existing ones) to be controllable via a centralized debug settings system, so that a future settings/debug menu can toggle them at runtime.

#### Acceptance Criteria

1. WHEN the Flight Path System is initialized THEN the system SHALL expose a public property for toggling spline path visualization
2. WHEN the Flight Path System is initialized THEN the system SHALL expose a public property for toggling waypoint indicator visibility
3. WHEN a debug property is changed at runtime THEN the Flight Path System SHALL update the visualization state immediately without requiring scene reload
4. WHEN debug properties are exposed THEN the Flight Path System SHALL use a centralized DebugSettings class that can be accessed by UI systems
5. WHEN the game starts THEN the Flight Path System SHALL read initial debug settings from a runtime-accessible configuration
6. WHEN the centralized DebugSettings class is created THEN the system SHALL migrate existing debug settings (such as SpawnPointManager.debugShowIndicators) to use this centralized system
7. WHEN any component needs debug settings THEN the component SHALL reference the centralized DebugSettings class rather than maintaining its own debug flags
8. WHEN the DebugSettings class is modified THEN the system SHALL notify all subscribed components via events to update their visualization state

### Requirement 5

**User Story:** As a developer, I want the spline interpolation to be configurable, so that I can tune the smoothness of duck flight paths.

#### Acceptance Criteria

1. WHEN calculating spline positions THEN the Flight Path System SHALL use Catmull-Rom spline interpolation by default
2. WHEN the spline tension parameter is adjusted THEN the Flight Path System SHALL modify the curvature of the path accordingly
3. WHEN a path has fewer than four control points THEN the Flight Path System SHALL add phantom points to ensure smooth interpolation
4. WHEN calculating duck position along the spline THEN the Flight Path System SHALL use arc-length parameterization for consistent speed

### Requirement 6

**User Story:** As a player, I want duck flight speed to remain consistent along curved paths and ensure a minimum flight duration, so that the gameplay feels fair and all ducks are hittable.

#### Acceptance Criteria

1. WHEN a duck moves along a curved path THEN the Duck Controller SHALL maintain the configured flight speed regardless of path curvature
2. WHEN a duck moves along a path with sharp curves THEN the Duck Controller SHALL not exceed the configured maximum speed
3. WHEN calculating travel time THEN the Flight Path System SHALL account for the total arc length of the spline path
4. WHEN the difficulty increases duck speed THEN the Flight Path System SHALL apply the speed multiplier uniformly along the entire path
5. WHEN a flight path would result in a duration shorter than the minimum THEN the Flight Path System SHALL extend the path or reduce speed to meet the minimum duration
6. WHEN the minimum flight duration is configured THEN the Flight Path System SHALL ensure all ducks are visible and hittable for at least that duration

### Requirement 7

**User Story:** As a developer, I want the path generation to be deterministic when seeded, so that I can reproduce specific flight patterns for testing.

#### Acceptance Criteria

1. WHEN generating a flight path THEN the Flight Path System SHALL accept an optional random seed parameter
2. WHEN the same seed is provided THEN the Flight Path System SHALL generate identical waypoint selections
3. WHEN no seed is provided THEN the Flight Path System SHALL use a random seed for varied gameplay
4. WHEN logging path generation THEN the Flight Path System SHALL output the seed used for debugging purposes

