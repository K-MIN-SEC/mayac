using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 스마트폰 - 메신저 앱. 대화방 목록과 대화 내용을 보여줌.
// 메시지가 오면 폰을 흔들고(진동) 배지를 켜며, 상대 메시지에 답장 선택지가 있으면 버튼으로 보여줌.
// 대화방마다 아바타/마지막 메시지 날짜도 같이 관리함.
public class MessengerAppUI : MonoBehaviour
{
    public static MessengerAppUI Instance { get; private set; }

    [System.Serializable]
    public class ChatThread
    {
        public string contactName = "???";
        public Sprite avatarSprite;                  // 목록에 보일 원형 아바타 (비워두면 프리팹 기본 이미지 사용)
        public List<MessengerMessageData> messages = new List<MessengerMessageData>();
        [HideInInspector] public bool hasUnread;
        [HideInInspector] public string lastMessageDate = ""; // 예: "7월 24일"
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
    public Image chatDetailAvatar; // 대화 상세 화면 상단에 보일 상대방 아바타
    public ScrollRect chatDetailScrollRect;

    [Header("답장 선택지 UI")]
    public Transform replyOptionsContent;      // 답장 버튼들이 들어갈 부모 (보통 ChatDetailView 하단)
    public GameObject replyOptionButtonPrefab; // 버튼 하나(안에 TMP 텍스트 포함) 프리팹

    [Header("알림 배지 (안읽은 메시지가 있으면 켜짐)")]
    public GameObject appIconBadge;
    public GameObject phoneButtonBadge;

    [Header("메시지가 오면 진동시킬 대상 (phone 버튼 등)")]
    public UIShaker phoneShaker;
    public UIShaker appIconShaker;

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

    // ---------- 메시지 수신 (다른 시스템에서 호출) ----------
    // avatarSprite: 대화방이 처음 생성될 때만 사용 (이미 있는 대화방이면 기존 아바타 유지)
    public void ReceiveMessage(string contactName, string text, Sprite photo = null, string[] replyOptions = null, Sprite avatarSprite = null)
    {
        ChatThread thread = threads.Find(t => t.contactName == contactName);
        if (thread == null)
        {
            thread = new ChatThread { contactName = contactName, avatarSprite = avatarSprite };
            threads.Add(thread);
        }

        thread.messages.Add(new MessengerMessageData
        {
            text = text,
            photo = photo,
            isFromMe = false,
            replyOptions = replyOptions,
            time = FormatTime(System.DateTime.Now)
        });
        thread.hasUnread = true;
        thread.lastMessageDate = System.DateTime.Now.ToString("M월 d일");

        RefreshUnreadBadges();

        if (phoneShaker != null) phoneShaker.Shake();
        if (appIconShaker != null) appIconShaker.Shake();

        if (chatListView != null && chatListView.activeSelf)
            RebuildChatList();

        if (openThread == thread && chatDetailView != null && chatDetailView.activeSelf)
            RebuildChatDetail();
    }

    // ---------- 목록 화면 ----------
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
                itemUI.Set(thread.contactName, preview, thread.hasUnread, thread.lastMessageDate, thread.avatarSprite);

                ChatThread capturedThread = thread;
                itemUI.onClick = () => OpenChatDetail(capturedThread);
            }
        }
    }

    // ---------- 상세 화면 ----------
    public void OpenChatDetail(ChatThread thread)
    {
        openThread = thread;
        thread.hasUnread = false;
        RefreshUnreadBadges();

        chatListView.SetActive(false);
        chatDetailView.SetActive(true);
        if (chatDetailTitle != null) chatDetailTitle.text = thread.contactName;
        if (chatDetailAvatar != null && thread.avatarSprite != null) chatDetailAvatar.sprite = thread.avatarSprite;

        RebuildChatDetail();
    }

    void RebuildChatDetail()
    {
        foreach (Transform child in chatDetailContent)
            Destroy(child.gameObject);

        float bubbleSpacing = 10f;
        float yCursor = 0f; // 위에서부터 차례로 쌓아나감

        foreach (MessengerMessageData msg in openThread.messages)
        {
            GameObject prefab = msg.isFromMe ? bubbleMePrefab : bubbleOtherPrefab;
            GameObject bubble = Instantiate(prefab, chatDetailContent);

            ChatBubbleUI bubbleUI = bubble.GetComponent<ChatBubbleUI>();
            if (bubbleUI != null)
                bubbleUI.Set(msg.text, msg.photo, msg.time);

            RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
            if (bubbleRect != null)
            {
                // 말풍선 프리팹 자체의 anchor(왼쪽/오른쪽)는 그대로 두고, 세로 위치만 코드로 내려줌
                Vector2 pos = bubbleRect.anchoredPosition;
                pos.y = -yCursor;
                bubbleRect.anchoredPosition = pos;

                yCursor += bubbleRect.sizeDelta.y + bubbleSpacing;
            }
        }

        RectTransform contentRect = chatDetailContent as RectTransform;
        if (contentRect != null)
        {
            Vector2 size = contentRect.sizeDelta;
            size.y = yCursor;
            contentRect.sizeDelta = size;
        }

        Canvas.ForceUpdateCanvases();
        if (chatDetailScrollRect != null)
        {
            RectTransform content = chatDetailScrollRect.content;
            RectTransform viewport = chatDetailScrollRect.viewport;

            // 대화 내용이 스크롤 창보다 길 때만 맨 아래(최신 메시지)로 이동시킴
            if (content != null && viewport != null && content.rect.height > viewport.rect.height)
                chatDetailScrollRect.verticalNormalizedPosition = 0f;
        }

        RebuildReplyOptions();
    }

    // 마지막 메시지가 상대방 메시지이고 답장 선택지가 있으면 버튼으로 보여줌
    void RebuildReplyOptions()
    {
        if (replyOptionsContent == null) return;

        foreach (Transform child in replyOptionsContent)
            Destroy(child.gameObject);

        if (openThread.messages.Count == 0) return;

        MessengerMessageData lastMsg = openThread.messages[openThread.messages.Count - 1];
        bool hasOptions = !lastMsg.isFromMe && lastMsg.replyOptions != null && lastMsg.replyOptions.Length > 0;

        replyOptionsContent.gameObject.SetActive(hasOptions);
        if (!hasOptions) return;

        foreach (string option in lastMsg.replyOptions)
        {
            GameObject btnObj = Instantiate(replyOptionButtonPrefab, replyOptionsContent);
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = option;

            Button btn = btnObj.GetComponent<Button>();
            string capturedOption = option;
            if (btn != null)
                btn.onClick.AddListener(() => SendReply(capturedOption));
        }
    }

    // 플레이어가 답장 선택지를 골랐을 때 호출됨
    public void SendReply(string text)
    {
        if (openThread == null) return;

        openThread.messages.Add(new MessengerMessageData
        {
            text = text,
            isFromMe = true,
            time = FormatTime(System.DateTime.Now)
        });

        RebuildChatDetail(); // 답장 버튼은 이 안에서 다시 사라짐 (마지막 메시지가 내 메시지가 되었으니까)
    }

    void RefreshUnreadBadges()
    {
        bool anyUnread = threads.Exists(t => t.hasUnread);

        if (appIconBadge != null) appIconBadge.SetActive(anyUnread);
        if (phoneButtonBadge != null) phoneButtonBadge.SetActive(anyUnread);
    }

    // "오후 3:30" 같은 한국어 오전/오후 시간 표기로 변환
    string FormatTime(System.DateTime dt)
    {
        string period = dt.Hour < 12 ? "오전" : "오후";
        int hour12 = dt.Hour % 12;
        if (hour12 == 0) hour12 = 12;
        return period + " " + hour12 + ":" + dt.Minute.ToString("D2");
    }
}