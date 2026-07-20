using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuarterViewV2FineLayout
{
    private const string Root = "Assets/ShadowDropMapV2";
    private const string ScenePath = Root + "/Scenes/QuarterViewV2.unity";
    private const string BuildingPrefabRoot = Root + "/Prefabs/Buildings";
    private const string PropPrefabRoot = Root + "/Prefabs/Props";
    private const string MarkerPath = Root + "/apply_fine_layout_once.txt";
    private const int LayoutVersion = 3;

    private readonly struct Placement
    {
        public Placement(string label, string asset, float x, float y, float z, float scale)
        {
            Label = label;
            Asset = asset;
            Position = new Vector3(x, y, z);
            Scale = scale;
        }

        public string Label { get; }
        public string Asset { get; }
        public Vector3 Position { get; }
        public float Scale { get; }
    }

    private static readonly Placement[] Buildings =
    {
        // Center-left civic block: rear pair, then one low frontage building.
        new("Fine_CL_Back_A", "apartment_mid", -18.0f, 0.2f, 0.75f, 0.40f),
        new("Fine_CL_Back_B", "villa_walled", -12.5f, 0.6f, 0.85f, 0.44f),
        new("Fine_CL_Front", "market_neighborhood", -15.1f, -4.4f, -0.35f, 0.43f),

        // Southern large block: two rows create depth without covering the road edge.
        new("Fine_S_Back_A", "house_low_darkroof", 11.5f, -17.0f, 0.90f, 0.48f),
        new("Fine_S_Back_B", "apartment_mid", 16.8f, -17.2f, 0.85f, 0.40f),
        new("Fine_S_Front_A", "convenience_awning", 12.2f, -22.0f, -0.25f, 0.45f),
        new("Fine_S_Front_B", "market_neighborhood", 18.0f, -21.8f, -0.20f, 0.42f),

        // North-west block: two rear residences and a lower street-facing shop.
        new("Fine_NW_Back_A", "villa_walled", -17.4f, 20.0f, 0.90f, 0.44f),
        new("Fine_NW_Back_B", "clinic_neighborhood", -10.7f, 19.8f, 0.90f, 0.42f),
        new("Fine_NW_Front", "restaurant_corner", -15.0f, 15.2f, 0.10f, 0.44f),

        // North-center block fills the deep empty half behind the existing shops.
        new("Fine_NC_Back", "apartment_gray_tall", 6.8f, 11.3f, 0.80f, 0.38f),
        new("Fine_NC_Front", "convenience_24", 7.2f, 6.5f, 0.00f, 0.44f),

        // Far-north block keeps the user's low houses as the foreground row.
        new("Fine_N_Back_A", "apartment_mid", -2.8f, 27.6f, 0.75f, 0.38f),
        new("Fine_N_Back_B", "market_neighborhood", 5.3f, 27.4f, 0.70f, 0.40f),

        // Medium and narrow parcels receive one compact building each.
        new("Fine_W_Mid", "convenience_awning", -18.5f, 12.5f, 0.35f, 0.40f),
        new("Fine_C_Mid", "house_low_darkroof", -4.2f, 3.0f, 0.10f, 0.42f),
        new("Fine_NW_Small", "house_low_darkroof", -21.4f, 22.0f, 0.65f, 0.40f),
    };

    private static readonly Placement[] Props =
    {
        new("FineP_CL_Bench", "bench_back", -20.1f, 1.3f, 0.55f, 0.32f),
        new("FineP_CL_Planter", "planter_flower_round", -9.2f, 1.4f, 0.75f, 0.32f),
        new("FineP_CL_Tree", "tree_small_square", -8.2f, -5.8f, -0.45f, 0.34f),

        new("FineP_S_Tree", "tree_square", 21.0f, -18.0f, 0.55f, 0.34f),
        new("FineP_S_Bench", "bench_simple", 10.3f, -24.2f, -0.45f, 0.32f),
        new("FineP_S_Vending", "vending_blue", 16.2f, -24.0f, -0.45f, 0.31f),

        new("FineP_NW_Tree", "tree_small_square", -4.5f, 20.2f, 0.85f, 0.34f),
        new("FineP_NW_Planter", "planter_shrub", -18.8f, 14.0f, -0.05f, 0.32f),
        new("FineP_NC_Lamp", "street_lamp_curve", 10.7f, 13.0f, 0.75f, 0.27f),
        new("FineP_NC_Bench", "bench_back", 10.0f, 4.5f, -0.20f, 0.31f),

        new("FineP_N_Tree", "tree_square", 7.4f, 25.8f, 0.55f, 0.34f),
        new("FineP_N_Bin", "trash_green", -4.2f, 25.4f, 0.45f, 0.31f),

        new("FineP_E_Planter", "planter_hedge", 35.0f, 11.7f, 0.45f, 0.31f),
        new("FineP_NE_Bench", "bench_simple", 26.3f, 16.6f, 0.45f, 0.31f),
        new("FineP_C_Mailbox", "mailbox_red", -10.4f, 6.75f, 0.15f, 0.31f),
        new("FineP_SW_Tree", "tree_small_square", -16.9f, -22.1f, -0.20f, 0.33f),
        new("FineP_SE_CallBox", "call_box_bollard", 24.0f, -8.8f, -0.15f, 0.31f),
        new("FineP_SE_Bin", "trash_green", 31.9f, -11.3f, -0.15f, 0.31f),
    };

    [InitializeOnLoadMethod]
    private static void ApplyAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(MarkerPath))
            {
                return;
            }

            Apply();
        };
    }

    [MenuItem("Shadow Drop/Apply Fine QuarterView Layout")]
    public static void Apply()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        Transform buildingParent = FindTransform(scene, "ManualBuildings");
        Transform propParent = FindTransform(scene, "ManualProps");
        if (buildingParent == null || propParent == null)
        {
            Debug.LogError("Fine layout could not find ManualBuildings or ManualProps.");
            return;
        }

        RemoveGeneratedChildren(buildingParent, "Fine_");
        RemoveGeneratedChildren(propParent, "FineP_");
        RemoveExactDuplicate(buildingParent, "06_villa_walled (1)");
        NormalizeBuildingScales(buildingParent);

        InstantiatePlacements(Buildings, BuildingPrefabRoot, buildingParent);
        InstantiatePlacements(Props, PropPrefabRoot, propParent);
        ConfigureDepthSorters(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.DeleteAsset(MarkerPath);
        AssetDatabase.SaveAssets();
        Debug.Log(
            $"Applied fine QuarterView layout v{LayoutVersion}: "
            + $"{Buildings.Length} buildings and {Props.Length} props added."
        );
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

    private static void RemoveGeneratedChildren(Transform parent, string prefix)
    {
        var remove = new List<GameObject>();
        for (int index = 0; index < parent.childCount; index++)
        {
            Transform child = parent.GetChild(index);
            if (child.name.StartsWith(prefix, StringComparison.Ordinal))
            {
                remove.Add(child.gameObject);
            }
        }

        foreach (GameObject target in remove)
        {
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    private static void RemoveExactDuplicate(Transform parent, string name)
    {
        Transform duplicate = parent.Find(name);
        if (duplicate != null)
        {
            UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
        }
    }

    private static void NormalizeBuildingScales(Transform parent)
    {
        for (int index = 0; index < parent.childCount; index++)
        {
            Transform child = parent.GetChild(index);
            Vector3 scale = child.localScale;
            child.localScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        }
    }

    private static void InstantiatePlacements(
        IEnumerable<Placement> placements,
        string prefabRoot,
        Transform parent
    )
    {
        foreach (Placement placement in placements)
        {
            string path = $"{prefabRoot}/{placement.Asset}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"Skipping missing layout prefab: {path}");
                continue;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = placement.Label + "_" + placement.Asset;
            instance.transform.localPosition = placement.Position;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * placement.Scale;
        }
    }

    private static void ConfigureDepthSorters(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (QuarterViewDepthSorter2D sorter in root.GetComponentsInChildren<QuarterViewDepthSorter2D>(true))
            {
                SerializedObject serialized = new SerializedObject(sorter);
                serialized.FindProperty("sortingOffset").intValue = 10000;
                serialized.FindProperty("precision").intValue = 100;
                serialized.FindProperty("zPrecision").intValue = 1000;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
