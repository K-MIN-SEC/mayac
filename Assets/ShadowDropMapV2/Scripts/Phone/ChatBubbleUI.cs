using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubbleUI : MonoBehaviour
{
    public TMP_Text messageText;
    public GameObject textBackground; // 형 변환: RectTransform -> GameObject로 변경
    public Image photoImage;
    public GameObject photoContainer;

    public void Set(string text, Sprite photo)
    {
        bool hasText = !string.IsNullOrEmpty(text);
        bool hasPhoto = photo != null;

        if (messageText != null) messageText.text = text;
        if (textBackground != null) textBackground.SetActive(hasText);

        if (photoImage != null && hasPhoto) photoImage.sprite = photo;
        if (photoContainer != null) photoContainer.SetActive(hasPhoto);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}