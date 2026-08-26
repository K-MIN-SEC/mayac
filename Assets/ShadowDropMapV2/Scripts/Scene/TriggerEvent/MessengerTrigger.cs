using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class MessengerTrigger : Trigger
{
    [SerializeField] TextAsset message;
    List<MessengerMessageData> textData;
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

    protected override void TriggerEvent(){}
    protected override void EndEvent() {}
}
