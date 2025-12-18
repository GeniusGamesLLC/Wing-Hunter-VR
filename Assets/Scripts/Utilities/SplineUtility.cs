using UnityEngine;

/// <summary>
/// Static utility class for Catmull-Rom spline calculations.
/// Provides methods for spline interpolation, arc-length parameterization,
/// and phantom point generation for smooth curved paths.
/// </summary>
public static class SplineUtility
{
    /// <summary>
    /// Default tension value for Catmull-Rom splines.
    /// 0.5 produces a standard Catmull-Rom spline.
    /// </summary>
    public const float DefaultTension = 0.5f;
    
    /// <summary>
    /// Default number of samples per segment for arc-length calculations.
    /// </summary>
    public const int DefaultSamplesPerSegment = 20;

    #region Catmull-Rom Spline Calculations

    /// <summary>
    /// Calculates a point on a Catmull-Rom spline segment.
    /// The spline interpolates between p1 and p2, using p0 and p3 as control points.
    /// </summary>
    /// <param name="p0">Control point before the segment start</param>
    /// <param name="p1">Segment start point</param>
    /// <param name="p2">Segment end point</param>
    /// <param name="p3">Control point after the segment end</param>
    /// <param name="t">Parameter value in range [0, 1]</param>
    /// <param name="tension">Tension parameter (default 0.5 for standard Catmull-Rom)</param>
    /// <returns>Interpolated position on the spline</returns>
    public static Vector3 CatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension = DefaultTension)
    {
        // Clamp t to valid range
        t = Mathf.Clamp01(t);
        
        float t2 = t * t;
        float t3 = t2 * t;
        
        // Catmull-Rom spline formula with tension parameter
        // P(t) = tension * ((-t³ + 2t² - t) * p0 + 
        //                   (3t³ - 5t² + 2) * p1 + 
        //                   (-3t³ + 4t² + t) * p2 + 
        //                   (t³ - t²) * p3)
        float c0 = -t3 + 2f * t2 - t;
        float c1 = 3f * t3 - 5f * t2 + 2f;
        float c2 = -3f * t3 + 4f * t2 + t;
        float c3 = t3 - t2;
        
        return tension * (c0 * p0 + c1 * p1 + c2 * p2 + c3 * p3);
    }

    /// <summary>
    /// Calculates the tangent (derivative) at a point on a Catmull-Rom spline segment.
    /// The tangent represents the direction of movement along the spline.
    /// </summary>
    /// <param name="p0">Control point before the segment start</param>
    /// <param name="p1">Segment start point</param>
    /// <param name="p2">Segment end point</param>
    /// <param name="p3">Control point after the segment end</param>
    /// <param name="t">Parameter value in range [0, 1]</param>
    /// <param name="tension">Tension parameter (default 0.5 for standard Catmull-Rom)</param>
    /// <returns>Tangent vector at the specified point (not normalized)</returns>
    public static Vector3 CatmullRomTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension = DefaultTension)
    {
        // Clamp t to valid range
        t = Mathf.Clamp01(t);
        
        float t2 = t * t;
        
        // Derivative of Catmull-Rom spline formula
        // P'(t) = tension * ((-3t² + 4t - 1) * p0 + 
        //                    (9t² - 10t) * p1 + 
        //                    (-9t² + 8t + 1) * p2 + 
        //                    (3t² - 2t) * p3)
        float c0 = -3f * t2 + 4f * t - 1f;
        float c1 = 9f * t2 - 10f * t;
        float c2 = -9f * t2 + 8f * t + 1f;
        float c3 = 3f * t2 - 2f * t;
        
        return tension * (c0 * p0 + c1 * p1 + c2 * p2 + c3 * p3);
    }

    /// <summary>
    /// Adds phantom points to a waypoint array to enable Catmull-Rom interpolation
    /// for paths with fewer than 4 control points.
    /// </summary>
    /// <param name="waypoints">Original waypoint array (minimum 2 points)</param>
    /// <returns>Waypoint array with phantom points added at start and end</returns>
    public static Vector3[] AddPhantomPoints(Vector3[] waypoints)
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("SplineUtility.AddPhantomPoints: Need at least 2 waypoints");
            return waypoints;
        }
        
        // Create new array with phantom points at start and end
        Vector3[] result = new Vector3[waypoints.Length + 2];
        
        // Copy original waypoints to middle of array
        for (int i = 0; i < waypoints.Length; i++)
        {
            result[i + 1] = waypoints[i];
        }
        
        // Add phantom point before start by extrapolation
        // phantom = 2 * endpoint - adjacent
        result[0] = 2f * waypoints[0] - waypoints[1];
        
        // Add phantom point after end by extrapolation
        result[result.Length - 1] = 2f * waypoints[waypoints.Length - 1] - waypoints[waypoints.Length - 2];
        
        return result;
    }

    #endregion

    #region Arc-Length Parameterization


    /// <summary>
    /// Calculates the approximate arc length of a single Catmull-Rom spline segment
    /// using numerical integration (sampling).
    /// </summary>
    /// <param name="p0">Control point before the segment start</param>
    /// <param name="p1">Segment start point</param>
    /// <param name="p2">Segment end point</param>
    /// <param name="p3">Control point after the segment end</param>
    /// <param name="samples">Number of samples for approximation (higher = more accurate)</param>
    /// <param name="tension">Tension parameter for the spline</param>
    /// <returns>Approximate arc length of the segment</returns>
    public static float CalculateSegmentArcLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int samples = DefaultSamplesPerSegment, float tension = DefaultTension)
    {
        if (samples < 2)
        {
            samples = 2;
        }
        
        float arcLength = 0f;
        Vector3 previousPoint = CatmullRomPoint(p0, p1, p2, p3, 0f, tension);
        
        for (int i = 1; i <= samples; i++)
        {
            float t = (float)i / samples;
            Vector3 currentPoint = CatmullRomPoint(p0, p1, p2, p3, t, tension);
            arcLength += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
        
        return arcLength;
    }

    /// <summary>
    /// Builds a cumulative arc-length lookup table for an entire spline path.
    /// The table maps segment indices and local t values to cumulative distances.
    /// </summary>
    /// <param name="waypoints">Array of waypoints (should include phantom points or have at least 4 points)</param>
    /// <param name="samplesPerSegment">Number of samples per segment for arc-length calculation</param>
    /// <param name="tension">Tension parameter for the spline</param>
    /// <returns>Array of cumulative arc lengths at each sample point</returns>
    public static float[] BuildArcLengthTable(Vector3[] waypoints, int samplesPerSegment = DefaultSamplesPerSegment, float tension = DefaultTension)
    {
        if (waypoints == null || waypoints.Length < 4)
        {
            Debug.LogWarning("SplineUtility.BuildArcLengthTable: Need at least 4 waypoints (including phantom points)");
            return new float[] { 0f };
        }
        
        // Number of interpolatable segments is (waypoints.Length - 3)
        // We interpolate between waypoints[1] and waypoints[n-2]
        int numSegments = waypoints.Length - 3;
        int totalSamples = numSegments * samplesPerSegment + 1;
        
        float[] arcLengthTable = new float[totalSamples];
        arcLengthTable[0] = 0f;
        
        float cumulativeLength = 0f;
        int tableIndex = 1;
        
        for (int segment = 0; segment < numSegments; segment++)
        {
            Vector3 p0 = waypoints[segment];
            Vector3 p1 = waypoints[segment + 1];
            Vector3 p2 = waypoints[segment + 2];
            Vector3 p3 = waypoints[segment + 3];
            
            Vector3 previousPoint = CatmullRomPoint(p0, p1, p2, p3, 0f, tension);
            
            for (int i = 1; i <= samplesPerSegment; i++)
            {
                float t = (float)i / samplesPerSegment;
                Vector3 currentPoint = CatmullRomPoint(p0, p1, p2, p3, t, tension);
                cumulativeLength += Vector3.Distance(previousPoint, currentPoint);
                arcLengthTable[tableIndex] = cumulativeLength;
                tableIndex++;
                previousPoint = currentPoint;
            }
        }
        
        return arcLengthTable;
    }

    /// <summary>
    /// Gets the total arc length from a precomputed arc-length table.
    /// </summary>
    /// <param name="arcLengthTable">Precomputed arc-length table</param>
    /// <returns>Total arc length of the spline</returns>
    public static float GetTotalArcLength(float[] arcLengthTable)
    {
        if (arcLengthTable == null || arcLengthTable.Length == 0)
        {
            return 0f;
        }
        return arcLengthTable[arcLengthTable.Length - 1];
    }

    /// <summary>
    /// Gets a point on the spline at a specific arc-length distance from the start.
    /// Uses binary search on the arc-length table for efficient lookup.
    /// </summary>
    /// <param name="waypoints">Array of waypoints (should include phantom points)</param>
    /// <param name="arcLengthTable">Precomputed arc-length table</param>
    /// <param name="targetLength">Target distance along the spline</param>
    /// <param name="tension">Tension parameter for the spline</param>
    /// <returns>Position on the spline at the specified distance</returns>
    public static Vector3 GetPointAtArcLength(Vector3[] waypoints, float[] arcLengthTable, float targetLength, float tension = DefaultTension)
    {
        if (waypoints == null || waypoints.Length < 4 || arcLengthTable == null || arcLengthTable.Length == 0)
        {
            Debug.LogWarning("SplineUtility.GetPointAtArcLength: Invalid waypoints or arc-length table");
            return waypoints != null && waypoints.Length > 0 ? waypoints[0] : Vector3.zero;
        }
        
        float totalLength = arcLengthTable[arcLengthTable.Length - 1];
        
        // Clamp target length to valid range
        targetLength = Mathf.Clamp(targetLength, 0f, totalLength);
        
        // Handle edge cases
        if (targetLength <= 0f)
        {
            return waypoints[1]; // First interpolatable point
        }
        if (targetLength >= totalLength)
        {
            return waypoints[waypoints.Length - 2]; // Last interpolatable point
        }
        
        // Binary search to find the segment containing the target length
        int low = 0;
        int high = arcLengthTable.Length - 1;
        
        while (low < high - 1)
        {
            int mid = (low + high) / 2;
            if (arcLengthTable[mid] < targetLength)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
        }
        
        // Interpolate within the found segment
        float segmentStartLength = arcLengthTable[low];
        float segmentEndLength = arcLengthTable[high];
        float segmentLength = segmentEndLength - segmentStartLength;
        
        float localT;
        if (segmentLength > 0.0001f)
        {
            localT = (targetLength - segmentStartLength) / segmentLength;
        }
        else
        {
            localT = 0f;
        }
        
        // Convert table index to segment and t value
        int numSegments = waypoints.Length - 3;
        int samplesPerSegment = (arcLengthTable.Length - 1) / numSegments;
        
        int segmentIndex = low / samplesPerSegment;
        int sampleInSegment = low % samplesPerSegment;
        
        // Clamp segment index
        segmentIndex = Mathf.Clamp(segmentIndex, 0, numSegments - 1);
        
        // Calculate the actual t value within the segment
        float tStart = (float)sampleInSegment / samplesPerSegment;
        float tEnd = (float)(sampleInSegment + 1) / samplesPerSegment;
        float t = Mathf.Lerp(tStart, tEnd, localT);
        
        // Get the control points for this segment
        Vector3 p0 = waypoints[segmentIndex];
        Vector3 p1 = waypoints[segmentIndex + 1];
        Vector3 p2 = waypoints[segmentIndex + 2];
        Vector3 p3 = waypoints[segmentIndex + 3];
        
        return CatmullRomPoint(p0, p1, p2, p3, t, tension);
    }

    /// <summary>
    /// Gets the tangent (direction) on the spline at a specific arc-length distance from the start.
    /// </summary>
    /// <param name="waypoints">Array of waypoints (should include phantom points)</param>
    /// <param name="arcLengthTable">Precomputed arc-length table</param>
    /// <param name="targetLength">Target distance along the spline</param>
    /// <param name="tension">Tension parameter for the spline</param>
    /// <returns>Normalized tangent vector at the specified distance</returns>
    public static Vector3 GetTangentAtArcLength(Vector3[] waypoints, float[] arcLengthTable, float targetLength, float tension = DefaultTension)
    {
        if (waypoints == null || waypoints.Length < 4 || arcLengthTable == null || arcLengthTable.Length == 0)
        {
            Debug.LogWarning("SplineUtility.GetTangentAtArcLength: Invalid waypoints or arc-length table");
            return Vector3.forward;
        }
        
        float totalLength = arcLengthTable[arcLengthTable.Length - 1];
        
        // Clamp target length to valid range
        targetLength = Mathf.Clamp(targetLength, 0f, totalLength);
        
        // Handle edge cases - use small offset to get tangent at endpoints
        if (targetLength <= 0f)
        {
            Vector3 startTangent = CatmullRomTangent(waypoints[0], waypoints[1], waypoints[2], waypoints[3], 0f, tension);
            return startTangent.magnitude > 0.0001f ? startTangent.normalized : Vector3.forward;
        }
        if (targetLength >= totalLength)
        {
            int lastSegment = waypoints.Length - 4;
            Vector3 endTangent = CatmullRomTangent(waypoints[lastSegment], waypoints[lastSegment + 1], waypoints[lastSegment + 2], waypoints[lastSegment + 3], 1f, tension);
            return endTangent.magnitude > 0.0001f ? endTangent.normalized : Vector3.forward;
        }
        
        // Binary search to find the segment containing the target length
        int low = 0;
        int high = arcLengthTable.Length - 1;
        
        while (low < high - 1)
        {
            int mid = (low + high) / 2;
            if (arcLengthTable[mid] < targetLength)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
        }
        
        // Interpolate within the found segment
        float segmentStartLength = arcLengthTable[low];
        float segmentEndLength = arcLengthTable[high];
        float segmentLength = segmentEndLength - segmentStartLength;
        
        float localT;
        if (segmentLength > 0.0001f)
        {
            localT = (targetLength - segmentStartLength) / segmentLength;
        }
        else
        {
            localT = 0f;
        }
        
        // Convert table index to segment and t value
        int numSegments = waypoints.Length - 3;
        int samplesPerSegment = (arcLengthTable.Length - 1) / numSegments;
        
        int segmentIndex = low / samplesPerSegment;
        int sampleInSegment = low % samplesPerSegment;
        
        // Clamp segment index
        segmentIndex = Mathf.Clamp(segmentIndex, 0, numSegments - 1);
        
        // Calculate the actual t value within the segment
        float tStart = (float)sampleInSegment / samplesPerSegment;
        float tEnd = (float)(sampleInSegment + 1) / samplesPerSegment;
        float t = Mathf.Lerp(tStart, tEnd, localT);
        
        // Get the control points for this segment
        Vector3 p0 = waypoints[segmentIndex];
        Vector3 p1 = waypoints[segmentIndex + 1];
        Vector3 p2 = waypoints[segmentIndex + 2];
        Vector3 p3 = waypoints[segmentIndex + 3];
        
        Vector3 tangent = CatmullRomTangent(p0, p1, p2, p3, t, tension);
        return tangent.magnitude > 0.0001f ? tangent.normalized : Vector3.forward;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets a point on the spline at a normalized time (0 to 1) along the entire path.
    /// Uses arc-length parameterization for constant speed.
    /// </summary>
    /// <param name="waypoints">Array of waypoints (should include phantom points)</param>
    /// <param name="arcLengthTable">Precomputed arc-length table</param>
    /// <param name="normalizedTime">Time value in range [0, 1]</param>
    /// <param name="tension">Tension parameter for the spline</param>
    /// <returns>Position on the spline at the specified normalized time</returns>
    public static Vector3 GetPointAtNormalizedTime(Vector3[] waypoints, float[] arcLengthTable, float normalizedTime, float tension = DefaultTension)
    {
        float totalLength = GetTotalArcLength(arcLengthTable);
        float targetLength = normalizedTime * totalLength;
        return GetPointAtArcLength(waypoints, arcLengthTable, targetLength, tension);
    }

    /// <summary>
    /// Gets the tangent on the spline at a normalized time (0 to 1) along the entire path.
    /// </summary>
    /// <param name="waypoints">Array of waypoints (should include phantom points)</param>
    /// <param name="arcLengthTable">Precomputed arc-length table</param>
    /// <param name="normalizedTime">Time value in range [0, 1]</param>
    /// <param name="tension">Tension parameter for the spline</param>
    /// <returns>Normalized tangent vector at the specified normalized time</returns>
    public static Vector3 GetTangentAtNormalizedTime(Vector3[] waypoints, float[] arcLengthTable, float normalizedTime, float tension = DefaultTension)
    {
        float totalLength = GetTotalArcLength(arcLengthTable);
        float targetLength = normalizedTime * totalLength;
        return GetTangentAtArcLength(waypoints, arcLengthTable, targetLength, tension);
    }

    /// <summary>
    /// Generates an array of points along the spline for visualization purposes.
    /// </summary>
    /// <param name="waypoints">Array of waypoints (should include phantom points)</param>
    /// <param name="arcLengthTable">Precomputed arc-length table</param>
    /// <param name="numPoints">Number of points to generate</param>
    /// <param name="tension">Tension parameter for the spline</param>
    /// <returns>Array of evenly-spaced points along the spline</returns>
    public static Vector3[] GenerateSplinePoints(Vector3[] waypoints, float[] arcLengthTable, int numPoints, float tension = DefaultTension)
    {
        if (numPoints < 2)
        {
            numPoints = 2;
        }
        
        Vector3[] points = new Vector3[numPoints];
        
        for (int i = 0; i < numPoints; i++)
        {
            float normalizedTime = (float)i / (numPoints - 1);
            points[i] = GetPointAtNormalizedTime(waypoints, arcLengthTable, normalizedTime, tension);
        }
        
        return points;
    }

    #endregion
}
