using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ShadowDropOriginalBuildingZonePlacer
{
    private const string AssetRoot = "Assets/ShadowDropMap/BuildingZonesOriginal";
    private const string MetadataPath = AssetRoot + "/building_zones_metadata.json";
    private const string ScenePath = "Assets/ShadowDropMap/Scenes/ShadowDrop_MapPrototype.unity";
    private const string PlaceOnceMarker = AssetRoot + "/place_original_zones_once.txt";
    private const float PixelsPerUnit = 128f / 7f;
    private const float Upscale = 2f;
    private const float UpscaledWidth = 2896f;
    private const float UpscaledHeight = 2172f;
    private const int SortingOrder = 100;

    [Serializable]
    private sealed class BuildingZoneMetadata
    {
        public BuildingZone[] zones;
    }

    [Serializable]
    private sealed class BuildingZone
    {
        public string name;
        public float[] mapPositionSourcePixels;
    }

    [InitializeOnLoadMethod]
    private static void PlaceRequestedZonesAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(PlaceOnceMarker) == null)
            {
                return;
            }

            if (!PlaceZonesInternal(showDialog: false))
            {
                return;
            }

            AssetDatabase.DeleteAsset(PlaceOnceMarker);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("Placed and saved the final original Shadow Drop building zones once.");
        };
    }

    [MenuItem("Shadow Drop/Place Original Building Zones")]
    public static void PlaceZones()
    {
        PlaceZonesInternal(showDialog: true);
    }

    private static bool PlaceZonesInternal(bool showDialog)
    {
        TextAsset metadataAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MetadataPath);
        if (metadataAsset == null)
        {
            Debug.LogError($"Building-zone metadata not found: {MetadataPath}");
            return false;
        }

        BuildingZoneMetadata metadata = JsonUtility.FromJson<BuildingZoneMetadata>(metadataAsset.text);
        if (metadata?.zones == null || metadata.zones.Length == 0)
        {
            Debug.LogError("Building-zone metadata contains no zones.");
            return false;
        }

        ConfigureSprites(metadata);
        AssetDatabase.Refresh();

        if (SceneManager.GetActiveScene().path != ScenePath)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject mapRoot = GameObject.Find("ShadowDrop_Map");
        if (mapRoot == null)
        {
            Debug.LogError("ShadowDrop_Map was not found in the prototype scene.");
            return false;
        }

        Transform oldContainer = mapRoot.transform.Find("OriginalBuildingZones");
        if (oldContainer != null)
        {
            Undo.DestroyObjectImmediate(oldContainer.gameObject);
        }

        DisablePreviousBuildingLayers(mapRoot.transform);

        GameObject container = new GameObject("OriginalBuildingZones");
        Undo.RegisterCreatedObjectUndo(container, "Place Original Building Zones");
        container.transform.SetParent(mapRoot.transform, false);

        foreach (BuildingZone zone in metadata.zones)
        {
            if (zone.mapPositionSourcePixels == null || zone.mapPositionSourcePixels.Length < 2)
            {
                Debug.LogWarning($"Skipping zone with invalid position: {zone.name}");
                continue;
            }

            string assetPath = $"{AssetRoot}/{zone.name}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                Debug.LogError($"Building-zone sprite not found: {assetPath}");
                continue;
            }

            GameObject item = new GameObject(zone.name);
            item.transform.SetParent(container.transform, false);
            item.transform.localPosition = SourcePixelToWorld(
                zone.mapPositionSourcePixels[0],
                zone.mapPositionSourcePixels[1]
            );

            SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = SortingOrder;
            item.AddComponent<ShadowDropBuildingZoneFade>();
        }

        Selection.activeGameObject = container;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        SceneView.RepaintAll();
        Debug.Log($"Placed {metadata.zones.Length} original building zones.");

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "Shadow Drop",
                $"Placed {metadata.zones.Length} original building zones. Existing colliders were unchanged.",
                "OK"
            );
        }

        return true;
    }

    private static void DisablePreviousBuildingLayers(Transform mapRoot)
    {
        Transform example = mapRoot.Find("IndividualBuildings_Example");
        if (example != null)
        {
            Undo.RecordObject(example.gameObject, "Disable Individual Building Example");
            example.gameObject.SetActive(false);
        }

        Transform legacy = mapRoot.Find("LegacyBuildingSheet_Disabled");
        if (legacy == null)
        {
            legacy = mapRoot.Find("Buildings");
        }

        if (legacy != null)
        {
            legacy.name = "LegacyBuildingSheet_Disabled";
            SpriteRenderer legacyRenderer = legacy.GetComponent<SpriteRenderer>();
            if (legacyRenderer != null)
            {
                Undo.RecordObject(legacyRenderer, "Disable Legacy Building Sheet");
                legacyRenderer.enabled = false;
            }
        }
    }

    private static Vector3 SourcePixelToWorld(float sourceX, float sourceY)
    {
        float upscaledX = sourceX * Upscale;
        float upscaledY = sourceY * Upscale;
        float worldX = (upscaledX - UpscaledWidth * 0.5f) / PixelsPerUnit;
        float worldY = (UpscaledHeight * 0.5f - upscaledY) / PixelsPerUnit;
        return new Vector3(worldX, worldY, 0f);
    }

    private static void ConfigureSprites(BuildingZoneMetadata metadata)
    {
        foreach (BuildingZone zone in metadata.zones)
        {
            string assetPath = $"{AssetRoot}/{zone.name}.png";
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"Building-zone texture not found: {assetPath}");
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
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
