using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ShadowDropMapSceneBuilder
{
    private const string Root = "Assets/ShadowDropMap";
    private const string TextureRoot = Root + "/Textures";
    private const string SceneRoot = Root + "/Scenes";
    private const string ScenePath = SceneRoot + "/ShadowDrop_MapPrototype.unity";
    private const float PixelsPerUnit = 128f / 7f;
    private const int MaxTextureSize = 4096;

    [InitializeOnLoadMethod]
    private static void ConfigureMapTexturesAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            ConfigureTexture(TextureRoot + "/shadowdrop_road_base.png", isMask: false, hasAlpha: false);
            ConfigureTexture(TextureRoot + "/shadowdrop_buildings.png", isMask: false, hasAlpha: true);
            ConfigureTexture(TextureRoot + "/shadowdrop_walkable_mask.png", isMask: true, hasAlpha: false);
        };
    }

    [MenuItem("Shadow Drop/Build Prototype Map Scene")]
    public static void BuildScene()
    {
        Directory.CreateDirectory(SceneRoot);

        string roadPath = TextureRoot + "/shadowdrop_road_base.png";
        string buildingsPath = TextureRoot + "/shadowdrop_buildings.png";
        string maskPath = TextureRoot + "/shadowdrop_walkable_mask.png";

        ConfigureTexture(roadPath, isMask: false, hasAlpha: false);
        ConfigureTexture(buildingsPath, isMask: false, hasAlpha: true);
        ConfigureTexture(maskPath, isMask: true, hasAlpha: false);

        AssetDatabase.Refresh();

        Sprite roadSprite = AssetDatabase.LoadAssetAtPath<Sprite>(roadPath);
        Sprite buildingsSprite = AssetDatabase.LoadAssetAtPath<Sprite>(buildingsPath);
        Texture2D walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "ShadowDrop_MapPrototype";

        GameObject root = new GameObject("ShadowDrop_Map");

        GameObject roadBase = new GameObject("RoadBase");
        roadBase.transform.SetParent(root.transform, false);
        SpriteRenderer roadRenderer = roadBase.AddComponent<SpriteRenderer>();
        roadRenderer.sprite = roadSprite;
        roadRenderer.sortingOrder = 0;

        GameObject buildings = new GameObject("Buildings");
        buildings.transform.SetParent(root.transform, false);
        SpriteRenderer buildingsRenderer = buildings.AddComponent<SpriteRenderer>();
        buildingsRenderer.sprite = buildingsSprite;
        buildingsRenderer.sortingOrder = 100;

        GameObject collision = new GameObject("CollisionFromWalkableMask");
        collision.transform.SetParent(root.transform, false);
        ShadowDropPixelMaskCollider2D maskCollider = collision.AddComponent<ShadowDropPixelMaskCollider2D>();
        SerializedObject serialized = new SerializedObject(maskCollider);
        serialized.FindProperty("walkableMask").objectReferenceValue = walkableMask;
        serialized.FindProperty("pixelsPerUnit").floatValue = PixelsPerUnit;
        serialized.FindProperty("cellSizePixels").intValue = 4;
        serialized.FindProperty("blockedThreshold").floatValue = 0.65f;
        serialized.FindProperty("boundaryOnly").boolValue = true;
        serialized.FindProperty("boundaryThicknessCells").intValue = 2;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        maskCollider.RebuildColliders();

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = roadSprite.bounds.size.y * 0.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.13f, 0.14f);
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.tag = "MainCamera";

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Shadow Drop fresh 2x prototype scene built: {ScenePath}");
        EditorUtility.DisplayDialog("Shadow Drop", $"Prototype map scene saved:\n{ScenePath}", "OK");
    }

    [MenuItem("Shadow Drop/Apply Fresh 2x Map To Current Scene")]
    public static void ApplyToCurrentScene()
    {
        string roadPath = TextureRoot + "/shadowdrop_road_base.png";
        string buildingsPath = TextureRoot + "/shadowdrop_buildings.png";
        string maskPath = TextureRoot + "/shadowdrop_walkable_mask.png";

        ConfigureTexture(roadPath, isMask: false, hasAlpha: false);
        ConfigureTexture(buildingsPath, isMask: false, hasAlpha: true);
        ConfigureTexture(maskPath, isMask: true, hasAlpha: false);
        AssetDatabase.Refresh();

        GameObject root = GameObject.Find("ShadowDrop_Map");
        if (root == null)
        {
            EditorUtility.DisplayDialog(
                "Shadow Drop",
                "ShadowDrop_Map was not found in the current scene. Use Build Prototype Map Scene first.",
                "OK"
            );
            return;
        }

        Transform roadBase = root.transform.Find("RoadBase");
        if (roadBase == null)
        {
            roadBase = root.transform.Find("BaseMap");
        }

        Transform buildings = root.transform.Find("Buildings");
        if (buildings == null)
        {
            buildings = root.transform.Find("BuildingOcclusion");
        }

        Transform collision = root.transform.Find("CollisionFromWalkableMask");
        if (roadBase == null || buildings == null || collision == null)
        {
            EditorUtility.DisplayDialog(
                "Shadow Drop",
                "The current map hierarchy is incomplete. Use Build Prototype Map Scene to rebuild it.",
                "OK"
            );
            return;
        }

        SpriteRenderer roadRenderer = roadBase.GetComponent<SpriteRenderer>();
        SpriteRenderer buildingsRenderer = buildings.GetComponent<SpriteRenderer>();
        ShadowDropPixelMaskCollider2D maskCollider = collision.GetComponent<ShadowDropPixelMaskCollider2D>();
        if (roadRenderer == null || buildingsRenderer == null || maskCollider == null)
        {
            EditorUtility.DisplayDialog(
                "Shadow Drop",
                "The current map components are incomplete. Use Build Prototype Map Scene to rebuild them.",
                "OK"
            );
            return;
        }

        Undo.RecordObjects(
            new Object[]
            {
                roadBase,
                buildings,
                collision,
                roadRenderer,
                buildingsRenderer,
                maskCollider,
            },
            "Apply Fresh Shadow Drop 2x Map"
        );

        roadBase.name = "RoadBase";
        buildings.name = "Buildings";
        roadBase.localPosition = Vector3.zero;
        roadBase.localScale = Vector3.one;
        buildings.localPosition = Vector3.zero;
        buildings.localScale = Vector3.one;
        collision.localPosition = Vector3.zero;
        collision.localScale = Vector3.one;
        roadRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(roadPath);
        buildingsRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(buildingsPath);
        roadRenderer.sortingOrder = 0;
        buildingsRenderer.sortingOrder = 100;

        SerializedObject serialized = new SerializedObject(maskCollider);
        serialized.FindProperty("walkableMask").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        serialized.FindProperty("pixelsPerUnit").floatValue = PixelsPerUnit;
        serialized.FindProperty("cellSizePixels").intValue = 4;
        serialized.FindProperty("blockedThreshold").floatValue = 0.65f;
        serialized.FindProperty("boundaryOnly").boolValue = true;
        serialized.FindProperty("boundaryThicknessCells").intValue = 2;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        maskCollider.RebuildColliders();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        SceneView.RepaintAll();
        Debug.Log("Shadow Drop fresh 2x map applied to the current scene.");
        EditorUtility.DisplayDialog(
            "Shadow Drop",
            "The current scene now uses the fresh 2x road base, aligned buildings, and matching road colliders.",
            "OK"
        );
    }

    private static void ConfigureTexture(string assetPath, bool isMask, bool hasAlpha)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Texture not found: {assetPath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.maxTextureSize = MaxTextureSize;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.mipmapEnabled = false;
        importer.isReadable = isMask;
        importer.alphaIsTransparency = hasAlpha;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = isMask ? FilterMode.Point : FilterMode.Bilinear;

        TextureImporterPlatformSettings defaultSettings =
            importer.GetDefaultPlatformTextureSettings();
        defaultSettings.maxTextureSize = MaxTextureSize;
        defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SetPlatformTextureSettings(defaultSettings);

        TextureImporterPlatformSettings standaloneSettings =
            importer.GetPlatformTextureSettings("Standalone");
        standaloneSettings.name = "Standalone";
        standaloneSettings.overridden = true;
        standaloneSettings.maxTextureSize = MaxTextureSize;
        standaloneSettings.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SetPlatformTextureSettings(standaloneSettings);

        importer.SaveAndReimport();
    }
}
