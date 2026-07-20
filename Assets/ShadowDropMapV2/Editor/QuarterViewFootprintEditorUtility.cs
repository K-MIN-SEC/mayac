using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class QuarterViewFootprintEditorUtility
{
    public static bool AddOrUpdateFootprint(GameObject root, bool building)
    {
        SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
        {
            return false;
        }

        Transform footprintTransform = FindSingleFootprint(root.transform);
        GameObject footprintObject;
        if (footprintTransform == null)
        {
            footprintObject = new GameObject("FootprintCollider");
            footprintObject.transform.SetParent(root.transform, false);
        }
        else
        {
            footprintObject = footprintTransform.gameObject;
        }

        footprintObject.transform.localPosition = Vector3.zero;
        footprintObject.transform.localRotation = Quaternion.identity;
        footprintObject.transform.localScale = Vector3.one;
        footprintObject.layer = root.layer;

        PolygonCollider2D polygon = footprintObject.GetComponent<PolygonCollider2D>();
        foreach (Collider2D existing in footprintObject.GetComponents<Collider2D>())
        {
            if (existing != polygon)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }
        }

        if (polygon == null)
        {
            polygon = footprintObject.AddComponent<PolygonCollider2D>();
        }

        polygon.pathCount = 1;
        polygon.SetPath(0, BuildFootprint(renderer.sprite, building));
        polygon.offset = Vector2.zero;
        polygon.isTrigger = false;

        QuarterViewFootprintObstacle2D obstacle =
            footprintObject.GetComponent<QuarterViewFootprintObstacle2D>();
        if (obstacle == null)
        {
            obstacle = footprintObject.AddComponent<QuarterViewFootprintObstacle2D>();
        }

        SerializedObject serialized = new SerializedObject(obstacle);
        serialized.FindProperty("footprint").objectReferenceValue = polygon;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    private static Transform FindSingleFootprint(Transform root)
    {
        var candidates = new List<Transform>();
        for (int index = 0; index < root.childCount; index++)
        {
            Transform child = root.GetChild(index);
            if (child.name == "FootprintCollider")
            {
                candidates.Add(child);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        Transform preferred = candidates[0];
        foreach (Transform candidate in candidates)
        {
            if (!PrefabUtility.IsAddedGameObjectOverride(candidate.gameObject))
            {
                preferred = candidate;
                break;
            }
        }

        foreach (Transform candidate in candidates)
        {
            if (candidate != preferred)
            {
                UnityEngine.Object.DestroyImmediate(candidate.gameObject);
            }
        }

        return preferred;
    }

    public static Vector2[] BuildFootprint(Sprite sprite, bool building)
    {
        Bounds bounds = sprite.bounds;
        if (building)
        {
            return BuildPreciseBuildingFootprint(bounds);
        }

        string assetName = sprite.name.ToLowerInvariant();
        Profile profile = GetPropProfile(assetName);

        float width = Mathf.Clamp(
            bounds.size.x * profile.WidthRatio,
            profile.MinimumWidth,
            bounds.size.x * 0.92f
        );
        float depth = Mathf.Clamp(
            width * profile.DepthRatio,
            profile.MinimumDepth,
            bounds.size.y * profile.MaximumHeightRatio
        );
        float basePadding = Mathf.Clamp(bounds.size.y * 0.012f, 0.025f, 0.14f);
        float frontY = bounds.min.y + basePadding;
        float halfWidth = width * 0.5f;
        float backY = frontY + depth;

        return new[]
        {
            new Vector2(0f, frontY),
            new Vector2(halfWidth, frontY + depth * profile.RightShoulderRatio),
            new Vector2(0f, backY),
            new Vector2(-halfWidth, frontY + depth * profile.LeftShoulderRatio),
        };
    }

    private static Vector2[] BuildPreciseBuildingFootprint(Bounds bounds)
    {
        float width = bounds.size.x;
        float bottom = bounds.min.y;

        // Calibrated from the hand-fitted Fine_NC_Front_convenience_24 footprint.
        return new[]
        {
            new Vector2(width * 0.04460967f, bottom + width * 0.02776208f),
            new Vector2(width * 0.46460965f, bottom + width * 0.29413176f),
            new Vector2(width * -0.07434944f, bottom + width * 0.5499212f),
            new Vector2(width * -0.46460965f, bottom + width * 0.3132913f),
        };
    }

    private static Profile GetBuildingProfile(string assetName)
    {
        if (ContainsAny(assetName, "apartment", "clinic_brick"))
        {
            return new Profile(0.76f, 0.42f, 1.65f, 1.05f, 0.45f, 0.55f, 0.30f);
        }

        if (ContainsAny(assetName, "walled", "gable", "stairs", "balcony", "house_red_tile"))
        {
            return new Profile(0.90f, 0.52f, 1.80f, 1.10f, 0.45f, 0.55f, 0.43f);
        }

        if (ContainsAny(assetName, "market", "convenience", "laundry", "restaurant", "tailor", "police"))
        {
            return new Profile(0.84f, 0.48f, 1.70f, 1.00f, 0.45f, 0.55f, 0.38f);
        }

        if (ContainsAny(assetName, "mixed", "villa", "house"))
        {
            return new Profile(0.86f, 0.50f, 1.70f, 1.00f, 0.45f, 0.55f, 0.42f);
        }

        return new Profile(0.80f, 0.46f, 1.60f, 0.95f, 0.45f, 0.55f, 0.36f);
    }

    private static Profile GetPropProfile(string assetName)
    {
        if (ContainsAny(assetName, "pole", "lamp", "signal", "sign", "bollard", "call_box", "hydrant"))
        {
            return new Profile(0.18f, 0.72f, 0.30f, 0.24f, 0.47f, 0.53f, 0.12f);
        }

        if (ContainsAny(assetName, "tree", "cypress"))
        {
            return new Profile(0.43f, 0.62f, 0.65f, 0.45f, 0.47f, 0.53f, 0.16f);
        }

        if (assetName.Contains("planter"))
        {
            return new Profile(0.79f, 0.48f, 0.60f, 0.38f, 0.46f, 0.54f, 0.28f);
        }

        if (assetName.Contains("bench"))
        {
            return new Profile(0.80f, 0.24f, 0.75f, 0.28f, 0.46f, 0.54f, 0.18f);
        }

        if (assetName.Contains("shelter"))
        {
            return new Profile(0.76f, 0.32f, 0.85f, 0.34f, 0.46f, 0.54f, 0.20f);
        }

        if (assetName.Contains("guardrail"))
        {
            return new Profile(0.86f, 0.16f, 0.75f, 0.22f, 0.47f, 0.53f, 0.12f);
        }

        if (ContainsAny(assetName, "vending", "utility_box", "mailbox", "trash", "recycle"))
        {
            return new Profile(0.60f, 0.48f, 0.48f, 0.34f, 0.46f, 0.54f, 0.25f);
        }

        return new Profile(0.56f, 0.46f, 0.40f, 0.30f, 0.46f, 0.54f, 0.23f);
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

    private readonly struct Profile
    {
        public Profile(
            float widthRatio,
            float depthRatio,
            float minimumWidth,
            float minimumDepth,
            float leftShoulderRatio,
            float rightShoulderRatio,
            float maximumHeightRatio
        )
        {
            WidthRatio = widthRatio;
            DepthRatio = depthRatio;
            MinimumWidth = minimumWidth;
            MinimumDepth = minimumDepth;
            LeftShoulderRatio = leftShoulderRatio;
            RightShoulderRatio = rightShoulderRatio;
            MaximumHeightRatio = maximumHeightRatio;
        }

        public float WidthRatio { get; }
        public float DepthRatio { get; }
        public float MinimumWidth { get; }
        public float MinimumDepth { get; }
        public float LeftShoulderRatio { get; }
        public float RightShoulderRatio { get; }
        public float MaximumHeightRatio { get; }
    }
}
