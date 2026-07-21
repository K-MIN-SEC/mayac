using UnityEngine;

// 문, 상자, 스위치 등 "움직이는" 상호작용 오브젝트에 붙이는 범용 스크립트
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("상호작용 설정")]
    public string objectName = "오브젝트";
    public bool isOneTime = false; // 한 번만 상호작용 가능하게 할지
    private bool used = false;

    [Header("동작 (선택 사항)")]
    public GameObject targetToToggle;       // 회전/이동시킬 대상 (없으면 비워둬도 됨)
    public Vector3 openLocalRotation = new Vector3(0, 90, 0);

    public void Interact()
    {
        if (isOneTime && used) return;
        used = true;

        Debug.Log($"[상호작용] {objectName}");

        if (targetToToggle != null)
        {
            targetToToggle.transform.localRotation = Quaternion.Euler(openLocalRotation);
        }

        // TODO: 아이템 획득, 사운드, 이펙트 등 필요한 동작을 여기에 추가
    }
}
