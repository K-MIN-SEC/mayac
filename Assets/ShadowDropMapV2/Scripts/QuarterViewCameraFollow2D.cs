using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class QuarterViewCameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer mapRenderer;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.16f;

    private Camera cachedCamera;
    private Vector3 velocity;

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null || mapRenderer == null)
        {
            return;
        }

        if (cachedCamera == null)
        {
            cachedCamera = GetComponent<Camera>();
        }

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        Bounds bounds = mapRenderer.bounds;
        float halfHeight = cachedCamera.orthographicSize;
        float halfWidth = halfHeight * cachedCamera.aspect;
        smoothed.x = ClampAxis(smoothed.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth, bounds.center.x);
        smoothed.y = ClampAxis(smoothed.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight, bounds.center.y);
        transform.position = smoothed;
    }

    private static float ClampAxis(float value, float minimum, float maximum, float fallback)
    {
        return minimum <= maximum ? Mathf.Clamp(value, minimum, maximum) : fallback;
    }
}
