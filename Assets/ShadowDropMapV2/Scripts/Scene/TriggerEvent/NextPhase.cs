using UnityEngine;
using UnityEngine.SceneManagement;

public class NextPhase : Trigger
{
    protected override void Start()
    {
        StoryManager.instance.onEvent += HandleDialogEvent;
        DialogueManager.instance.onDialogueEnd += HandleDialogEvent;
    }

    protected override void TriggerEvent()
    {
        StoryManager.instance.SetupNextPhase();
    }

    void HandleDialogEvent()
    {
        if(StoryManager.instance.triggerId == "NextPhase") TriggerEvent();
    }

    protected override void HandleDialogEvent(string curEvent)
    {
        if (curEvent == "NextScene") StoryManager.instance.SetupNextScene();
    }
}
