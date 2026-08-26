using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubbleUI : MonoBehaviour
{
    public TMP_Text messageText;
    public GameObject textBackground; 
    public Image photoImage;
    public GameObject photoContainer;

    public void Set(string text, Sprite photo)
    {
        bool hasText = !string.IsNullOrEmpty(text);
        bool hasPhoto = photo != null;

        if (messageText != null) messageText.text = text;
        if (textBackground != null) textBackground.SetActive(hasText);

        if (photoImage != null && hasPhoto) 
        {
            photoImage.sprite = photo;
            
            if (photoImage.TryGetComponent<AspectRatioFitter>(out var fitter))
            {
                fitter.aspectRatio = photo.rect.width / photo.rect.height;
            }
        }
        
        if (photoContainer != null) photoContainer.SetActive(hasPhoto);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}