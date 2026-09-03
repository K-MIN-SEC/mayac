using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientConversation : MonoBehaviour
{
    [Header("Data")]
    public string conversationId; // CSV에서 읽어올 대화 ID
    
    [Header("Settings")]
    public float textDelay = 2.0f; // 다음 대사로 넘어가는 간격
    public bool playOnlyOnce = false;
    private bool hasPlayed = false;
    private bool isPlaying = false;

    // 대화에 참여하는 NPC 목록 (ID나 이름으로 매핑)
    //public List<NpcBubbleController> groupNpcs; 

    private Coroutine conversationCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isPlaying || (playOnlyOnce && hasPlayed)) return;
            
            // 대화 데이터 로드 (미리 로드해두는 것이 더 좋습니다)
            // List<Dialogue> data = CsvManager.GetAmbient(conversationId);
            
            //conversationCoroutine = StartCoroutine(PlayConversation(data));
        }
    }

    // private void OnTriggerExit2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Player"))
    //     {
    //         // 플레이어가 멀어지면 대화 강제 종료 및 말풍선 끄기
    //         if (conversationCoroutine != null) StopCoroutine(conversationCoroutine);
    //         isPlaying = false;

    //         foreach (var npc in groupNpcs)
    //         {
    //             npc.HideBubble(); // NPC 말풍선 끄기
    //         }
    //     }
    // }

    private IEnumerator PlayConversation(List<Dialogue> dialogues)
    {
        isPlaying = true;

        foreach (var dialogue in dialogues)
        {
            // 화자 찾기
            // var speaker = groupNpcs.Find(n => n.npcName == dialogue.name);
            // if (speaker != null)
            // {
            //     speaker.ShowBubble(dialogue.text);
            // }

            // // 플레이어가 읽을 시간 부여
            yield return new WaitForSeconds(textDelay);
            
            // if (speaker != null) speaker.HideBubble();
        }

        isPlaying = false;
        hasPlayed = true;
    }
}