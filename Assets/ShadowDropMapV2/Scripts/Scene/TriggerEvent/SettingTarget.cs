using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;

public class SettingTarget : Trigger
{
    protected override string eventName => "SettingTarget";
    [SerializeField] List<Transform> targetPos = new();

    protected override void TriggerEvent()
    {
        int currentIndex = StoryManager.instance.targetIndex;
        if (currentIndex >= targetPos.Count || targetPos[currentIndex] == null) return;

        DetectorAppUI.Instance.Activate(targetPos[currentIndex]);
        StoryManager.instance.targetIndex++;
    }
}
