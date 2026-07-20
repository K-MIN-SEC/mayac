using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuarterViewV2AdditionalBuildingPass
{
    private const string Root = "Assets/ShadowDropMapV2";
    private const string ScenePath = Root + "/Scenes/QuarterViewV2.unity";
    private const string BuildingRoot = Root + "/Buildings";
    private const string BuildingPrefabRoot = Root + "/Prefabs/Buildings";
    private const string PropPrefabRoot = Root + "/Prefabs/Props";
    private const string MapTexturePath = Root + "/Textures/quarterview_map_background.png";
    private const string MarkerPath = Root + "/apply_additional_buildings_once.txt";
    private const float PixelsPerUnit = 32f;

    private readonly struct Replacement
    {
        public Replacement(string target, string asset)
        {
            Target = target;
            Asset = asset;
        }

        public string Target { get; }
        public string Asset { get; }
    }

    private static readonly string[] NewBuildings =
    {
        "house_brick_darkroof",
        "villa_brick_balcony",
        "apartment_brick_narrow",
        "tailor_brick",
        "apartment_brick_balconies",
        "villa_concrete_balcony",
        "house_brick_stairs",
        "clinic_brick",
        "restaurant_brick",
        "apartment_brick_plain",
        "mixed_concrete_corner",
        "house_brick_gable",
    };

    private static readonly Replacement[] Replacements =
    {
        new("Fine_NW_Small_house_low_darkroof (1)", "house_brick_darkroof"),
        new("Fine_NW_Small_house_low_darkroof (2)", "house_brick_stairs"),
        new("Fine_NW_Small_house_low_darkroof (3)", "villa_brick_balcony"),
        new("Fine_NW_Small_house_low_darkroof (4)", "house_brick_gable"),
        new("Fine_NW_Small_house_low_darkroof (5)", "villa_concrete_balcony"),
        new("Fine_S_Back_A_house_low_darkroof (1)", "mixed_concrete_corner"),
        new("Fine_S_Back_A_house_low_darkroof (2)", "restaurant_brick"),
        new("Fine_C_Mid_house_low_darkroof (1)", "tailor_brick"),
        new("Fine_N_Back_A_apartment_mid", "apartment_brick_balconies"),
        new("Fine_CL_Back_A_apartment_mid", "apartment_brick_plain"),
        new("Fine_S_Back_B_apartment_mid", "apartment_brick_narrow"),
        new("Fine_CL_Back_B_villa_walled", "villa_brick_balcony"),
        new("Fine_NW_Back_A_villa_walled", "house_brick_gable"),
        new("Fine_CL_Front_market_neighborhood", "tailor_brick"),
        new("Fine_S_Front_B_market_neighborhood", "clinic_brick"),
        new("20_apartment_mid (1)", "apartment_brick_balconies"),
        new("20_apartment_mid (2)", "apartment_brick_plain"),
        new("21_villa_walled (1)", "villa_concrete_balcony"),
        new("21_villa_walled (2)", "house_brick_stairs"),
        new("16_apartment_red_tall (1)", "apartment_brick_narrow"),
        new("18_apartment_gray_tall (1)", "clinic_brick"),
        new("10_house_red_tile (3)", "house_brick_darkroof"),
    };

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

    [MenuItem("Shadow Drop/Apply Additional Buildings And Footprints")]
    public static void Apply()
    {
        ConfigureMapTexture();
        ConfigureNewBuildingSprites();
        AssetDatabase.Refresh();
        CreateNewBuildingPrefabs();
        ConfigurePrefabFootprints(BuildingPrefabRoot, true);
        ConfigurePrefabFootprints(PropPrefabRoot, false);

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        Transform buildingParent = FindTransform(scene, "ManualBuildings");
        Transform propParent = FindTransform(scene, "ManualProps");
        if (buildingParent == null || propParent == null)
        {
            Debug.LogError("Additional building pass could not find the placement roots.");
            return;
        }

        int replacementCount = ReplaceRepeatedBuildings(buildingParent);
        int buildingFootprints = ConfigureSceneFootprints(buildingParent, true);
        int propFootprints = ConfigureSceneFootprints(propParent, false);
        ConfigurePlayerAndNavigator(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.DeleteAsset(MarkerPath);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        Debug.Log(
            $"Additional building pass complete: {replacementCount} replacements, "
            + $"{buildingFootprints} building footprints, {propFootprints} prop footprints."
        );
    }

    private static void ConfigureMapTexture()
    {
        TextureImporter importer = AssetImporter.GetAtPath(MapTexturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"Map texture was not found: {MapTexturePath}");
            return;
        }

        importer.isReadable = true;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();
    }

    private static void ConfigureNewBuildingSprites()
    {
        foreach (string name in NewBuildings)
        {
            string path = $"{BuildingRoot}/{name}.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"New building texture was not found: {path}");
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 0f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }

    private static void CreateNewBuildingPrefabs()
    {
        Directory.CreateDirectory(BuildingPrefabRoot);
        foreach (string name in NewBuildings)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{BuildingRoot}/{name}.png");
            if (sprite == null)
            {
                continue;
            }

            GameObject instance = new GameObject(name);
            SpriteRenderer renderer = instance.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 10000;

            QuarterViewDepthSorter2D sorter = instance.AddComponent<QuarterViewDepthSorter2D>();
            SerializedObject serializedSorter = new SerializedObject(sorter);
            serializedSorter.FindProperty("sortingOffset").intValue = 10000;
            serializedSorter.FindProperty("precision").intValue = 100;
            serializedSorter.FindProperty("zPrecision").intValue = 1000;
            serializedSorter.ApplyModifiedPropertiesWithoutUndo();

            instance.AddComponent<QuarterViewBuildingOccluder2D>();
            AddOrUpdateFootprint(instance, true);
            PrefabUtility.SaveAsPrefabAsset(instance, $"{BuildingPrefabRoot}/{name}.prefab");
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private static void ConfigurePrefabFootprints(string prefabRoot, bool building)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { prefabRoot }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (contents.GetComponent<SpriteRenderer>() == null)
                {
                    continue;
                }

                AddOrUpdateFootprint(contents, building);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
    }

    private static int ReplaceRepeatedBuildings(Transform root)
    {
        int replaced = 0;
        foreach (Replacement replacement in Replacements)
        {
            Transform target = FindTransform(root, replacement.Target);
            if (target == null)
            {
                continue;
            }

            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                $"{BuildingRoot}/{replacement.Asset}.png"
            );
            if (renderer == null || sprite == null)
            {
                continue;
            }

            string oldAsset = renderer.sprite != null ? renderer.sprite.name : string.Empty;
            renderer.sprite = sprite;
            if (!string.IsNullOrEmpty(oldAsset) && target.name.Contains(oldAsset))
            {
                target.name = target.name.Replace(oldAsset, replacement.Asset);
            }
            else
            {
                target.name = replacement.Asset + "_replaced";
            }

            replaced++;
        }

        return replaced;
    }

    private static int ConfigureSceneFootprints(Transform parent, bool building)
    {
        int count = 0;
        for (int index = 0; index < parent.childCount; index++)
        {
            GameObject child = parent.GetChild(index).gameObject;
            if (child.GetComponent<SpriteRenderer>() == null)
            {
                continue;
            }

            AddOrUpdateFootprint(child, building);
            count++;
        }

        return count;
    }

    private static void AddOrUpdateFootprint(GameObject root, bool building)
    {
        QuarterViewFootprintEditorUtility.AddOrUpdateFootprint(root, building);
    }

    private static float GetPropWidthRatio(string assetName)
    {
        if (ContainsAny(assetName, "pole", "lamp", "signal", "sign", "bollard", "call_box", "hydrant"))
        {
            return 0.34f;
        }

        if (ContainsAny(assetName, "tree", "cypress", "planter"))
        {
            return 0.58f;
        }

        if (ContainsAny(assetName, "bench", "shelter", "guardrail", "recycle"))
        {
            return 0.72f;
        }

        if (ContainsAny(assetName, "vending", "utility_box", "mailbox", "trash"))
        {
            return 0.56f;
        }

        return 0.52f;
    }

    private static bool ContainsAny(string value, params string[] fragments)
    {
        foreach (string fragment in fragments)
        {
            if (value.Contains(fragment))
            {
                return true;
            }
        }

        return false;
    }

    private static void ConfigurePlayerAndNavigator(Scene scene)
    {
        QuarterViewWalkableNavigator2D navigator = null;
        Rigidbody2D playerBody = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (navigator == null)
            {
                navigator = root.GetComponentInChildren<QuarterViewWalkableNavigator2D>(true);
            }

            if (playerBody == null)
            {
                Transform player = FindTransform(root.transform, "Player_TouchMove");
                if (player != null)
                {
                    playerBody = player.GetComponent<Rigidbody2D>();
                }
            }
        }

        if (playerBody != null)
        {
            playerBody.useFullKinematicContacts = true;
        }

        if (navigator == null)
        {
            Debug.LogWarning("Quarter-view navigator was not found.");
            return;
        }

        Texture2D mapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(MapTexturePath);
        SerializedObject serialized = new SerializedObject(navigator);
        serialized.FindProperty("walkableMask").objectReferenceValue = mapTexture;
        serialized.FindProperty("pathCellPixels").intValue = 8;
        serialized.FindProperty("mapAlphaCoverage").floatValue = 0.12f;
        serialized.FindProperty("obstaclePadding").floatValue = 0.12f;
        serialized.FindProperty("destinationSnapRadiusCells").intValue = 16;
        serialized.ApplyModifiedPropertiesWithoutUndo();
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
