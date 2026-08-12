using UnityEngine;

[CreateAssetMenu(fileName = "HidingLocation", menuName = "Shadow Drop/Hiding Location")]
public class HidingLocationData : ScriptableObject
{
    public string locationName;
    public Sprite background;
    public Sprite[] spotSprites;
    public string[] spotNames;
}
