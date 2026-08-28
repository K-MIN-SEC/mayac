using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubbleUI : MonoBehaviour
{
    public TMP_Text messageText;
    public GameObject textBackground;
    public GameObject photoContainer;

    private Image cachedPhotoImg;
    private AspectRatioFitter cachedPhotoFitter;
    private LayoutElement cachedPhotoLayout;
    private LayoutElement cachedTextLayout;

    [Header("UI Settings")]
    public float maxTextWidth = 240f;
    public float maxPhotoWidth = 240f;

    void Awake()
    {
        cachedPhotoImg = photoContainer.GetComponent<Image>();
        cachedPhotoFitter = photoContainer.GetComponent<AspectRatioFitter>();
        cachedPhotoLayout = photoContainer.GetComponent<LayoutElement>();
        cachedTextLayout = messageText.GetComponent<LayoutElement>();
    }

    public void Set(string text, Sprite photo)
    {
        bool hasText = !string.IsNullOrEmpty(text);
        bool hasPhoto = photo != null;

        if (hasPhoto)
        {
            cachedPhotoImg.sprite = photo;
            cachedPhotoFitter.aspectRatio = photo.rect.width / photo.rect.height;
            cachedPhotoLayout.preferredWidth = maxPhotoWidth;
        }

        photoContainer.SetActive(hasPhoto);

        messageText.text = text;
        cachedTextLayout.preferredWidth = Mathf.Min(messageText.preferredWidth, maxTextWidth);

        textBackground.SetActive(hasText);

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}