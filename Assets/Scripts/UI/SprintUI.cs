using UnityEngine;
using UnityEngine.UI;

public class SprintUI : MonoBehaviour
{
    [SerializeField]
    private Image _fillImage;

    [SerializeField]
    private Image _backgroundImage;

    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private float _inactiveAlpha = 0.3f;
    private const float _activeAlpha = 1f;

    private bool _isSprinting;
    private bool _isFull;

    private void OnEnable()
    {
        _playerMovement.OnSprintChanged += UpdateCircleBar;
        _playerMovement.OnSprintActive += UpdateSprintActive;
    }

    private void OnDisable()
    {
        _playerMovement.OnSprintChanged -= UpdateCircleBar;
        _playerMovement.OnSprintActive -= UpdateSprintActive;
    }

    private void UpdateCircleBar(float ratio)
    {
        _fillImage.fillAmount = ratio;

        bool full = ratio >= 1f;
        if (_isFull != full)
        {
            _isFull = full;
            ApplyAlpha();
        }
    }

    private void UpdateSprintActive(bool isActive)
    {
        _isSprinting = isActive;
        ApplyAlpha();
    }

    private void ApplyAlpha()
    {
        float a = (_isSprinting || _isFull) ? _activeAlpha : _inactiveAlpha;

        Color fillColor = _fillImage.color;
        Color backColor = _backgroundImage.color;
        fillColor.a = a;
        backColor.a = a;
        _fillImage.color = fillColor;
        _backgroundImage.color = backColor;
    }
}
