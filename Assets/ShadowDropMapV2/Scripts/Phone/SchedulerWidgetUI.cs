using TMPro;
using UnityEngine;

// 홈 화면에 항상 보이는 스케쥴러 위젯. 현재 목표(할 일)를 한 줄로 보여줌
public class SchedulerWidgetUI : MonoBehaviour
{
    public static SchedulerWidgetUI Instance { get; private set; }

    public TMP_Text objectiveText;

    [Header("아직 목표가 없을 때 표시할 문구")]
    [TextArea]
    public string defaultText = "오늘 할 일이 없습니다.";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SetObjective(defaultText);
    }

    public void SetObjective(string text)
    {
        if (objectiveText != null)
            objectiveText.text = text;
    }
}