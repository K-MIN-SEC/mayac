using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuarterViewV2SceneBuilder
{
    private const string Root = "Assets/ShadowDropMapV2";
    private const string TextureRoot = Root + "/Textures";
    private const string BuildingRoot = Root + "/Buildings";
    private const string PropRoot = Root + "/Props";
    private const string PrefabRoot = Root + "/Prefabs";
    private const string BuildingPrefabRoot = PrefabRoot + "/Buildings";
    private const string PropPrefabRoot = PrefabRoot + "/Props";
    private const string SceneRoot = Root + "/Scenes";
    private const string ScenePath = SceneRoot + "/QuarterViewV2.unity";
    private const string MetadataPath = Root + "/quarterview_v2_metadata.json";
    private const string BuildOnceMarker = Root + "/build_quarterview_v2_once.txt";
    private const float MapPixelsPerUnit = 32f;
    private const float PlayerPixelsPerUnit = 64f;
    private const float Upscale = 2f;
    private const float UpscaledWidth = 2896f;
    private const float UpscaledHeight = 2172f;

    [Serializable]
    private sealed class Metadata
    {
        public AssetDefinition[] buildings;
        public AssetDefinition[] props;
        public Placement[] placements;
        public Placement[] propPlacements;
    }

    [Serializable]
    private sealed class AssetDefinition
    {
        public string name;
    }

    [Serializable]
    private sealed class Placement
    {
        public string name;
        public float[] sourcePosition;
        public float scale;
    }

    [InitializeOnLoadMethod]
    private static void BuildRequestedSceneAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(BuildOnceMarker))
            {
                return;
            }

            if (!BuildSceneInternal())
            {
                return;
            }

            AssetDatabase.DeleteAsset(BuildOnceMarker);
            Debug.Log("Built and saved the QuarterViewV2 touch-navigation prototype once.");
        };
    }

    [MenuItem("Shadow Drop/Build QuarterView V2 Prototype")]
    public static void BuildScene()
    {
        if (BuildSceneInternal())
        {
            EditorUtility.DisplayDialog(
                "Shadow Drop",
                "QuarterViewV2 scene created with touch A* navigation and building occlusion.",
                "OK"
            );
        }
    }

    public static void BuildSceneBatch()
    {
        if (!BuildSceneInternal())
        {
            throw new InvalidOperationException("QuarterViewV2 scene build failed.");
        }

        AssetDatabase.DeleteAsset(BuildOnceMarker);
        Debug.Log("Built and saved the QuarterViewV2 prototype in batch mode.");
    }

    public static void VerifyNavigationBatch()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        QuarterViewWalkableNavigator2D navigator =
            UnityEngine.Object.FindFirstObjectByType<QuarterViewWalkableNavigator2D>();
        if (navigator == null)
        {
            throw new InvalidOperationException("QuarterViewV2 navigator was not found in the scene.");
        }

        MethodInfo awake = typeof(QuarterViewWalkableNavigator2D).GetMethod(
            "Awake",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        FieldInfo pathField = typeof(QuarterViewWalkableNavigator2D).GetField(
            "path",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        if (awake == null || pathField == null)
        {
            throw new InvalidOperationException("QuarterViewV2 navigation internals changed unexpectedly.");
        }

        awake.Invoke(navigator, null);
        bool accepted = navigator.SetDestination(new Vector2(-17.125f, -13.5625f));
        List<Vector2> generatedPath = pathField.GetValue(navigator) as List<Vector2>;
        int waypointCount = generatedPath?.Count ?? 0;
        if (!accepted || waypointCount < 2)
        {
            throw new InvalidOperationException(
                $"QuarterViewV2 navigation smoke test failed. accepted={accepted}, waypoints={waypointCount}"
            );
        }

        Debug.Log($"QuarterViewV2 navigation smoke test passed with {waypointCount} waypoints.");
    }

    private static bool BuildSceneInternal()
    {
        Directory.CreateDirectory(SceneRoot);
        TextAsset metadataAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MetadataPath);
        if (metadataAsset == null)
        {
            Debug.LogError($"QuarterViewV2 metadata not found: {MetadataPath}");
            return false;
        }

        Metadata metadata = JsonUtility.FromJson<Metadata>(metadataAsset.text);
        if (metadata?.buildings == null || metadata.buildings.Length == 0)
        {
            Debug.LogError("QuarterViewV2 metadata contains no building assets.");
            return false;
        }

        if (metadata.props == null)
        {
            metadata.props = Array.Empty<AssetDefinition>();
        }

        if (metadata.placements == null)
        {
            metadata.placements = Array.Empty<Placement>();
        }

        if (metadata.propPlacements == null)
        {
            metadata.propPlacements = Array.Empty<Placement>();
        }

        ConfigureSprite(TextureRoot + "/quarterview_map_background.png", MapPixelsPerUnit, new Vector2(0.5f, 0.5f), 4096);
        ConfigureMask(TextureRoot + "/quarterview_walkable_mask.png");
        ConfigureSprite(TextureRoot + "/quarterview_player_placeholder.png", PlayerPixelsPerUnit, new Vector2(0.5f, 0f), 512);
        foreach (AssetDefinition building in metadata.buildings)
        {
            ConfigureSprite($"{BuildingRoot}/{building.name}.png", MapPixelsPerUnit, new Vector2(0.5f, 0f), 2048);
        }
        foreach (AssetDefinition prop in metadata.props)
        {
            ConfigureSprite($"{PropRoot}/{prop.name}.png", MapPixelsPerUnit, new Vector2(0.5f, 0f), 2048);
        }

        AssetDatabase.Refresh();
        CreateManualPlacementPrefabs(metadata);

        Sprite mapSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TextureRoot + "/quarterview_map_background.png");
        Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TextureRoot + "/quarterview_player_placeholder.png");
        Texture2D walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureRoot + "/quarterview_walkable_mask.png");
        if (mapSprite == null || playerSprite == null || walkableMask == null)
        {
            Debug.LogError("QuarterViewV2 core textures failed to import.");
            return false;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "QuarterViewV2";

        GameObject root = new GameObject("QuarterViewV2_Map");

        GameObject mapObject = new GameObject("MapBackground");
        mapObject.transform.SetParent(root.transform, false);
        SpriteRenderer mapRenderer = mapObject.AddComponent<SpriteRenderer>();
        mapRenderer.sprite = mapSprite;
        mapRenderer.sortingOrder = 0;

        GameObject walkableArea = new GameObject("WalkableArea_AStarMask");
        walkableArea.transform.SetParent(root.transform, false);

        GameObject player = new GameObject("Player_TouchMove");
        player.transform.SetParent(root.transform, false);
        player.transform.localPosition = SourcePixelToWorld(680f, 600f);
        player.tag = "Player";
        SpriteRenderer playerRenderer = player.AddComponent<SpriteRenderer>();
        playerRenderer.sprite = playerSprite;
        playerRenderer.sortingOrder = 10000;

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D playerCollider = player.AddComponent<CircleCollider2D>();
        playerCollider.radius = 0.28f;
        playerCollider.offset = new Vector2(0f, 0.28f);

        QuarterViewDepthSorter2D playerSorter = player.AddComponent<QuarterViewDepthSorter2D>();
        ConfigureDepthSorter(playerSorter);

        GameObject buildings = new GameObject("ManualBuildings");
        buildings.transform.SetParent(root.transform, false);
        for (int index = 0; index < metadata.placements.Length; index++)
        {
            Placement placement = metadata.placements[index];
            if (placement.sourcePosition == null || placement.sourcePosition.Length < 2)
            {
                continue;
            }

            Sprite buildingSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{BuildingRoot}/{placement.name}.png");
            if (buildingSprite == null)
            {
                Debug.LogWarning($"Skipping missing building sprite: {placement.name}");
                continue;
            }

            GameObject building = new GameObject($"{index + 1:00}_{placement.name}");
            building.transform.SetParent(buildings.transform, false);
            building.transform.localPosition = SourcePixelToWorld(
                placement.sourcePosition[0],
                placement.sourcePosition[1]
            );
            building.transform.localScale = Vector3.one * placement.scale;

            SpriteRenderer renderer = building.AddComponent<SpriteRenderer>();
            renderer.sprite = buildingSprite;
            renderer.sortingOrder = 10000;

            QuarterViewDepthSorter2D sorter = building.AddComponent<QuarterViewDepthSorter2D>();
            ConfigureDepthSorter(sorter);

            QuarterViewBuildingOccluder2D occluder = building.AddComponent<QuarterViewBuildingOccluder2D>();
            SerializedObject serializedOccluder = new SerializedObject(occluder);
            serializedOccluder.FindProperty("target").objectReferenceValue = player.transform;
            serializedOccluder.ApplyModifiedPropertiesWithoutUndo();
        }

        GameObject props = new GameObject("ManualProps");
        props.transform.SetParent(root.transform, false);
        for (int index = 0; index < metadata.propPlacements.Length; index++)
        {
            Placement placement = metadata.propPlacements[index];
            if (placement.sourcePosition == null || placement.sourcePosition.Length < 2)
            {
                continue;
            }

            Sprite propSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{PropRoot}/{placement.name}.png");
            if (propSprite == null)
            {
                Debug.LogWarning($"Skipping missing prop sprite: {placement.name}");
                continue;
            }

            GameObject prop = new GameObject($"{index + 1:00}_{placement.name}");
            prop.transform.SetParent(props.transform, false);
            prop.transform.localPosition = SourcePixelToWorld(
                placement.sourcePosition[0],
                placement.sourcePosition[1]
            );
            prop.transform.localScale = Vector3.one * placement.scale;

            SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = propSprite;
            renderer.sortingOrder = 10000;

            QuarterViewDepthSorter2D sorter = prop.AddComponent<QuarterViewDepthSorter2D>();
            ConfigureDepthSorter(sorter);
        }

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 13.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.10f);
        cameraObject.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
        cameraObject.tag = "MainCamera";

        QuarterViewCameraFollow2D follow = cameraObject.AddComponent<QuarterViewCameraFollow2D>();
        SerializedObject serializedFollow = new SerializedObject(follow);
        serializedFollow.FindProperty("target").objectReferenceValue = player.transform;
        serializedFollow.FindProperty("mapRenderer").objectReferenceValue = mapRenderer;
        serializedFollow.ApplyModifiedPropertiesWithoutUndo();

        QuarterViewWalkableNavigator2D navigator = player.AddComponent<QuarterViewWalkableNavigator2D>();
        SerializedObject serializedNavigator = new SerializedObject(navigator);
        serializedNavigator.FindProperty("walkableMask").objectReferenceValue = walkableMask;
        serializedNavigator.FindProperty("mapTransform").objectReferenceValue = mapObject.transform;
        serializedNavigator.FindProperty("inputCamera").objectReferenceValue = camera;
        serializedNavigator.FindProperty("characterRenderer").objectReferenceValue = playerRenderer;
        serializedNavigator.FindProperty("pixelsPerUnit").floatValue = MapPixelsPerUnit;
        serializedNavigator.FindProperty("pathCellPixels").intValue = 16;
        serializedNavigator.FindProperty("moveSpeed").floatValue = 5f;
        serializedNavigator.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = player;
        SceneView.lastActiveSceneView?.FrameSelected();
        SceneView.RepaintAll();
        Debug.Log(
            $"QuarterViewV2 manual-placement scene built with {metadata.buildings.Length} building prefabs "
            + $", {metadata.props.Length} prop prefabs, "
            + $"{metadata.placements.Length} buildings, and {metadata.propPlacements.Length} props: {ScenePath}"
        );
        return true;
    }

    private static void CreateManualPlacementPrefabs(Metadata metadata)
    {
        Directory.CreateDirectory(BuildingPrefabRoot);
        Directory.CreateDirectory(PropPrefabRoot);
        AssetDatabase.Refresh();

        foreach (AssetDefinition building in metadata.buildings)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{BuildingRoot}/{building.name}.png");
            if (sprite == null)
            {
                Debug.LogWarning($"Skipping missing building sprite: {building.name}");
                continue;
            }

            GameObject instance = CreateSpriteObject(building.name, sprite, true);
            PrefabUtility.SaveAsPrefabAsset(instance, $"{BuildingPrefabRoot}/{building.name}.prefab");
            UnityEngine.Object.DestroyImmediate(instance);
        }

        foreach (AssetDefinition prop in metadata.props)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{PropRoot}/{prop.name}.png");
            if (sprite == null)
            {
                Debug.LogWarning($"Skipping missing prop sprite: {prop.name}");
                continue;
            }

            GameObject instance = CreateSpriteObject(prop.name, sprite, false);
            PrefabUtility.SaveAsPrefabAsset(instance, $"{PropPrefabRoot}/{prop.name}.prefab");
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private static GameObject CreateSpriteObject(string name, Sprite sprite, bool fadesWhenOccluding)
    {
        GameObject instance = new GameObject(name);
        SpriteRenderer renderer = instance.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 10000;

        QuarterViewDepthSorter2D sorter = instance.AddComponent<QuarterViewDepthSorter2D>();
        ConfigureDepthSorter(sorter);
        if (fadesWhenOccluding)
        {
            instance.AddComponent<QuarterViewBuildingOccluder2D>();
        }

        return instance;
    }

    private static void ConfigureDepthSorter(QuarterViewDepthSorter2D sorter)
    {
        SerializedObject serialized = new SerializedObject(sorter);
        serialized.FindProperty("sortingOffset").intValue = 10000;
        serialized.FindProperty("precision").intValue = 100;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Vector3 SourcePixelToWorld(float sourceX, float sourceY)
    {
        float x = (sourceX * Upscale - UpscaledWidth * 0.5f) / MapPixelsPerUnit;
        float y = (UpscaledHeight * 0.5f - sourceY * Upscale) / MapPixelsPerUnit;
        return new Vector3(x, y, 0f);
    }

    private static void ConfigureSprite(
        string assetPath,
        float pixelsPerUnit,
        Vector2 pivot,
        int maxTextureSize
    )
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Sprite texture not found: {assetPath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = maxTextureSize;
        importer.npotScale = TextureImporterNPOTScale.None;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = pivot;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static void ConfigureMask(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Walkable mask not found: {assetPath}");
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();
    }

    private static void AddSceneToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(item => item.path == ScenePath))
        {
            return;
        }

        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
