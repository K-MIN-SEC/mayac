using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// 우측 하단, 폰 버튼 옆에 항상 떠있는 나침반(탐색기) 아이콘.
// MissionTarget.Current를 향해 바늘이 회전하고, 가까워질수록 반짝임/진동 빈도가 빨라짐 (금속탐지기 느낌).
public class CompassNavigator : MonoBehaviour
{
    [Header("항상 화면에 표시할지, 목표가 있을 때만 표시할지")]
    public bool alwaysVisible = true;

    [Header("참조")]
    public Transform player;           // 플레이어 캐릭터 (비워두면 "Player" 태그로 자동 탐색)
    public RectTransform needle;       // 나침반 바늘 이미지 (목표 방향을 가리키도록 회전시킴)

    [Header("탐지 범위")]
    public float detectRadius = 15f;   // 이 거리 밖이면 아예 반응 없음
    public float closeRadius = 1.5f;   // 이 거리 안으로 들어오면 최대 속도로 반응

    [Header("반응 속도(핑 간격, 초) - 멀수록 느리게, 가까울수록 빠르게")]
    public float slowestPingInterval = 1.2f; // detectRadius 경계에서의 간격
    public float fastestPingInterval = 0.15f; // closeRadius 안쪽에서의 간격

    [Header("알림 연출")]
    public Image compassIcon;
    public Color normalColor = Color.white;
    public Color alertColor = Color.yellow;
    public AudioSource pingSound;      // 핑 소리 (비워두면 무시)
    public UIShaker shaker;            // 핑마다 살짝 흔들기 (비워두면 무시)

    [Header("이벤트")]
    public UnityEvent onPing;            // 핑 울릴 때마다 호출 (거리 상관없이)
    public UnityEvent onEnterDetectRange; // 탐지 범위 진입 시 1회 호출
    public UnityEvent onExitDetectRange;  // 탐지 범위 이탈 시 1회 호출

    private bool inDetectRange;
    private float pingTimer;

    void Awake()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        MissionTarget target = MissionTarget.Current;
        bool hasTarget = target != null && player != null;

        if (!hasTarget)
        {
            if (inDetectRange)
            {
                inDetectRange = false;
                SetIdleVisual();
                onExitDetectRange?.Invoke();
            }
            return;
        }

        Vector2 toTarget = (Vector2)target.transform.position - (Vector2)player.position;
        float distance = toTarget.magnitude;

        // 바늘 회전 (바늘 이미지가 "위쪽"을 기본 방향으로 그려졌다고 가정)
        if (needle != null && toTarget.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            needle.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        bool nowInRange = distance <= detectRadius;

        if (nowInRange && !inDetectRange)
        {
            inDetectRange = true;
            pingTimer = 0f; // 진입하자마자 바로 한 번 핑
            onEnterDetectRange?.Invoke();
        }
        else if (!nowInRange && inDetectRange)
        {
            inDetectRange = false;
            SetIdleVisual();
            onExitDetectRange?.Invoke();
        }

        if (inDetectRange)
        {
            float t = Mathf.InverseLerp(detectRadius, closeRadius, distance); // 가까울수록 1에 가까움
            float currentInterval = Mathf.Lerp(slowestPingInterval, fastestPingInterval, t);

            pingTimer -= Time.deltaTime;
            if (pingTimer <= 0f)
            {
                pingTimer = currentInterval;
                Ping();
            }
        }
    }

    // 핑 한 번 - 색 반짝임 + 소리 + 흔들림
    void Ping()
    {
        if (compassIcon != null) compassIcon.color = alertColor;
        if (pingSound != null) pingSound.Play();
        if (shaker != null) shaker.Shake();
        onPing?.Invoke();

        // 짧게 색을 되돌리기 위해 다음 프레임에라도 서서히 정상색으로
        CancelInvoke(nameof(ResetColorAfterPing));
        Invoke(nameof(ResetColorAfterPing), 0.15f);
    }

    void ResetColorAfterPing()
    {
        if (compassIcon != null) compassIcon.color = normalColor;
    }

    void SetIdleVisual()
    {
        if (compassIcon != null) compassIcon.color = normalColor;
    }
}