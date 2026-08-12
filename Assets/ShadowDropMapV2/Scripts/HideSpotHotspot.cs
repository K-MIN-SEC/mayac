using UnityEngine;
using UnityEngine.UI;

public class HideSpotHotspot : MonoBehaviour
{
    public string spotName = "Spot";

    [SerializeField] private Image spotImage;

    public void Configure(string newSpotName, Sprite sprite)
    {
        spotName = newSpotName;

        if (spotImage == null)
            spotImage = GetComponent<Image>();

        if (spotImage != null)
        {
            spotImage.sprite = sprite;
            spotImage.preserveAspect = true;
        }
    }

    public void OnHotspotClicked()
    {
        if (HidingPanelManager.Instance != null)
            HidingPanelManager.Instance.RequestHide(spotName);
    }
}
