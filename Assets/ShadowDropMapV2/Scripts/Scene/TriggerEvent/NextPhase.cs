using UnityEngine;
using UnityEngine.SceneManagement;

public class NextPhase : Trigger
{
    protected override void Start()
    {
        DialogueManager.instance.onDialogueEnd += HandleDialogEvent;
    }

    protected override void TriggerEvent()
    {
        StoryManager.instance.SetupNextPhase();
    }

    void HandleDialogEvent()
    {
        if(StoryManager.instance.triggerId == "NextPhase") TriggerEvent();
        if(StoryManager.instance.triggerId == "NextScene") StoryManager.instance.SetupNextScene();
    }
}
