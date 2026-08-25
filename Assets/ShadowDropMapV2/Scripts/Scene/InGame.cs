using UnityEngine;
using DG.Tweening;

public class InGame : StoryManager
{

    void FadeInOut(bool isOut)
    {
        if (isOut)
        {
            screenFilterImage.color = new Color(1, 1, 1, 0);
            screenFilterImage.DOFade(1, 0.5f);
        }
        else
        {
            screenFilterImage.color = new Color(1, 1, 1, 1);
            screenFilterImage.DOFade(0, 0.5f);
        }
    }

    public void ChangeToNight()
    {
        Color nightColor = new Color(0.0f, 0.1f, 0.4f, 0.5f);
        screenFilterImage.DOColor(nightColor, 3.0f);
    }


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