using System;
using UnityEngine;

public class AngerSystem : MonoBehaviour
{
    [SerializeField]
    private float _angerPerSecond = 1.5f;

    [SerializeField]
    private float _missionReduction = 20f;

    [SerializeField]
    private float _maxAnger = 200f;

    // 게임 시작 시 순서대로 발화되는 인트로 메시지 키
    private readonly string[] _introKeys = { "ANGER_0", "ANGER_1", "ANGER_2" };

    // 분노 게이지 % 임계값 기반 메시지 (0%는 인트로로 분리)
    private float[] _thresholds = { 10f, 25f, 50f, 75f, 90f };

    private float _currentAnger;
    private bool[] _triggered;
    private bool _gameOverTriggered;
    private CauseDeath cause = CauseDeath.Anger;

    public float Anger => _currentAnger;
    public event Action<float> OnAngerChanged;
    public event Action<(string, string)> OnMessasgeTriggered;

    private void Start()
    {
        _triggered = new bool[_thresholds.Length];
        MissionManager.Instance.OnMissionCompleted += HandleMissionCompleted;

        foreach (var key in _introKeys)
        {
            var (msg, sender) = StringTableManager.Instance.GetMessage(key);
            if (!string.IsNullOrEmpty(msg))
                OnMessasgeTriggered?.Invoke((msg, sender));
        }
    }

    private void OnDisable()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionCompleted -= HandleMissionCompleted;
        }
    }

    private void Update()
    {
        if (_gameOverTriggered)
            return;

        _currentAnger = Mathf.Min(_currentAnger + _angerPerSecond * Time.deltaTime, _maxAnger);
        float percent = _currentAnger / _maxAnger * 100f;

        for (int i = 0; i < _thresholds.Length; i++)
        {
            if (!_triggered[i] && percent >= _thresholds[i])
            {
                _triggered[i] = true;
                (string msg, string sender) = StringTableManager.Instance.GetAngerMessage(_thresholds[i]);
                OnMessasgeTriggered?.Invoke((msg, sender));
            }
        }
        
        OnAngerChanged?.Invoke(_currentAnger / _maxAnger);

        if (_currentAnger >= _maxAnger)
        {
            _gameOverTriggered = true;
            GameManager.Instance.GameOver(cause);
        }
    }

    private void HandleMissionCompleted(ItemData _)
    {
        _currentAnger = Mathf.Max(0f, _currentAnger - _missionReduction);
    }
}
