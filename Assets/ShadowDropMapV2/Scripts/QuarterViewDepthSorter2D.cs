using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class QuarterViewDepthSorter2D : MonoBehaviour
{
    [SerializeField] private int sortingOffset = 10000;
    [SerializeField, Min(1)] private int precision = 100;
    [SerializeField, Min(1)] private int zPrecision = 1000;

    private SpriteRenderer spriteRenderer;
    private bool isPlayer;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        isPlayer = CompareTag("Player");
        UpdateOrder();
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
            sortingOrder = QuarterViewBuildingOccluder2D.ResolvePlayerSortingOrder(
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
}
