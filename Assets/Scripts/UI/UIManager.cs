using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField]
    private GameObject _hudPanel;

    // --- 추후 여러 판넬 추가할 예정
    // 1. 아내 개미에게 오는 카톡 알림 ui
    // 2. 게임 오버 판넬

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
    }

    public void ShowGameOver()
    {
        SetPanel(_hudPanel, false);
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
