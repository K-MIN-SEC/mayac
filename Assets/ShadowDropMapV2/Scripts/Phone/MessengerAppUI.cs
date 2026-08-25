using System.Collections.Generic;
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
    public AudioSource notificationSound;

    private ChatThread openThread;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureNotificationSound();
    }

    private void Start()
    {
        RefreshUnreadBadges();
    }

    public void ReceiveMessage(MessengerMessageData data, Sprite avatarSprite = null)
    {
        ChatThread thread = threads.Find(item => item.contactName == data.name);
        if (thread == null)
        {
            thread = new ChatThread { contactName = data.name, avatarSprite = avatarSprite };
            threads.Add(thread);
        }

        data.isFromMe = false;
        thread.messages.Add(data);
        thread.hasUnread = true;
        thread.lastMessageDate = System.DateTime.Now.ToString("M월 d일");

        RefreshUnreadBadges();
        phoneShaker?.Shake();
        appIconShaker?.Shake();
        notificationSound?.Play();

        if (chatListView != null && chatListView.activeSelf) RebuildChatList();
        if (openThread == thread && chatDetailView != null && chatDetailView.activeSelf)
            RebuildChatDetail();
    }

    public void OpenChatList()
    {
        if (chatDetailView != null) chatDetailView.SetActive(false);
        if (chatListView != null) chatListView.SetActive(true);
        RebuildChatList();
    }

    private void RebuildChatList()
    {
        if (chatListContent == null || chatListItemPrefab == null) return;

        foreach (Transform child in chatListContent) Destroy(child.gameObject);
        if (emptyStateText != null) emptyStateText.SetActive(threads.Count == 0);

        foreach (ChatThread thread in threads)
        {
            GameObject item = Instantiate(chatListItemPrefab, chatListContent);
            ChatListItemUI itemUI = item.GetComponent<ChatListItemUI>();
            if (itemUI == null) continue;

            string preview = thread.messages.Count > 0
                ? thread.messages[thread.messages.Count - 1].text
                : "";
            itemUI.Set(thread.contactName, preview, thread.hasUnread,
                thread.lastMessageDate, thread.avatarSprite);

            ChatThread capturedThread = thread;
            itemUI.onClick = () => OpenChatDetail(capturedThread);
        }
    }

    public void OpenChatDetail(ChatThread thread)
    {
        if (thread == null) return;

        openThread = thread;
        thread.hasUnread = false;
        RefreshUnreadBadges();

        if (chatListView != null) chatListView.SetActive(false);
        if (chatDetailView != null) chatDetailView.SetActive(true);
        if (chatDetailTitle != null) chatDetailTitle.text = thread.contactName;
        if (chatDetailAvatar != null && thread.avatarSprite != null)
            chatDetailAvatar.sprite = thread.avatarSprite;
        RebuildChatDetail();
    }

    private void RebuildChatDetail()
    {
        if (openThread == null || chatDetailContent == null) return;

        foreach (Transform child in chatDetailContent) Destroy(child.gameObject);

        const float bubbleSpacing = 10f;
        float yCursor = 0f;
        foreach (MessengerMessageData message in openThread.messages)
        {
            GameObject prefab = message.isSystemBox
                ? systemBoxPrefab
                : (message.isFromMe ? bubbleMePrefab : bubbleOtherPrefab);
            if (prefab == null) continue;

            GameObject bubble = Instantiate(prefab, chatDetailContent);
            ChatBubbleUI bubbleUI = bubble.GetComponent<ChatBubbleUI>();
            if (bubbleUI != null) bubbleUI.Set(message.text, message.photo);

            RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
            if (bubbleRect == null) continue;
            Vector2 position = bubbleRect.anchoredPosition;
            position.y = -yCursor;
            bubbleRect.anchoredPosition = position;
            yCursor += bubbleRect.sizeDelta.y + bubbleSpacing;
        }

        RectTransform contentRect = chatDetailContent as RectTransform;
        if (contentRect != null)
        {
            Vector2 size = contentRect.sizeDelta;
            size.y = yCursor;
            contentRect.sizeDelta = size;
        }

        Canvas.ForceUpdateCanvases();
        if (chatDetailScrollRect != null && chatDetailScrollRect.content != null
            && chatDetailScrollRect.viewport != null
            && chatDetailScrollRect.content.rect.height > chatDetailScrollRect.viewport.rect.height)
            chatDetailScrollRect.verticalNormalizedPosition = 0f;

        RebuildReplyOptions();
    }

    private void RebuildReplyOptions()
    {
        if (replyOptionsContent == null) return;

        foreach (Transform child in replyOptionsContent) Destroy(child.gameObject);
        if (openThread == null || openThread.messages.Count == 0) return;

        MessengerMessageData lastMessage = openThread.messages[openThread.messages.Count - 1];
        bool hasOptions = !lastMessage.isFromMe
            && lastMessage.replyOptions != null
            && lastMessage.replyOptions.Length > 0;
        replyOptionsContent.gameObject.SetActive(hasOptions);
        if (!hasOptions || replyOptionButtonPrefab == null) return;

        foreach (string option in lastMessage.replyOptions)
        {
            GameObject buttonObject = Instantiate(replyOptionButtonPrefab, replyOptionsContent);
            TMP_Text label = buttonObject.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = option;

            Button button = buttonObject.GetComponent<Button>();
            string capturedOption = option;
            if (button != null) button.onClick.AddListener(() => SendReply(capturedOption));
        }
    }

    public void SendReply(string text)
    {
        if (openThread == null) return;

        openThread.messages.Add(new MessengerMessageData { text = text, isFromMe = true });

        if (TryGetComponent(out StoryTarget storyTarget))
        {
            StoryManager.instance.TryPlayStory(storyTarget.targetId);
        }
        // if (text == "네, 할게요")
        // {
        //     if (DetectorAppUI.Instance != null) DetectorAppUI.Instance.ShowAlertBorder();
        //     MissionMessageSender mission = MissionMessageSender.LastSent;
        //     // if (mission != null && !string.IsNullOrEmpty(mission.missionTitle))
        //     // {
        //     //     openThread.messages.Add(new MessengerMessageData
        //     //     {
        //     //         text = "다음 임무:\n" + mission.missionTitle,
        //     //         isFromMe = false,
        //     //         isSystemBox = true
        //     //     });
        //     // }
        // }

        RebuildChatDetail();
    }

    private void RefreshUnreadBadges()
    {
        bool hasUnread = threads.Exists(thread => thread.hasUnread);
        if (appIconBadge != null) appIconBadge.SetActive(hasUnread);
        if (phoneButtonBadge != null) phoneButtonBadge.SetActive(hasUnread);
    }

    private void EnsureNotificationSound()
    {
        if (notificationSound == null)
            notificationSound = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (notificationSound.clip != null) return;

        const int sampleRate = 22050;
        const float duration = 0.16f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int index = 0; index < sampleCount; index++)
        {
            float time = (float)index / sampleRate;
            float fade = 1f - (float)index / sampleCount;
            samples[index] = Mathf.Sin(2f * Mathf.PI * 880f * time) * fade * 0.18f;
        }

        AudioClip clip = AudioClip.Create("MessageNotification", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        notificationSound.playOnAwake = false;
        notificationSound.clip = clip;
    }
}
