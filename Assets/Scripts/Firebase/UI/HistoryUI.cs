using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private GameObject historyPanel;
    [SerializeField]
    private TextMeshProUGUI bestScoreText;

    [SerializeField]
    private Transform historyListParent;

    [SerializeField]
    private GameObject historyItemPrefab;

    [SerializeField]
    private Button refreshButton;

    [SerializeField]
    private Button closeButton;

    private void Start()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => historyPanel.SetActive(false));
        }
    }

    private void OnEnable()
    {
        LoadAndDisplayHistoryAsync().Forget();
    }

    private void OnRefreshButtonClicked()
    {
        LoadAndDisplayHistoryAsync().Forget();
    }

    private async UniTask LoadAndDisplayHistoryAsync()
    {
        float bestScore = await ScoreManager.Instance.LoadBestScoreAsync();
        bestScoreText.text = bestScore > 0
            ? $"최고 기록: {Mathf.FloorToInt(bestScore / 60):D2}:{Mathf.FloorToInt(bestScore % 60):D2}"
            : "최고 기록: -";

        List<ScoreData> history = await ScoreManager.Instance.LoadHistoryAsync();

        foreach (Transform child in historyListParent)
        {
            Destroy(child.gameObject);
        }

        if (history.Count == 0)
        {
            GameObject emptyItem = Instantiate(historyItemPrefab, historyListParent);
            TextMeshProUGUI text = emptyItem.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "게임 기록이 없습니다.";
        }
        else
        {
            foreach(ScoreData data in history)
            {
                GameObject item = Instantiate(historyItemPrefab, historyListParent);
                TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                int totalSeconds = Mathf.FloorToInt(data.playtime);
                text.text = $"{totalSeconds / 60:D2}:{totalSeconds % 60:D2} - {data.GetDateString()}";
            }
        }

        Debug.Log($"[HistoryUI] 히스토리 표시 완료: {history.Count}");
    }
}
