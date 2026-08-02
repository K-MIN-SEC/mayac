using UnityEngine;

// 미션 메시지(사진 + "여기다가 숨겨주세용~")를 메신저로 보내고,
// 동시에 나침반이 가리킬 목표(MissionTarget)를 켜주는 트리거용 스크립트.
// InteractionPoint2D의 On Interact 같은 UnityEvent에 SendMissionMessage() 하나만 연결하면 됨.
public class MissionMessageSender : MonoBehaviour
{
    [Header("메신저로 보낼 내용")]
    public string contactName = "???";
    [TextArea] public string messageText = "여기다가 숨겨주세용~";
    public Sprite photo;

    [Header("이 메시지가 오면 같이 켤 미션 타겟")]
    public GameObject missionTarget;

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
            MessengerAppUI.Instance.ReceiveMessage(contactName, messageText, photo);

        if (missionTarget != null)
            missionTarget.SetActive(true);

        alreadySent = true;
    }
}