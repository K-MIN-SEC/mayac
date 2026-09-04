using UnityEngine;

[CreateAssetMenu(fileName = "HidingLocation", menuName = "Shadow Drop/Hiding Location")]
public class HidingLocationData : ScriptableObject
{
    public string locationName;
    public Sprite background;
    public Sprite[] spotSprites;
    public string[] spotNames;

    [Header("정답 지점, none = -1")]
    public int correctSpotIndex = -1;

    [Header("각 지점의 화면 위치")]
    public Vector2[] spotPositions;
    public Vector2[] spotSizes;
}
