using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using DuckHunt.Data;

/// <summary>
/// Editor utility to migrate existing spawn points to the new prefab-based system.
/// Converts legacy spawn/target points to prefab instances with standardized naming.
/// </summary>
public static class SpawnPointMigrationTool
{
    private const string SPAWN_POINT_PREFAB_PATH = "Assets/Prefabs/SpawnPoint.prefab";
    private const string TARGET_POINT_PREFAB_PATH = "Assets/Prefabs/TargetPoint.prefab";
    private const string SPAWN_POINTS_PARENT_NAME = "SpawnPoints";
    private const string TARGET_POINTS_PARENT_NAME = "TargetPoints";

    /// <summary>
    /// Result of a migration operation
    /// </summary>
    public struct MigrationResult
    {
        public int SpawnPointsMigrated;
        public int TargetPointsMigrated;
        public int PointsRenamed;
        public List<string> Warnings;

        public int TotalMigrated => SpawnPointsMigrated + TargetPointsMigrated;
    }

    [MenuItem("Tools/Spawn Points/Migrate to Prefab System")]
    public static void MigrateSpawnPoints()
    {
        // Check if scene is saved
        var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (activeScene.isDirty)
        {
            bool saveFirst = EditorUtility.DisplayDialog(
                "Save Scene?",
                "The current scene has unsaved changes. Would you like to save before migrating?",
                "Save and Continue",
                "Cancel"
            );

            if (saveFirst)
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
            }
            else
            {
                return;
            }
        }

        // Load prefabs
        GameObject spawnPointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SPAWN_POINT_PREFAB_PATH);
        GameObject targetPointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TARGET_POINT_PREFAB_PATH);

        if (spawnPointPrefab == null)
        {
            EditorUtility.DisplayDialog(
                "Migration Error",
                $"SpawnPoint prefab not found at:\n{SPAWN_POINT_PREFAB_PATH}\n\nPlease create the prefab first.",
                "OK"
            );
            return;
        }

        if (targetPointPrefab == null)
        {
            EditorUtility.DisplayDialog(
                "Migration Error",
                $"TargetPoint prefab not found at:\n{TARGET_POINT_PREFAB_PATH}\n\nPlease create the prefab first.",
                "OK"
            );
            return;
        }

        // Find or create parent containers
        Transform spawnPointsParent = FindOrCreateParent(SPAWN_POINTS_PARENT_NAME);
        Transform targetPointsParent = FindOrCreateParent(TARGET_POINTS_PARENT_NAME);

        // Run migration
        MigrationResult result = MigratePoints(
            spawnPointsParent,
            targetPointsParent,
            spawnPointPrefab,
            targetPointPrefab
        );

        // Display results
        string message = $"Migration Complete!\n\n" +
                        $"Spawn Points Migrated: {result.SpawnPointsMigrated}\n" +
                        $"Target Points Migrated: {result.TargetPointsMigrated}\n" +
                        $"Points Renamed: {result.PointsRenamed}\n" +
                        $"Total: {result.TotalMigrated}";

        if (result.Warnings != null && result.Warnings.Count > 0)
        {
            message += $"\n\nWarnings ({result.Warnings.Count}):\n";
            foreach (var warning in result.Warnings.Take(5))
            {
                message += $"• {warning}\n";
            }
            if (result.Warnings.Count > 5)
            {
                message += $"... and {result.Warnings.Count - 5} more (see Console)";
            }
        }

        EditorUtility.DisplayDialog("Spawn Point Migration", message, "OK");

        // Log full warnings to console
        if (result.Warnings != null)
        {
            foreach (var warning in result.Warnings)
            {
                Debug.LogWarning($"[SpawnPointMigration] {warning}");
            }
        }

        Debug.Log($"[SpawnPointMigration] Migration complete. {result.TotalMigrated} points processed.");
    }

    /// <summary>
    /// Migrates spawn and target points to the prefab system
    /// </summary>
    public static MigrationResult MigratePoints(
        Transform spawnPointsParent,
        Transform targetPointsParent,
        GameObject spawnPointPrefab,
        GameObject targetPointPrefab)
    {
        MigrationResult result = new MigrationResult
        {
            Warnings = new List<string>()
        };

        // Find existing spawn points
        List<PointData> existingSpawnPoints = FindExistingSpawnPoints();
        List<PointData> existingTargetPoints = FindExistingTargetPoints();

        // Register undo group
        Undo.SetCurrentGroupName("Migrate Spawn Points to Prefab System");
        int undoGroup = Undo.GetCurrentGroup();

        // Migrate spawn points
        result.SpawnPointsMigrated = MigratePointList(
            existingSpawnPoints,
            spawnPointsParent,
            spawnPointPrefab,
            SpawnPointType.Spawn,
            "SpawnPoint",
            result.Warnings
        );
        result.PointsRenamed += existingSpawnPoints.Count(p => p.NeedsRename);

        // Migrate target points
        result.TargetPointsMigrated = MigratePointList(
            existingTargetPoints,
            targetPointsParent,
            targetPointPrefab,
            SpawnPointType.Target,
            "TargetPoint",
            result.Warnings
        );
        result.PointsRenamed += existingTargetPoints.Count(p => p.NeedsRename);

        // Collapse undo operations
        Undo.CollapseUndoOperations(undoGroup);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        return result;
    }

    /// <summary>
    /// Data structure to hold information about an existing point
    /// </summary>
    private struct PointData
    {
        public GameObject GameObject;
        public Vector3 WorldPosition;
        public Quaternion WorldRotation;
        public string OriginalName;
        public int ExtractedIndex;
        public bool IsPrefabInstance;
        public bool NeedsRename;
    }

    /// <summary>
    /// Finds all existing spawn points in the scene
    /// </summary>
    private static List<PointData> FindExistingSpawnPoints()
    {
        return FindPointsInContainer(SPAWN_POINTS_PARENT_NAME, SpawnPointType.Spawn);
    }

    /// <summary>
    /// Finds all existing target points in the scene
    /// </summary>
    private static List<PointData> FindExistingTargetPoints()
    {
        return FindPointsInContainer(TARGET_POINTS_PARENT_NAME, SpawnPointType.Target);
    }

    /// <summary>
    /// Finds all direct children of a container that should be migrated
    /// </summary>
    private static List<PointData> FindPointsInContainer(string containerName, SpawnPointType expectedType)
    {
        List<PointData> points = new List<PointData>();

        // Find the container
        GameObject container = GameObject.Find(containerName);
        if (container == null)
        {
            Debug.LogWarning($"[SpawnPointMigration] Container '{containerName}' not found in scene");
            return points;
        }

        // Get all direct children of the container
        foreach (Transform child in container.transform)
        {
            // Skip if this is not a point (e.g., it's a pool or other object)
            string nameLower = child.name.ToLower();
            bool isSpawnPoint = nameLower.Contains("spawn") && !nameLower.Contains("manager") && !nameLower.Contains("pool");
            bool isTargetPoint = nameLower.Contains("target");
            
            if (expectedType == SpawnPointType.Spawn && !isSpawnPoint)
                continue;
            if (expectedType == SpawnPointType.Target && !isTargetPoint)
                continue;

            // Check if it already has a SpawnPointMarker
            SpawnPointMarker marker = child.GetComponent<SpawnPointMarker>();
            int index = marker != null ? marker.PointIndex : ExtractIndexFromName(child.name);
            
            points.Add(CreatePointData(child.gameObject, index));
        }

        // Sort by extracted index, then by name
        return points.OrderBy(p => p.ExtractedIndex).ThenBy(p => p.OriginalName).ToList();
    }

    /// <summary>
    /// Creates a PointData structure from a GameObject
    /// </summary>
    private static PointData CreatePointData(GameObject obj, int index)
    {
        bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(obj);
        string standardName = GetStandardizedName(obj.name, index);
        bool needsRename = obj.name != standardName;

        return new PointData
        {
            GameObject = obj,
            WorldPosition = obj.transform.position,
            WorldRotation = obj.transform.rotation,
            OriginalName = obj.name,
            ExtractedIndex = index,
            IsPrefabInstance = isPrefabInstance,
            NeedsRename = needsRename
        };
    }

    /// <summary>
    /// Extracts a numeric index from a point name
    /// </summary>
    private static int ExtractIndexFromName(string name)
    {
        // Try to find a number in the name
        Match match = Regex.Match(name, @"(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
        {
            return index;
        }
        return -1; // Will be assigned a new index
    }

    /// <summary>
    /// Gets the standardized name for a point
    /// </summary>
    private static string GetStandardizedName(string originalName, int index)
    {
        string prefix = originalName.ToLower().Contains("target") ? "TargetPoint" : "SpawnPoint";
        return $"{prefix}_{index:D2}";
    }

    /// <summary>
    /// Migrates a list of points to prefab instances
    /// </summary>
    private static int MigratePointList(
        List<PointData> points,
        Transform parent,
        GameObject prefab,
        SpawnPointType pointType,
        string namePrefix,
        List<string> warnings)
    {
        int migratedCount = 0;
        HashSet<int> usedIndices = new HashSet<int>();

        // First pass: collect all valid indices
        foreach (var point in points)
        {
            if (point.ExtractedIndex >= 0)
            {
                usedIndices.Add(point.ExtractedIndex);
            }
        }

        // Second pass: assign indices to points without valid ones
        int nextIndex = 0;
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            if (point.ExtractedIndex < 0)
            {
                while (usedIndices.Contains(nextIndex))
                {
                    nextIndex++;
                }
                point.ExtractedIndex = nextIndex;
                points[i] = point;
                usedIndices.Add(nextIndex);
                nextIndex++;
            }
        }

        // Re-sort by index
        points = points.OrderBy(p => p.ExtractedIndex).ToList();

        // Check for duplicate indices
        var duplicates = points.GroupBy(p => p.ExtractedIndex)
                               .Where(g => g.Count() > 1)
                               .ToList();

        foreach (var dup in duplicates)
        {
            warnings.Add($"Duplicate index {dup.Key} found for {namePrefix}. Only first instance will use this index.");
            
            // Reassign indices for duplicates
            bool first = true;
            foreach (var point in dup)
            {
                if (first)
                {
                    first = false;
                    continue;
                }

                // Find next available index
                while (usedIndices.Contains(nextIndex))
                {
                    nextIndex++;
                }

                int pointIndex = points.FindIndex(p => p.GameObject == point.GameObject);
                if (pointIndex >= 0)
                {
                    var updatedPoint = points[pointIndex];
                    updatedPoint.ExtractedIndex = nextIndex;
                    points[pointIndex] = updatedPoint;
                    usedIndices.Add(nextIndex);
                    nextIndex++;
                }
            }
        }

        // Re-sort again after fixing duplicates
        points = points.OrderBy(p => p.ExtractedIndex).ToList();

        // Migrate each point
        foreach (var point in points)
        {
            try
            {
                MigrateSinglePoint(point, parent, prefab, pointType, namePrefix, warnings);
                migratedCount++;
            }
            catch (System.Exception ex)
            {
                warnings.Add($"Failed to migrate {point.OriginalName}: {ex.Message}");
                Debug.LogError($"[SpawnPointMigration] Error migrating {point.OriginalName}: {ex}");
            }
        }

        return migratedCount;
    }

    /// <summary>
    /// Migrates a single point to a prefab instance
    /// </summary>
    private static void MigrateSinglePoint(
        PointData point,
        Transform parent,
        GameObject prefab,
        SpawnPointType pointType,
        string namePrefix,
        List<string> warnings)
    {
        string newName = $"{namePrefix}_{point.ExtractedIndex:D2}";

        // Check if already a correct prefab instance with correct name and parent
        SpawnPointMarker existingMarker = point.GameObject.GetComponent<SpawnPointMarker>();
        if (existingMarker != null && 
            existingMarker.PointType == pointType &&
            point.GameObject.name == newName &&
            point.GameObject.transform.parent == parent)
        {
            // Already migrated, just update index if needed
            if (existingMarker.PointIndex != point.ExtractedIndex)
            {
                Undo.RecordObject(existingMarker, "Update Point Index");
                existingMarker.PointIndex = point.ExtractedIndex;
            }
            return;
        }

        // Create new prefab instance
        GameObject newPoint = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        Undo.RegisterCreatedObjectUndo(newPoint, "Create Migrated Point");

        // Set transform
        newPoint.transform.position = point.WorldPosition;
        newPoint.transform.rotation = point.WorldRotation;

        // Set name
        newPoint.name = newName;

        // Configure SpawnPointMarker
        SpawnPointMarker marker = newPoint.GetComponent<SpawnPointMarker>();
        if (marker != null)
        {
            marker.PointIndex = point.ExtractedIndex;
        }
        else
        {
            warnings.Add($"Prefab {prefab.name} is missing SpawnPointMarker component");
        }

        // Delete original object
        Undo.DestroyObjectImmediate(point.GameObject);
    }

    /// <summary>
    /// Finds or creates a parent container GameObject
    /// </summary>
    private static Transform FindOrCreateParent(string name)
    {
        // First try to find the container directly
        GameObject parent = GameObject.Find(name);
        if (parent != null)
        {
            return parent.transform;
        }

        // Try to find it under "--- Spawn System ---"
        GameObject spawnSystem = GameObject.Find("--- Spawn System ---");
        if (spawnSystem != null)
        {
            Transform existingChild = spawnSystem.transform.Find(name);
            if (existingChild != null)
            {
                return existingChild;
            }

            // Create under spawn system
            parent = new GameObject(name);
            parent.transform.SetParent(spawnSystem.transform);
            Undo.RegisterCreatedObjectUndo(parent, $"Create {name} Container");
            Debug.Log($"[SpawnPointMigration] Created parent container: {name} under --- Spawn System ---");
            return parent.transform;
        }

        // Fall back to creating at root
        parent = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(parent, $"Create {name} Container");
        Debug.Log($"[SpawnPointMigration] Created parent container: {name}");
        return parent.transform;
    }

    [MenuItem("Tools/Spawn Points/Validate Point Naming")]
    public static void ValidatePointNaming()
    {
        List<string> issues = new List<string>();

        // Check spawn points
        SpawnPointMarker[] markers = Object.FindObjectsOfType<SpawnPointMarker>(true);
        
        var spawnPoints = markers.Where(m => m.PointType == SpawnPointType.Spawn).ToList();
        var targetPoints = markers.Where(m => m.PointType == SpawnPointType.Target).ToList();

        // Validate spawn point naming
        foreach (var point in spawnPoints)
        {
            string expectedName = $"SpawnPoint_{point.PointIndex:D2}";
            if (point.gameObject.name != expectedName)
            {
                issues.Add($"Spawn point '{point.gameObject.name}' should be named '{expectedName}'");
            }

            // Check parent - can be directly under SpawnPoints or nested deeper
            bool hasCorrectParent = false;
            Transform current = point.transform.parent;
            while (current != null)
            {
                if (current.name == SPAWN_POINTS_PARENT_NAME)
                {
                    hasCorrectParent = true;
                    break;
                }
                current = current.parent;
            }
            if (!hasCorrectParent)
            {
                issues.Add($"Spawn point '{point.gameObject.name}' should be under '{SPAWN_POINTS_PARENT_NAME}'");
            }
        }

        // Validate target point naming
        foreach (var point in targetPoints)
        {
            string expectedName = $"TargetPoint_{point.PointIndex:D2}";
            if (point.gameObject.name != expectedName)
            {
                issues.Add($"Target point '{point.gameObject.name}' should be named '{expectedName}'");
            }

            // Check parent - can be directly under TargetPoints or nested deeper
            bool hasCorrectParent = false;
            Transform current = point.transform.parent;
            while (current != null)
            {
                if (current.name == TARGET_POINTS_PARENT_NAME)
                {
                    hasCorrectParent = true;
                    break;
                }
                current = current.parent;
            }
            if (!hasCorrectParent)
            {
                issues.Add($"Target point '{point.gameObject.name}' should be under '{TARGET_POINTS_PARENT_NAME}'");
            }
        }

        // Check for matching pairs
        var spawnIndices = spawnPoints.Select(p => p.PointIndex).ToHashSet();
        var targetIndices = targetPoints.Select(p => p.PointIndex).ToHashSet();

        foreach (var index in spawnIndices)
        {
            if (!targetIndices.Contains(index))
            {
                issues.Add($"SpawnPoint_{index:D2} has no matching TargetPoint_{index:D2}");
            }
        }

        foreach (var index in targetIndices)
        {
            if (!spawnIndices.Contains(index))
            {
                issues.Add($"TargetPoint_{index:D2} has no matching SpawnPoint_{index:D2}");
            }
        }

        // Display results
        if (issues.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Validation Passed",
                $"All {spawnPoints.Count} spawn points and {targetPoints.Count} target points are correctly named and organized.",
                "OK"
            );
        }
        else
        {
            string message = $"Found {issues.Count} issues:\n\n";
            foreach (var issue in issues.Take(10))
            {
                message += $"• {issue}\n";
            }
            if (issues.Count > 10)
            {
                message += $"\n... and {issues.Count - 10} more (see Console)";
            }

            EditorUtility.DisplayDialog("Validation Issues", message, "OK");

            foreach (var issue in issues)
            {
                Debug.LogWarning($"[SpawnPointValidation] {issue}");
            }
        }
    }
}
