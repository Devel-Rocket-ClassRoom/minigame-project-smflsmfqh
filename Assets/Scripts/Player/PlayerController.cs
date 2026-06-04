using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("오디오")]
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private AudioClip _pickupSound;

    [SerializeField]
    private AudioClip _jumpSound;

    [SerializeField]
    private AudioClip _rollSound;

    [SerializeField]
    private ParticleSystem healParticle;

    [SerializeField]
    private ParticleSystem speedBoostParticle;

    [SerializeField]
    private ParticleSystem invincibleParticle;
    public event Action OnMissionTogglePressed;
    private PlayerHealth _ph;
    private PlayerMovement _pm;
    private PlayerInput _pi;

    private Dictionary<EffectParticleType, Coroutine> _particleCorutine =
        new Dictionary<EffectParticleType, Coroutine>();

    private void Awake()
    {
        _ph = GetComponent<PlayerHealth>();
        _pm = GetComponent<PlayerMovement>();
        _pi = GetComponent<PlayerInput>();

        if (healParticle != null)
            healParticle.gameObject.SetActive(false);
        if (speedBoostParticle != null)
            speedBoostParticle.gameObject.SetActive(false);
        if (invincibleParticle != null)
            invincibleParticle.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _ph.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        _ph.OnDied -= HandleDied;
    }

    private void OnInteract(InputValue value)
    {
        if (!value.isPressed)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, 1f);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractive>(out var item))
            {
                Debug.Log($"[아이템 획득] 아이템: {item.GetItemName()}");
                item.Interact(this);
                break;
            }
        }
    }

    private void OnMissionToggle(InputValue value)
    {
        if (value.isPressed)
            OnMissionTogglePressed?.Invoke();
    }

    private void OnPause(InputValue value)
    {
        if (value.isPressed)
            GameManager.Instance.TogglePause();
    }

    public void Heal(int amount)
    {
        _ph.Heal(amount);
    }

    public void SetInvincible(float sec, string source = "unknown")
    {
        _ph.SetInvincible(sec, source);
    }

    public void SetSpeedBoost(float speed, float sec)
    {
        _pm.SetSpeedBoost(speed, sec);
    }

    private void HandleDied(CauseDeath _)
    {
        _pi.DeactivateInput();
    }

    public void PlayPickupSound()
    {
        if (_audioSource != null && _pickupSound != null)
            _audioSource.PlayOneShot(_pickupSound);
    }

    public void PlayJumpSound()
    {
        if (_audioSource != null && _jumpSound != null)
            _audioSource.PlayOneShot(_jumpSound);
    }

    public void PlayRollSound()
    {
        if (_audioSource != null && _rollSound != null)
            _audioSource.PlayOneShot(_rollSound);
    }

    public void PlayEffect(EffectParticleType type, float duration = 0f)
    {
        ParticleSystem particle = null;
        switch (type)
        {
            case EffectParticleType.HealParticle:
                particle = healParticle;
                break;
            case EffectParticleType.SpeedBoostParticle:
                particle = speedBoostParticle;
                break;
            case EffectParticleType.InvincibleParticle:
                particle = invincibleParticle;
                break;
        }

        if (particle == null)
        {
            Debug.LogWarning(
                $"[PlayerController] PlayEffect: ParticleSystem이 null입니다. 타입: {type}"
            );
            return;
        }

        if (_particleCorutine.TryGetValue(type, out var prev) && prev != null)
        {
            StopCoroutine(prev);
        }

        particle.gameObject.SetActive(true);
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particle.Play();

        _particleCorutine[type] = StartCoroutine(DeactivateParticleAfter(particle, type, duration));
    }

    private IEnumerator DeactivateParticleAfter(
        ParticleSystem particle,
        EffectParticleType type,
        float duration
    )
    {
        if (duration > 0f)
            yield return new WaitForSeconds(duration);
        else
        {
            yield return null;
            yield return new WaitWhile(() => particle.isPlaying);
        }

        particle.Stop();
        particle.gameObject.SetActive(false);
        _particleCorutine[type] = null;
    }
}
