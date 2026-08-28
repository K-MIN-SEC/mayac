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
    public string triggerId = "";

    private bool isTransitioning = false;
    

    private void HandleDialogueEnd()
    {
        if (!isTransitioning && string.IsNullOrEmpty(triggerId))
        {
            Debug.Log("handle?");
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
        //Debug.Log($"{triggerId} {inputId}");
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
}
