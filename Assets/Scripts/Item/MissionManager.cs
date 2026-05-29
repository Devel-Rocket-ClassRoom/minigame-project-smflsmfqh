using System;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Serializable]
    private struct HintEntry
    {
        public float unlockTime;
        public string csvKey;
    }

    [SerializeField] private ItemData[] _missionPool;
    [SerializeField] private float[] _unlockTimes = { 2f, 10f, 10f, 20f, 30f };
    [SerializeField] private HintEntry[] _hints;
    [SerializeField] private ItemData _optionalMission;

    public event Action<ItemData> OnMissionAssigned;
    public event Action<ItemData> OnMissionCompleted;
    public event Action<string, string> OnHintAssigned;
    public event Action OnOptionalMissionUnlocked;

    private float _elapsedTime;
    private bool[] _assigned;
    private bool[] _completed;
    private bool[] _hintTriggered;
    private int _completedCount;
    private bool _optionalUnlocked;
    private bool _optionalCompleted;

    public bool OnAllMissionCompleted => _completedCount >= _missionPool.Length;
    public bool IsOptionalUnlocked => _optionalUnlocked;
    public ItemData OptionalMission => _optionalMission;

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
        _hintTriggered = new bool[_hints?.Length ?? 0];
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        for (int i = 0; i < _missionPool.Length; i++)
        {
            if (!_assigned[i] && _elapsedTime >= _unlockTimes[i])
            {
                _assigned[i] = true;
                OnMissionAssigned?.Invoke(_missionPool[i]);
            }
        }

        if (_hints == null) return;

        for (int i = 0; i < _hints.Length; i++)
        {
            if (!_hintTriggered[i] && _elapsedTime >= _hints[i].unlockTime)
            {
                _hintTriggered[i] = true;
                var (msg, sender) = StringTableManager.Instance.GetMessage(_hints[i].csvKey);
                OnHintAssigned?.Invoke(msg, sender);
            }
        }
    }

    public void ReportCollected(string itemName)
    {
        for (int i = 0; i < _missionPool.Length; i++)
        {
            if (_assigned[i] && !_completed[i] && _missionPool[i].itemName == itemName)
            {
                _completed[i] = true;
                _completedCount++;
                OnMissionCompleted?.Invoke(_missionPool[i]);

                if (_completedCount >= _missionPool.Length && !_optionalUnlocked)
                {
                    _optionalUnlocked = true;
                    OnOptionalMissionUnlocked?.Invoke();
                }
                return;
            }
        }

        if (_optionalMission != null && _optionalUnlocked && !_optionalCompleted
            && _optionalMission.itemName == itemName)
        {
            _optionalCompleted = true;
            OnMissionCompleted?.Invoke(_optionalMission);
        }
    }
}
