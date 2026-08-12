using UnityEngine;

// 미션 메시지(사진 + "여기다가 숨겨주세용~")를 메신저로 보내고,
// 동시에 나침반이 가리킬 목표(MissionTarget)를 켜주는 트리거용 스크립트.
// + 탐색기 앱이 필요한 메시지라면, 탐색기 앱도 같이 켜서 돋보기 위치로 안내함.
// InteractionPoint2D의 On Interact 같은 UnityEvent에 SendMissionMessage() 하나만 연결하면 됨.
public class MissionMessageSender : MonoBehaviour
{
    // 가장 최근에 메시지를 보낸 MissionMessageSender (답장 화면에서 "누구 미션인지" 알아낼 때 씀)
    public static MissionMessageSender LastSent { get; private set; }

    [Header("메신저로 보낼 내용")]
    public string contactName = "???";
    [TextArea] public string messageText = "여기다가 숨겨주세용~";
    public Sprite photo;

    [Header("답장 선택지 (플레이어가 고를 수 있는 답장, 비워두면 답장 버튼 안 뜸)")]
    public string[] replyOptions = new string[] { "네, 할게요", "싫어요" };

    [Header("수락 시 뜨는 임무 토스트에 표시할 제목 (예: '수리의 마음')")]
    public string missionTitle = "";

    [Header("이 메시지가 오면 같이 켤 미션 타겟 (기존 시스템, 건드리지 않음)")]
    public GameObject missionTarget;

    [Header("탐색기 앱 연동 (이 메시지가 탐색기를 켜야 하는 메시지라면 체크)")]
    public bool activatesDetectorApp = false;
    public Transform detectorTarget; // 탐색기가 반짝이며 안내할 목표 (보통 돋보기/조사 지점)

    [Header("한 번만 보내고 다시는 안 보내려면 체크")]
    public bool sendOnlyOnce = true;

    private bool alreadySent = false;

    [Header("플레이어가 이 지점에 들어오면 자동 발동 (Collider2D의 Is Trigger 체크 필요)")]
    public bool triggerOnPlayerEnter = true;
    public string playerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggerOnPlayerEnter) return;
        if (!other.CompareTag(playerTag)) return;
        SendMissionMessage();
    }

    public void SendMissionMessage()
    {
        if (sendOnlyOnce && alreadySent)
            return;

        if (MessengerAppUI.Instance != null)
            MessengerAppUI.Instance.ReceiveMessage(contactName, messageText, photo, replyOptions);

        if (missionTarget != null)
            missionTarget.SetActive(true);

        if (activatesDetectorApp && DetectorAppUI.Instance != null && detectorTarget != null)
            DetectorAppUI.Instance.Activate(detectorTarget);

        LastSent = this;
        alreadySent = true;
    }
}