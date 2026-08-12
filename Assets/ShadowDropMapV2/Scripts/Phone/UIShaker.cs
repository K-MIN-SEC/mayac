using UnityEngine;
using System.Collections;

// 아이콘이나 UI 요소를 잠깐 흔들어서(진동) 알림을 표현하는 범용 컴포넌트.
// phone 버튼, compassicon 등 아무 UI 오브젝트에나 붙여서 Shake()만 호출하면 됨.
public class UIShaker : MonoBehaviour
{
    [Header("흔들리는 정도")]
    public float shakeDuration = 0.4f;
    public float shakeStrength = 8f;

    private RectTransform rt;
    private Vector2 originalPos;
    private Coroutine shakeRoutine;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        originalPos = rt.anchoredPosition;
    }

    public void Shake()
    {
        if (!gameObject.activeInHierarchy) return;
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float strength = shakeStrength * (1f - elapsed / shakeDuration); // 시간이 지날수록 약해짐
            Vector2 offset = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * strength;
            rt.anchoredPosition = originalPos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = originalPos;
    }
}