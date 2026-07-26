using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// 우측 하단, 폰 버튼 옆에 항상 떠있는 나침반(탐색기) 아이콘.
public class CompassNavigator : MonoBehaviour
{
    [Header("항상 화면에 표시할지, 목표가 있을 때만 표시할지")]
    public bool alwaysVisible = true;

    [Header("참조")]
    public Transform player;
    public RectTransform needle;

    [Header("탐지 설정")]
    public float alertRadius = 3f;

    [Header("알림 연출")]
    public Image compassIcon;
    public Color normalColor = Color.white;
    public Color alertColor = Color.yellow;
    public AudioSource alertSound;
    public GameObject alertPulseFX;

    [Header("이벤트")]
    public UnityEvent onEnterAlertRange;
    public UnityEvent onExitAlertRange;

    private bool inAlertRange;

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
            if (inAlertRange)
            {
                inAlertRange = false;
                OnExitAlert();
            }
            return;
        }

        Vector2 toTarget = (Vector2)target.transform.position - (Vector2)player.position;
        float distance = toTarget.magnitude;

        if (needle != null && toTarget.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            needle.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        bool nowInRange = distance <= alertRadius;

        if (nowInRange && !inAlertRange)
        {
            inAlertRange = true;
            OnEnterAlert();
        }
        else if (!nowInRange && inAlertRange)
        {
            inAlertRange = false;
            OnExitAlert();
        }
    }

    void OnEnterAlert()
    {
        if (compassIcon != null) compassIcon.color = alertColor;
        if (alertPulseFX != null) alertPulseFX.SetActive(true);
        if (alertSound != null) alertSound.Play();
        onEnterAlertRange?.Invoke();
    }

    void OnExitAlert()
    {
        if (compassIcon != null) compassIcon.color = normalColor;
        if (alertPulseFX != null) alertPulseFX.SetActive(false);
        onExitAlertRange?.Invoke();
    }
}