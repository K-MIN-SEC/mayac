using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class PhotoData
{
    public string photoId; // 예: "MissionPhoto_1"
    public Sprite photoSprite; // Assets 하위 아무 폴더에 있는 이미지나 드래그 앤 드롭 가능
}

public class MessengerTrigger : Trigger
{
    [SerializeField] TextAsset message;
    public List<PhotoData> photoList = new List<PhotoData>();
    private Dictionary<string, Sprite> photoDict = new Dictionary<string, Sprite>();

    public int index;

    protected override void HandleDialogEvent(string curEvent, bool isEnd)
    {
        if (curEvent == "SendMessage")
        {
            StartCoroutine(SendMessage());
        }
        if (curEvent == "ReplyMessage")
        {
            ReplyMessage();
        }
    }

    private IEnumerator SendMessage()
    {
        int index = StoryManager.instance.index;
        var textData = DataManager.instance.ParseMessageData(message.text, index);
        
        for (int i = 0; i < textData.Count; i++)
        {
            yield return new WaitForSeconds(0.7f);
            MessengerAppUI.Instance.ReceiveMessage(textData[i]);
        }
        StoryManager.instance.TryPlayStory("WaitNPC");
    }

    private void ReplyMessage()
    {
        int index = StoryManager.instance.index;
        var textData = DataManager.instance.ParseMessageData(message.text, index);

        for (int i = 0; i < textData.Count; i++)
        {
            if (!textData[i].isFromMe)
            {
                Debug.Log("ERROR");
                return;
            }
            MessengerAppUI.Instance.ShowReplyOptions(textData[i].replyOptions);
        }
    }

    void Awake()
    {
        // 런타임에 빠르게 찾을 수 있도록 딕셔너리로 변환
        foreach (var data in photoList)
        {
            photoDict.Add(data.photoId, data.photoSprite);
        }
    }

    public Sprite GetPhoto(string id)
    {
        if (photoDict.TryGetValue(id, out Sprite sprite))
        {
            return sprite;
        }
        return null;
    }

    protected override void TriggerEvent(){}
    protected override void EndEvent() {}
}