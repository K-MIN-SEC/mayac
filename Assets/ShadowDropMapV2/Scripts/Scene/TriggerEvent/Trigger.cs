using System;
using UnityEngine;

public abstract class Trigger : MonoBehaviour
{
    protected string eventName;

    protected virtual void OnEnable()
    {
        StoryManager.instance.onEvent += HandleDialogEvent;
    }

    protected virtual void OnDisable()
    {
        StoryManager.instance.onEvent -= HandleDialogEvent;
    }

    protected virtual void HandleDialogEvent(string curEvent, bool isEnd)
    {
        if (curEvent == eventName)
        {
            if (isEnd) EndEvent();
            else TriggerEvent();
        }
    }
    
    protected abstract void TriggerEvent();
    protected abstract void EndEvent();
}