using UnityEngine;

public class NextPhase : Trigger
{
    protected override string eventName => "NextPhase";

    protected override void TriggerEvent()
    {
        StoryManager.instance.SetupNextPhase();
    }
}
