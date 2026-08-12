using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 폰 화면 상단(StatusBar)에 붙는 날짜 + 낮/밤 표시 위젯.
// 실제 기기 시간(System.DateTime.Now) 기준으로 날짜와 낮/밤을 보여줌.
public class DateWidgetUI : MonoBehaviour
{
    [Header("표시할 텍스트")]
    public TMP_Text dateText;   // 예: "8월 11일 화요일"

    [Header("낮/밤 아이콘 (둘 중 하나만 켜짐)")]
    public GameObject dayIcon;   // 해 모양 등
    public GameObject nightIcon; // 달 모양 등

    [Header("낮으로 취급할 시간 범위 (24시간 기준)")]
    public int dayStartHour = 6;  // 이 시간부터
    public int dayEndHour = 18;   // 이 시간 전까지 낮

    [Header("갱신 주기 (초) - 자정 넘어가는 것까지 반영하려면 60초 정도면 충분함")]
    public float refreshInterval = 30f;

    private static readonly string[] WeekdayNames =
        { "일요일", "월요일", "화요일", "수요일", "목요일", "금요일", "토요일" };

    private float timer;

    void Start()
    {
        Refresh();
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= refreshInterval)
        {
            timer = 0f;
            Refresh();
        }
    }

    void Refresh()
    {
        System.DateTime now = System.DateTime.Now;

        if (dateText != null)
        {
            string weekday = WeekdayNames[(int)now.DayOfWeek];
            dateText.text = now.Month + "월 " + now.Day + "일 " + weekday;
        }

        bool isDay = now.Hour >= dayStartHour && now.Hour < dayEndHour;

        if (dayIcon != null) dayIcon.SetActive(isDay);
        if (nightIcon != null) nightIcon.SetActive(!isDay);
    }
}