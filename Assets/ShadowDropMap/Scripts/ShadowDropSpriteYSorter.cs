using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class ShadowDropSpriteYSorter : MonoBehaviour
{
    [SerializeField] private int sortingOffset;
    [SerializeField] private int precision = 100;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer.sortingOrder = sortingOffset - Mathf.RoundToInt(transform.position.y * precision);
    }
}
