using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField]
    private Image _heart;

    [SerializeField]
    private Image _fillImage;

    [SerializeField]
    private PlayerHealth _playerHealth;

    private void Start()
    {
        _heart.color = Color.red;
    }

    private void OnEnable()
    {
        _playerHealth.OnHealthChanged += UpdateBar;
    }

    private void OnDisable()
    {
        _playerHealth.OnHealthChanged -= UpdateBar;
    }

    private void Update()
    {
        if (_playerHealth.isDead)
        {
            _heart.color = Color.black;
        }
    }

    private void UpdateBar(float ratio)
    {
        _fillImage.fillAmount = ratio;
    }
}
