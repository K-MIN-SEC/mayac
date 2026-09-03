using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro : StoryManager
{
    [SerializeField] private Image fade;
    bool isEnd;
    public bool isEndding = false;

    public override bool Trigger()
    {
        return base.Trigger();
    }

    protected override void Start()
    {
        base.Start();
        DialogueManager.instance.onDialogueEnd += FadeOut;
    }

    void FadeOut()
    {
        if (isEnd) return;
        isEnd = true;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(fade.DOFade(1, 0.5f));
        if (!isEndding)
        {
            sequence.OnComplete(() => { SceneManager.LoadScene(2); });
        }

    }

    void Update()
    {
        //if(Input.GetTouch(0).phase == TouchPhase.Began)
        if (Input.GetMouseButtonDown(0))
        {
            if (!isDialogue)
            {
                isDialogue = true;
                Trigger();
            }
            else isDialogue = DialogueManager.instance.TriggerDialogue();
        }

    }
}
