using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public sealed class InteractionIcon2D : MonoBehaviour
{
    public void OnIconTapped()
    {
        IInteractable interactable = GetComponentInParent<IInteractable>();
        interactable?.Interact();
    }
}
