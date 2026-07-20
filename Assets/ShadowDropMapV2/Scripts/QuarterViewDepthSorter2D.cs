using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class QuarterViewDepthSorter2D : MonoBehaviour
{
    private const float BehindMargin = 0.1f;
    private static readonly List<QuarterViewDepthSorter2D> ActiveObstacles = new();

    [SerializeField] private int sortingOffset = 10000;
    [SerializeField, Min(1)] private int precision = 100;
    [SerializeField, Min(1)] private int zPrecision = 1000;

    private SpriteRenderer spriteRenderer;
    private QuarterViewFootprintObstacle2D footprintObstacle;
    private bool isPlayer;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        isPlayer = CompareTag("Player");
        if (!isPlayer)
        {
            footprintObstacle = GetComponentInChildren<QuarterViewFootprintObstacle2D>(true);
            if (footprintObstacle != null && !ActiveObstacles.Contains(this))
            {
                ActiveObstacles.Add(this);
            }
        }

        UpdateOrder();
    }

    private void OnDisable()
    {
        ActiveObstacles.Remove(this);
    }

    private void LateUpdate()
    {
        UpdateOrder();
    }

    private void UpdateOrder()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        int sortingOrder = sortingOffset - Mathf.RoundToInt(transform.position.y * precision);
        if (isPlayer)
        {
            sortingOrder = ResolvePlayerSortingOrder(
                transform.position,
                spriteRenderer.bounds,
                sortingOrder
            );
        }
        else
        {
            sortingOrder -= Mathf.RoundToInt(transform.position.z * zPrecision);
        }

        spriteRenderer.sortingOrder = sortingOrder;
    }

    private static int ResolvePlayerSortingOrder(
        Vector2 playerFeet,
        Bounds playerBounds,
        int naturalOrder
    )
    {
        int minimumOrder = int.MinValue;
        int maximumOrder = int.MaxValue;

        for (int index = ActiveObstacles.Count - 1; index >= 0; index--)
        {
            QuarterViewDepthSorter2D obstacle = ActiveObstacles[index];
            if (obstacle == null)
            {
                ActiveObstacles.RemoveAt(index);
                continue;
            }

            if (!obstacle.TryGetDepthConstraint(
                    playerFeet,
                    playerBounds,
                    out bool playerIsBehind,
                    out int obstacleOrder
                ))
            {
                continue;
            }

            if (playerIsBehind)
            {
                maximumOrder = Mathf.Min(maximumOrder, obstacleOrder - 1);
            }
            else
            {
                minimumOrder = Mathf.Max(minimumOrder, obstacleOrder + 1);
            }
        }

        if (minimumOrder <= maximumOrder)
        {
            return Mathf.Clamp(naturalOrder, minimumOrder, maximumOrder);
        }

        // Prefer hiding behind the foremost object when constraints overlap.
        return maximumOrder;
    }

    private bool TryGetDepthConstraint(
        Vector2 playerFeet,
        Bounds playerBounds,
        out bool playerIsBehind,
        out int obstacleOrder
    )
    {
        playerIsBehind = false;
        obstacleOrder = CalculateStaticOrder();
        if (spriteRenderer == null || footprintObstacle == null)
        {
            return false;
        }

        Bounds obstacleBounds = spriteRenderer.bounds;
        bool overlapsHorizontally =
            playerBounds.max.x >= obstacleBounds.min.x
            && playerBounds.min.x <= obstacleBounds.max.x;
        bool overlapsVertically =
            playerBounds.max.y >= obstacleBounds.min.y
            && playerBounds.min.y <= obstacleBounds.max.y;
        if (!overlapsHorizontally || !overlapsVertically)
        {
            return false;
        }

        float frontEdgeY = transform.position.y;
        footprintObstacle.TryGetFrontEdgeY(playerFeet.x, out frontEdgeY);
        playerIsBehind = playerFeet.y > frontEdgeY + BehindMargin;
        return true;
    }

    private int CalculateStaticOrder()
    {
        return sortingOffset
            - Mathf.RoundToInt(transform.position.y * precision)
            - Mathf.RoundToInt(transform.position.z * zPrecision);
    }
}
