using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 메신저 대화 목록의 아이템 하나(프리팹에 부착).
// 아바타 / 이름 / 날짜 / 마지막 메시지 미리보기 / 안읽음 표시를 담당
public class ChatListItemUI : MonoBehaviour
{
    public Image avatarImage;       // 왼쪽 원형 아바타
    public TMP_Text nameText;       // 이름/번호 (굵게)
    public TMP_Text dateText;       // 오른쪽 위 날짜
    public TMP_Text previewText;    // 아래쪽 회색 미리보기
    public GameObject unreadDot;
    public Button button;

    [HideInInspector] public Action onClick;

    void Awake()
    {
        if (button != null)
            button.onClick.AddListener(() => onClick?.Invoke());
    }

    public void Set(string contactName, string preview, bool unread, string date, Sprite avatar)
    {
        if (nameText != null) nameText.text = contactName;
        if (previewText != null) previewText.text = preview;
        if (dateText != null) dateText.text = date;
        if (unreadDot != null) unreadDot.SetActive(unread);

        if (avatarImage != null && avatar != null)
            avatarImage.sprite = avatar;
    }
}