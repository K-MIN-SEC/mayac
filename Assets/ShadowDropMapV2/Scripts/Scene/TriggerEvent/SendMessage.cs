using System;
using System.Collections.Generic;
using UnityEngine;

public class SendMessage : Trigger
{
    [SerializeField] TextAsset message;
    List<MessengerMessageData> textData;
    public int index;


    protected override void TriggerEvent()
    {
        textData = DataManager.instance.ParseMessageData(message.text, index);
        for(int i = 0; i < textData.Count; i++)
        {
            MessengerAppUI.Instance.ReceiveMessage(textData[i]);
        }
        index++;
    }

    protected override void EndEvent()
    {

    }

    void Start()
    {
        eventName = "SendMessage";
    }
}
