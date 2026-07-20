using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class QuarterViewDepthSorter2D : MonoBehaviour
{
    [SerializeField] private int sortingOffset = 10000;
    [SerializeField, Min(1)] private int precision = 100;
    [SerializeField, Min(1)] private int zPrecision = 1000;

    private SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        spriteRenderer.sortingOrder = sortingOffset
            - Mathf.RoundToInt(transform.position.y * precision)
            - Mathf.RoundToInt(transform.position.z * zPrecision);
    }
}
