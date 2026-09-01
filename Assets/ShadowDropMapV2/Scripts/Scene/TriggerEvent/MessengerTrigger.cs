using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MessageData
{
    public TextAsset message;
    public List<PhotoData> photoList = new();
}

[Serializable]
public class PhotoData
{
    public string photoId;
    public Sprite photoSprite;
}

public class MessengerTrigger : Trigger
{
    public List<MessageData> messageList = new();
    private Dictionary<string, Sprite> photoDict = new();
    [SerializeField] private Sprite error;

    protected override void HandleDialogEvent(string curEvent)
    {
        base.HandleDialogEvent(curEvent);
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
        StoryManager.instance.isNext = false;
        int index = StoryManager.instance.dialogueTextIndex;
        int messageIndex = StoryManager.instance.dialogueIndex;
        if (messageList[messageIndex].message != null)
        {
            var textData = DataManager.instance.ParseMessageData(messageList[messageIndex].message.text, index);

            for (int i = 0; i < textData.Count; i++)
            {
                yield return new WaitForSeconds(1);
                if (!string.IsNullOrEmpty(textData[i].photoName)) textData[i].photo = GetPhoto(textData[i].photoName);
                MessengerAppUI.Instance.ReceiveMessage(textData[i]);
            }
        }


        StoryManager.instance.isNext = true;
        DialogueManager.instance.TriggerDialogue();
    }

    private void ReplyMessage()
    {
        int index = StoryManager.instance.dialogueTextIndex;
        int messageIndex = StoryManager.instance.dialogueIndex;
        if (messageList[messageIndex].message != null)
        {
            var textData = DataManager.instance.ParseMessageData(messageList[messageIndex].message.text, index);

            for (int i = 0; i < textData.Count; i++)
            {
                if (!textData[i].isFromMe) return;
                if (!string.IsNullOrEmpty(textData[i].photoName)) textData[i].photo = GetPhoto(textData[i].photoName);
                MessengerAppUI.Instance.ShowReplyOptions(textData[i].replyOptions);
            }
        }
    }

    protected override void Init()
    {
        photoDict.Clear();
        int messageIndex = StoryManager.instance.dialogueIndex;
        if (messageList[messageIndex].photoList != null)
        {
            foreach (var data in messageList[messageIndex].photoList)
                photoDict[data.photoId] = data.photoSprite;
        }
    }

    public Sprite GetPhoto(string id)
    {
        if (photoDict.TryGetValue(id, out Sprite sprite)) return sprite;
        return error;
    }

    protected override void TriggerEvent() { }
}