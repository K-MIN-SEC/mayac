using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public sealed class InteractionPoint2D : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject iconObject;
    [SerializeField] private UnityEvent onInteract;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void Awake()
    {
        SetIconVisible(false);
    }

    private void OnDisable()
    {
        SetIconVisible(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetIconVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetIconVisible(false);
        }
    }

    public void Interact()
    {
        Debug.Log($"[Interaction] {gameObject.name}", this);
        onInteract?.Invoke();
    }

    private void SetIconVisible(bool visible)
    {
        if (iconObject != null && iconObject.activeSelf != visible)
        {
            iconObject.SetActive(visible);
        }
    }
}
