using UnityEngine;
using UnityEngine.UI;

// 폰의 "탐색기" 앱 화면.
// 탐색기 관련 메시지가 오기 전엔 "탐색기를 사용할 일이 없습니다" 안내만 보여줌.
// 메시지가 오면(Activate 호출) 지정된 목표(돋보기 조사 지점) 쪽으로,
// 가까워질수록 자주 반짝이며 알려줌 (거리 멀면 느리게, 가까우면 빠르게).
public class DetectorAppUI : MonoBehaviour
{
    public static DetectorAppUI Instance { get; private set; }

    [Header("화면 요소")]
    public GameObject inactiveMessage;   // "탐색기를 사용할 일이 없습니다" 문구 오브젝트
    public GameObject activeContent;     // 실제 탐지기 UI(바늘/아이콘 등) 오브젝트

    [Header("탐색기 연출")]
    public Transform player;             // 비워두면 "Player" 태그로 자동 탐색
    public RectTransform needle;         // 목표 방향을 가리키는 바늘 (없으면 비워둬도 됨)
    public Image detectorIcon;
    public Color normalColor = Color.white;
    public Color alertColor = Color.yellow;
    public UIShaker shaker;

    [Header("홈 화면 탐색기 아이콘 알림 배지 (AppIconBadge(detector) 오브젝트 연결)")]
    public GameObject appIconBadgeDetector; // 평소엔 꺼져있다가, "네, 할게요" 답장하면 켜지는 빨간 점

    [Header("탐지 범위")]
    public float detectRadius = 15f;
    public float closeRadius = 1.5f;
    public float slowestPingInterval = 1.2f;
    public float fastestPingInterval = 0.15f;

    private bool isActivated = false;   // 탐색기 사용 가능 여부 (메시지로 활성화됨)
    private bool isScreenOpen = false;  // 지금 이 화면을 실제로 보고 있는지
    private Transform currentTarget;
    private float pingTimer;

    void Awake()
    {
        Instance = this;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Start()
    {
        RefreshVisibility();
    }

    // MissionMessageSender 등에서, "탐색기를 써야 하는" 메시지가 왔을 때 이걸 호출해서 켬
    public void Activate(Transform target)
    {
        isActivated = true;
        currentTarget = target;
        RefreshVisibility();
    }

    // SmartphoneUIManager가 이 화면을 열 때/떠날 때 호출해줌
    public void OnScreenOpened()
    {
        isScreenOpen = true;
        HideAlertBorder(); // 앱을 열어서 확인했으니 알림 테두리는 꺼줌
    }

    public void OnScreenClosed()
    {
        isScreenOpen = false;
    }

    // "네, 할게요" 답장을 고르면 호출되어 AppIconBadge(detector)를 켬
    public void ShowAlertBorder()
    {
        if (appIconBadgeDetector != null)
            appIconBadgeDetector.SetActive(true);
    }

    public void HideAlertBorder()
    {
        if (appIconBadgeDetector != null)
            appIconBadgeDetector.SetActive(false);
    }

    void RefreshVisibility()
    {
        if (inactiveMessage != null) inactiveMessage.SetActive(!isActivated);
        if (activeContent != null) activeContent.SetActive(isActivated);
    }

    void Update()
    {
        // 탐색기가 활성화(메시지로 켜짐) + 지금 이 화면을 실제로 보고 있을 때만 동작
        if (!isActivated || !isScreenOpen || currentTarget == null || player == null) return;

        Vector2 toTarget = (Vector2)currentTarget.position - (Vector2)player.position;
        float distance = toTarget.magnitude;

        if (needle != null && toTarget.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            needle.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        if (distance > detectRadius)
            return; // 범위 밖이면 조용히 대기

        float t = Mathf.InverseLerp(detectRadius, closeRadius, distance); // 가까울수록 1에 가까움
        float currentInterval = Mathf.Lerp(slowestPingInterval, fastestPingInterval, t);

        pingTimer -= Time.deltaTime;
        if (pingTimer <= 0f)
        {
            pingTimer = currentInterval;
            Ping();
        }
    }

    void Ping()
    {
        if (detectorIcon != null) detectorIcon.color = alertColor;
        if (shaker != null) shaker.Shake();

        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), 0.15f);
    }

    void ResetColor()
    {
        if (detectorIcon != null) detectorIcon.color = normalColor;
    }
}