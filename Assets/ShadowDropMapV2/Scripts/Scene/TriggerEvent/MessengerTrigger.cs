using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PhotoData
{
    public string photoId;
    public Sprite photoSprite;
}

public class MessengerTrigger : Trigger
{
    [SerializeField] TextAsset message;
    public List<PhotoData> photoList = new();
    private Dictionary<string, Sprite> photoDict = new();

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
            yield return new WaitForSeconds(1);
            if(!string.IsNullOrEmpty(textData[i].photoName)) textData[i].photo = GetPhoto(textData[i].photoName);
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
            if (!textData[i].isFromMe) return;
            if(!string.IsNullOrEmpty(textData[i].photoName)) textData[i].photo = GetPhoto(textData[i].photoName);
            MessengerAppUI.Instance.ShowReplyOptions(textData[i].replyOptions);
        }
    }

    void Start()
    {
        foreach (var data in photoList) photoDict.Add(data.photoId, data.photoSprite);
    }

    public Sprite GetPhoto(string id)
    {
        if (photoDict.TryGetValue(id, out Sprite sprite)) return sprite;
        photoDict.TryGetValue("Error", out Sprite error);
        return error;
    }

    protected override void TriggerEvent(){}
    protected override void EndEvent() {}
}