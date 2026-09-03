using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public abstract class StoryManager : MonoBehaviour
{
    public static StoryManager instance { get; private set; }
    public Action<string> onEvent;

    public void Event(string eventName)
    {
        //if(!string.IsNullOrEmpty(eventName)) Debug.Log($"[StoryManager] Event Triggered: {eventName}", this);
        onEvent?.Invoke(eventName);
    }

    [SerializeField] protected List<TextAsset> dialogue;
    [SerializeField] protected List<TextAsset> npcDialogue;
    [SerializeField] protected Image screenFilterImage;
    [SerializeField] protected Image fadeImage;
    [SerializeField] protected bool isDialogue;

    [Header("Story State")]
    public int dialogueIndex;
    public int dialogueTextIndex;
    public int targetIndex;

    public int indexWeight = 1;
    public bool isNext;
    public bool isFading = false;
    public string triggerId = "";

    protected bool isTransitioning = false;


    private void HandleDialogueEnd()
    {
        if (!isTransitioning && string.IsNullOrEmpty(triggerId))
        {
            //Debug.Log($"HandleDialogueEnd {dialogueTextIndex} + {indexWeight} = {dialogueTextIndex + indexWeight}");
            dialogueTextIndex += indexWeight;
            indexWeight = 1;
        }
    }

    public virtual bool Trigger()
    {
        var temp = DataManager.instance.ParseDialogueData(dialogue[dialogueIndex].text, dialogueTextIndex);
        if (temp.Count == 0) return false;
        DialogueManager.instance.InitDialogue(temp);
        return true;
    }

    public bool TryPlayStory(string inputId)
    {
        if (triggerId == inputId)
        {
            isTransitioning = true;

            //Debug.Log($"TryPlayStory {dialogueTextIndex} + {indexWeight} = {dialogueTextIndex + indexWeight}");
            dialogueTextIndex += indexWeight;
            triggerId = "";
            indexWeight = 1;

            isDialogue = Trigger();

            isTransitioning = false;
            return true;
        }

        return false;
    }

    void Awake()
    {
        instance = this;
    }

    protected virtual void Start()
    {
        DialogueManager.instance.onDialogueEnd += HandleDialogueEnd;
    }

    public void FadeInOut(bool isOut)
    {
        isFading = true;
        Sequence sequence = DOTween.Sequence();
        if (isOut)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            sequence.Append(fadeImage.DOFade(1, 1f));
        }
        else
        {
            fadeImage.color = Color.black;
            sequence.Append(fadeImage.DOFade(0, 1f)).SetEase(Ease.Linear);
        }
        sequence.OnComplete(() => { isFading = false; });
    }

    public void ChangeToNight(bool isNight)
    {
        // if (isNight) screenFilterImage.DOColor(new Color(0.0f, 0.1f, 0.4f, 0.5f), 3.0f);
        // else screenFilterImage.DOColor(new Color(0, 0, 0, 0), 3.0f);
        if (isNight) screenFilterImage.color = new Color(0.0f, 0.1f, 0.4f, 0.5f);
        else screenFilterImage.color = new Color(0, 0, 0, 0);
    }

    public virtual void SetupNextPhase() { }
    public virtual void SetupNextScene() { }
}
