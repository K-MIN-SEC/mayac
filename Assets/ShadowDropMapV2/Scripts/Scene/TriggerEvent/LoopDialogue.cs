using UnityEngine;

public class LoopDialogue : Trigger
{
    protected override void HandleDialogEvent(string curEvent)
    {
        if (curEvent == "Escape")
        {
            StoryManager.instance.indexWeight = 2;
            //Debug.Log($"[Escape] {StoryManager.instance.indexWeight}");
        }
        if (curEvent == "Loop")
        {
            StoryManager.instance.indexWeight = -2;
            //Debug.Log($"[LoopDialogue] {StoryManager.instance.indexWeight}");
        }
        
    }

    protected override void TriggerEvent(){}
}
