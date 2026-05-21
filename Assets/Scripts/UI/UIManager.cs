using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField]
    private GameObject _hudPanel;

    [SerializeField]
    private GameObject _gameOverPanel;

    [SerializeField]
    private TextMeshProUGUI _gameOverDescText;

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
    }

    private void Start()
    {
        ShowHUD();
        SetPanel(_gameOverPanel, false);
    }

    public void ShowGameOver(CauseDeath cause)
    {
        SetPanel(_hudPanel, false);
        SetPanel(_gameOverPanel, true);

        _gameOverDescText.text = StringTableManager.Instance.GetDeathMessage(cause);
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
