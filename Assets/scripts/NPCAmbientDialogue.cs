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

    bool showing = false;

    void Start()
    {
        if (bubble != null)
            bubble.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (showing)
            return;

        StartCoroutine(ShowDialogue());
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

        bubble.SetActive(false);

        showing = false;
    }

    void ResizeBubble()
    {
        dialogueText.ForceMeshUpdate();

        Vector2 size = dialogueText.GetRenderedValues(false);

        float width = Mathf.Clamp(size.x + 40f, 120f, 350f);
        float height = size.y + 30f;

        bubbleRect.sizeDelta = new Vector2(width, height);
    }
}