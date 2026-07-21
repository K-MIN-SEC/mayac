using UnityEngine;

// 떠있는 아이콘(느낌표/물음표/돋보기 등) 오브젝트에 부착
// InteractionPoint2D 오브젝트의 자식으로 두면 자동으로 연결됨
// 아이콘 자체에 작은 Collider2D 필요, "InteractableIcon" 전용 레이어 지정 필요
[RequireComponent(typeof(Collider2D))]
public class InteractionIcon2D : MonoBehaviour
{
    private IInteractable interactable;

    void Awake()
    {
        interactable = GetComponentInParent<IInteractable>();
    }

    // SimpleClickMove2D 쪽에서 이 아이콘을 클릭했을 때 이 함수를 호출함
    public void OnIconTapped()
    {
        if (interactable != null)
        {
            interactable.Interact();
        }
    }
}
