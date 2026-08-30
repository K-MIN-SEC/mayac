using UnityEngine;
using DG.Tweening;

public class InGame : StoryManager
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isNext) return;

            if (!isDialogue && string.IsNullOrEmpty(triggerId)) isDialogue = Trigger();
            else isDialogue = DialogueManager.instance.TriggerDialogue();
        }
    }

}