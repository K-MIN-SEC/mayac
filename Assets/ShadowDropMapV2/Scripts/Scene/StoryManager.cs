using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public abstract class StoryManager : MonoBehaviour
{
    public static StoryManager instance { get; private set; }
    public Action<string, bool> onEvent;

    public void Event(string eventName, bool isEnd)
    {
        onEvent?.Invoke(eventName, isEnd);
    }

    [SerializeField] protected TextAsset dialogue;
    [SerializeField] protected Image screenFilterImage;
    [SerializeField] protected bool isDialogue;
    [SerializeField] public int index;
    public bool isNext;
    public string triggerId = "";

    

    private void HandleDialogueEnd()
    {
        if (string.IsNullOrEmpty(triggerId)) index++;
    }

    public bool Trigger()
    {
        var temp = DataManager.instance.ParseDialogueData(dialogue.text, index);
        if (temp.Count == 0) return false;
        DialogueManager.instance.InitDialogue(temp);
        return true;
    }

    public bool TryPlayStory(string inputId)
    {
        //Debug.Log($"{triggerId} {inputId}");
        if (triggerId == inputId)
        {
            index++;
            triggerId = "";

            isDialogue = Trigger();

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
