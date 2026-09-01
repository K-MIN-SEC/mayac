using System.Collections.Generic;
using UnityEngine;
using System;

public class SettingObj : Trigger
{
    protected override string eventName => "SettingObj";
    [SerializeField] List<DayObj    > dayList = new();
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
        for (int i = 0; i < dayList.Count; i++)
        {
            if (curIndex == i)
            {
                dayList[i].gameObject.SetActive(true);
                if(dayList[i].isNight) StoryManager.instance.ChangeToNight(true);
                else StoryManager.instance.ChangeToNight(false);

            }
            else dayList[i].gameObject.SetActive(false);
        }
        for(int i = 0; i < targets.Count; i++)
        {
            if (curIndex != i) continue;
            for (int j = 0; j < targets[i].targetPos.Count; j++) targets[i].targetPos[j].SetActive(false);
        }
        index = 0;
    }

    protected override void Start()
    {
        base.Start();
        Init();
    }
}

[Serializable]
public class TargetObj
{
    public List<GameObject> targetPos = new();
}
