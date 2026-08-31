using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public abstract class StoryManager : MonoBehaviour
{
    public static StoryManager instance { get; private set; }
    public Action<string> onEvent;

    public void Event(string eventName)
    {
        onEvent?.Invoke(eventName);
    }

    [SerializeField] protected TextAsset dialogue;
    [SerializeField] protected Image screenFilterImage;
    [SerializeField] protected bool isDialogue;
    [SerializeField] public int dialogueIndex;
    [SerializeField] public int targetIndex;
    public int indexWeight = 1;
    public bool isNext;
    public bool isFading = false;
    public string triggerId = "";

    private bool isTransitioning = false;


    private void HandleDialogueEnd()
    {
        if (!isTransitioning && string.IsNullOrEmpty(triggerId))
        {
            dialogueIndex++;
        }
    }

    public virtual bool Trigger()
    {
        var temp = DataManager.instance.ParseDialogueData(dialogue.text, dialogueIndex);
        if (temp.Count == 0) return false;
        DialogueManager.instance.InitDialogue(temp);
        return true;
    }

    public bool TryPlayStory(string inputId)
    {
        if (triggerId == inputId)
        {
            isTransitioning = true;

            dialogueIndex += indexWeight;
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
        if (isOut) sequence.Append(screenFilterImage.DOColor(new Color(0, 0, 0, 1), 1.5f));
        else
        {
            screenFilterImage.color = Color.black;
            sequence.Append(screenFilterImage.DOColor(new Color(0, 0, 0, 0), 1.5f));
        }
        sequence.OnComplete(() => { isFading = false; });
    }

    public void ChangeToNight(bool isNight)
    {
        if (isNight) screenFilterImage.DOColor(new Color(0.0f, 0.1f, 0.4f, 0.5f), 3.0f);
        else screenFilterImage.DOColor(new Color(0, 0, 0, 0), 3.0f);
    }
}
