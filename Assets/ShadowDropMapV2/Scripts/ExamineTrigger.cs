using UnityEngine;

// 돋보기 상호작용 지점(InteractionPoint2D)이 붙은 오브젝트에 같이 부착
// InteractionPoint2D의 On Interact 이벤트에 이 스크립트의 Open()을 연결하면 됨
public class ExamineTrigger : MonoBehaviour
{
    [Header("이 지점에서 보여줄 사진")]
    public Sprite photo;

    public void Open()
    {
        HidingPanelManager.Instance.OpenExaminePanel(photo);
    }
}
