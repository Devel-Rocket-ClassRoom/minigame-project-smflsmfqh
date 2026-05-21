using System;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [SerializeField]
    private ItemData[] _missionPool;

    [SerializeField]
    private float[] _unlockTimes = { 2f, 10f, 20f };

    public event Action<ItemData> OnMissionAssigned;
    public event Action<ItemData> OnMissionCompleted;

    private float _elapsedTime;
    private bool[] _assigned;
    private bool[] _completed;
    private int _completedCount;

    public bool OnAllMissionCompleted => _completedCount >= _missionPool.Length;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _assigned = new bool[_missionPool.Length];
        _completed = new bool[_missionPool.Length];
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        for (int i = 0; i < _missionPool.Length; i++)
        {
            if (!_assigned[i] && _elapsedTime >= _unlockTimes[i])
            {
                _assigned[i] = true;
                Debug.Log($"[미션] {i}번 미션 발동 — 경과 시간: {_elapsedTime:F2}s");
                OnMissionAssigned?.Invoke(_missionPool[i]);
            }
        }
    }

    public void ReportCollected(string itemName)
    {
        for (int i = 0; i < _missionPool.Length; i++)
        {
            Debug.Log("[ReportCollected] 실행!");
            if (_assigned[i] && !_completed[i] && _missionPool[i].itemName == itemName)
            {
                _completed[i] = true;
                _completedCount++;
                OnMissionCompleted?.Invoke(_missionPool[i]);
                Debug.Log("[ReportCollected] 미션 완료!");

                return;
            }
        }
    }
}
