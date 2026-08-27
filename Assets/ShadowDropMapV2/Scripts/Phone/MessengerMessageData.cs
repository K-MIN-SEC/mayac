using UnityEngine;

// 메신저 대화 속 메시지 하나 (텍스트 + 선택적 사진 + 선택적 답장 선택지)
[System.Serializable]
public class MessengerMessageData
{
    public string name;
    [TextArea] public string text;
    public bool isFromMe = false;

    public string photoName;
    public Sprite photo;
    public string[] replyOptions;

    public bool isSystemBox = false;
}