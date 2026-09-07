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

    public Sprite curPointImage;

    [Header("Selectable hiding spots")]
    [SerializeField] private HideSpotHotspot[] hotspotButtons;

    [Header("Confirmation dialog")]
    public GameObject confirmDialog;
    public TMP_Text confirmText;

    private string pendingSpotName;
    private bool pendingIsCorrect;
    bool isHiding = false;
    [HideInInspector] public int hidingResultState = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        examinePanel.SetActive(false);
        confirmDialog.SetActive(false);
    }

    public void OpenExaminePanel(HidingLocationData data)
    {
        if(StoryManager.instance.isDialogue) return;

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
        if(StoryManager.instance.isDialogue) return;

        examinePanel.SetActive(false);
        confirmDialog.SetActive(false);
    }

    public void RequestHide(string spotName, Sprite pointImage, bool isCorrectSpot)
    {
        pendingSpotName = spotName;
        pendingIsCorrect = isCorrectSpot;
        curPointImage = pointImage;
        isHiding = false;

        confirmText.text = $"Hide in {spotName}?";
        Debug.Log($"RequestHide: {spotName}, isCorrectSpot: {isCorrectSpot}");
        confirmDialog.SetActive(true);
    }

    public void ConfirmYes()
    {
        if(isHiding) return;
        isHiding = true;
        hidingResultState = pendingIsCorrect ? 1 : 2;
        StoryManager.instance.TryPlayStory("Hiding");

        //HidingRecord.Add(pendingSpotName, pendingIsCorrect);
        examinePanel.SetActive(false);
        confirmDialog.SetActive(false);
    }

    public void ConfirmNo()
    {
        confirmDialog.SetActive(false);
    }
}