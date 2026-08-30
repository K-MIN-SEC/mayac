using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    public static MainCanvas instance { get; private set; }

    public Canvas canvas;
    public Transform bubble;

    void Awake()
    {
        instance = this;
    }
}
