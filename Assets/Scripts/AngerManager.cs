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

    private float[] _thresholds = { 0f, 25f, 50f, 75f, 90f };

    private float _currentAnger;
    private bool[] _triggered;
    private bool _gameOverTriggered;
    private CauseDeath cause = CauseDeath.Anger;

    public float Anger => _currentAnger;
    public event Action<float> OnAngerChanged;
    public event Action<string> OnMessasgeTriggered;

    private void Start()
    {
        _triggered = new bool[_thresholds.Length];
        MissionManager.Instance.OnMissionCompleted += HandleMissionCompleted;
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
                string msg = StringTableManager.Instance.GetAngerMessage(_thresholds[i]);
                OnMessasgeTriggered?.Invoke(msg);
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
