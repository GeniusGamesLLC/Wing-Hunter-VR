using UnityEngine;

/// <summary>
/// Immutable data class representing a complete flight path with precomputed arc-length data.
/// Provides position and tangent queries by distance or normalized time for smooth duck movement.
/// </summary>
public class FlightPath
{
    #region Properties

    /// <summary>
    /// The original waypoints defining the path (spawn + intermediates + target).
    /// Does not include phantom points.
    /// </summary>
    public Vector3[] Waypoints { get; }

    /// <summary>
    /// Total arc length of the spline path in world units.
    /// </summary>
    public float TotalArcLength { get; }

    /// <summary>
    /// Precomputed cumulative arc-length lookup table for constant-speed traversal.
    /// </summary>
    public float[] ArcLengthTable { get; }

    /// <summary>
    /// Random seed used to generate this path (for reproducibility).
    /// </summary>
    public int Seed { get; }

    /// <summary>
    /// Number of intermediate waypoints (excluding spawn and target).
    /// </summary>
    public int IntermediateWaypointCount { get; }

    /// <summary>
    /// Waypoints array with phantom points added for Catmull-Rom interpolation.
    /// </summary>
    public Vector3[] WaypointsWithPhantoms { get; }

    /// <summary>
    /// Spline tension parameter used for this path.
    /// </summary>
    public float Tension { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new FlightPath from waypoints, building the arc-length table.
    /// </summary>
    /// <param name="waypoints">Array of waypoints (spawn + intermediates + target). Minimum 2 points.</param>
    /// <param name="seed">Random seed used to generate this path</param>
    /// <param name="tension">Spline tension parameter (default 0.5)</param>
    /// <param name="samplesPerSegment">Samples per segment for arc-length calculation</param>
    public FlightPath(Vector3[] waypoints, int seed = 0, float tension = SplineUtility.DefaultTension, int samplesPerSegment = SplineUtility.DefaultSamplesPerSegment)
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogError("FlightPath: Need at least 2 waypoints (spawn and target)");
            // Create a minimal valid path
            Waypoints = new Vector3[] { Vector3.zero, Vector3.forward };
            Seed = seed;
            Tension = tension;
            IntermediateWaypointCount = 0;
            WaypointsWithPhantoms = SplineUtility.AddPhantomPoints(Waypoints);
            ArcLengthTable = SplineUtility.BuildArcLengthTable(WaypointsWithPhantoms, samplesPerSegment, tension);
            TotalArcLength = SplineUtility.GetTotalArcLength(ArcLengthTable);
            return;
        }

        // Store original waypoints (immutable copy)
        Waypoints = new Vector3[waypoints.Length];
        System.Array.Copy(waypoints, Waypoints, waypoints.Length);

        Seed = seed;
        Tension = tension;

        // Calculate intermediate waypoint count (total - spawn - target)
        IntermediateWaypointCount = Mathf.Max(0, waypoints.Length - 2);

        // Add phantom points for Catmull-Rom interpolation
        WaypointsWithPhantoms = SplineUtility.AddPhantomPoints(Waypoints);

        // Build arc-length table for constant-speed traversal
        ArcLengthTable = SplineUtility.BuildArcLengthTable(WaypointsWithPhantoms, samplesPerSegment, tension);

        // Store total arc length
        TotalArcLength = SplineUtility.GetTotalArcLength(ArcLengthTable);
    }

    #endregion

    #region Position and Tangent Queries

    /// <summary>
    /// Gets the position on the path at a specific distance from the start.
    /// Uses arc-length parameterization for constant-speed movement.
    /// </summary>
    /// <param name="distance">Distance along the path in world units</param>
    /// <returns>World position on the spline at the specified distance</returns>
    public Vector3 GetPositionAtDistance(float distance)
    {
        return SplineUtility.GetPointAtArcLength(WaypointsWithPhantoms, ArcLengthTable, distance, Tension);
    }

    /// <summary>
    /// Gets the tangent (movement direction) on the path at a specific distance from the start.
    /// </summary>
    /// <param name="distance">Distance along the path in world units</param>
    /// <returns>Normalized tangent vector at the specified distance</returns>
    public Vector3 GetTangentAtDistance(float distance)
    {
        return SplineUtility.GetTangentAtArcLength(WaypointsWithPhantoms, ArcLengthTable, distance, Tension);
    }

    /// <summary>
    /// Gets the position on the path at a normalized time (0 to 1).
    /// Uses arc-length parameterization for constant-speed movement.
    /// </summary>
    /// <param name="normalizedTime">Time value in range [0, 1] where 0 is start and 1 is end</param>
    /// <returns>World position on the spline at the specified normalized time</returns>
    public Vector3 GetPositionAtTime(float normalizedTime)
    {
        return SplineUtility.GetPointAtNormalizedTime(WaypointsWithPhantoms, ArcLengthTable, normalizedTime, Tension);
    }

    /// <summary>
    /// Gets the tangent (movement direction) on the path at a normalized time (0 to 1).
    /// </summary>
    /// <param name="normalizedTime">Time value in range [0, 1] where 0 is start and 1 is end</param>
    /// <returns>Normalized tangent vector at the specified normalized time</returns>
    public Vector3 GetTangentAtTime(float normalizedTime)
    {
        return SplineUtility.GetTangentAtNormalizedTime(WaypointsWithPhantoms, ArcLengthTable, normalizedTime, Tension);
    }

    /// <summary>
    /// Converts a distance along the path to a normalized time value.
    /// </summary>
    /// <param name="distance">Distance along the path in world units</param>
    /// <returns>Normalized time value in range [0, 1]</returns>
    public float GetTimeAtDistance(float distance)
    {
        if (TotalArcLength <= 0f)
        {
            return 0f;
        }
        return Mathf.Clamp01(distance / TotalArcLength);
    }

    /// <summary>
    /// Converts a normalized time value to a distance along the path.
    /// </summary>
    /// <param name="normalizedTime">Time value in range [0, 1]</param>
    /// <returns>Distance along the path in world units</returns>
    public float GetDistanceAtTime(float normalizedTime)
    {
        return Mathf.Clamp01(normalizedTime) * TotalArcLength;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets the spawn point (first waypoint) of the path.
    /// </summary>
    public Vector3 SpawnPoint => Waypoints.Length > 0 ? Waypoints[0] : Vector3.zero;

    /// <summary>
    /// Gets the target point (last waypoint) of the path.
    /// </summary>
    public Vector3 TargetPoint => Waypoints.Length > 0 ? Waypoints[Waypoints.Length - 1] : Vector3.zero;

    /// <summary>
    /// Checks if a given distance represents the end of the path.
    /// </summary>
    /// <param name="distance">Distance along the path</param>
    /// <returns>True if the distance is at or past the end of the path</returns>
    public bool IsAtEnd(float distance)
    {
        return distance >= TotalArcLength;
    }

    /// <summary>
    /// Generates an array of evenly-spaced points along the path for visualization.
    /// </summary>
    /// <param name="numPoints">Number of points to generate</param>
    /// <returns>Array of world positions along the spline</returns>
    public Vector3[] GenerateVisualizationPoints(int numPoints = 50)
    {
        return SplineUtility.GenerateSplinePoints(WaypointsWithPhantoms, ArcLengthTable, numPoints, Tension);
    }

    /// <summary>
    /// Calculates the estimated flight duration at a given speed.
    /// </summary>
    /// <param name="speed">Flight speed in units per second</param>
    /// <returns>Estimated duration in seconds</returns>
    public float GetEstimatedDuration(float speed)
    {
        if (speed <= 0f)
        {
            return float.MaxValue;
        }
        return TotalArcLength / speed;
    }

    /// <summary>
    /// Returns a string representation of the flight path for debugging.
    /// </summary>
    public override string ToString()
    {
        return $"FlightPath[Waypoints={Waypoints.Length}, Intermediates={IntermediateWaypointCount}, " +
               $"ArcLength={TotalArcLength:F2}, Seed={Seed}]";
    }

    #endregion
}
