using UnityEngine;
using UnityEngine.UI;

public class AngryBarUI : MonoBehaviour
{
    [SerializeField]
    private Image _fillImage;

    [SerializeField]
    private AngerSystem _angerSystem;

    [SerializeField]
    private Gradient _colorGradient;

    private void OnEnable()
    {
        _angerSystem.OnAngerChanged += UpdateBar;
    }

    private void OnDisable()
    {
        _angerSystem.OnAngerChanged -= UpdateBar;
    }

    private void UpdateBar(float normalized)
    {
        _fillImage.fillAmount = normalized;
        _fillImage.color = _colorGradient.Evaluate(normalized);
    }
}
