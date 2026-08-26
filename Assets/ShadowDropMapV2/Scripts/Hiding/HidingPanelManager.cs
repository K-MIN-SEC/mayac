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
        if (examinePanel != null) examinePanel.SetActive(false);
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }

    public void OpenExaminePanel(Sprite photo)
    {
        OpenExaminePanel(photo, null, null, -1, null);
    }

    public void OpenExaminePanel(Sprite photo, Sprite[] spotSprites, string[] spotNames)
    {
        OpenExaminePanel(photo, spotSprites, spotNames, -1, null);
    }

    public void OpenExaminePanel(Sprite photo, Sprite[] spotSprites, string[] spotNames, int correctSpotIndex)
    {
        OpenExaminePanel(photo, spotSprites, spotNames, correctSpotIndex, null);
    }

    // spotPositions: 각 지점을 사진 속 어디에 배치할지 (없으면 null, 그러면 위치는 그대로 둠)
    public void OpenExaminePanel(Sprite photo, Sprite[] spotSprites, string[] spotNames, int correctSpotIndex, Vector2[] spotPositions)
    {
        if (photoImage != null)
        {
            photoImage.sprite = photo;
            photoImage.preserveAspect = true;
        }

        if (examinePanel != null) examinePanel.SetActive(true);
        if (confirmDialog != null) confirmDialog.SetActive(false);
        ConfigureHotspots(spotSprites, spotNames, correctSpotIndex, spotPositions);
    }

    private void ConfigureHotspots(Sprite[] spotSprites, string[] spotNames, int correctSpotIndex, Vector2[] spotPositions)
    {
        if ((hotspotButtons == null || hotspotButtons.Length == 0) && examinePanel != null)
            hotspotButtons = examinePanel.GetComponentsInChildren<HideSpotHotspot>(true);
        if (hotspotButtons == null || hotspotButtons.Length == 0)
            return;

        bool hasConfiguredSpots = spotSprites != null && spotSprites.Length > 0;
        if (hasConfiguredSpots)
            EnsureHotspotCapacity(spotSprites.Length);

        for (int index = 0; index < hotspotButtons.Length; index++)
        {
            if (!hasConfiguredSpots)
            {
                hotspotButtons[index].gameObject.SetActive(true);
                continue;
            }

            bool hasSpot = index < spotSprites.Length && spotSprites[index] != null;
            hotspotButtons[index].gameObject.SetActive(hasSpot);
            if (!hasSpot)
                continue;

            string spotName = spotNames != null && index < spotNames.Length
                ? spotNames[index]
                : $"Spot {index + 1}";
            bool isCorrect = index == correctSpotIndex;

            Vector2? position = null;
            if (spotPositions != null && index < spotPositions.Length)
                position = spotPositions[index];

            hotspotButtons[index].Configure(spotName, spotSprites[index], isCorrect, position);
        }
    }

    private void EnsureHotspotCapacity(int requiredCount)
    {
        if (requiredCount <= hotspotButtons.Length)
            return;

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
        if (examinePanel != null) examinePanel.SetActive(false);
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }

    public void RequestHide(string spotName)
    {
        RequestHide(spotName, false);
    }

    public void RequestHide(string spotName, bool isCorrectSpot)
    {
        pendingSpotName = spotName;
        pendingIsCorrect = isCorrectSpot;
        if (confirmText != null) confirmText.text = $"Hide in {spotName}?";
        if (confirmDialog != null) confirmDialog.SetActive(true);
    }

    public void ConfirmYes()
    {
        string correctText = pendingIsCorrect ? "정답!" : "오답";
        Debug.Log($"Hidden at {pendingSpotName}. ({correctText})");
        HidingRecord.Add(pendingSpotName, pendingIsCorrect);
        CloseExaminePanel();
    }

    public void ConfirmNo()
    {
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }
}
