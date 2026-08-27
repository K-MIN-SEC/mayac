using System;
using UnityEngine;

public abstract class Trigger : MonoBehaviour
{
    protected virtual string eventName => "";

    protected virtual void Start()
    {
        StoryManager.instance.onEvent += HandleDialogEvent;
    }

    protected virtual void OnDisable()
    {
        StoryManager.instance.onEvent -= HandleDialogEvent;
    }

    protected virtual void HandleDialogEvent(string curEvent)
    {
        if (curEvent == eventName) TriggerEvent();
    }
    
    protected abstract void TriggerEvent();
}