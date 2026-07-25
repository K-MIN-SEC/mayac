using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 씬에 하나만 존재. 돋보기로 조사했을 때 뜨는 사진 패널 + 숨기 확인창을 관리
public class HidingPanelManager : MonoBehaviour
{
    public static HidingPanelManager Instance { get; private set; }

    [Header("조사 패널 (사진 + 뒤로가기 버튼)")]
    public GameObject examinePanel;   // 패널 전체 (평소엔 꺼져있음)
    public Image photoImage;          // 사진을 띄울 Image

    [Header("숨기 확인창 (예/아니오)")]
    public GameObject confirmDialog;  // 확인창 전체 (평소엔 꺼져있음)
    public TMP_Text confirmText;      // "~~에 숨기겠습니까?" 문구

    private string pendingSpotName;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        examinePanel.SetActive(false);
        confirmDialog.SetActive(false);
    }

    // 돋보기 아이콘 상호작용 시 호출 - 조사 패널 열기
    public void OpenExaminePanel(Sprite photo)
    {
        photoImage.sprite = photo;
        examinePanel.SetActive(true);
        confirmDialog.SetActive(false);
    }

    // 뒤로가기 버튼에 연결 - 패널 전체 닫기
    public void CloseExaminePanel()
    {
        examinePanel.SetActive(false);
        confirmDialog.SetActive(false);
    }

    // 사진 속 숨을 수 있는 지점(핫스팟)을 눌렀을 때 호출
    public void RequestHide(string spotName)
    {
        pendingSpotName = spotName;
        confirmText.text = $"Hide in {spotName}?"; // 화면 UI는 일단 영어로 (폰트 문제 회피용, 나중에 한글 폰트 넣으면 다시 한글로 바꾸면 됨)
        confirmDialog.SetActive(true);
    }

    // 예 버튼에 연결
    public void ConfirmYes()
    {
        Debug.Log($"{pendingSpotName}에 숨겼습니다.");
        confirmDialog.SetActive(false);
        examinePanel.SetActive(false); // 사진 패널까지 같이 닫아서 플레이 화면으로 복귀
        // TODO: 실제 숨기기 처리(캐릭터 상태 변경, 점수 반영 등)는 여기에 추가
    }

    // 아니오 버튼에 연결
    public void ConfirmNo()
    {
        confirmDialog.SetActive(false);
        // 조사 패널(사진)은 그대로 유지 -> 첫 화면으로 복귀
    }
}
