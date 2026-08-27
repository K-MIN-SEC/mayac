using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuarterViewV2FootprintPolygonPass
{
    private const string Root = "Assets/ShadowDropMapV2";
    private const string ScenePath = Root + "/Scenes/QuarterViewV2.unity";
    private const string BuildingPrefabRoot = Root + "/Prefabs/Buildings";
    private const string PropPrefabRoot = Root + "/Prefabs/Props";
    private const string MarkerPath = Root + "/apply_polygon_footprints_once.txt";

    [InitializeOnLoadMethod]
    private static void ApplyAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (File.Exists(MarkerPath))
            {
                Apply();
            }
        };
    }

    [MenuItem("Shadow Drop/Rebuild Ground Footprint Polygons")]
    public static void Apply()
    {
        int buildingPrefabs = ConfigurePrefabs(BuildingPrefabRoot, true);
        int propPrefabs = ConfigurePrefabs(PropPrefabRoot, false);

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        Transform buildingParent = FindTransform(scene, "ManualBuildings");
        Transform propParent = FindTransform(scene, "ManualProps");
        if (buildingParent == null || propParent == null)
        {
            Debug.LogError("Polygon footprint pass could not find the placement roots.");
            return;
        }

        int buildings = ConfigureSceneObjects(buildingParent, true);
        int props = ConfigureSceneObjects(propParent, false);

        QuarterViewWalkableNavigator2D navigator =
            Object.FindAnyObjectByType<QuarterViewWalkableNavigator2D>();
        if (navigator != null)
        {
            SerializedObject serialized = new SerializedObject(navigator);
            serialized.FindProperty("pathCellPixels").intValue = 8;
            serialized.FindProperty("obstaclePadding").floatValue = 0.08f;
            serialized.FindProperty("destinationSnapRadiusCells").intValue = 20;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.DeleteAsset(MarkerPath);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log(
            $"Ground footprint polygons rebuilt: {buildingPrefabs} building prefabs, "
            + $"{propPrefabs} prop prefabs, {buildings} scene buildings, {props} scene props."
        );
    }

    private static int ConfigurePrefabs(string root, bool building)
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { root }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (QuarterViewFootprintEditorUtility.AddOrUpdateFootprint(contents, building))
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    count++;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        return count;
    }

    private static int ConfigureSceneObjects(Transform parent, bool building)
    {
        int count = 0;
        for (int index = 0; index < parent.childCount; index++)
        {
            if (QuarterViewFootprintEditorUtility.AddOrUpdateFootprint(
                parent.GetChild(index).gameObject,
                building
            ))
            {
                count++;
            }
        }

        return count;
    }

    private static Transform FindTransform(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindTransform(root.transform, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static Transform FindTransform(Transform current, string name)
    {
        if (current.name == name)
        {
            return current;
        }

        for (int index = 0; index < current.childCount; index++)
        {
            Transform found = FindTransform(current.GetChild(index), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
