using UnityEngine;
using TMPro;

public class OffScreenBubbleUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform bubbleBodyRect;
    public RectTransform tailPivot;
    public TMP_Text dialogueText;

    [Header("Settings")]
    public float baseMargin = 20f;
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    public float minTailDistance = 30f;

    private Transform targetNPC;
    private Camera mainCam;
    private Canvas parentCanvas;

    private void Awake()
    {
        mainCam = Camera.main;
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void Initialize(Transform npc, string text)
    {
        targetNPC = npc;
        dialogueText.text = text;
    }

    private void LateUpdate()
    {
        Vector3 targetWorldPos = targetNPC.position + worldOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(targetWorldPos);

        float scale = parentCanvas.scaleFactor;

        float scaledHalfWidth = (bubbleBodyRect.rect.width * 0.5f * scale) + (baseMargin * scale);
        float scaledHalfHeight = (bubbleBodyRect.rect.height * 0.5f * scale) + (baseMargin * scale);

        float minX = scaledHalfWidth;
        float maxX = Screen.width - scaledHalfWidth;
        float minY = scaledHalfHeight;
        float maxY = Screen.height - scaledHalfHeight;

        bool isOffScreen = screenPos.x < minX || screenPos.x > maxX ||
                           screenPos.y < minY || screenPos.y > maxY;

        Vector3 clampedPos = screenPos;
        clampedPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
        clampedPos.y = Mathf.Clamp(screenPos.y, minY, maxY);

        transform.position = clampedPos;

        if (isOffScreen)
        {
            Vector3 npcScreenPos = mainCam.WorldToScreenPoint(targetNPC.position);
            Vector2 dir = (npcScreenPos - clampedPos).normalized;

            float canvasScale = parentCanvas.scaleFactor;

            float logicalHalfW = bubbleBodyRect.rect.width * 0.5f;
            float logicalHalfH = bubbleBodyRect.rect.height * 0.5f;

            float absDirX = Mathf.Max(Mathf.Abs(dir.x), 0.0001f);
            float absDirY = Mathf.Max(Mathf.Abs(dir.y), 0.0001f);

            float logicalT = Mathf.Min(logicalHalfW / absDirX, logicalHalfH / absDirY);
            float pixelT = logicalT * canvasScale;
            float distToNPC = Vector2.Distance(clampedPos, npcScreenPos);

            if (distToNPC > pixelT + minTailDistance)
            {
                tailPivot.localPosition = dir * logicalT;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                tailPivot.gameObject.SetActive(true);
                tailPivot.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                tailPivot.gameObject.SetActive(false);
            }
        }
        else
        {
            tailPivot.gameObject.SetActive(false);
        }
    }
}