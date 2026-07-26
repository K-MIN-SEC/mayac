using UnityEngine;
using UnityEngine.UI;

// 씬에 하나만 존재. 우측 하단 폰 버튼 -> 스마트폰 패널 열기/닫기 및 화면(홈/메신저/설정) 전환 관리
public class SmartphoneUIManager : MonoBehaviour
{
    public static SmartphoneUIManager Instance { get; private set; }

    [Header("폰 버튼 (항상 화면에 보임, 우측 하단)")]
    public Button phoneButton;

    [Header("스마트폰 패널")]
    public GameObject phonePanel;      // 패널 전체 (평소엔 꺼져있음)
    public GameObject homeScreen;      // 앱 아이콘 + 위젯이 있는 홈 화면
    public GameObject messengerScreen; // 메신저 앱 화면 (목록+상세를 담는 부모)
    public GameObject settingsScreen;  // 설정 앱 화면

    [Header("홈 화면 버튼")]
    public Button messengerAppIcon;
    public Button settingsAppIcon;
    public Button closeButton;         // 패널 완전히 닫기
    public Button backButton;          // 앱 화면 -> 뒤로가기 (메신저 상세면 목록으로, 아니면 홈으로)

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        phonePanel.SetActive(false);

        if (phoneButton != null) phoneButton.onClick.AddListener(TogglePhone);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePhone);
        if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
        if (messengerAppIcon != null) messengerAppIcon.onClick.AddListener(OpenMessenger);
        if (settingsAppIcon != null) settingsAppIcon.onClick.AddListener(OpenSettings);
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
    }

    public void ClosePhone()
    {
        phonePanel.SetActive(false);
    }

    public void GoHome()
    {
        homeScreen.SetActive(true);
        messengerScreen.SetActive(false);
        settingsScreen.SetActive(false);
        if (backButton != null) backButton.gameObject.SetActive(false);
    }

    public void OpenMessenger()
    {
        homeScreen.SetActive(false);
        messengerScreen.SetActive(true);
        settingsScreen.SetActive(false);
        if (backButton != null) backButton.gameObject.SetActive(true);

        if (MessengerAppUI.Instance != null)
            MessengerAppUI.Instance.OpenChatList();
    }

    public void OpenSettings()
    {
        homeScreen.SetActive(false);
        messengerScreen.SetActive(false);
        settingsScreen.SetActive(true);
        if (backButton != null) backButton.gameObject.SetActive(true);
    }

    void OnBackPressed()
    {
        bool inMessengerDetail = messengerScreen.activeSelf
            && MessengerAppUI.Instance != null
            && MessengerAppUI.Instance.chatDetailView.activeSelf;

        if (inMessengerDetail)
        {
            MessengerAppUI.Instance.OpenChatList();
        }
        else
        {
            GoHome();
        }
    }
}