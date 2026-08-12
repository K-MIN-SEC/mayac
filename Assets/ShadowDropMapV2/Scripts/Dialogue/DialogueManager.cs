using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

[System.Serializable]
public class Dialogue
{
    public int index;
    public string name;
    public string text;

    public bool isOption;
    public string OptionA;
    public string OptionB;
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
    WaitForSeconds waitTime;
    [HideInInspector] public bool isTyping;
    [HideInInspector] public bool panelState;

    [HideInInspector] public bool isEnd;
    bool isSkip = true;
    bool isOption = false;
    bool isOptionA = false;

    void Start()
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

    public void InitDialogue(Queue<Dialogue> dialogueBox)
    {
        QuarterViewWalkableNavigator2D.instance.isDialogue = true;
        this.dialogueBox = dialogueBox;
        OnOffDialogue(true);
        InputDialogue(dialogueBox.Dequeue());
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
            if (!isOptionA) dialogueBox.Dequeue();
            InputDialogue(dialogueBox.Dequeue());
            if (isOption) dialogueBox.Dequeue();
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
        }
        focusUI.SetActive(false);
        QuarterViewWalkableNavigator2D.instance.isDialogue = false;
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
        isOption = false;
        isTyping = false;
        optionUI.SetActive(false);
    }

    public void InputDialogue(Dialogue dialogue)
    {
        curDialogue = dialogue;
        Debug.Log($"[Dialogue] {curDialogue.name} : {curDialogue.text}\nOptionA : {curDialogue.OptionA} OptionB : {curDialogue.OptionB}");
        nameText.rectTransform.anchoredPosition = new Vector2(0, nameText.rectTransform.anchoredPosition.y);
        nameText.text = curDialogue.name;
        tempText = curDialogue.text;

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
    }

    public void Skip()
    {
        isEnd = true;
        isTyping = true;
        isSkip = true;
    }
}
