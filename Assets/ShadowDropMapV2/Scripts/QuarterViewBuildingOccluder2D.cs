using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class QuarterViewBuildingOccluder2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Range(0f, 1f)] private float occludedAlpha = 0.42f;
    [SerializeField, Min(0f)] private float fadeSpeed = 5f;
    [SerializeField, Range(0.1f, 1f)] private float horizontalCoverage = 0.72f;
    [SerializeField] private float behindMargin = 0.1f;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer targetRenderer;
    private QuarterViewFootprintObstacle2D footprintObstacle;
    private float visibleAlpha = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        visibleAlpha = spriteRenderer.color.a;
        ResolveFootprint();
        ResolveTarget();
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

        bool shouldFade = target != null
            && IsTargetBehind(target.position, GetTargetBounds());
        float desiredAlpha = shouldFade ? occludedAlpha : visibleAlpha;
        Color color = spriteRenderer.color;
        color.a = Mathf.MoveTowards(color.a, desiredAlpha, fadeSpeed * Time.deltaTime);
        spriteRenderer.color = color;
    }

    private void OnDisable()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color color = spriteRenderer.color;
        color.a = visibleAlpha;
        spriteRenderer.color = color;
    }

    private bool IsTargetBehind(Vector2 targetFeet, Bounds targetBounds)
    {
        EnsureReferences();
        if (spriteRenderer == null)
        {
            return false;
        }

        Bounds buildingBounds = spriteRenderer.bounds;
        float coveredHalfWidth = buildingBounds.extents.x * horizontalCoverage;
        float coveredMinX = buildingBounds.center.x - coveredHalfWidth;
        float coveredMaxX = buildingBounds.center.x + coveredHalfWidth;
        bool overlapsHorizontally =
            targetBounds.max.x >= coveredMinX && targetBounds.min.x <= coveredMaxX;
        bool overlapsVertically =
            targetBounds.max.y >= buildingBounds.min.y
            && targetBounds.min.y <= buildingBounds.max.y;
        if (!overlapsHorizontally || !overlapsVertically)
        {
            return false;
        }

        float frontEdgeY = transform.position.y;
        if (footprintObstacle != null)
        {
            footprintObstacle.TryGetFrontEdgeY(targetFeet.x, out frontEdgeY);
        }

        return targetFeet.y > frontEdgeY + behindMargin;
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

    private void ResolveTarget()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            targetRenderer = player.GetComponent<SpriteRenderer>();
        }
    }

    private void ResolveFootprint()
    {
        footprintObstacle = GetComponentInChildren<QuarterViewFootprintObstacle2D>(true);
    }

    private void EnsureReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (footprintObstacle == null)
        {
            ResolveFootprint();
        }
    }
}
