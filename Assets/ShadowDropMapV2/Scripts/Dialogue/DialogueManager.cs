using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class Dialogue
{
    public int index;
    public string name;
    public string text;

    public string triggerType;

    public bool isOption;
    public string OptionA;
    public string OptionB;

    public string startEventName;
    public string endEventName;

    public bool isSound;
}


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance { get; private set; }

    [Header("Dialogue")]
    [SerializeField] Image textBar;
    [SerializeField] Image nameBar;
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_Text nameText;

    [SerializeField] GameObject focusUI;
    [SerializeField] GameObject optionUI;
    [SerializeField] TMP_Text optionAText;
    [SerializeField] TMP_Text optionBText;

    private string tempText;
    [SerializeField] float typingTime;

    public Queue<Dialogue> dialogueBox = new();
    [SerializeField] Dialogue curDialogue;

    [HideInInspector] public bool isTyping;
    [HideInInspector] public bool panelState;
    [HideInInspector] public bool isEnd;
    bool isSkip = true;
    bool isOption = false;
    bool isOptionA = false;

    public Action onDialogueEnd;

    WaitForSeconds waitTime;

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        instance = this;
        text.text = null;
        nameText.text = null;
        waitTime = new WaitForSeconds(typingTime);
    }

    public void InitDialogue(Queue<Dialogue> InputDialogue)
    {
        
        dialogueBox = InputDialogue;
        OnOffDialogue(true);
        if (dialogueBox.Count > 0)
        {
            InputText(dialogueBox.Dequeue());
        }
    }

    public bool TriggerDialogue()
    {
        if (dialogueBox.Count == 0 && !isTyping || isEnd)
        {
            OnOffDialogue(false);
            return false;
        }
        else if (!isTyping)
        {
            if (isOption && !isOptionA) {dialogueBox.Dequeue(); isOption = false;}
            InputText(dialogueBox.Dequeue());
            if (isOption && isOptionA) {dialogueBox.Dequeue(); isOption = false;}
        }
        else if (!isOption)
        {
            StartCoroutine(TypingText());
        }
        return true;
    }

    public void OnOffDialogue(bool isOn)
    {
        if (isOn)
        {
            //cam.DOOrthoSize(3.5f, 0.5f).SetEase(Ease.OutCubic);
            nameText.text = null;
            text.text = null;
            nameBar.rectTransform.DOLocalMoveX(-660, 0.5f);
        }
        else
        {
            text.text = null;
            nameBar.rectTransform.DOLocalMoveX(-1410, 0.5f);
            onDialogueEnd?.Invoke();
        }
        focusUI.SetActive(isOn);
        //임시 행동 정지
        if(QuarterViewWalkableNavigator2D.instance != null) QuarterViewWalkableNavigator2D.instance.isDialogue = isOn;
        textBar.rectTransform.DOSizeDelta(isOn ? new(1920, 300) : Vector2.zero, 0.5f);
        isSkip = !isSkip;
    }

    void OnOption()
    {
        optionAText.text = curDialogue.OptionA;
        optionBText.text = curDialogue.OptionB;
        optionUI.SetActive(true);
    }

    public void ChooseOption(bool isA)
    {
        Debug.Log("Choose");
        isOptionA = isA;
        isTyping = false;
        optionUI.SetActive(false);
        TriggerDialogue();
    }

    public void InputText(Dialogue dialogue)
    {
        Debug.Log($"[Dialogue] {dialogue.name} : {dialogue.text}");
        curDialogue = dialogue;
        nameText.rectTransform.anchoredPosition = new Vector2(0, nameText.rectTransform.anchoredPosition.y);
        nameText.text = curDialogue.name;
        tempText = curDialogue.text;
        tempText = tempText.Replace("\\", "\n");
        tempText = tempText.Replace("|", ",");
        StoryManager.instance.Event(curDialogue.startEventName, false);
        StoryManager.instance.triggerId = curDialogue.triggerType;
        isSkip = false;

        StartCoroutine(TypingText());
    }


    public IEnumerator TypingText()
    {
        if (isTyping) { isSkip = true; yield break; }
        isTyping = true;
        text.text = null;
        for (int i = 0; i < tempText.Length; i++)
        {
            if (isSkip)
            {
                text.text = tempText;
                isSkip = false;
                break;
            }
            text.text += tempText[i];
            yield return waitTime;
        }
        if (curDialogue.isOption)
        {
            isOption = true;
            OnOption();
        }else isTyping = false;
        StoryManager.instance.Event(curDialogue.endEventName, true);
    }

    public void Skip()
    {
        isEnd = true;
        isTyping = true;
        isSkip = true;
    }
}
