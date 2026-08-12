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
        OpenExaminePanel(photo, null, null);
    }

    public void OpenExaminePanel(Sprite photo, Sprite[] spotSprites, string[] spotNames)
    {
        if (photoImage != null)
        {
            photoImage.sprite = photo;
            photoImage.preserveAspect = true;
        }

        if (examinePanel != null) examinePanel.SetActive(true);
        if (confirmDialog != null) confirmDialog.SetActive(false);
        ConfigureHotspots(spotSprites, spotNames);
    }

    private void ConfigureHotspots(Sprite[] spotSprites, string[] spotNames)
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
            hotspotButtons[index].Configure(spotName, spotSprites[index]);
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
        pendingSpotName = spotName;
        if (confirmText != null) confirmText.text = $"Hide in {spotName}?";
        if (confirmDialog != null) confirmDialog.SetActive(true);
    }

    public void ConfirmYes()
    {
        Debug.Log($"Hidden at {pendingSpotName}.");
        CloseExaminePanel();
    }

    public void ConfirmNo()
    {
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }
}
