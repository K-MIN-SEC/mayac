using UnityEngine;
using DG.Tweening;

public class InGame : StoryManager
{
    protected override void Start()
    {
        base.Start();
        FadeInOut(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isNext || isFading) return;

            if (!isDialogue && string.IsNullOrEmpty(triggerId)) isDialogue = Trigger();
            else isDialogue = DialogueManager.instance.TriggerDialogue();
        }
    }

}