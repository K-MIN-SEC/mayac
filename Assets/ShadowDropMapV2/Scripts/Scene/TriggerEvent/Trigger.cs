using System;
using UnityEngine;

public abstract class Trigger : MonoBehaviour
{
    protected virtual string eventName => "_";

    protected virtual void Start()
    {
        StoryManager.instance.onEvent += HandleDialogEvent;
        Init();
    }

    protected virtual void OnDisable()
    {
        StoryManager.instance.onEvent -= HandleDialogEvent;
    }

    protected virtual void HandleDialogEvent(string curEvent)
    {
        if (curEvent == eventName) TriggerEvent();
        if(curEvent == "Init") Init();
    }
    
    protected abstract void TriggerEvent();
    protected virtual void Init(){}
}