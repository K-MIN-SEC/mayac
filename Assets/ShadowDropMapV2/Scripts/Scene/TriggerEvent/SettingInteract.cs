using System.Collections.Generic;
using UnityEngine;

public class SettingInteract : Trigger
{
    protected override string eventName => "SettingInteract";
    [SerializeField] List<GameObject> targetPos = new();
    int index;

    protected override void Start()
    {
        base.Start();
        foreach(var n in targetPos)
        {
            n.SetActive(false);
        }
    }

    protected override void TriggerEvent()
    {
        if (index >= targetPos.Count || targetPos[index] == null) return;
        targetPos[index].SetActive(true);
        index++;
    }
}
