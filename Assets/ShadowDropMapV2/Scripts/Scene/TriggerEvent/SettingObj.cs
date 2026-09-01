using System.Collections.Generic;
using UnityEngine;
using System;

public class SettingObj : Trigger
{
    protected override string eventName => "SettingObj";
    [SerializeField] List<GameObject> dayList = new();
    [SerializeField] List<TargetObj> targets = new();
    int index;

    protected override void TriggerEvent()
    {
        int curIndex = StoryManager.instance.dialogueIndex;
        if (curIndex >= targets.Count || targets[curIndex] == null) return;
        if (index >= targets[curIndex].targetPos.Count) return;

        GameObject targetObj = targets[curIndex].targetPos[index];
        targetObj.SetActive(true);

        if (targetObj.TryGetComponent(out ExamineTrigger examineTrigger))
        {
            DetectorAppUI.Instance.Activate(targetObj.transform);
        }
        index++;
    }

    protected override void Init()
    {
        int curIndex = StoryManager.instance.dialogueIndex;
        for (int i = 0; i < targets.Count; i++)
        {
            if (curIndex != i) dayList[i].SetActive(false);
            else dayList[i].SetActive(true);
        }
        index = 0;
    }
}

[Serializable]
public class TargetObj
{
    public List<GameObject> targetPos = new();
}
