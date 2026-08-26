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

    [Header("정답 지점 / 위치 (location을 안 쓰고 직접 입력할 때만 사용)")]
    public int correctSpotIndex = -1;
    public Vector2[] spotPositions;

    public void Open()
    {
        if (HidingPanelManager.Instance == null)
            return;

        if (location != null)
            HidingPanelManager.Instance.OpenExaminePanel(
                location.background, location.spotSprites, location.spotNames,
                location.correctSpotIndex, location.spotPositions);
        else
            HidingPanelManager.Instance.OpenExaminePanel(
                photo, spotSprites, spotNames,
                correctSpotIndex, spotPositions);
    }
}
