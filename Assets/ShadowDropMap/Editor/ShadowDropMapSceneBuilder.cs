using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
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
            ConfigureTexture(TextureRoot + "/shadowdrop_map_base.png", isMask: false, hasAlpha: false);
            ConfigureTexture(TextureRoot + "/shadowdrop_occlusion_layer.png", isMask: false, hasAlpha: true);
            ConfigureTexture(TextureRoot + "/shadowdrop_walkable_mask.png", isMask: true, hasAlpha: false);
        };
    }

    [MenuItem("Shadow Drop/Build Prototype Map Scene")]
    public static void BuildScene()
    {
        Directory.CreateDirectory(SceneRoot);

        string basePath = TextureRoot + "/shadowdrop_map_base.png";
        string occlusionPath = TextureRoot + "/shadowdrop_occlusion_layer.png";
        string maskPath = TextureRoot + "/shadowdrop_walkable_mask.png";

        ConfigureTexture(basePath, isMask: false, hasAlpha: false);
        ConfigureTexture(occlusionPath, isMask: false, hasAlpha: true);
        ConfigureTexture(maskPath, isMask: true, hasAlpha: false);

        AssetDatabase.Refresh();

        Sprite baseSprite = AssetDatabase.LoadAssetAtPath<Sprite>(basePath);
        Sprite occlusionSprite = AssetDatabase.LoadAssetAtPath<Sprite>(occlusionPath);
        Texture2D walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "ShadowDrop_MapPrototype";

        GameObject root = new GameObject("ShadowDrop_Map");

        GameObject baseMap = new GameObject("BaseMap");
        baseMap.transform.SetParent(root.transform, false);
        SpriteRenderer baseRenderer = baseMap.AddComponent<SpriteRenderer>();
        baseRenderer.sprite = baseSprite;
        baseRenderer.sortingOrder = 0;

        GameObject occlusion = new GameObject("BuildingOcclusion");
        occlusion.transform.SetParent(root.transform, false);
        SpriteRenderer occlusionRenderer = occlusion.AddComponent<SpriteRenderer>();
        occlusionRenderer.sprite = occlusionSprite;
        occlusionRenderer.sortingOrder = 100;

        GameObject collision = new GameObject("CollisionFromWalkableMask");
        collision.transform.SetParent(root.transform, false);
        ShadowDropPixelMaskCollider2D maskCollider = collision.AddComponent<ShadowDropPixelMaskCollider2D>();
        SerializedObject serialized = new SerializedObject(maskCollider);
        serialized.FindProperty("walkableMask").objectReferenceValue = walkableMask;
        serialized.FindProperty("pixelsPerUnit").floatValue = PixelsPerUnit;
        serialized.FindProperty("cellSizePixels").intValue = 4;
        serialized.FindProperty("blockedThreshold").floatValue = 0.65f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        maskCollider.RebuildColliders();

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = baseSprite.bounds.size.y * 0.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.13f, 0.14f);
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.tag = "MainCamera";

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Shadow Drop", $"Prototype map scene saved:\n{ScenePath}", "OK");
    }

    [MenuItem("Shadow Drop/Apply 2x Map To Current Scene")]
    public static void ApplyToCurrentScene()
    {
        string basePath = TextureRoot + "/shadowdrop_map_base.png";
        string occlusionPath = TextureRoot + "/shadowdrop_occlusion_layer.png";
        string maskPath = TextureRoot + "/shadowdrop_walkable_mask.png";

        ConfigureTexture(basePath, isMask: false, hasAlpha: false);
        ConfigureTexture(occlusionPath, isMask: false, hasAlpha: true);
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

        Transform baseMap = root.transform.Find("BaseMap");
        Transform occlusion = root.transform.Find("BuildingOcclusion");
        Transform collision = root.transform.Find("CollisionFromWalkableMask");
        if (baseMap == null || occlusion == null || collision == null)
        {
            EditorUtility.DisplayDialog(
                "Shadow Drop",
                "The current map hierarchy is incomplete. Use Build Prototype Map Scene to rebuild it.",
                "OK"
            );
            return;
        }

        SpriteRenderer baseRenderer = baseMap.GetComponent<SpriteRenderer>();
        SpriteRenderer occlusionRenderer = occlusion.GetComponent<SpriteRenderer>();
        ShadowDropPixelMaskCollider2D maskCollider = collision.GetComponent<ShadowDropPixelMaskCollider2D>();
        if (baseRenderer == null || occlusionRenderer == null || maskCollider == null)
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
                baseMap,
                occlusion,
                collision,
                baseRenderer,
                occlusionRenderer,
                maskCollider,
            },
            "Apply Shadow Drop 2x Map"
        );

        baseMap.localPosition = Vector3.zero;
        baseMap.localScale = Vector3.one;
        occlusion.localPosition = Vector3.zero;
        occlusion.localScale = Vector3.one;
        collision.localPosition = Vector3.zero;
        collision.localScale = Vector3.one;
        baseRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(basePath);
        occlusionRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(occlusionPath);

        SerializedObject serialized = new SerializedObject(maskCollider);
        serialized.FindProperty("walkableMask").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        serialized.FindProperty("pixelsPerUnit").floatValue = PixelsPerUnit;
        serialized.FindProperty("cellSizePixels").intValue = 4;
        serialized.FindProperty("blockedThreshold").floatValue = 0.65f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        maskCollider.RebuildColliders();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        SceneView.RepaintAll();
        EditorUtility.DisplayDialog(
            "Shadow Drop",
            "The current scene now uses the 2x map, rebuilt building mask, and matching colliders.",
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
        EnsureFullTextureSprite(importer, assetPath);
    }

    private static void EnsureFullTextureSprite(TextureImporter importer, string assetPath)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (texture == null)
        {
            return;
        }

        SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider dataProvider =
            factories.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        SpriteRect[] spriteRects = dataProvider.GetSpriteRects();
        if (
            spriteRects.Length == 1
            && spriteRects[0].rect == new Rect(0f, 0f, texture.width, texture.height)
        )
        {
            return;
        }

        SpriteRect fullRect = spriteRects.Length > 0
            ? spriteRects[0]
            : new SpriteRect { spriteID = GUID.Generate() };
        fullRect.name = Path.GetFileNameWithoutExtension(assetPath);
        fullRect.rect = new Rect(0f, 0f, texture.width, texture.height);
        fullRect.alignment = SpriteAlignment.Center;
        fullRect.pivot = new Vector2(0.5f, 0.5f);
        fullRect.border = Vector4.zero;
        dataProvider.SetSpriteRects(new[] { fullRect });

        ISpriteNameFileIdDataProvider nameProvider =
            dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameProvider.SetNameFileIdPairs(
            new[] { new SpriteNameFileIdPair(fullRect.name, fullRect.spriteID) }
        );
        dataProvider.Apply();
        importer.SaveAndReimport();
    }
}
