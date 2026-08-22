using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    [SerializeField] private TextAsset dialogue;
    [SerializeField] private int index;
    [SerializeField] private Image img;
    private bool isDialogue;
    bool isEnd;

    public void Trigger()
    {
        var temp = DataManager.instance.ParseDialogueData(dialogue.text, index);
        if(temp.Count == 0) return;
        DialogueManager.instance.InitDialogue(temp);
        index++;
    }

    void Start()
    {
        DialogueManager.instance.onDialogueEnd += FadeOut;
    }

    void FadeOut()
    {
        var temp = DataManager.instance.ParseDialogueData(dialogue.text, index);
        if(temp.Count != 0) return;
        img.DOFade(1, 1f).OnComplete(() =>
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
                Trigger();
                isDialogue = true;
            }else isDialogue = DialogueManager.instance.TriggerDialogue();
        }

    }
}
