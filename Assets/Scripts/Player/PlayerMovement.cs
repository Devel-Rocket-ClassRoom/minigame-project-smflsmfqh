using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;  

public class PlayerMovement : MonoBehaviour
{
    public enum MoveMode
    {
        Move,
        Sprint,
        Roll,
        Crouch,
        None,
    }
    [Header("PlayerHealth")]
    [SerializeField]
    private PlayerHealth _playerHealth;

    [Header("Animation")]
    [SerializeField]
    private Animator _animator;

    private string _currentFace = string.Empty;

    [Header("Movement Speed")]
    [SerializeField]
    private float _moveSpeed = 0.8f;

    [Header("Jump Field")]
    [SerializeField]
    private float _jumpForce = 2.5f;

    [SerializeField]
    private LayerMask _groundLayerMask;

    [Header("Player Rotation")]
    [SerializeField]
    private float _mouseSensitivity = 0.15f;

    [SerializeField]
    private float _minPitch = -20f;

    [SerializeField]
    private float _maxPitch = 10f;

    [Header("Roll Field")]
    private bool _isRolling = false;
    private float _lastRollTime = -999f;
    private Vector3 _rollDirection;
    private Coroutine _rollCoroutine;

    [SerializeField]
    private float _rollSpeed = 2f;

    [SerializeField]
    private float _rollDuration = 0.7f;

    [SerializeField]
    private float _rollCoolDown = 5f;

    private float _standHeight;

    [Header("Crouch Field")]
    private float _crouchHeight;

    [SerializeField]
    private float _crouchSpeed = 0.5f;

    [SerializeField]
    private float _crouchTotalTime = 3.5f;
    private float _crouchDuration;
    private bool _isCrouching = false;

    private Vector3 _prevColliderCenter;

    [Header("Sprint Field")]
    [SerializeField]
    private float _sprintSpeed = 1.7f;

    [SerializeField]
    private float _sprintTotalTime = 7f;
    private float _sprintDuration;
    private bool _isSprint = false;

    private Rigidbody _rb;
    private CapsuleCollider _collider;
    private Vector2 _inputDir;
    private float _currentSpeed;
    private float _boostSpeed;
    private MoveMode _mode = MoveMode.Move;
    private bool _isSpeedBoost = false;
    private Coroutine _speedCo;

    // --- 이벤트 관련 필드 ---
    public event Action<float> OnSprintChanged;
    public event Action<bool> OnSprintActive;

    // --- 외부 호출 가능 속성 필드 ---
    public float Yaw { get; private set; }
    public float Pitch { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _animator = GetComponent<Animator>();

        _standHeight = _collider.height;
        _crouchHeight = _standHeight * 0.5f;
        _prevColliderCenter = _collider.center;
        _sprintDuration = _sprintTotalTime;
        Yaw = transform.eulerAngles.y;
    }

    private void Start()
    {
        OnSprintChanged?.Invoke(_sprintDuration / _sprintTotalTime);
        OnSprintActive?.Invoke(_isSprint);
    }

    private void FixedUpdate()
    {
        _rb.MoveRotation(Quaternion.Euler(0f, Yaw, 0f));

        // 우선순위: Roll > Crouch > Sprint > Move
        if (_isRolling)
            _mode = MoveMode.Roll;
        else if (_isSpeedBoost)
            _mode = MoveMode.None;
        else if (_isCrouching)
            _mode = MoveMode.Crouch;
        else if (_isSprint && _sprintDuration > 0)
            _mode = MoveMode.Sprint;
        else
            _mode = MoveMode.Move;

        switch (_mode)
        {
            case MoveMode.Move:
                {
                    _currentSpeed = _moveSpeed;
                    _collider.height = _standHeight;
                    _collider.center = _prevColliderCenter;

                    _sprintDuration += Time.fixedDeltaTime;
                    if (_sprintDuration >= _sprintTotalTime)
                        _sprintDuration = _sprintTotalTime;

                    OnSprintChanged?.Invoke(_sprintDuration / _sprintTotalTime);
                }
                break;
            case MoveMode.Sprint:
                {
                    _currentSpeed = _moveSpeed + _sprintSpeed;
                    _sprintDuration -= Time.fixedDeltaTime;

                    if (_sprintDuration <= 0)
                    {
                        _isSprint = false;
                        _sprintDuration = 0f;
                        OnSprintActive?.Invoke(_isSprint);
                    }
                    OnSprintChanged?.Invoke(_sprintDuration / _sprintTotalTime);
                }
                break;
            case MoveMode.Roll:
            {
                Vector3 rollVelocity = _rollDirection * _rollSpeed;
                rollVelocity.y = _rb.linearVelocity.y;
                _rb.linearVelocity = rollVelocity;
                break;
            }
            case MoveMode.Crouch:
                {
                    _currentSpeed = _crouchSpeed;
                    _collider.height = _crouchHeight;
                    _collider.center = new Vector3(
                        _prevColliderCenter.x,
                        _prevColliderCenter.y - (_standHeight - _crouchHeight) / 2f,
                        _prevColliderCenter.z
                    );
                    _crouchDuration -= Time.fixedDeltaTime;

                    if (_crouchDuration <= 0)
                    {
                        _isCrouching = false;
                        _crouchDuration = 0f;
                    }
                }
                break;
        }

        if (_isSpeedBoost)
        {
            _currentSpeed = _boostSpeed;
            if (_isSprint && _sprintDuration > 0)
            {
                _currentSpeed += _sprintSpeed;
                _sprintDuration -= Time.fixedDeltaTime;
                if (_sprintDuration <= 0)
                {
                    _isSprint = false;
                    _sprintDuration = 0f;
                }
                OnSprintChanged?.Invoke(_sprintDuration / _sprintTotalTime);
            }
        }

        if (_mode != MoveMode.Roll)
        {
            Vector3 move =
                (transform.forward * _inputDir.y + transform.right * _inputDir.x) * _currentSpeed;
            move.y = _rb.linearVelocity.y;
            _rb.linearVelocity = move;
        }

        if (_mode != MoveMode.Roll && IsGrounded() && _rb.linearVelocity.y <= 0f)
        {
            var v = _rb.linearVelocity;
            v.y = 0f;
            _rb.linearVelocity = v;
        }

        float horizSpeed = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z).magnitude;
        _animator.SetFloat("Speed", horizSpeed);
        _animator.SetBool("IsGrounded", IsGrounded());
        _animator.SetBool("IsRolling", _isRolling);
        _animator.SetBool("isCrouching", _isCrouching);
    }

    public void PlayFace(string stateName)
    {
        if (_currentFace == stateName || _animator == null)
            return;

        _currentFace = stateName;
        _animator.Play(stateName, 1);
    }

    public void ResetFace() => PlayFace("Eyes_Blink");

    public void SetFaceDamaged() => PlayFace("Eyes_Cry");

    public void SetFaceDead() => PlayFace("Eyes_Dead");

    public void SetFaceHappy() => PlayFace("Eyes_Happy");

    //public void SetFaceShrink() => PlayFace("Eyes_Shrink");

    private void OnMove(InputValue value)
    {
        _inputDir = value.Get<Vector2>();
    }

    private void OnSprint(InputValue value)
    {
        if (value.isPressed)
        {
            _isSprint = true;
            OnSprintActive?.Invoke(_isSprint);
        }
        else
        {
            _isSprint = false;
            OnSprintActive?.Invoke(_isSprint);
        }
    }

    private void OnLook(InputValue value)
    {
        Vector2 delta = value.Get<Vector2>() * _mouseSensitivity;
        Yaw += delta.x;
        Pitch = Mathf.Clamp(Pitch - delta.y, _minPitch, _maxPitch);
    }

    private void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded())
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _animator.SetTrigger("IsJumping");
        }
    }

    private void OnCrouch(InputValue value)
    {
        _isCrouching = value.isPressed;
    }

    private void OnRoll(InputValue value)
    {
        if (!value.isPressed || _isRolling)
            return;

        if (Time.time - _lastRollTime < _rollCoolDown)
            return;

        _rollDirection =
            (_inputDir.magnitude > 0.1f)
                ? (transform.forward * _inputDir.y + transform.right * _inputDir.x).normalized
                : transform.forward;
        _rollCoroutine = StartCoroutine(RollCoroutine());
    }

    public void CancelRoll()
    {
        if (_rollCoroutine == null)
            return;
        StopCoroutine(_rollCoroutine);
        _rollCoroutine = null;
        _isRolling = false;
    }

    private IEnumerator RollCoroutine()
    {
        _isRolling = true;
        _lastRollTime = Time.time;
        _playerHealth.SetInvincible(_rollDuration, "roll");

        yield return new WaitForSeconds(_rollDuration);

        _rollCoroutine = null;
        _isRolling = false;
        Debug.Log($"[Roll] 롤 종료 t={Time.time:F3}");
    }

    private bool IsGrounded()
    {
        float scale = transform.localScale.y;
        float halfHeight = _collider.height * scale / 2f;
        float centerOffsetY = _collider.center.y * scale;
        Vector3 bottom = transform.position + Vector3.up * (centerOffsetY - halfHeight + 0.01f);

        float spread = _collider.radius * transform.localScale.x * 0.7f;
        float rayDist = 0.05f;
        LayerMask mask = _groundLayerMask == 0 ? ~0 : _groundLayerMask;

        Vector3[] origins = new Vector3[5]
        {
            bottom,
            bottom + transform.forward * spread,
            bottom - transform.forward * spread,
            bottom + transform.right * spread,
            bottom - transform.right * spread,
        };

        foreach (var origin in origins)
        {
            Debug.DrawRay(origin, Vector3.down * rayDist, Color.red);
            if (Physics.Raycast(origin, Vector3.down, rayDist, mask))
                return true;
        }

        return false;
    }

    public void SetSpeedBoost(float speed, float sec)
    {
        if (_speedCo != null)
            StopCoroutine(_speedCo);

        _speedCo = StartCoroutine(SpeedBoostCoroutine(speed, sec));
    }

    private IEnumerator SpeedBoostCoroutine(float speed, float sec)
    {
        _isSpeedBoost = true;
        _boostSpeed = speed;
        Debug.Log($"[아이템 획득] 속도 부스터 효과: 속도 - {_currentSpeed}, 지속 시간 - {sec}");

        yield return new WaitForSeconds(sec);

        _isSpeedBoost = false;
        _currentSpeed = _moveSpeed;
    }
}
