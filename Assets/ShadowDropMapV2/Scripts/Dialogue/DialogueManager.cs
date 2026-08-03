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
    private string tempText;
    [SerializeField] float typingTime;
    [SerializeField] Queue<Dialogue> dialogueBox = new();

    WaitForSeconds waitTime;
    [HideInInspector] public bool isTyping;
    [HideInInspector] public bool panelState;

    [HideInInspector] public bool isEnd;
    bool isSkip;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnOffDialogue(isSkip);
        }
    }
    
    public void OnOffDialogue(bool isOn)
    {
        if (isOn)
        {
            //cam.DOOrthoSize(3.5f, 0.5f).SetEase(Ease.OutCubic);
            nameText.text = null;
            text.text = null;
            nameBar.rectTransform.DOLocalMoveX(-660, 0.5f);
            focusUI.SetActive(true);
        }
        else
        {
            text.text = null;
            nameBar.rectTransform.DOLocalMoveX(-1410, 0.5f);
            focusUI.SetActive(false);
        }
        textBar.rectTransform.DOSizeDelta(isOn ? new(1920, 300) : Vector2.zero, 0.5f);
        isSkip = !isSkip;
    }

    public void InputDialogue(Dialogue dialogue)
    {
        Debug.Log($"test");
        nameText.rectTransform.anchoredPosition = new Vector2(0, nameText.rectTransform.anchoredPosition.y);
        nameText.text = dialogue.name;
        tempText = dialogue.text;

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
                isTyping = false;
                isSkip = false;
                yield break;
            }
            text.text += tempText[i];
            yield return waitTime;
        }
        isTyping = false;
    }

    public void Skip()
    {
        isEnd = true;
        isTyping = true;
        isSkip = true;
    }
}
