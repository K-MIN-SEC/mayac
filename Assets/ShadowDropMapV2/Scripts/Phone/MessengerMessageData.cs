using UnityEngine;

// 메신저 대화 속 메시지 하나 (텍스트 + 선택적 사진 + 선택적 답장 선택지 + 보낸 시간)
[System.Serializable]
public class MessengerMessageData
{
    [TextArea] public string text;
    public Sprite photo;
    public bool isFromMe = false;
    public string time = ""; // 예: "오후 3:30" (ReceiveMessage/SendReply에서 자동으로 채워짐)

    // 상대방 메시지에만 채워넣기: 플레이어가 고를 수 있는 답장 선택지
    // (내가 보낸 메시지에는 보통 비워두면 됨)
    public string[] replyOptions;
}