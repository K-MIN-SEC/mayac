using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 스마트폰 - 메신저 앱. 대화방 목록과 대화 내용을 보여줌.
public class MessengerAppUI : MonoBehaviour
{
    public static MessengerAppUI Instance { get; private set; }

    [System.Serializable]
    public class ChatThread
    {
        public string contactName = "???";
        public List<MessengerMessageData> messages = new List<MessengerMessageData>();
        [HideInInspector] public bool hasUnread;
    }

    [Header("데이터 (인스펙터에서 미리 채워도 되고, 코드로 ReceiveMessage 호출해도 됨)")]
    public List<ChatThread> threads = new List<ChatThread>();

    [Header("대화 목록 화면")]
    public GameObject chatListView;
    public Transform chatListContent;
    public GameObject chatListItemPrefab;
    public GameObject emptyStateText; // "오늘 할 일이 없습니다" 같은 빈 상태 문구 오브젝트

    [Header("대화 상세 화면")]
    public GameObject chatDetailView;
    public Transform chatDetailContent;
    public GameObject bubbleMePrefab;
    public GameObject bubbleOtherPrefab;
    public TMP_Text chatDetailTitle;
    public ScrollRect chatDetailScrollRect;

    [Header("알림 배지 (안읽은 메시지가 있으면 켜짐)")]
    public GameObject appIconBadge;
    public GameObject phoneButtonBadge;

    private ChatThread openThread;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        RefreshUnreadBadges();
    }

    public void ReceiveMessage(string contactName, string text, Sprite photo = null)
    {
        ChatThread thread = threads.Find(t => t.contactName == contactName);
        if (thread == null)
        {
            thread = new ChatThread { contactName = contactName };
            threads.Add(thread);
        }

        thread.messages.Add(new MessengerMessageData
        {
            text = text,
            photo = photo,
            isFromMe = false
        });
        thread.hasUnread = true;

        RefreshUnreadBadges();

        if (chatListView != null && chatListView.activeSelf)
            RebuildChatList();

        if (openThread == thread && chatDetailView != null && chatDetailView.activeSelf)
            RebuildChatDetail();
    }

    public void OpenChatList()
    {
        chatDetailView.SetActive(false);
        chatListView.SetActive(true);
        RebuildChatList();
    }

    void RebuildChatList()
    {
        foreach (Transform child in chatListContent)
            Destroy(child.gameObject);

        if (emptyStateText != null)
            emptyStateText.SetActive(threads.Count == 0);

        foreach (ChatThread thread in threads)
        {
            GameObject item = Instantiate(chatListItemPrefab, chatListContent);
            ChatListItemUI itemUI = item.GetComponent<ChatListItemUI>();
            if (itemUI != null)
            {
                string preview = thread.messages.Count > 0
                    ? thread.messages[thread.messages.Count - 1].text
                    : "";
                itemUI.Set(thread.contactName, preview, thread.hasUnread);

                ChatThread capturedThread = thread;
                itemUI.onClick = () => OpenChatDetail(capturedThread);
            }
        }
    }

    public void OpenChatDetail(ChatThread thread)
    {
        openThread = thread;
        thread.hasUnread = false;
        RefreshUnreadBadges();

        chatListView.SetActive(false);
        chatDetailView.SetActive(true);
        if (chatDetailTitle != null) chatDetailTitle.text = thread.contactName;

        RebuildChatDetail();
    }

    void RebuildChatDetail()
    {
        foreach (Transform child in chatDetailContent)
            Destroy(child.gameObject);

        foreach (MessengerMessageData msg in openThread.messages)
        {
            GameObject prefab = msg.isFromMe ? bubbleMePrefab : bubbleOtherPrefab;
            GameObject bubble = Instantiate(prefab, chatDetailContent);

            ChatBubbleUI bubbleUI = bubble.GetComponent<ChatBubbleUI>();
            if (bubbleUI != null)
                bubbleUI.Set(msg.text, msg.photo);
        }

        Canvas.ForceUpdateCanvases();
        if (chatDetailScrollRect != null)
            chatDetailScrollRect.verticalNormalizedPosition = 0f;
    }

    void RefreshUnreadBadges()
    {
        bool anyUnread = threads.Exists(t => t.hasUnread);

        if (appIconBadge != null) appIconBadge.SetActive(anyUnread);
        if (phoneButtonBadge != null) phoneButtonBadge.SetActive(anyUnread);
    }
}