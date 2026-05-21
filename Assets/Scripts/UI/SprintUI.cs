using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SprintUI : MonoBehaviour
{
    [SerializeField]
    private Image _icon;

    [SerializeField]
    private Image _fillImage;

    [SerializeField]
    private Image _backgroundImage;

    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private float _inactiveAlpha = 0.3f;
    private const float _activeAlpha = 1f;

    private void OnEnable()
    {
        _playerMovement.OnSprintChanged += UpdateCircleBar;
        _playerMovement.OnSprintActive += UpdateAlpha;
    }

    private void OnDisable()
    {
        _playerMovement.OnSprintChanged -= UpdateCircleBar;
        _playerMovement.OnSprintActive -= UpdateAlpha;
    }

    private void UpdateCircleBar(float ratio)
    {
        _fillImage.fillAmount = ratio;
    }

    private void UpdateAlpha(bool isActive)
    {
        Color fillColor = _fillImage.color;
        Color backColor = _backgroundImage.color;
        Color iconColor = _icon.color;

        fillColor.a = isActive ? _activeAlpha : _inactiveAlpha;
        backColor.a = isActive ? _activeAlpha : _inactiveAlpha;
        iconColor.a = isActive ? _activeAlpha : _inactiveAlpha;

        _fillImage.color = fillColor;
        _backgroundImage.color = backColor;
        _icon.color = iconColor;
    }
}
