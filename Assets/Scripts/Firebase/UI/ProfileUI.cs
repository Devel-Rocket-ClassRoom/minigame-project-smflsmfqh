using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject profilePanel;

    [Header("Profile Info")]
    [SerializeField]
    private TextMeshProUGUI nicknameText;

    [SerializeField]
    private TextMeshProUGUI userIdText;

    [Header("Buttons")]
    [SerializeField]
    private Button editProfileButton;

    [SerializeField]
    private Button logoutButton;

    [SerializeField]
    private Button closeProfileButton;

    [Header("References")]
    [SerializeField]
    private LoginUI loginUI;

    [SerializeField]
    private ProfileEditUI profileEditUI;

    private void Start()
    {
        editProfileButton.onClick.AddListener(OnEditProfileButtonClicked);
        logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        closeProfileButton.onClick.AddListener(OnCloseProfileButtonClicked);

        profilePanel.SetActive(false);
    }

    public async UniTaskVoid OpenProfilePanel()
    {
        await UpdateProfileUIAsync();

        profilePanel.SetActive(true);

        // if (GameManager.Instance != null)
        // {
        //     GameManager.Instance.PauseGame();
        // }
    }

    public async UniTask UpdateProfileUIAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn)
            return;
        
        string userId = AuthManager.Instance.UserId;
        var (profile, _) = await ProfileManager.Instance.LoadProfileAsync();
        if (profile != null)
        {
            nicknameText.text = $"nickname: {profile.nickname}";
        }
        else
        {
            nicknameText.text = "nickname: (Empty)";
        }
        Debug.Log($"[ProfileUI] 로그인 완료 닉네임: {nicknameText.text}");
        userIdText.text = $"User ID: {userId}";
    }

    private void OnEditProfileButtonClicked()
    {
        if (profileEditUI != null)
        {
            profileEditUI.OpenProfileEditPanelAsync().Forget();
        }
    }

    private void OnCloseProfileButtonClicked()
    {
        profilePanel.SetActive(false);

        // if (GameManager.Instance != null)
        // {
        //     GameManager.Instance.ResumeGame();
        // }
    }

    private void OnLogoutButtonClicked()
    {
        AuthManager.Instance.SignOut();
        profilePanel.SetActive(false);
        loginUI.UpdateUI().Forget();
    }
}
