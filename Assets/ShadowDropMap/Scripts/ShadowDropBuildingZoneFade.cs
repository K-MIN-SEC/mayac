using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class ShadowDropBuildingZoneFade : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float occludedAlpha = 0.35f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.12f;

    private SpriteRenderer spriteRenderer;
    private float visibleAlpha = 1f;
    private float targetAlpha = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        visibleAlpha = spriteRenderer.color.a;
        targetAlpha = visibleAlpha;
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Color color = spriteRenderer.color;
        float speed = fadeDuration <= 0f ? float.PositiveInfinity : 1f / fadeDuration;
        color.a = Mathf.MoveTowards(color.a, targetAlpha, speed * Time.deltaTime);
        spriteRenderer.color = color;
    }

    public void SetOccluded(bool isOccluded)
    {
        targetAlpha = isOccluded ? occludedAlpha : visibleAlpha;
    }

    public void SetTargetAlpha(float alpha)
    {
        targetAlpha = Mathf.Clamp01(alpha);
    }

    public void ShowImmediately()
    {
        targetAlpha = visibleAlpha;
        SetRendererAlpha(visibleAlpha);
    }

    private void SetRendererAlpha(float alpha)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
