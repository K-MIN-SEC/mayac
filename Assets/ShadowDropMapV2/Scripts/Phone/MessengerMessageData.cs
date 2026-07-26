using UnityEngine;

// 메신저 대화 속 메시지 하나 (텍스트 + 선택적 사진)
[System.Serializable]
public class MessengerMessageData
{
    [TextArea] public string text;
    public Sprite photo;
    public bool isFromMe = false;
}