using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 메신저 대화 상세 화면의 말풍선 하나(프리팹에 부착). 텍스트만 있을 수도, 사진이 같이 있을 수도 있음
public class ChatBubbleUI : MonoBehaviour
{
    public TMP_Text messageText;
    public Image photoImage;
    public GameObject photoContainer; // 사진이 없을 때 꺼둘 부모 오브젝트 (비워두면 무시)

    public void Set(string text, Sprite photo)
    {
        bool hasText = !string.IsNullOrEmpty(text);
        bool hasPhoto = photo != null;

        if (messageText != null)
        {
            messageText.gameObject.SetActive(hasText);
            messageText.text = text;
        }

        if (photoContainer != null)
            photoContainer.SetActive(hasPhoto);

        if (photoImage != null && hasPhoto)
            photoImage.sprite = photo;
    }
}