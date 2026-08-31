using System.Collections.Generic;
using UnityEngine;

public class SettingTarget : Trigger
{
    protected override string eventName => "SettingTarget";
    [SerializeField] List<Transform> targetPos = new();

    protected override void TriggerEvent()
    {
        int currentIndex = StoryManager.instance.targetIndex;
        if (currentIndex >= targetPos.Count || targetPos[currentIndex] == null) return;

        targetPos[currentIndex].gameObject.SetActive(true);
        DetectorAppUI.Instance.Activate(targetPos[currentIndex]);
        StoryManager.instance.targetIndex++;
    }
}
