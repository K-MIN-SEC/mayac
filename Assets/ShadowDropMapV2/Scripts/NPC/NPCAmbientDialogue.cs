using System.Collections;
using UnityEngine;

public class NPCAmbientDialogue : MonoBehaviour
{
    [Header("Dialogue Data")]
    [TextArea] public string[] dialogues;
    
    [Header("Sequential Mode")]
    public bool useSequentialDialogue;
    [TextArea] public string[] sequentialDialogues;
    [Min(0f)] public float dialogueInterval = 2f;

    [Header("UI Reference")]
    public GameObject bubblePrefab;
    public Transform canvasTransform; 

    [Header("Timing")]
    [Min(0f)] public float showTime = 3f;
    [Min(0f)] public float dialogueDelay = 1f;

    private bool showing;
    private Coroutine dialogueRoutine;
    private GameObject activeBubble;

    void Start()
    {
        canvasTransform = MainCanvas.instance.bubble;
    }

    private void OnDisable()
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }
        ClearBubble();
        showing = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || showing || !CanShowDialogue())
            return;

        dialogueRoutine = StartCoroutine(ShowDialogueRoutine());
    }

    private bool CanShowDialogue()
    {
        if (bubblePrefab == null || canvasTransform == null) return false;
        
        string[] activeDialogs = useSequentialDialogue ? sequentialDialogues : dialogues;
        return activeDialogs != null && activeDialogs.Length > 0;
    }

    private IEnumerator ShowDialogueRoutine()
    {
        showing = true;
        yield return new WaitForSeconds(dialogueDelay);

        if (useSequentialDialogue)
        {
            yield return ShowSequentialRoutine();
        }
        else
        {
            int randomIndex = Random.Range(0, dialogues.Length);
            SpawnOrUpdateBubble(dialogues[randomIndex]);
            yield return new WaitForSeconds(showTime);
        }

        ClearBubble();
        showing = false;
        dialogueRoutine = null;
    }

    private IEnumerator ShowSequentialRoutine()
    {
        for (int index = 0; index < sequentialDialogues.Length; index++)
        {
            SpawnOrUpdateBubble(sequentialDialogues[index]);

            bool isLastDialogue = (index == sequentialDialogues.Length - 1);
            float waitTime = isLastDialogue ? showTime : dialogueInterval;
            
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnOrUpdateBubble(string text)
    {
        if (activeBubble == null)
        {
            activeBubble = Instantiate(bubblePrefab, canvasTransform);
        }
        
        if (activeBubble.TryGetComponent(out OffScreenBubbleUI bubbleUI))
        {
            bubbleUI.Initialize(this.transform, text);
        }
    }

    private void ClearBubble()
    {
        if (activeBubble != null)
        {
            Destroy(activeBubble);
            activeBubble = null;
        }
    }
}