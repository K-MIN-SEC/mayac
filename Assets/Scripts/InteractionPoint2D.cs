using UnityEngine;
using UnityEngine.Events;

// 어떤 오브젝트와도 연결할 필요 없이, 이거 하나만 원하는 위치(허공, 화분 뒤 등)에 두면 끝
// 캐릭터가 가까이 오면 아이콘이 뜨고, 아이콘을 터치하면 아래 On Interact 이벤트가 실행됨
[RequireComponent(typeof(Collider2D))]
public class InteractionPoint2D : MonoBehaviour, IInteractable
{
    [Header("범위 안에 들어왔을 때 보여줄 아이콘")]
    public GameObject iconObject; // 느낌표/물음표/돋보기 등 스프라이트가 붙은 자식 오브젝트

    [Header("아이콘을 터치했을 때 실행할 동작")]
    public UnityEvent onInteract; // Inspector에서 원하는 함수를 자유롭게 연결 (사운드 재생, 점수 추가, 대사 출력 등)

    void Awake()
    {
        if (iconObject != null) iconObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && iconObject != null)
        {
            iconObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && iconObject != null)
        {
            iconObject.SetActive(false);
        }
    }

    // IInteractable 구현 - 아이콘을 탭했을 때 SimpleClickMove2D/InteractionIcon2D가 이 함수를 호출함
    public void Interact()
    {
        Debug.Log($"[상호작용] {gameObject.name}");
        onInteract?.Invoke();
    }
}
