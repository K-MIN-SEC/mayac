using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 메신저 대화 상세 화면의 말풍선 하나(프리팹에 부착). 텍스트만 있을 수도, 사진이 같이 있을 수도 있음
// 텍스트 길이에 맞춰 말풍선 폭을 min~max 사이에서 직접 계산해줌
// (Content Size Fitter의 Horizontal Fit은 줄바꿈 텍스트에서 오작동하는 경우가 있어서 코드로 처리함)
public class ChatBubbleUI : MonoBehaviour
{
    public TMP_Text messageText;
    public TMP_Text timeText; // 말풍선 옆에 작게 뜨는 시간
    public Image photoImage;
    public GameObject photoContainer; // 사진이 없을 때 꺼둘 부모 오브젝트 (비워두면 무시)

    [Header("말풍선 너비 (텍스트 길이에 따라 이 범위 안에서 자동 조절됨)")]
    public float minWidth = 60f;
    public float maxWidth = 250f;
    public float horizontalPadding = 24f; // 텍스트 좌우 여백 합
    public float verticalPadding = 16f;   // 텍스트 상하 여백 합

    private RectTransform rootRect;
    private RectTransform textRect;

    void Awake()
    {
        rootRect = GetComponent<RectTransform>();
        if (messageText != null)
            textRect = messageText.rectTransform;
    }

    public void Set(string text, Sprite photo, string time = "")
    {
        bool hasText = !string.IsNullOrEmpty(text);
        bool hasPhoto = photo != null;

        if (messageText != null)
        {
            messageText.gameObject.SetActive(hasText);
            messageText.text = text;

            if (hasText)
                ResizeToFitText();
        }

        if (timeText != null)
            timeText.text = time;

        if (photoContainer != null)
            photoContainer.SetActive(hasPhoto);

        if (photoImage != null && hasPhoto)
            photoImage.sprite = photo;
    }

    // 텍스트 폭/높이를 코드로 직접 계산해서 말풍선 크기를 맞춤
    // (Content Size Fitter를 쓰지 않음 - Layout Group과 충돌하지 않도록 완전히 수동으로 처리)
    public float LastHeight { get; private set; }

    void ResizeToFitText()
    {
        if (messageText == null || textRect == null) return;

        // 1) 줄바꿈 없이 한 줄로 쳤을 때 필요한 폭 (min~max로 제한)
        Vector2 unwrappedSize = messageText.GetPreferredValues(messageText.text, 5000f, 0f);
        float textWidth = Mathf.Clamp(unwrappedSize.x, minWidth - horizontalPadding, maxWidth - horizontalPadding);

        // 2) 그 폭으로 줄바꿈했을 때 필요한 높이
        Vector2 wrappedSize = messageText.GetPreferredValues(messageText.text, textWidth, 0f);
        float textHeight = wrappedSize.y;

        Vector2 tSize = textRect.sizeDelta;
        tSize.x = textWidth;
        tSize.y = textHeight;
        textRect.sizeDelta = tSize;

        if (rootRect != null)
        {
            Vector2 rSize = rootRect.sizeDelta;
            rSize.x = textWidth + horizontalPadding;
            rSize.y = textHeight + verticalPadding;
            rootRect.sizeDelta = rSize;
            LastHeight = rSize.y;
        }
    }
}