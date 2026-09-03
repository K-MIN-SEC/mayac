using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientConversation : MonoBehaviour
{
    [Header("Data")]
    public int textIndex;
    [SerializeField] private List<string> dialogues;

    [Header("Settings")]
    public float textDelay = 2.0f;     // 1. 말풍선이 화면에 떠 있는 시간 (읽는 시간)
    public float intervalDelay = 0.5f; // 2. 말풍선 사이의 간격 (대화가 넘어갈 때의 쉬는 시간)
    public float cooldownTime = 3.0f;  // 3. 대화가 완전히 끝난 후 재발동까지의 대기 시간
    
    public bool playOnlyOnce = false;
    
    private bool hasPlayed = false;
    private bool isPlaying = false;
    private bool isLoaded = false;

    public List<NPCAmbientDialogue> groupNpcs;

    private void Start()
    {
        groupNpcs = new List<NPCAmbientDialogue>(GetComponentsInChildren<NPCAmbientDialogue>());
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        
        if (!isLoaded)
        {
            dialogues = DataManager.instance.ParseNPCDialogueData(textIndex);
            isLoaded = true;
        }

        if (isPlaying || (playOnlyOnce && hasPlayed)) return;
        if (dialogues != null && dialogues.Count > 0)
        {
            StartCoroutine(PlayConversation(dialogues));
        }
    }

    private IEnumerator PlayConversation(List<string> dialogues)
    {
        isPlaying = true;

        for(int i = 0; i < dialogues.Count; i++)
        {
            int npcIndex = i % groupNpcs.Count; 
            
            groupNpcs[npcIndex].ShowBubble(dialogues[i]);
            
            yield return new WaitForSeconds(textDelay);
            
            groupNpcs[npcIndex].HideBubble(); 
            
            if (i < dialogues.Count - 1)
            {
                yield return new WaitForSeconds(intervalDelay);
            }
        }

        yield return new WaitForSeconds(cooldownTime);

        isPlaying = false;
        hasPlayed = true;
    }
}