using UnityEngine;

public class NPCAmbientDialogue : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject bubblePrefab;
    
    private Transform canvasTransform; 
    private GameObject activeBubble;

    void Start()
    {
        canvasTransform = MainCanvas.instance.bubble;
    }

    private void OnDisable()
    {
        HideBubble();
    }

    public void ShowBubble(string text)
    {
        if (activeBubble == null)
        {
            activeBubble = Instantiate(bubblePrefab, canvasTransform);
        }
        
        if (activeBubble.TryGetComponent(out OffScreenBubbleUI bubbleUI))
        {
            bubbleUI.Initialize(this.transform, text);
        }
    }

    public void HideBubble()
    {
        if (activeBubble != null)
        {
            Destroy(activeBubble);
            activeBubble = null;
        }
    }
}