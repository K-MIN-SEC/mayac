using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;


public class DialogueManager : MonoBehaviour
{
    [SerializeField] Image panel;

    [Header("Dialogue")]
    [SerializeField] Image textBar;
    [SerializeField] Image nameBar;
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_Text nameText;
    [SerializeField] GameObject focusUI;
    private string tempText;
    [SerializeField] float typingTime;
    WaitForSeconds waitTime;
    [HideInInspector] public bool isTyping;
    [HideInInspector] public bool panelState;
    [HideInInspector] public bool isEnd;
    bool isSkip;

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
        panel.rectTransform.DOSizeDelta(Vector2.zero, 0.5f);
    }
}
