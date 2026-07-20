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
    private float visibleAlpha = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        visibleAlpha = spriteRenderer.color.a;
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

        bool shouldFade = false;
        if (target != null)
        {
            Bounds bounds = spriteRenderer.bounds;
            float horizontalDistance = Mathf.Abs(target.position.x - transform.position.x);
            shouldFade =
                horizontalDistance <= bounds.extents.x * horizontalCoverage
                && target.position.y > transform.position.y + behindMargin
                && target.position.y < bounds.max.y;
        }

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

    private void ResolveTarget()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }
}
