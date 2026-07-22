using UnityEngine;
using TMPro;
using System.Collections;

public class NPCAmbientDialogue : MonoBehaviour
{
    [Header("대사")]
    [TextArea]
    public string dialogue;

    [Header("설정")]
    public GameObject dialogueObject;     // 머리 위 텍스트
    public TMP_Text dialogueText;

    public float showTime = 2.5f;

    private bool hasShown = false;

    private void Start()
    {
        Debug.Log("NPC 시작");

        dialogueObject.SetActive(true);

        dialogueText.text = "테스트";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasShown)
        {
            StartCoroutine(ShowDialogue());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hasShown = false;
            dialogueObject.SetActive(false);
        }
    }

    IEnumerator ShowDialogue()
    {
        hasShown = true;

        dialogueObject.SetActive(true);

        yield return new WaitForSeconds(showTime);

        dialogueObject.SetActive(false);
    }
}