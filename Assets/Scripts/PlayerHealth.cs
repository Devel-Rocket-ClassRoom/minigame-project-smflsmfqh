using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private const float k_maxHealth = 100f;
    private float _currentHealth;

    [SerializeField]
    private float _healthInterval = 2f;
    private float _healthTimer;

    [SerializeField]
    private float _addedHealth = 1f;
    private float _damage = 25f;
    public bool isDead = false;

    private void Awake()
    {
        Reset();
    }

    private void Reset()
    {
        isDead = false;
        _healthTimer = 0f;
        _currentHealth = k_maxHealth;
    }

    private void Update()
    {
        _healthTimer += Time.deltaTime;

        if (_healthTimer >= _healthInterval)
        {
            _healthTimer = 0f;
            _currentHealth += _addedHealth;
            if (_currentHealth >= k_maxHealth)
            {
                _currentHealth = k_maxHealth;
            }

            Debug.Log($"[Player Health] 체력 증가: {_addedHealth}, 현재 체력: {_currentHealth}");
        }
    }

    public void TakeDamage()
    {
        _currentHealth -= _damage;
        Debug.Log($"[Player Health] Damage: {_damage}, 현재 체력: {_currentHealth}");
        if (_currentHealth <= 0)
        {
            _currentHealth = 0f;
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Debug.Log($"[Player Health] 게임 오버! 플레이어 죽음: isDead {isDead}");
    }
}
