using UnityEngine;

[DisallowMultipleComponent]
public sealed class QuarterViewFootprintObstacle2D : MonoBehaviour
{
    [SerializeField] private Collider2D footprint;

    private PolygonCollider2D polygonFootprint;
    private Vector2[] footprintPath;

    public Collider2D Footprint
    {
        get
        {
            if (footprint == null)
            {
                footprint = GetComponent<Collider2D>();
            }

            return footprint;
        }
    }

    private void Reset()
    {
        footprint = GetComponent<Collider2D>();
        CachePolygonPath();
    }

    private void Awake()
    {
        if (footprint == null)
        {
            footprint = GetComponent<Collider2D>();
        }

        CachePolygonPath();
    }

    public bool TryGetFrontEdgeY(float worldX, out float frontEdgeY)
    {
        if (polygonFootprint == null || footprintPath == null || footprintPath.Length < 3)
        {
            CachePolygonPath();
        }

        frontEdgeY = transform.position.y;
        if (polygonFootprint == null || footprintPath == null || footprintPath.Length < 3)
        {
            return false;
        }

        bool found = false;
        float nearestFrontY = float.PositiveInfinity;
        for (int index = 0; index < footprintPath.Length; index++)
        {
            Vector2 localA = footprintPath[index] + polygonFootprint.offset;
            Vector2 localB =
                footprintPath[(index + 1) % footprintPath.Length] + polygonFootprint.offset;
            Vector2 worldA = polygonFootprint.transform.TransformPoint(localA);
            Vector2 worldB = polygonFootprint.transform.TransformPoint(localB);
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
            frontEdgeY = nearestFrontY;
        }

        return found;
    }

    private void CachePolygonPath()
    {
        polygonFootprint = Footprint as PolygonCollider2D;
        footprintPath = polygonFootprint != null && polygonFootprint.pathCount > 0
            ? polygonFootprint.GetPath(0)
            : null;
    }
}
