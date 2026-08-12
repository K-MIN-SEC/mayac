using UnityEngine;
using UnityEngine.UI;

// 씬에 하나만 존재. 우측 하단 폰 버튼 -> 스마트폰 패널 열기/닫기 및 화면(홈/메신저/설정/탐색기) 전환 관리
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
    public GameObject detectorScreen;  // 탐색기 앱 화면

    [Header("홈 화면 버튼")]
    public Button messengerAppIcon;
    public Button settingsAppIcon;
    public Button detectorAppIcon;
    public Button closeButton;         // 패널 완전히 닫기
    public Button backButton;          // 앱 화면 -> 뒤로가기 (메신저 상세면 목록으로, 아니면 홈으로)

    [Header("하단 내비게이션 바 (NavigationBar)")]
    public Button navHomeButton;       // 누르면 홈 화면으로
    public Button navBackButton;       // 누르면 뒤로가기 (backButton과 동일 동작)
    public Button navRecentsButton;    // 지금은 장식용, 필요하면 나중에 기능 추가

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
        if (detectorAppIcon != null) detectorAppIcon.onClick.AddListener(OpenDetector);

        // 하단 내비게이션 바 연결: 홈 버튼은 홈 화면으로, 뒤로가기 버튼은 backButton과 동일하게 동작
        if (navHomeButton != null) navHomeButton.onClick.AddListener(GoHome);
        if (navBackButton != null) navBackButton.onClick.AddListener(OnBackPressed);
        if (navRecentsButton != null) navRecentsButton.onClick.AddListener(ClosePhone);
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

    // phone 버튼(과 그 자식 이미지들)의 투명도를 한번에 조절
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

        if (MessengerAppUI.Instance != null)
            MessengerAppUI.Instance.OpenChatList();
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

    // 뒤로가기 버튼 하나로 "상세 -> 목록 -> 홈 -> 패널 닫기" 순서로 자연스럽게 빠지도록 처리
    // public으로 되어있어서 NavBackButton의 On Click()에도 직접 연결 가능
    public void OnBackPressed()
    {
        bool inMessengerDetail = messengerScreen.activeSelf
            && MessengerAppUI.Instance != null
            && MessengerAppUI.Instance.chatDetailView.activeSelf;

        if (inMessengerDetail)
        {
            // 메신저 대화 상세 화면 -> 대화 목록으로
            MessengerAppUI.Instance.OpenChatList();
        }
        else if (messengerScreen.activeSelf || settingsScreen.activeSelf
                 || (detectorScreen != null && detectorScreen.activeSelf))
        {
            // 메신저/설정/탐색기 화면 -> 홈 화면으로 (패널은 그대로 유지)
            GoHome();
        }
        else
        {
            // 이미 홈 화면이면 (다른 화면이 아무것도 안 켜져있으면) -> 패널 자체를 닫기
            ClosePhone();
        }
    }
}