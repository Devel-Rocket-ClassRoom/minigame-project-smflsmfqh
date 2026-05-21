using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // --- GameClear UI용 ---
    [SerializeField]
    private GameObject _gameClearPanel;

    [SerializeField]
    private TextMeshProUGUI __scoreText;

    [SerializeField]
    private Button _restartButton2;
    [SerializeField]
    private Button _exitButton2;


    // --- Pause UI용 ---
    [SerializeField]
    private GameObject _pausePanel;

    [SerializeField]
    private Button _resumeButton;

    [SerializeField]
    private Button _exitButton3;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _restartButton1.onClick.AddListener(() => GameManager.Instance.Restart());
        _restartButton2.onClick.AddListener(() => GameManager.Instance.Restart());
        _exitButton1.onClick.AddListener(() => GameManager.Instance.Exit());
        _exitButton2.onClick.AddListener(() => GameManager.Instance.Exit());
        _resumeButton.onClick.AddListener(() => GameManager.Instance.TogglePause());
        _exitButton3.onClick.AddListener(() => GameManager.Instance.Exit());

        SetPanel(_gameOverPanel, false);
        SetPanel(_gameClearPanel, false);
        SetPanel(_missionCheckListUI, false);
        SetPanel(_pausePanel, false);

        MissionManager.Instance.OnMissionAssigned += EnableMissionCheckListUI;
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionAssigned -= EnableMissionCheckListUI;
    }

    private void EnableMissionCheckListUI(ItemData _)
    {
        SetPanel(_missionCheckListUI, true);
        MissionManager.Instance.OnMissionAssigned -= EnableMissionCheckListUI;
    }

    private void Start()
    {
        ShowHUD();
    }

    public void ShowGameOver(CauseDeath cause)
    {
        SetPanel(_hudPanel, false);
        SetPanel(_gameOverPanel, true);

        _gameOverDescText.text = StringTableManager.Instance.GetDeathMessage(cause);
    }

    public void ShowGameClear()
    {
        Debug.Log(
            $"[UIManager] ShowGameClear 호출, _gameClearPanel null? {_gameClearPanel == null}"
        );
        SetPanel(_gameClearPanel, true);
        __scoreText.text = $"SCORE: {GameManager.Instance.Score}";
    }

    public void ShowHUD()
    {
        SetPanel(_hudPanel, true);
    }

    public void ShowPause()
    {
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
