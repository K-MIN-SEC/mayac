using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ObjMove : Trigger
{
    protected override string eventName => "ObjMove";
    [SerializeField] private List<RectTransform> objList = new();
    [SerializeField] private List<Vector2> targetPos = new();

    [SerializeField] int index = -1;

    protected override void TriggerEvent()
    {
        if(index > 0)  objList[index].DOComplete();
        index++;

        objList[index].DOAnchorPos(targetPos[index], 0.5f);
    }
}
