using UnityEngine;

public class NextPhase : Trigger
{
    protected override string eventName => "NextPhase";

    protected override void TriggerEvent()
    {
        Debug.Log("NextPhase Triggered");
        StoryManager.instance.SetupNextPhase();
    }
}
