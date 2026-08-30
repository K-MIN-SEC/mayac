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
    public string questName;

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
    [SerializeField] TMP_Text questText;

    [SerializeField] GameObject focusUI;
    [SerializeField] GameObject optionUI;
    [SerializeField] TMP_Text optionAText;
    [SerializeField] TMP_Text optionBText;

    private string tempText;
    [SerializeField] float typingTime;

    public Queue<Dialogue> dialogueBox = new();
    [SerializeField] Dialogue curDialogue;

    [HideInInspector] public bool isTyping;
    [HideInInspector] public bool isEnd;

    bool isOn;
    bool isSkip;
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
            if (isOption && !isOptionA) { dialogueBox.Dequeue(); isOption = false; }
            InputText(dialogueBox.Dequeue());
            if (isOption && isOptionA) { dialogueBox.Dequeue(); isOption = false; }
        }
        else if (!isOption)
        {
            StartCoroutine(TypingText());
        }
        return true;
    }

    public void OnOffDialogue(bool onOff)
    {
        if (onOff)
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
            if(!string.IsNullOrEmpty(curDialogue.questName) && questText != null) questText.text = curDialogue.questName;
            onDialogueEnd?.Invoke();
        }
        focusUI.SetActive(onOff);
        //임시 행동 정지
        if (QuarterViewWalkableNavigator2D.instance != null) QuarterViewWalkableNavigator2D.instance.isDialogue = onOff;
        textBar.rectTransform.DOSizeDelta(onOff ? new(1920, 300) : Vector2.zero, 0.5f);
        isSkip = false;
    }

    void OnOption()
    {
        optionAText.text = curDialogue.OptionA;
        optionBText.text = curDialogue.OptionB;
        optionUI.SetActive(true);
    }

    public void ChooseOption(bool isA)
    {
        //Debug.Log("Choose");
        isOptionA = isA;
        isTyping = false;
        optionUI.SetActive(false);
        TriggerDialogue();
    }

    public void InputText(Dialogue dialogue)
    {
        //Debug.Log($"[Dialogue] {dialogue.name} : {dialogue.text}");
        curDialogue = dialogue;
        StoryManager.instance.triggerId = curDialogue.triggerType;
        StoryManager.instance.Event(curDialogue.startEventName);

        if (string.IsNullOrEmpty(curDialogue.text))
        {
            isTyping = false;
            StoryManager.instance.Event(curDialogue.endEventName);

            if (dialogueBox.Count == 0) OnOffDialogue(false);
            return;
        }

        OnOffDialogue(true);
        nameText.rectTransform.anchoredPosition = new Vector2(0, nameText.rectTransform.anchoredPosition.y);
        nameText.text = curDialogue.name;
        tempText = curDialogue.text;
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
        }
        else isTyping = false;
        StoryManager.instance.Event(curDialogue.endEventName);
    }

    //그거 해놔야함
    //다이얼로그 오브젝트 띄웠을때,얘말고 다른 상호작용 안되게 막아야함

    public void Skip()
    {
        isEnd = true;
        isTyping = true;
        isSkip = true;
    }
}
