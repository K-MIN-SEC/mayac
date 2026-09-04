using UnityEngine;

public class ExamineTrigger : MonoBehaviour
{
    [Header("Reusable location data")]
    public HidingLocationData location;

    public void Open()
    {
        if (HidingPanelManager.Instance == null)
            return;

        HidingPanelManager.Instance.OpenExaminePanel(location);
    }
}
