using UnityEngine;

public class LoopDialogue : Trigger
{
    protected override void HandleDialogEvent(string curEvent)
    {
        if (curEvent == "Escape")
        {
            StoryManager.instance.indexWeight = 2;
        }
        if (curEvent == "Loop")
        {
            StoryManager.instance.indexWeight = -2;
        }
    }

    protected override void TriggerEvent(){}
}
