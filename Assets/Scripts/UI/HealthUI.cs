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
        if (_heart == null || _fillImage == null || _playerHealth == null)
            Debug.Log("[HealthUI] 인스펙터 연결이 필요합니다.");

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
