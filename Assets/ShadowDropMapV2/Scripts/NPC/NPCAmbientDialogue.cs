using System.Collections;
using TMPro;
using UnityEngine;

public class NPCAmbientDialogue : MonoBehaviour
{
    [Header("Random or single dialogue")]
    [TextArea]
    public string[] dialogues;

    [Header("Sequential dialogue")]
    [Tooltip("When enabled, random dialogue is ignored and sequentialDialogues are shown in order.")]
    public bool useSequentialDialogue;

    [TextArea]
    public string[] sequentialDialogues;

    [Min(0f)]
    public float dialogueInterval = 2f;

    [Header("UI")]
    public GameObject bubble;
    public TMP_Text dialogueText;
    public RectTransform bubbleRect;

    [Header("Timing")]
    [Min(0f)]
    public float showTime = 3f;

    [Min(0f)]
    public float dialogueDelay = 1f;

    private bool showing;
    private Coroutine dialogueRoutine;

    private void Start()
    {
        HideBubble();
    }

    private void OnDisable()
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }

        HideBubble();
        showing = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || showing || !CanShowDialogue())
            return;

        dialogueRoutine = StartCoroutine(ShowDialogue());
    }

    private bool CanShowDialogue()
    {
        if (bubble == null || dialogueText == null)
            return false;

        string[] activeDialogues =
            useSequentialDialogue ? sequentialDialogues : dialogues;

        return activeDialogues != null && activeDialogues.Length > 0;
    }

    private IEnumerator ShowDialogue()
    {
        showing = true;

        // 말풍선이 나타나기 전 대기
        yield return new WaitForSeconds(dialogueDelay);

        bubble.SetActive(true);

        if (useSequentialDialogue)
        {
            yield return ShowSequentialDialogue();
        }
        else
        {
            int randomIndex = Random.Range(0, dialogues.Length);

            SetDialogue(dialogues[randomIndex]);

            yield return new WaitForSeconds(showTime);
        }

        HideBubble();
        showing = false;
        dialogueRoutine = null;
    }

    private IEnumerator ShowSequentialDialogue()
    {
        for (int index = 0; index < sequentialDialogues.Length; index++)
        {
            SetDialogue(sequentialDialogues[index]);

            bool isLastDialogue =
                index == sequentialDialogues.Length - 1;

            float waitTime =
                isLastDialogue ? showTime : dialogueInterval;

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SetDialogue(string text)
    {
        dialogueText.text = text ?? string.Empty;
        ResizeBubble();
    }

    private void HideBubble()
    {
        if (bubble != null)
            bubble.SetActive(false);
    }

    private void ResizeBubble()
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