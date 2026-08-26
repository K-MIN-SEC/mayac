using UnityEngine;
using UnityEngine.UI;

public class HideSpotHotspot : MonoBehaviour
{
    public string spotName = "Spot";
    public bool isCorrectSpot = false; // 이 지점이 정답인지 여부 (Configure에서 자동으로 채워짐)

    [SerializeField] private Image spotImage;
    private RectTransform rectTransform;

    public void Configure(string newSpotName, Sprite sprite, bool correctSpot = false)
    {
        Configure(newSpotName, sprite, correctSpot, null);
    }

    // position이 null이 아니면, 이 사진 속 정확한 위치로 버튼을 옮김
    public void Configure(string newSpotName, Sprite sprite, bool correctSpot, Vector2? position)
    {
        spotName = newSpotName;
        isCorrectSpot = correctSpot;

        if (spotImage == null)
            spotImage = GetComponent<Image>();

        if (spotImage != null)
        {
            spotImage.sprite = sprite;
            spotImage.preserveAspect = true;
        }

        if (position.HasValue)
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (rectTransform != null)
                rectTransform.anchoredPosition = position.Value;
        }
    }

    public void OnHotspotClicked()
    {
        if (HidingPanelManager.Instance != null)
            HidingPanelManager.Instance.RequestHide(spotName, isCorrectSpot);
    }
}
