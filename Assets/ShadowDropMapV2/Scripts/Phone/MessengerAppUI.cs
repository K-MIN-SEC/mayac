using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessengerAppUI : MonoBehaviour
{
    public static MessengerAppUI Instance { get; private set; }

    [System.Serializable]
    public class ChatThread
    {
        public string contactName = "???";
        public Sprite avatarSprite;
        public List<MessengerMessageData> messages = new List<MessengerMessageData>();
        [HideInInspector] public bool hasUnread;
        [HideInInspector] public string lastMessageDate = "";
    }

    [Header("Data")]
    public List<ChatThread> threads = new List<ChatThread>();

    [Header("Chat list")]
    public GameObject chatListView;
    public Transform chatListContent;
    public GameObject chatListItemPrefab;
    public GameObject emptyStateText;

    [Header("Chat detail")]
    public GameObject chatDetailView;
    public Transform chatDetailContent;
    public GameObject bubbleMePrefab;
    public GameObject bubbleOtherPrefab;
    public GameObject systemBoxPrefab;
    public TMP_Text chatDetailTitle;
    public Image chatDetailAvatar;
    public ScrollRect chatDetailScrollRect;

    [Header("Reply options")]
    public Transform replyOptionsContent;
    public GameObject replyOptionButtonPrefab;

    [Header("Notifications")]
    public GameObject appIconBadge;
    public GameObject phoneButtonBadge;
    public UIShaker phoneShaker;
    public UIShaker appIconShaker;
    public AudioClip notificationSound;

    private ChatThread openThread;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RefreshUnreadBadges();
    }

    private void AddMessageToThread(ChatThread thread, MessengerMessageData data)
    {
        thread.messages.Add(data);
        thread.lastMessageDate = System.DateTime.Now.ToString("M월 d일");

        // 내가 보낸 게 아니라면 알림 처리
        if (!data.isFromMe)
        {
            thread.hasUnread = true;
            RefreshUnreadBadges();
            phoneShaker?.Shake();
            appIconShaker?.Shake();
            SoundManager.instance.SetAudio(notificationSound, false);
        }

        // 현재 켜져있는 화면 갱신
        if (chatListView != null && chatListView.activeSelf) RebuildChatList();
        if (openThread == thread && chatDetailView != null && chatDetailView.activeSelf) AppendMessageToDetail(data);
    }

    // 외부에서 NPC 메시지 수신할 때 호출
    public void ReceiveMessage(MessengerMessageData data, Sprite avatarSprite = null)
    {
        ChatThread thread = threads.Find(item => item.contactName == data.name);
        if (thread == null)
        {
            thread = new ChatThread { contactName = data.name, avatarSprite = avatarSprite };
            threads.Add(thread);
        }

        data.isFromMe = false;
        if (data.replyOptions != null && data.replyOptions.Length > 0) ShowReplyOptions(data.replyOptions);
        AddMessageToThread(thread, data);
    }

    // 유저가 답장 버튼을 눌렀을 때 호출
    public void SendReply(string text)
    {
        if (openThread == null) return;

        var myMessage = new MessengerMessageData { text = text, isFromMe = true };
        AddMessageToThread(openThread, myMessage);

        // 스토리에 트리거 전달
        StoryManager.instance.TryPlayStory("Reply");
    }

    public void OpenChatList()
    {
        chatDetailView?.SetActive(false);
        chatListView?.SetActive(true);
        RebuildChatList();
    }

    private void AppendMessageToDetail(MessengerMessageData message)
    {
        if (chatDetailContent == null) return;

        GameObject prefab = message.isSystemBox ? systemBoxPrefab : (message.isFromMe ? bubbleMePrefab : bubbleOtherPrefab);
        if (prefab == null) return;

        GameObject bubble = Instantiate(prefab, chatDetailContent);
        if (bubble.TryGetComponent(out ChatBubbleUI bubbleUI))
        {
            bubbleUI.Set(message.text, message.photo);
        }


        Canvas.ForceUpdateCanvases();
        if (chatDetailScrollRect != null) chatDetailScrollRect.verticalNormalizedPosition = 0f;
    }

    private void RebuildChatList()
    {
        if (chatListContent == null || chatListItemPrefab == null) return;

        foreach (Transform child in chatListContent) Destroy(child.gameObject);
        emptyStateText?.SetActive(threads.Count == 0);

        foreach (ChatThread thread in threads)
        {
            GameObject item = Instantiate(chatListItemPrefab, chatListContent);
            if (item.TryGetComponent(out ChatListItemUI itemUI))
            {
                string preview = thread.messages.Count > 0 ? thread.messages[^1].text : "";
                itemUI.Set(thread.contactName, preview, thread.hasUnread, thread.lastMessageDate, thread.avatarSprite);

                ChatThread capturedThread = thread;
                itemUI.onClick = () => OpenChatDetail(capturedThread);
            }
        }
    }

    public void OpenChatDetail(ChatThread thread)
    {
        if (thread == null) return;

        openThread = thread;
        thread.hasUnread = false;
        RefreshUnreadBadges();

        chatListView?.SetActive(false);
        chatDetailView?.SetActive(true);

        if (chatDetailTitle != null) chatDetailTitle.text = thread.contactName;
        if (chatDetailAvatar != null && thread.avatarSprite != null) chatDetailAvatar.sprite = thread.avatarSprite;

        RebuildChatDetail();
    }

    private void RebuildChatDetail()
    {
        if (openThread == null || chatDetailContent == null) return;

        foreach (Transform child in chatDetailContent) Destroy(child.gameObject);

        foreach (MessengerMessageData message in openThread.messages)
        {
            GameObject prefab = message.isSystemBox ? systemBoxPrefab : (message.isFromMe ? bubbleMePrefab : bubbleOtherPrefab);
            if (prefab == null) continue;

            GameObject bubble = Instantiate(prefab, chatDetailContent);
            if (bubble.TryGetComponent(out ChatBubbleUI bubbleUI))
            {
                bubbleUI.Set(message.text, message.photo);
            }
        }

        // 스크롤 맨 아래로 내리기 (레이아웃 갱신 후 실행되도록 딜레이 적용 필요할 수 있음)
        Canvas.ForceUpdateCanvases();
        if (chatDetailScrollRect != null) chatDetailScrollRect.verticalNormalizedPosition = 0f;
    }

    public void ShowReplyOptions(string[] options)
    {
        if (replyOptionsContent == null) return;

        foreach (Transform child in replyOptionsContent) Destroy(child.gameObject);

        if (options == null || options.Length == 0)
        {
            replyOptionsContent.gameObject.SetActive(false);
            return;
        }
        replyOptionsContent.gameObject.SetActive(true);

        foreach (string option in options)
        {
            GameObject buttonObject = Instantiate(replyOptionButtonPrefab, replyOptionsContent);
            buttonObject.GetComponentInChildren<TMP_Text>().text = option;

            string capturedOption = option;
            buttonObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                SendReply(capturedOption);
                replyOptionsContent.gameObject.SetActive(false);
            });
        }
    }

    private void RefreshUnreadBadges()
    {
        bool hasUnread = threads.Exists(thread => thread.hasUnread);
        appIconBadge?.SetActive(hasUnread);
        phoneButtonBadge?.SetActive(hasUnread);
    }
}