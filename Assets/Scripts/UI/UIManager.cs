using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField]
    private GameObject _hudPanel;

    [SerializeField]
    private GameObject _missionCheckListUI;

    // --- GameOver UI용 ---

    [SerializeField]
    private GameObject _gameOverPanel;

    [SerializeField]
    private TextMeshProUGUI _gameOverDescText;

    [SerializeField]
    private Button _restartButton1;

    [SerializeField]
    private Button _exitButton1;

    [SerializeField]
    private Button _backButton1;

    // --- GameClear UI용 ---
    [SerializeField]
    private GameObject _gameClearPanel;

    [SerializeField]
    private TextMeshProUGUI __scoreText;

    [SerializeField]
    private TextMeshProUGUI _clearTaglineText;

    [SerializeField]
    private Button _restartButton2;

    [SerializeField]
    private Button _exitButton2;

    [SerializeField]
    private Button _backButton2;

    // --- Pause UI용 ---
    [SerializeField]
    private GameObject _pausePanel;

    [SerializeField]
    private Button _resumeButton;

    [SerializeField]
    private Button _exitButton3;

    [SerializeField]
    private Button _backButton3;

    [SerializeField]
    private TextMeshProUGUI _pauseExitText;

    [SerializeField]
    private ProximityPanelFeedbackUI _proximityPanelFeedbackUI;

    [Header("히든 엔딩 영상")]
    [SerializeField]
    private GameObject _hiddenVideoPanel;

    [SerializeField]
    private VideoPlayer _hiddenVideoPlayer;

    [SerializeField]
    private Button _hiddenSkipButton;

    [Header("SFX")]
    [SerializeField]
    private AudioSource _sfxSource;

    [SerializeField]
    private AudioClip _buttonClickSound;

    [Header("BGM")]
    [SerializeField]
    private AudioSource _bgmSource;

    [SerializeField]
    private AudioClip _gameplayBGM;

    [SerializeField]
    private AudioClip _gameOverBGM;

    [SerializeField]
    private AudioClip _clearBGM;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _restartButton1.onClick.AddListener(() =>
        {
            PlayButtonSound();
            GameManager.Instance.Restart();
        });
        _restartButton2.onClick.AddListener(() =>
        {
            PlayButtonSound();
            GameManager.Instance.Restart();
        });
        _exitButton1.onClick.AddListener(() =>
        {
            PlayButtonSound();
            GameManager.Instance.Exit();
        });
        _exitButton2.onClick.AddListener(() =>
        {
            PlayButtonSound();
            GameManager.Instance.Exit();
        });
        _backButton1?.onClick.AddListener(() =>
        {
            PlayButtonSound();
            GameManager.Instance.GoToTitle();
        });
        _backButton2?.onClick.AddListener(() =>
        {
            PlayButtonSound();
            GameManager.Instance.GoToTitle();
        });
        _resumeButton.onClick.AddListener(() =>
        {
            PlayButtonSound();
            GameManager.Instance.TogglePause();
        });
        _exitButton3.onClick.AddListener(() =>
        {
            PlayButtonSound();
            GameManager.Instance.Exit();
        });
        _backButton3?.onClick.AddListener(() =>
        {
            PlayButtonSound();
            GameManager.Instance.GoToTitle();
        });

        SetPanel(_gameOverPanel, false);
        SetPanel(_gameClearPanel, false);
        SetPanel(_pausePanel, false);
        SetPanel(_hiddenVideoPanel, false);

        if (_hiddenSkipButton != null)
            _hiddenSkipButton.onClick.AddListener(() =>
            {
                PlayButtonSound();
                SkipHiddenVideo();
            });

        MissionManager.Instance.OnMissionDisplayed += EnableMissionCheckListUI;
    }

    private void PlayButtonSound()
    {
        if (_sfxSource != null && _buttonClickSound != null)
            _sfxSource.PlayOneShot(_buttonClickSound);
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionDisplayed -= EnableMissionCheckListUI;
    }

    private void EnableMissionCheckListUI(ItemData _)
    {
        SetPanel(_missionCheckListUI, true);
        MissionManager.Instance.OnMissionDisplayed -= EnableMissionCheckListUI;
    }

    private void Start()
    {
        ShowHUD();
        PlayBGM(_gameplayBGM);
    }

    private void PlayBGM(AudioClip clip)
    {
        if (_bgmSource == null || clip == null)
            return;
        _bgmSource.Stop();
        _bgmSource.clip = clip;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    public void ShowGameOver(CauseDeath cause)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayBGM(_gameOverBGM);
        SetPanel(_hudPanel, false);
        SetPanel(_gameOverPanel, true);
        if (_proximityPanelFeedbackUI != null)
            _proximityPanelFeedbackUI.ForceHide();

        string deathMsg = StringTableManager.Instance.GetDeathMessage(cause);
        Debug.Log(
            $"[GameOver] Language={StringTableManager.Instance.CurrentLanguage}, cause={cause}, msg={deathMsg}"
        );
        _gameOverDescText.text = deathMsg;
    }

    public void ShowGameClear(int angerSeconds)
    {
        Time.timeScale = 0f;
        if (_proximityPanelFeedbackUI != null)
            _proximityPanelFeedbackUI.ForceHide();

        if (
            MissionManager.Instance.IsOptionalCompleted
            && _hiddenVideoPanel != null
            && _hiddenVideoPlayer != null
            && _hiddenVideoPlayer.clip != null
        )
        {
            StartCoroutine(PlayHiddenVideoThenClear(angerSeconds));
        }
        else
        {
            ShowClearPanel(angerSeconds);
        }
    }

    private IEnumerator PlayHiddenVideoThenClear(int angerSeconds)
    {
        SetPanel(_hudPanel, false);
        SetPanel(_hiddenVideoPanel, true);
        PlayBGM(_clearBGM);

        _hiddenVideoPlayer.Prepare();

        // Prepare 타임아웃 5초 — isPrepared가 되거나 타임아웃이면 진행
        float elapsed = 0f;
        while (!_hiddenVideoPlayer.isPrepared && elapsed < 5f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (_hiddenVideoPlayer.isPrepared)
        {
            float duration = (float)_hiddenVideoPlayer.clip.length;
            _hiddenVideoPlayer.Play();
            yield return new WaitForSecondsRealtime(duration);
        }

        _hiddenVideoPlayer.Stop();
        SetPanel(_hiddenVideoPanel, false);
        ShowClearPanel(angerSeconds);
    }

    private void SkipHiddenVideo()
    {
        StopAllCoroutines();
        _hiddenVideoPlayer.Stop();
        SetPanel(_hiddenVideoPanel, false);

        // angerSeconds를 보존하지 못하므로 GameManager에서 직접 가져옴
        ShowClearPanel(Mathf.RoundToInt(GameManager.Instance.PlayTime));
    }

    private void ShowClearPanel(int angerSeconds)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (_bgmSource == null || _bgmSource.clip != _clearBGM)
            PlayBGM(_clearBGM);
        Debug.Log(
            $"[UIManager] ShowGameClear 호출, _gameClearPanel null? {_gameClearPanel == null}"
        );
        SetPanel(_gameClearPanel, true);
        int min = angerSeconds / 60;
        int sec = angerSeconds % 60;
        string key = min > 0 ? "GAMECLEAR_SCORE_MINSEC" : "GAMECLEAR_SCORE_SEC";
        string fmt = StringTableManager.Instance.GetMessage(key).message;
        __scoreText.text = min > 0 ? string.Format(fmt, min, sec) : string.Format(fmt, sec);

        if (_clearTaglineText != null)
            _clearTaglineText.text = StringTableManager
                .Instance.GetMessage("GAMECLEAR_TAGLINE")
                .message;
    }

    public void ShowHUD()
    {
        SetPanel(_hudPanel, true);
    }

    public void ShowPause()
    {
        if (_pauseExitText != null)
            _pauseExitText.text = StringTableManager.Instance.GetMessage("PAUSE_EXIT").message;
        SetPanel(_pausePanel, true);
    }

    public void HidePause()
    {
        SetPanel(_pausePanel, false);
    }

    public void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
