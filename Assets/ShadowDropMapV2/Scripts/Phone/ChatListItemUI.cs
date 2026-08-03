using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 메신저 대화 목록의 아이템 하나(프리팹에 부착). 이름 / 마지막 메시지 미리보기 / 안읽음 표시를 담당
public class ChatListItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text previewText;
    public GameObject unreadDot;
    public Button button;

    [HideInInspector] public Action onClick;

    void Awake()
    {
        if (button != null)
            button.onClick.AddListener(() => onClick?.Invoke());
    }

    public void Set(string contactName, string preview, bool unread)
    {
        if (nameText != null) nameText.text = contactName;
        if (previewText != null) previewText.text = preview;
        if (unreadDot != null) unreadDot.SetActive(unread);
    }
}