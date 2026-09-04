using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HidingPanelManager : MonoBehaviour
{
    public static HidingPanelManager Instance { get; private set; }

    [Header("Examine panel")]
    public GameObject examinePanel;
    public Image photoImage;

    [Header("Selectable hiding spots")]
    [SerializeField] private HideSpotHotspot[] hotspotButtons;

    [Header("Confirmation dialog")]
    public GameObject confirmDialog;
    public TMP_Text confirmText;

    private string pendingSpotName;
    private bool pendingIsCorrect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // 💡 굳이 null 체크를 하지 않습니다. 할당이 안 되어 있으면 여기서 에러가 나도록 유도합니다.
        examinePanel.SetActive(false);
        confirmDialog.SetActive(false);
    }

    public void OpenExaminePanel(HidingLocationData data)
    {
        photoImage.sprite = data.background;
        photoImage.preserveAspect = true;

        examinePanel.SetActive(true);
        confirmDialog.SetActive(false);
        
        StoryManager.instance.TryPlayStory("OpenPanel");
        ConfigureHotspots(data);
    }

    private void ConfigureHotspots(HidingLocationData data)
    {
        if (data.spotSprites == null || data.spotSprites.Length == 0)
        {
            foreach (var btn in hotspotButtons) btn.gameObject.SetActive(false);
            return; 
        }

        EnsureHotspotCapacity(data.spotSprites.Length);

        for (int index = 0; index < hotspotButtons.Length; index++)
        {
            bool hasSpot = index < data.spotSprites.Length;
            hotspotButtons[index].gameObject.SetActive(hasSpot);
            
            if (!hasSpot) continue;

            hotspotButtons[index].Configure(
                data.spotNames[index], 
                data.spotSprites[index], 
                index == data.correctSpotIndex, 
                data.spotPositions[index],
                data.spotSizes[index]
            );
        }
    }

    private void EnsureHotspotCapacity(int requiredCount)
    {
        if (requiredCount <= hotspotButtons.Length) return;

        HideSpotHotspot template = hotspotButtons[hotspotButtons.Length - 1];
        int previousCount = hotspotButtons.Length;
        Array.Resize(ref hotspotButtons, requiredCount);

        for (int index = previousCount; index < requiredCount; index++)
        {
            HideSpotHotspot clone = Instantiate(template, template.transform.parent);
            clone.name = $"HideSpotHotspot_{index + 1}";
            clone.gameObject.SetActive(true);
            hotspotButtons[index] = clone;
        }
    }

    public void CloseExaminePanel()
    {
        examinePanel.SetActive(false);
        confirmDialog.SetActive(false);
    }

    public void RequestHide(string spotName, bool isCorrectSpot = false)
    {
        pendingSpotName = spotName;
        pendingIsCorrect = isCorrectSpot;
        
        confirmText.text = $"Hide in {spotName}?";
        confirmDialog.SetActive(true);
    }

    public void ConfirmYes()
    {
        StoryManager.instance.TryPlayStory("Hiding");
        if (!pendingIsCorrect) StoryManager.instance.indexWeight++;

        HidingRecord.Add(pendingSpotName, pendingIsCorrect);
        CloseExaminePanel();
    }

    public void ConfirmNo()
    {
        confirmDialog.SetActive(false);
    }
}