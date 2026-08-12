using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 메신저 대화 상세 화면의 말풍선 하나(프리팹에 부착). 텍스트만 있을 수도, 사진이 같이 있을 수도 있음
// 구조: BubbleXXX(루트, 배경 없음) - PhotoContainer(사진), TextBackground(둥근 배경) - MessageText(그 자식)
// MessageText는 TextBackground 안에서 Stretch(여백만 고정)로 앉혀두면 코드가 위치를 안 건드려도 됨
public class ChatBubbleUI : MonoBehaviour
{
    public TMP_Text messageText;
    public RectTransform textBackground; // 텍스트를 감싸는 둥근 배경 (MessageText의 부모)
    public Image photoImage;
    public GameObject photoContainer;    // 사진 부모 오브젝트 (배경 없이 사진만 보여줌)

    [Header("말풍선 너비 (텍스트 길이에 따라 이 범위 안에서 자동 조절됨)")]
    public float minWidth = 60f;
    public float maxWidth = 250f;
    public float horizontalPadding = 24f; // 텍스트 배경 좌우 여백 합 (MessageText Stretch 마진과 맞춰야 함)
    public float verticalPadding = 16f;   // 텍스트 배경 상하 여백 합

    [Header("사진이 같이 있을 때")]
    public float photoWidth = 200f;
    public float maxPhotoHeight = 150f;
    public float photoTextSpacing = 8f;

    private RectTransform rootRect;
    private RectTransform photoContainerRect;

    void Awake()
    {
        rootRect = GetComponent<RectTransform>();
        if (photoContainer != null) photoContainerRect = photoContainer.GetComponent<RectTransform>();
    }

    public void Set(string text, Sprite photo)
    {
        bool hasText = !string.IsNullOrEmpty(text);
        bool hasPhoto = photo != null;

        if (messageText != null)
            messageText.text = text;

        if (textBackground != null)
            textBackground.gameObject.SetActive(hasText);

        if (photoContainer != null)
            photoContainer.SetActive(hasPhoto);

        if (photoImage != null && hasPhoto)
            photoImage.sprite = photo;

        Layout(hasText, hasPhoto, photo);
    }

    void Layout(bool hasText, bool hasPhoto, Sprite photo)
    {
        float contentWidth = minWidth - horizontalPadding;
        float textHeight = 0f;
        float photoHeight = 0f;
        float actualPhotoWidth = photoWidth;

        // 1) 사진 크기 계산 (원본 비율 유지, 세로 최대치 넘으면 폭도 같이 축소)
        if (hasPhoto && photo != null && photo.rect.width > 0)
        {
            photoHeight = actualPhotoWidth * (photo.rect.height / photo.rect.width);
            if (photoHeight > maxPhotoHeight)
            {
                float scale = maxPhotoHeight / photoHeight;
                photoHeight = maxPhotoHeight;
                actualPhotoWidth *= scale;
            }
        }

        // 2) 텍스트 폭/높이 계산 (MessageText 자체는 안 건드림 - Stretch가 알아서 채움)
        if (hasText && messageText != null)
        {
            Vector2 unwrapped = messageText.GetPreferredValues(messageText.text, 5000f, 0f);
            float textWidth = Mathf.Clamp(unwrapped.x, minWidth - horizontalPadding, maxWidth - horizontalPadding);
            contentWidth = Mathf.Max(contentWidth, textWidth);

            Vector2 wrapped = messageText.GetPreferredValues(messageText.text, contentWidth, 0f);
            textHeight = wrapped.y;
        }

        // 3) 위에서부터 사진 → 텍스트배경 순서로 세로 위치 배치
        float yCursor = 0f;

        if (hasPhoto && photoContainerRect != null)
        {
            Vector2 pSize = photoContainerRect.sizeDelta;
            pSize.x = actualPhotoWidth;
            pSize.y = photoHeight;
            photoContainerRect.sizeDelta = pSize;

            Vector2 pPos = photoContainerRect.anchoredPosition;
            pPos.y = -yCursor;
            photoContainerRect.anchoredPosition = pPos;

            yCursor += photoHeight;
            if (hasText) yCursor += photoTextSpacing;
        }

        if (hasText && textBackground != null)
        {
            Vector2 bgSize = textBackground.sizeDelta;
            bgSize.x = contentWidth + horizontalPadding;
            bgSize.y = textHeight + verticalPadding;
            textBackground.sizeDelta = bgSize;

            Vector2 bgPos = textBackground.anchoredPosition;
            bgPos.y = -yCursor;
            textBackground.anchoredPosition = bgPos;

            yCursor += bgSize.y;
        }

        // 4) 루트(전체 레이아웃 영역) 크기 = 사진 + 텍스트배경 합친 크기
        float rootWidth = Mathf.Max(hasPhoto ? actualPhotoWidth : 0f, hasText ? contentWidth + horizontalPadding : 0f);
        if (rootRect != null)
        {
            Vector2 rSize = rootRect.sizeDelta;
            rSize.x = rootWidth;
            rSize.y = yCursor;
            rootRect.sizeDelta = rSize;
        }
    }
}