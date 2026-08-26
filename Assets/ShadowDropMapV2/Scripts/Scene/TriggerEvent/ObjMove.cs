using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ObjMove : Trigger
{
    [SerializeField] private List<RectTransform> objList = new();
    [SerializeField] private List<Vector2> targetPos = new();

    [SerializeField] int index = -1;

    protected override void TriggerEvent()
    {
        if(index > 0)  objList[index].DOComplete();
        index++;

        Debug.Log($"{index} Trigger");
        objList[index].DOAnchorPos(targetPos[index], 0.5f);
    }

    protected override void EndEvent()
    {
        Debug.Log($"{index} End");
        
    }

    void Start()
    {
        eventName = "ObjMove";
    }
}
