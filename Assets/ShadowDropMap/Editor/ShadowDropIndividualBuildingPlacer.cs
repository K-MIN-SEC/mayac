using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ShadowDropIndividualBuildingPlacer
{
    private const string SpriteRoot = "Assets/ShadowDropMap/IndividualSprites";
    private const float PixelsPerUnit = 128f / 7f;
    private const float Upscale = 2f;
    private const float UpscaledWidth = 2896f;
    private const float UpscaledHeight = 2172f;
    private const int SortingOffset = 10000;
    private const string PlaceOnceMarker = SpriteRoot + "/place_example_once.txt";

    private readonly struct Placement
    {
        public Placement(string asset, int sourceX, int sourceY, float scale)
        {
            Asset = asset;
            SourceX = sourceX;
            SourceY = sourceY;
            Scale = scale;
        }

        public string Asset { get; }
        public int SourceX { get; }
        public int SourceY { get; }
        public float Scale { get; }
    }

    private static readonly Placement[] ExamplePlacements =
    {
        new Placement("building_01_red_brick", 185, 485, 0.56f),
        new Placement("building_02_beige_balcony", 565, 395, 0.54f),
        new Placement("building_03_white_pilotis", 955, 390, 0.54f),
        new Placement("building_04_corner_store", 1250, 350, 0.52f),
        new Placement("building_05_small_red_house", 680, 575, 0.62f),
        new Placement("tree_01_round", 350, 720, 0.35f),
        new Placement("tree_02_large", 1060, 700, 0.33f),
    };

    [InitializeOnLoadMethod]
    private static void PlaceRequestedExampleAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(PlaceOnceMarker) == null)
            {
                return;
            }

            if (!PlaceExampleInternal(showDialog: false))
            {
                return;
            }

            AssetDatabase.DeleteAsset(PlaceOnceMarker);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("Placed and saved the requested individual building example once.");
        };
    }

    [MenuItem("Shadow Drop/Place Individual Building Example")]
    public static void PlaceExample()
    {
        PlaceExampleInternal(showDialog: true);
    }

    private static bool PlaceExampleInternal(bool showDialog)
    {
        ConfigureSprites();
        AssetDatabase.Refresh();

        GameObject mapRoot = GameObject.Find("ShadowDrop_Map");
        if (mapRoot == null)
        {
            EditorUtility.DisplayDialog(
                "Shadow Drop",
                "ShadowDrop_Map was not found in the current scene.",
                "OK"
            );
            return false;
        }

        Transform oldExample = mapRoot.transform.Find("IndividualBuildings_Example");
        if (oldExample != null)
        {
            Undo.DestroyObjectImmediate(oldExample.gameObject);
        }

        Transform legacyBuildings = mapRoot.transform.Find("Buildings");
        if (legacyBuildings == null)
        {
            legacyBuildings = mapRoot.transform.Find("LegacyBuildingSheet_Disabled");
        }

        if (legacyBuildings != null)
        {
            Undo.RecordObject(legacyBuildings.gameObject, "Disable Legacy Building Sheet");
            legacyBuildings.name = "LegacyBuildingSheet_Disabled";
            SpriteRenderer legacyRenderer = legacyBuildings.GetComponent<SpriteRenderer>();
            if (legacyRenderer != null)
            {
                Undo.RecordObject(legacyRenderer, "Disable Legacy Building Sheet");
                legacyRenderer.enabled = false;
            }
        }

        GameObject container = new GameObject("IndividualBuildings_Example");
        Undo.RegisterCreatedObjectUndo(container, "Place Individual Building Example");
        container.transform.SetParent(mapRoot.transform, false);

        for (int index = 0; index < ExamplePlacements.Length; index++)
        {
            Placement placement = ExamplePlacements[index];
            string assetPath = $"{SpriteRoot}/{placement.Asset}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                Debug.LogError($"Individual map sprite not found: {assetPath}");
                continue;
            }

            GameObject item = new GameObject($"{index + 1:00}_{placement.Asset}");
            item.transform.SetParent(container.transform, false);
            item.transform.localPosition = SourcePixelToWorld(placement.SourceX, placement.SourceY);
            item.transform.localScale = Vector3.one * placement.Scale;

            SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = SortingOffset - Mathf.RoundToInt(item.transform.position.y * 100f);

            ShadowDropSpriteYSorter sorter = item.AddComponent<ShadowDropSpriteYSorter>();
            SerializedObject serializedSorter = new SerializedObject(sorter);
            serializedSorter.FindProperty("sortingOffset").intValue = SortingOffset;
            serializedSorter.FindProperty("precision").intValue = 100;
            serializedSorter.ApplyModifiedPropertiesWithoutUndo();
        }

        Selection.activeGameObject = container;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        SceneView.RepaintAll();
        Debug.Log("Placed the Shadow Drop individual building example.");
        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "Shadow Drop",
                "Placed five individual buildings and two trees. The legacy building sheet is disabled.",
                "OK"
            );
        }

        return true;
    }

    private static Vector3 SourcePixelToWorld(int sourceX, int sourceY)
    {
        float upscaledX = sourceX * Upscale;
        float upscaledY = sourceY * Upscale;
        float worldX = (upscaledX - UpscaledWidth * 0.5f) / PixelsPerUnit;
        float worldY = (UpscaledHeight * 0.5f - upscaledY) / PixelsPerUnit;
        return new Vector3(worldX, worldY, 0f);
    }

    private static void ConfigureSprites()
    {
        foreach (Placement placement in ExamplePlacements)
        {
            string assetPath = $"{SpriteRoot}/{placement.Asset}.png";
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"Individual map texture not found: {assetPath}");
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

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 0f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
