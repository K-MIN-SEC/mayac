using UnityEngine;

// 탐색기(CompassNavigator)가 추적할 현재 목표 지점.
public class MissionTarget : MonoBehaviour
{
    public static MissionTarget Current { get; private set; }

    void OnEnable()
    {
        Current = this;
    }

    void OnDisable()
    {
        if (Current == this)
            Current = null;
    }
}