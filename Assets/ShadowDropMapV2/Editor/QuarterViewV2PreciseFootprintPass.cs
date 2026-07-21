using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class QuarterViewV2PreciseFootprintPass
{
    private const string MenuPath = "Shadow Drop/Refit Existing Building Footprints Only";
    private const string MarkerName = "apply_precise_footprints_once.txt";
    private const string BuildingPrefabFolder =
        "Assets/ShadowDropMapV2/Prefabs/Buildings";

    static QuarterViewV2PreciseFootprintPass()
    {
        string markerPath = GetMarkerPath();
        if (File.Exists(markerPath))
        {
            EditorApplication.delayCall += ApplyFromMarker;
        }
    }

    [MenuItem(MenuPath)]
    public static void Apply()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            throw new InvalidOperationException("No loaded scene is available.");
        }

        int gameObjectsBefore = CountSceneGameObjects(scene);
        int prefabRootsBefore = CountPrefabRoots(scene);
        int prefabColliders = UpdateBuildingPrefabs();
        int sceneColliders = UpdateSceneBuildings(scene);
        int gameObjectsAfter = CountSceneGameObjects(scene);
        int prefabRootsAfter = CountPrefabRoots(scene);

        if (gameObjectsBefore != gameObjectsAfter || prefabRootsBefore != prefabRootsAfter)
        {
            throw new InvalidOperationException(
                $"Footprint-only pass changed scene structure: " +
                $"GameObjects {gameObjectsBefore}->{gameObjectsAfter}, " +
                $"prefab roots {prefabRootsBefore}->{prefabRootsAfter}."
            );
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log(
            $"Precise building footprints applied without hierarchy changes: " +
            $"{prefabColliders} prefab colliders, {sceneColliders} scene colliders."
        );
    }

    private static void ApplyFromMarker()
    {
        string markerPath = GetMarkerPath();
        if (!File.Exists(markerPath) || EditorApplication.isCompiling)
        {
            return;
        }

        try
        {
            Apply();
            File.Delete(markerPath);
            AssetDatabase.Refresh();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static int UpdateBuildingPrefabs()
    {
        int updated = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { BuildingPrefabFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                if (renderer == null || renderer.sprite == null)
                {
                    continue;
                }

                foreach (PolygonCollider2D polygon in root.GetComponentsInChildren<PolygonCollider2D>(true))
                {
                    if (polygon.gameObject.name != "FootprintCollider")
                    {
                        continue;
                    }

                    polygon.pathCount = 1;
                    polygon.SetPath(
                        0,
                        QuarterViewFootprintEditorUtility.BuildFootprint(renderer.sprite, true)
                    );
                    EditorUtility.SetDirty(polygon);
                    updated++;
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        return updated;
    }

    private static int UpdateSceneBuildings(Scene scene)
    {
        int updated = 0;
        foreach (QuarterViewFootprintObstacle2D obstacle in
                 UnityEngine.Object.FindObjectsByType<QuarterViewFootprintObstacle2D>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None
                 ))
        {
            if (obstacle.gameObject.scene != scene)
            {
                continue;
            }

            Transform root = obstacle.transform.parent;
            SpriteRenderer renderer = root != null ? root.GetComponent<SpriteRenderer>() : null;
            PolygonCollider2D polygon = obstacle.GetComponent<PolygonCollider2D>();
            if (renderer == null || renderer.sprite == null || polygon == null ||
                !IsBuilding(root.gameObject, renderer))
            {
                continue;
            }

            polygon.pathCount = 1;
            polygon.SetPath(
                0,
                QuarterViewFootprintEditorUtility.BuildFootprint(renderer.sprite, true)
            );
            EditorUtility.SetDirty(polygon);
            updated++;
        }

        return updated;
    }

    private static bool IsBuilding(GameObject root, SpriteRenderer renderer)
    {
        string spritePath = AssetDatabase.GetAssetPath(renderer.sprite).Replace('\\', '/');
        if (spritePath.Contains("/Buildings/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string prefabPath = (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root) ?? string.Empty)
            .Replace('\\', '/');
        if (prefabPath.Contains("/Buildings/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        for (Transform current = root.transform; current != null; current = current.parent)
        {
            if (current.name == "ManualBuildings")
            {
                return true;
            }
        }

        return false;
    }

    private static int CountSceneGameObjects(Scene scene)
    {
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            count += root.GetComponentsInChildren<Transform>(true).Length;
        }

        return count;
    }

    private static int CountPrefabRoots(Scene scene)
    {
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static string GetMarkerPath()
    {
        return Path.Combine(Application.dataPath, "ShadowDropMapV2", MarkerName);
    }
}
