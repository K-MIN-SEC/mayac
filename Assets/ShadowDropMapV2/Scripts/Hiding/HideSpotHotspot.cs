using UnityEngine;
using UnityEngine.UI;

public class HideSpotHotspot : MonoBehaviour
{
    public string spotName = "Spot";
    public bool isCorrectSpot = false;

    [SerializeField] private Image spotImage;
    [SerializeField] private RectTransform rectTransform;

    public void Configure(string newSpotName, Sprite sprite, bool correctSpot, Vector2 position, Vector2 size)
    {
        spotName = newSpotName;
        isCorrectSpot = correctSpot;

        spotImage.sprite = sprite;
        spotImage.preserveAspect = true;

        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
    }

    public void OnHotspotClicked()
    {
        HidingPanelManager.Instance.RequestHide(spotName, spotImage.sprite, isCorrectSpot);
    }
}
