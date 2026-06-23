using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject loginPanel;

    [Header("Profile Button")]
    [SerializeField]
    private Button profileButton;

    [SerializeField]
    private TextMeshProUGUI profileButtonText;

    [Header("Login Form")]
    [SerializeField]
    private TMP_InputField emailInput;

    [SerializeField]
    private TMP_InputField passwordInput;

    [SerializeField]
    private Button loginButton;

    [SerializeField]
    private Button signupButton;

    [SerializeField]
    private Button anonymousButton;

    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private TextMeshProUGUI errorText;

    [Header("References")]
    [SerializeField]
    private ProfileUI profileUI;

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => ProfileManager.Instance.IsInitialized);


        profileButton.onClick.AddListener(OnProfileButtonClicked);
        loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        signupButton.onClick.AddListener(() => OnSignupButtonClicked().Forget());
        anonymousButton.onClick.AddListener(() => OnAnonymousButtonClicked().Forget());
        exitButton.onClick.AddListener(onClickExit);

        UpdateUI().Forget();
    }

    public async UniTaskVoid UpdateUI()
    {
        if (!AuthManager.Instance.IsInitialized)
            return;
        
        bool isLoggedIn = AuthManager.Instance.IsLoggedIn;
        
        if (isLoggedIn)
        {
            var (profile, error) = await ProfileManager.Instance.LoadProfileAsync();
            if (profile != null)
            {
                errorText.color = Color.blue;
                errorText.text = $"{profile.nickname}";
            }
            else
            {
                errorText.color = Color.blue;
                errorText.text = "Guest";
            }
         }
        else
        {
            errorText.text = "로그인이 필요합니다.";
        }
    }

    private void onClickExit()
    {
        loginPanel.SetActive(false);
    }

    private async UniTaskVoid OnLoginButtonClicked()
    {
        string email = emailInput.text.Trim();
        string passwd = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwd))
        {
            ShowError("이메일과 비밀번호를 입력하세요.");
            return;
        }

        SetButtonsInteractable(false);
       
        var (success, error) = await AuthManager.Instance.SignInUserWithEmailAsync(email, passwd);
        if (success)
        {
            UpdateUI().Forget();
        }
        else
        {
            ShowError(error);
        }

        SetButtonsInteractable(true);
    }

    private async UniTaskVoid OnSignupButtonClicked()
    {
        string email = emailInput.text.Trim();
        string passwd = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwd))
        {
            ShowError("이메일과 비밀번호를 입력하세요.");
            return;
        }

        SetButtonsInteractable(false);
       
        var (success, error) = await AuthManager.Instance.CreateUserWithEmailAsync(email, passwd);
        if (success)
        {
            UpdateUI().Forget();
        }
        else
        {
            ShowError(error);
        }

        SetButtonsInteractable(true);
    }

    private async UniTaskVoid OnAnonymousButtonClicked()
    {
        SetButtonsInteractable(false);

        var (success, error) = await AuthManager.Instance.SignInAnonymouslyAsync();

        if (success)
        {
            UpdateUI().Forget();
        }
        else
        {
            ShowError(error);
        }

        SetButtonsInteractable(true);
    }

    private void OnProfileButtonClicked()
    {
        if (AuthManager.Instance.IsLoggedIn)
        {
            profileUI.OpenProfilePanel().Forget();
        }
        else
        {
            loginPanel.SetActive(true);
        }
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.color = Color.red;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        loginButton.interactable = interactable;
        signupButton.interactable = interactable;
        anonymousButton.interactable = interactable;
    }
}
