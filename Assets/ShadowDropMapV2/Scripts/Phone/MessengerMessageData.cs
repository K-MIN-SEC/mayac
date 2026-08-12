using UnityEngine;

// 메신저 대화 속 메시지 하나 (텍스트 + 선택적 사진 + 선택적 답장 선택지)
[System.Serializable]
public class MessengerMessageData
{
    [TextArea] public string text;
    public Sprite photo;
    public bool isFromMe = false;

    // 상대방 메시지에만 채워넣기: 플레이어가 고를 수 있는 답장 선택지
    // (내가 보낸 메시지에는 보통 비워두면 됨)
    public string[] replyOptions;

    // true면 왼쪽/오른쪽 말풍선이 아니라 가운데 정렬된 "알림 상자"로 표시됨 (예: 임무 수락 안내)
    public bool isSystemBox = false;
}