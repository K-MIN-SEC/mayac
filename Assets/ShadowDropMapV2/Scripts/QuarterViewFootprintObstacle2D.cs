using UnityEngine;

[DisallowMultipleComponent]
public sealed class QuarterViewFootprintObstacle2D : MonoBehaviour
{
    [SerializeField] private Collider2D footprint;

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
    }

    private void Awake()
    {
        if (footprint == null)
        {
            footprint = GetComponent<Collider2D>();
        }
    }
}
