using UnityEngine;
using UnityEngine.UI;

public class SmartphoneUIManager : MonoBehaviour
{
    public static SmartphoneUIManager Instance { get; private set; }

    [Header("�� ��ư (�׻� ȭ�鿡 ����, ���� �ϴ�)")]
    public Button phoneButton;

    [Header("����Ʈ�� �г�")]
    public GameObject phonePanel;
    public GameObject homeScreen;
    public GameObject messengerScreen;
    public GameObject settingsScreen;
    public GameObject detectorScreen;

    [Header("Ȩ ȭ�� ��ư")]
    public Button messengerAppIcon;
    public Button settingsAppIcon;
    public Button detectorAppIcon;
    public Button closeButton;
    public Button backButton;

    [Header("�ϴ� ������̼� �� (NavigationBar)")]
    public Button navHomeButton;
    public Button navBackButton;
    public Button navRecentsButton;

    public AudioClip clickSound;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        phonePanel.SetActive(false);

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
        if (detectorScreen != null) detectorScreen.SetActive(false);

        if (DetectorAppUI.Instance != null) DetectorAppUI.Instance.OnScreenClosed();
    }

    public void OpenMessenger()
    {
        homeScreen.SetActive(false);
        messengerScreen.SetActive(true);
        settingsScreen.SetActive(false);
        if (detectorScreen != null) detectorScreen.SetActive(false);

        if (DetectorAppUI.Instance != null) DetectorAppUI.Instance.OnScreenClosed();
        if (MessengerAppUI.Instance != null) MessengerAppUI.Instance.OpenChatList();
    }

    public void OpenSettings()
    {
        homeScreen.SetActive(false);
        messengerScreen.SetActive(false);
        settingsScreen.SetActive(true);
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
        bool inMessengerDetail = messengerScreen.activeSelf
            && MessengerAppUI.Instance != null
            && MessengerAppUI.Instance.chatDetailView.activeSelf;

        if (inMessengerDetail) MessengerAppUI.Instance.OpenChatList();
        else if (messengerScreen.activeSelf || settingsScreen.activeSelf
                 || (detectorScreen != null && detectorScreen.activeSelf)) GoHome();
        else ClosePhone();
    }
}