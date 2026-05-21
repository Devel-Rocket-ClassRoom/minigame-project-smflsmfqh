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

    // --- GameOver UI용 ---

    [SerializeField]
    private GameObject _gameOverPanel;

    [SerializeField]
    private TextMeshProUGUI _gameOverDescText;

    [SerializeField]
    private Button _restartButton1;

    // --- GameClear UI용 ---
    [SerializeField]
    private GameObject _gameClearPanel;

    [SerializeField]
    private TextMeshProUGUI __scoreText;

    [SerializeField]
    private Button _restartButton2;

    // --- 추후 여러 판넬 추가할 예정
    // 1. 아내 개미에게 오는 카톡 알림 ui

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
        SetPanel(_gameOverPanel, false);
        SetPanel(_gameClearPanel, false);
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
        SetPanel(_gameClearPanel, true);
    }

    public void ShowHUD()
    {
        SetPanel(_hudPanel, true);
    }

    public void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
