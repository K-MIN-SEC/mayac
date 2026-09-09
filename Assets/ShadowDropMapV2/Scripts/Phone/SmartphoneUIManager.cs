using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SmartphoneUIManager : MonoBehaviour
{
    public Button phoneButton;

    public GameObject phonePanel;
    public GameObject homeScreen;
    public GameObject messengerScreen;
    public GameObject settingsScreen;
    public GameObject detectorScreen;

    public Button messengerAppIcon;
    public Button settingsAppIcon;
    public Button detectorAppIcon;
    public Button closeButton;
    public Button backButton;

    public Button navHomeButton;
    public Button navBackButton;
    public Button navRecentsButton;

    public AudioClip clickSound;
    private Image backButtonSprite;
    private Tween blinkTween;
    private Color originalColor = new Color32(178, 178, 178, 255);

    void Start()
    {
        phonePanel.SetActive(false);
        backButtonSprite = navBackButton.GetComponent<Image>();

        BindButton(phoneButton, TogglePhone);
        BindButton(closeButton, ClosePhone);
        BindButton(backButton, OnBackPressed);
        BindButton(messengerAppIcon, OpenMessenger);
        BindButton(settingsAppIcon, OpenSettings);
        BindButton(detectorAppIcon, OpenDetector);
        BindButton(navHomeButton, GoHome);
        BindButton(navBackButton, OnBackPressed);
        BindButton(navRecentsButton, ClosePhone);
    }

    private void Update()
    {
        if (phonePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackPressed();
        }
    }

    private void OnDisable()
    {
        blinkTween?.Kill();
        backButtonSprite.color = originalColor;
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
            button.onClick.AddListener(() => SoundManager.instance.SetAudio(clickSound, false)); // 미리 만들어둔 사운드 출력 함수
        }
    }

    public void TogglePhone()
    {
        if (phonePanel.activeSelf) ClosePhone();
        else OpenPhone();
    }

    public void OpenPhone()
    {
        phonePanel.SetActive(true);
        GoHome();

        if (phoneButton != null)
        {
            phoneButton.interactable = false;
            SetPhoneButtonAlpha(0f);
        }
    }

    public void ClosePhone()
    {
        phonePanel.SetActive(false);

        if (DetectorAppUI.Instance != null) DetectorAppUI.Instance.OnScreenClosed();

        if (phoneButton != null)
        {
            phoneButton.interactable = true;
            SetPhoneButtonAlpha(1f);
        }
    }

    void SetPhoneButtonAlpha(float alpha)
    {
        Graphic[] graphics = phoneButton.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic g in graphics)
        {
            Color c = g.color;
            c.a = alpha;
            g.color = c;
        }
    }

    public void GoHome()
    {
        homeScreen.SetActive(true);
        messengerScreen.SetActive(false);
        settingsScreen.SetActive(false);
        OnOffBack(false);
        if (detectorScreen != null) detectorScreen.SetActive(false);

        if (DetectorAppUI.Instance != null) DetectorAppUI.Instance.OnScreenClosed();
    }

    public void OpenMessenger()
    {
        homeScreen.SetActive(false);
        messengerScreen.SetActive(true);
        settingsScreen.SetActive(false);
        OnOffBack(true);
        if (detectorScreen != null) detectorScreen.SetActive(false);

        if (DetectorAppUI.Instance != null) DetectorAppUI.Instance.OnScreenClosed();
        if (MessengerAppUI.Instance != null) MessengerAppUI.Instance.OpenChatList();
    }

    public void OpenSettings()
    {
        homeScreen.SetActive(false);
        messengerScreen.SetActive(false);
        settingsScreen.SetActive(true);
        OnOffBack(true);
        if (detectorScreen != null) detectorScreen.SetActive(false);

        if (DetectorAppUI.Instance != null) DetectorAppUI.Instance.OnScreenClosed();
    }

    public void OpenDetector()
    {
        homeScreen.SetActive(false);
        messengerScreen.SetActive(false);
        settingsScreen.SetActive(false);
        if (detectorScreen != null) detectorScreen.SetActive(true);

        if (DetectorAppUI.Instance != null) DetectorAppUI.Instance.OnScreenOpened();
    }

    public void OnBackPressed()
    {
        if (!phonePanel.activeSelf) return;

        if (homeScreen.activeSelf) ClosePhone();
        else GoHome();
    }

    void OnOffBack(bool onOff)
    {
        if (onOff)
        {
            if (blinkTween != null && blinkTween.IsActive()) return;
            backButtonSprite.color = originalColor;
            blinkTween = DOTween.Sequence()
                .Append(backButtonSprite.DOColor(Color.red, 0.5f)) 
                .AppendInterval(0.5f)                    
                .Append(backButtonSprite.DOColor(originalColor, 0.5f)) 
                .AppendInterval(0.5f)    
                .SetLoops(-1);
        }
        else
        {
            blinkTween?.Kill();
            backButtonSprite.color = originalColor;
        }
    }
}
