using UnityEngine;
using UnityEngine.EventSystems;

// 2D 프로젝트용 클릭(터치) 이동 + 상호작용 스크립트
// 캐릭터에 Rigidbody2D (Dynamic, Gravity Scale 0, Freeze Rotation Z) + 이 스크립트를 붙이면 됨
[RequireComponent(typeof(Rigidbody2D))]
public class SimpleClickMove2D : MonoBehaviour
{
    [Header("참조")]
    public Camera mainCamera;
    public LayerMask interactableLayer; // 상호작용 가능한 오브젝트에 씌운 레이어

    [Header("이동 속도")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private IInteractable pendingInteract;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (mainCamera == null) mainCamera = Camera.main;
        targetPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // UI(버튼, 대화창 등) 클릭이면 무시
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            HandleClick();
        }
    }

    void FixedUpdate()
    {
        if (!isMoving) return;

        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, targetPosition) < 0.05f)
        {
            isMoving = false;
            if (pendingInteract != null)
            {
                pendingInteract.Interact();
                pendingInteract = null;
            }
        }
    }

    void HandleClick()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // 상호작용 가능한 오브젝트를 클릭했는지 확인
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld, interactableLayer);
        if (hit != null)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Vector3 dir = (transform.position - hit.transform.position).normalized;
                targetPosition = hit.transform.position + dir * 1f; // 대상 앞에서 멈춤
                pendingInteract = interactable;
                isMoving = true;
                return;
            }
        }

        // 그냥 바닥(빈 공간) 클릭이면 이동만
        targetPosition = mouseWorld;
        pendingInteract = null;
        isMoving = true;
    }
}
