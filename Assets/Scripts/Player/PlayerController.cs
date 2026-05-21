using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static event Action OnMissionTogglePressed;
    private PlayerHealth _ph;
    private PlayerMovement _pm;
    private PlayerInput _pi;

    private void Awake()
    {
        _ph = GetComponent<PlayerHealth>();
        _pm = GetComponent<PlayerMovement>();
        _pi = GetComponent<PlayerInput>();
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

    public void SetInvincible(float sec)
    {
        _ph.SetInvincible(sec);
    }

    public void SetSpeedBoost(float speed, float sec)
    {
        _pm.SetSpeedBoost(speed, sec);
    }

    private void HandleDied(CauseDeath _)
    {
        _pi.DeactivateInput();
    }
}
