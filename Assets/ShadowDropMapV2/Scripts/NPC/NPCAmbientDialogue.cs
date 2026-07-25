using System.Collections;
using TMPro;
using UnityEngine;

public class NPCAmbientDialogue : MonoBehaviour
{
    [Header("대사")]
    [TextArea]
    public string[] dialogues;

    [Header("UI")]
    public GameObject bubble;
    public TMP_Text dialogueText;
    public RectTransform bubbleRect;

    [Header("설정")]
    public float showTime = 3f;

    bool showing;
    Coroutine dialogueRoutine;

    void Start()
    {
        if (bubble != null)
            bubble.SetActive(false);
    }

    void OnDisable()
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }

        if (bubble != null)
            bubble.SetActive(false);

        showing = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (showing)
            return;

        if (dialogues == null || dialogues.Length == 0 || bubble == null || dialogueText == null)
            return;

        dialogueRoutine = StartCoroutine(ShowDialogue());
    }

    IEnumerator ShowDialogue()
    {
        showing = true;

        bubble.SetActive(true);

        // 랜덤 대사 출력
        int randomIndex = Random.Range(0, dialogues.Length);
        dialogueText.text = dialogues[randomIndex];

        // 텍스트 크기 계산
        ResizeBubble();

        yield return new WaitForSeconds(showTime);

        if (bubble != null)
            bubble.SetActive(false);

        showing = false;
        dialogueRoutine = null;
    }

    void ResizeBubble()
    {
        if (dialogueText == null || bubbleRect == null)
            return;

        dialogueText.ForceMeshUpdate();

        Vector2 size = dialogueText.GetRenderedValues(false);

        float width = Mathf.Clamp(size.x + 40f, 120f, 350f);
        float height = size.y + 30f;

        bubbleRect.sizeDelta = new Vector2(width, height);
    }
}
