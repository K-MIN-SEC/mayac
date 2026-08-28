using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro : StoryManager
{
    [SerializeField] private Image fade;
    bool isEnd;

    public override bool Trigger()
    {
        return base.Trigger();
        // var temp = DataManager.instance.ParseDialogueData(dialogue.text, dialogueIndex);
        // if (temp.Count == 0) return false;
        // DialogueManager.instance.InitDialogue(temp);
        // return true;
    }

    protected override void Start()
    {
        base.Start();
        DialogueManager.instance.onDialogueEnd += FadeOut;
    }

    void FadeOut()
    {
        var temp = DataManager.instance.ParseDialogueData(dialogue.text, dialogueIndex);
        if(temp.Count != 0) return;
        fade.DOFade(1, 0.5f).OnComplete(() =>
                {
                    Debug.Log($"[Intro] Load Scene");
                    SceneManager.LoadScene(2);
                });
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
            }else isDialogue = DialogueManager.instance.TriggerDialogue();
        }

    }
}
