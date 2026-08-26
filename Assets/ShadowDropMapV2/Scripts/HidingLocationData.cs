using UnityEngine;

[CreateAssetMenu(fileName = "HidingLocation", menuName = "Shadow Drop/Hiding Location")]
public class HidingLocationData : ScriptableObject
{
    public string locationName;
    public Sprite background;
    public Sprite[] spotSprites;
    public string[] spotNames;

    [Header("정답 지점 (spotNames 배열에서 몇 번째가 정답인지, 0부터 시작. 정답 없으면 -1)")]
    public int correctSpotIndex = -1;

    [Header("각 지점의 화면 위치 (사진 안에서 이 물건이 있는 자리, Anchored Position 기준)")]
    public Vector2[] spotPositions;
}
