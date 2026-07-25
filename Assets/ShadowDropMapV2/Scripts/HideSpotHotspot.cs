using UnityEngine;

// 조사 패널 사진 안에서, 숨을 수 있는 특정 위치에 배치한 버튼(빈 오브젝트+콜라이더 or UI Button)에 부착
// 이 오브젝트의 Button 컴포넌트 OnClick()에 OnHotspotClicked()를 연결하면 됨
public class HideSpotHotspot : MonoBehaviour
{
    [Header("이 지점 이름 (확인 메시지에 사용됨)")]
    public string spotName = "책상 밑";

    // Button의 OnClick()에 연결하는 함수
    public void OnHotspotClicked()
    {
        HidingPanelManager.Instance.RequestHide(spotName);
    }
}
