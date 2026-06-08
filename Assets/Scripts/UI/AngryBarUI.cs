using UnityEngine;
using UnityEngine.UI;

public class AngryBarUI : MonoBehaviour
{
    [SerializeField]
    private Sprite _queenFace;

    [SerializeField]
    private Sprite _angryQueenFace;

    [SerializeField]
    private Sprite _veryAngryQueenFace;

    [SerializeField]
    private Image _faceImage;

    [SerializeField]
    private Image _fillImage;

    [SerializeField]
    private AngerSystem _angerSystem;

    [SerializeField]
    private Gradient _colorGradient;

    private void Awake()
    {
        _faceImage.sprite = _queenFace;
    }

    private void OnEnable()
    {
        if (_angerSystem != null)
            _angerSystem.OnAngerChanged += UpdateBar;
    }

    private void OnDisable()
    {
        if (_angerSystem != null)
            _angerSystem.OnAngerChanged -= UpdateBar;
    }

    private void UpdateBar(float normalized)
    {
        if (normalized >= 0.7)
        {
            _faceImage.sprite = _veryAngryQueenFace;
        }
        else if (normalized >= 0.5)
        {
            _faceImage.sprite = _angryQueenFace;
        }
        else
        {
            _faceImage.sprite = _queenFace;
        }

        _fillImage.fillAmount = normalized;
        _fillImage.color = _colorGradient.Evaluate(normalized);
    }
}
