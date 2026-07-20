using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class QuarterViewBuildingOccluder2D : MonoBehaviour
{
    private static readonly List<QuarterViewBuildingOccluder2D> ActiveOccluders = new();

    [SerializeField] private Transform target;
    [SerializeField, Range(0f, 1f)] private float occludedAlpha = 0.42f;
    [SerializeField, Min(0f)] private float fadeSpeed = 5f;
    [SerializeField, Range(0.1f, 1f)] private float horizontalCoverage = 0.72f;
    [SerializeField] private float behindMargin = 0.1f;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer targetRenderer;
    private PolygonCollider2D footprint;
    private Vector2[] footprintPath;
    private float visibleAlpha = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        visibleAlpha = spriteRenderer.color.a;
        ResolveFootprint();
        ResolveTarget();
    }

    private void OnEnable()
    {
        if (!ActiveOccluders.Contains(this))
        {
            ActiveOccluders.Add(this);
        }
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (target == null)
        {
            ResolveTarget();
        }

        Bounds targetBounds = GetTargetBounds();
        bool shouldFade = target != null
            && TryGetDepthConstraint(target.position, targetBounds, out bool targetIsBehind, out _)
            && targetIsBehind;

        float desiredAlpha = shouldFade ? occludedAlpha : visibleAlpha;
        Color color = spriteRenderer.color;
        color.a = Mathf.MoveTowards(color.a, desiredAlpha, fadeSpeed * Time.deltaTime);
        spriteRenderer.color = color;
    }

    private void OnDisable()
    {
        ActiveOccluders.Remove(this);

        if (spriteRenderer == null)
        {
            return;
        }

        Color color = spriteRenderer.color;
        color.a = visibleAlpha;
        spriteRenderer.color = color;
    }

    private void ResolveTarget()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            targetRenderer = player.GetComponent<SpriteRenderer>();
        }
    }

    public static int ResolvePlayerSortingOrder(
        Vector2 playerFeet,
        Bounds playerBounds,
        int naturalOrder
    )
    {
        int minimumOrder = int.MinValue;
        int maximumOrder = int.MaxValue;

        for (int index = ActiveOccluders.Count - 1; index >= 0; index--)
        {
            QuarterViewBuildingOccluder2D occluder = ActiveOccluders[index];
            if (occluder == null)
            {
                ActiveOccluders.RemoveAt(index);
                continue;
            }

            if (!occluder.TryGetDepthConstraint(
                    playerFeet,
                    playerBounds,
                    out bool playerIsBehind,
                    out int buildingOrder
                ))
            {
                continue;
            }

            if (playerIsBehind)
            {
                maximumOrder = Mathf.Min(maximumOrder, buildingOrder - 1);
            }
            else
            {
                minimumOrder = Mathf.Max(minimumOrder, buildingOrder + 1);
            }
        }

        if (minimumOrder <= maximumOrder)
        {
            return Mathf.Clamp(naturalOrder, minimumOrder, maximumOrder);
        }

        // A foreground building wins if overlapping buildings produce conflicting constraints.
        return maximumOrder;
    }

    private bool TryGetDepthConstraint(
        Vector2 playerFeet,
        Bounds playerBounds,
        out bool playerIsBehind,
        out int buildingOrder
    )
    {
        EnsureReferences();
        playerIsBehind = false;
        buildingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
        if (spriteRenderer == null)
        {
            return false;
        }

        Bounds buildingBounds = spriteRenderer.bounds;
        float coveredHalfWidth = buildingBounds.extents.x * horizontalCoverage;
        float coveredMinX = buildingBounds.center.x - coveredHalfWidth;
        float coveredMaxX = buildingBounds.center.x + coveredHalfWidth;
        bool overlapsHorizontally =
            playerBounds.max.x >= coveredMinX && playerBounds.min.x <= coveredMaxX;
        bool overlapsVertically =
            playerBounds.max.y >= buildingBounds.min.y
            && playerBounds.min.y <= buildingBounds.max.y;
        if (!overlapsHorizontally || !overlapsVertically)
        {
            return false;
        }

        float frontEdgeY = transform.position.y;
        TryGetFootprintFrontY(playerFeet.x, out frontEdgeY);
        playerIsBehind = playerFeet.y > frontEdgeY + behindMargin;
        return true;
    }

    private bool TryGetFootprintFrontY(float worldX, out float frontY)
    {
        EnsureReferences();
        frontY = transform.position.y;
        if (footprint == null || footprintPath == null || footprintPath.Length < 3)
        {
            return false;
        }

        bool found = false;
        float nearestFrontY = float.PositiveInfinity;
        for (int index = 0; index < footprintPath.Length; index++)
        {
            Vector2 localA = footprintPath[index] + footprint.offset;
            Vector2 localB = footprintPath[(index + 1) % footprintPath.Length] + footprint.offset;
            Vector2 worldA = footprint.transform.TransformPoint(localA);
            Vector2 worldB = footprint.transform.TransformPoint(localB);
            float minX = Mathf.Min(worldA.x, worldB.x);
            float maxX = Mathf.Max(worldA.x, worldB.x);
            if (worldX < minX || worldX > maxX)
            {
                continue;
            }

            float deltaX = worldB.x - worldA.x;
            float edgeY;
            if (Mathf.Abs(deltaX) <= Mathf.Epsilon)
            {
                edgeY = Mathf.Min(worldA.y, worldB.y);
            }
            else
            {
                float t = Mathf.Clamp01((worldX - worldA.x) / deltaX);
                edgeY = Mathf.Lerp(worldA.y, worldB.y, t);
            }

            nearestFrontY = Mathf.Min(nearestFrontY, edgeY);
            found = true;
        }

        if (found)
        {
            frontY = nearestFrontY;
        }

        return found;
    }

    private Bounds GetTargetBounds()
    {
        if (targetRenderer == null && target != null)
        {
            targetRenderer = target.GetComponent<SpriteRenderer>();
        }

        return targetRenderer != null
            ? targetRenderer.bounds
            : new Bounds(target != null ? target.position : Vector3.zero, Vector3.zero);
    }

    private void ResolveFootprint()
    {
        QuarterViewFootprintObstacle2D obstacle =
            GetComponentInChildren<QuarterViewFootprintObstacle2D>(true);
        footprint = obstacle != null ? obstacle.Footprint as PolygonCollider2D : null;
        footprintPath = footprint != null && footprint.pathCount > 0
            ? footprint.GetPath(0)
            : null;
    }

    private void EnsureReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (footprint == null)
        {
            ResolveFootprint();
        }
    }
}
