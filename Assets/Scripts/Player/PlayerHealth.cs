using System;
using System.Collections;
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
    private const float k_damageInvincibleTime = 0.7f;
    public bool isDead = false;
    private bool _isInvincible = false;

    private Coroutine _invincibleCo;

    // --- 이벤트 관련 필드 ---
    public event Action<float> OnHealthChanged;
    public event Action OnDied;

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

    private void Start()
    {
        OnHealthChanged?.Invoke(_currentHealth / k_maxHealth);
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
            OnHealthChanged?.Invoke(_currentHealth / k_maxHealth);

            Debug.Log($"[Player Health] 체력 증가: {_addedHealth}, 현재 체력: {_currentHealth}");
        }
    }

    public void TakeDamage()
    {
        if (_isInvincible)
            return;

        _currentHealth -= _damage;
        SetInvincible(k_damageInvincibleTime);

        if (_currentHealth <= 0)
        {
            _currentHealth = 0f;
        }
        OnHealthChanged?.Invoke(_currentHealth / k_maxHealth);

        Debug.Log($"[Player Health] 데미지 입음: {_damage}, 현재 체력: {_currentHealth}");

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        var prevH = _currentHealth;
        _currentHealth += amount;
        if (_currentHealth >= k_maxHealth)
        {
            _currentHealth = k_maxHealth;
        }
        OnHealthChanged?.Invoke(_currentHealth / k_maxHealth);

        Debug.Log($"[아이템 획득] 치료 효과: {prevH} -> {_currentHealth}");
    }

    public void SetInvincible(float sec)
    {
        if (_invincibleCo != null)
            StopCoroutine(_invincibleCo);

        _invincibleCo = StartCoroutine(InvincibleCoroutine(sec));
    }

    private IEnumerator InvincibleCoroutine(float sec)
    {
        _isInvincible = true;
        Debug.Log($"[아이템 획득] 무적 효과 - 지속 시간 {sec}");

        yield return new WaitForSeconds(sec);

        _isInvincible = false;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Debug.Log($"[Player Health] 게임 오버! 플레이어 죽음: isDead {isDead}");
        OnDied?.Invoke();
    }
}
