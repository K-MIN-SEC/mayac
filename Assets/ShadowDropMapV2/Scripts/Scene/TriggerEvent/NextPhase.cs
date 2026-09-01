using UnityEngine;

public class NextPhase : Trigger
{
    protected override string eventName => "NextPhase";

    protected override void Start()
    {
        DialogueManager.instance.onDialogueEnd += HandleDialogEvent;
    }

    protected override void TriggerEvent()
    {
        StoryManager.instance.triggerId = string.Empty;
        StoryManager.instance.SetupNextPhase();
    }

    void HandleDialogEvent()
    {
        if(StoryManager.instance.triggerId == eventName) TriggerEvent();
    }
}
