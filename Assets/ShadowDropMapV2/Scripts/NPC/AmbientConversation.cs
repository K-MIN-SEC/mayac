using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientConversation : MonoBehaviour
{
    [Header("Data")]
    public int textIndex;
    [SerializeField] private List<string> dialogues;

    [Header("Settings")]
    public float textDelay = 1f;
    
    private bool isPlaying = false;
    private bool isLoaded = false;

    public List<NPCAmbientDialogue> groupNpcs;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        
        if (!isLoaded)
        {
            dialogues = DataManager.instance.ParseNPCDialogueData(textIndex);
            isLoaded = true;
        }

        if (isPlaying) return;
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
        }

        isPlaying = false;
    }
}