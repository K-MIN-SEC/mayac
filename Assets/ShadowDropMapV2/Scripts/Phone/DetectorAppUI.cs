using UnityEngine;
using UnityEngine.UI;

public class DetectorAppUI : MonoBehaviour
{
    public static DetectorAppUI Instance { get; private set; }

    [Header("UI 상태 관리")]
    public GameObject inactiveMessage;
    public GameObject activeContent;
    public GameObject appIconBadgeDetector;

    [Header("금속 탐지기 연출")]
    public Transform player;
    public Image detectorIcon;
    public Color normalColor = Color.white;
    public Color alertColor = Color.yellow;
    public UIShaker shaker; // 화면 흔들림 연출

    [Header("탐지 설정")]
    public float detectRadius = 20f;         // 감지 시작 거리
    public float closeRadius = 1.5f;         // 최고 속도 도달 거리
    public float slowestPingInterval = 1.2f; // 멀 때의 알람 주기 (초)
    public float fastestPingInterval = 0.15f;// 가까울 때의 알람 주기 (초)

    private bool isActivated = false;
    private bool isScreenOpen = false;
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

    // 외부 이벤트(메시지)에서 타겟을 지정하며 활성화할 때 호출
    public void Activate(Transform target)
    {
        isActivated = true;
        currentTarget = target;
        RefreshVisibility();
    }

    public void OnScreenOpened()
    {
        isScreenOpen = true;
        HideAlertBorder();
    }

    public void OnScreenClosed()
    {
        isScreenOpen = false;
    }

    public void ShowAlertBorder()
    {
        if (appIconBadgeDetector != null) appIconBadgeDetector.SetActive(true);
    }

    public void HideAlertBorder()
    {
        if (appIconBadgeDetector != null) appIconBadgeDetector.SetActive(false);
    }

    private void RefreshVisibility()
    {
        if (inactiveMessage != null) inactiveMessage.SetActive(!isActivated);
        if (activeContent != null) activeContent.SetActive(isActivated);
    }

    void Update()
    {
        // 앱이 켜져있고, 타겟이 존재할 때만 거리 연산 수행
        if (!isActivated || !isScreenOpen || currentTarget == null || player == null) return;

        float distance = Vector2.Distance(player.position, currentTarget.position);

        // 탐지 범위 밖이면 아무 반응 없음
        if (distance > detectRadius) return;

        // 금속 탐지기 핵심 로직: 거리에 비례하여 알람 주기(Interval) 계산
        float t = Mathf.InverseLerp(detectRadius, closeRadius, distance);
        float currentInterval = Mathf.Lerp(slowestPingInterval, fastestPingInterval, t);

        pingTimer -= Time.deltaTime;
        if (pingTimer <= 0f)
        {
            pingTimer = currentInterval;
            Ping();
        }
    }

    private void Ping()
    {
        if (detectorIcon != null) detectorIcon.color = alertColor;
        if (shaker != null) shaker.Shake();

        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), 0.15f); // 0.15초 뒤 원래 색으로 복귀
    }

    private void ResetColor()
    {
        if (detectorIcon != null) detectorIcon.color = normalColor;
    }
}