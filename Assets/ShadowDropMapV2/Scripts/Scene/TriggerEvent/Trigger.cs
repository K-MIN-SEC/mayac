using System;
using UnityEngine;

public abstract class Trigger : MonoBehaviour
{
    [SerializeField] protected string eventName;

    protected virtual void OnEnable()
    {
        GlobalEvent.onEvent += HandleDialogEvent;
    }

    protected virtual void OnDisable()
    {
        GlobalEvent.onEvent -= HandleDialogEvent;
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

public static class GlobalEvent
{
    public static Action<string, bool> onEvent;

    public static void Event(string eventName, bool isEnd)
    {
        onEvent?.Invoke(eventName, isEnd);
    }
}
