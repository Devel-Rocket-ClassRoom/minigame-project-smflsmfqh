using UnityEngine;

public class ProximityPanelFeedbackUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _panel;

    [SerializeField]
    private float _blinkInterval = 0.07f;

    private bool _isInDanger;
    private float _blinkTimer;

    private void Awake()
    {
        ProximityFeedback.OnIntensityChanged += HandleIntensity;
        if (_panel != null)
            _panel.SetActive(false);
    }

    private void OnDestroy()
    {
        ProximityFeedback.OnIntensityChanged -= HandleIntensity;
    }

    private void Update()
    {
        if (!_isInDanger)
            return;

        _blinkTimer += Time.deltaTime;
        if (_blinkTimer >= _blinkInterval)
        {
            _blinkTimer = 0f;
            _panel.SetActive(!_panel.activeSelf);
        }
    }

    private void HandleIntensity(float intensity)
    {
        _isInDanger = intensity > 0f;
        if (!_isInDanger)
        {
            _blinkTimer = 0f;
            _panel.SetActive(false);
        }
    }
}
