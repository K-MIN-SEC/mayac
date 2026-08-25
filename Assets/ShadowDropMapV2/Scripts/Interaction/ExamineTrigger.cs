using UnityEngine;

public class ExamineTrigger : MonoBehaviour
{
    [Header("Reusable location data")]
    public HidingLocationData location;

    [Header("Location background")]
    public Sprite photo;

    [Header("Selectable hiding spots")]
    public Sprite[] spotSprites;
    public string[] spotNames;

    public void Open()
    {
        if (HidingPanelManager.Instance == null)
            return;

        if (location != null)
            HidingPanelManager.Instance.OpenExaminePanel(location.background, location.spotSprites, location.spotNames);
        else
            HidingPanelManager.Instance.OpenExaminePanel(photo, spotSprites, spotNames);
    }
}
