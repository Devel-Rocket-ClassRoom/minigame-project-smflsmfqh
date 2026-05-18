using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speed")]
    [SerializeField]
    private float _moveSpeed = 5f;

    [Header("Jump Field")]
    [SerializeField]
    private float _jumpForce = 1f;
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
    [SerializeField]
    private float _rollSpeed = 2f;
    [SerializeField]
    private float _rollDuration = 0.5f;
    [SerializeField]
    private float _rollCoolDown = 1f;
    
    private bool _isRolling = false;
    private float _lastRollTime = -999f;
    private Vector3 _rollDirection;
    private Coroutine _rollCoroutine;

    [Header("Crouch Field")]
    private const float _standHeight = 2f;
    private const float _crouchHeight = 1f;
    [SerializeField]
    private float _crouchSpeed = 0.5f;
    private bool _isCrouching = false;

    private Rigidbody _rb;
    private CapsuleCollider _collider;
    private Vector2 _inputDir;

    public float Yaw { get; private set; }
    public float Pitch { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        Yaw = transform.eulerAngles.y;
    }

    private void OnMove(InputValue value)
    {
        _inputDir = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        Vector2 delta = value.Get<Vector2>() * _mouseSensitivity;
        Yaw += delta.x;
        Pitch = Mathf.Clamp(Pitch - delta.y, _minPitch, _maxPitch);
        //transform.rotation = Quaternion.Euler(0f, Yaw, 0f);
    }

    private void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded())
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCrouch(InputValue value)
    {
        if (!value.isPressed && !CanStandUp()) return;
        _isCrouching = value.isPressed;
    }

    private void OnRoll(InputValue value)
    {
        if (!value.isPressed || _isRolling) return;
        if (Time.time - _lastRollTime < _rollCoolDown) return;
        
        _rollDirection = (_inputDir.magnitude > 0.1f) ? (transform.forward * _inputDir.y + transform.right * _inputDir.x).normalized : transform.forward;
        _rollCoroutine = StartCoroutine(RollCoroutine());
    }

    public void CancelRoll()
    {
        if (_rollCoroutine == null) return;
        StopCoroutine(_rollCoroutine);
        _rollCoroutine = null;
        _isRolling = false;
    }

    private IEnumerator RollCoroutine()
    {
        _isRolling = true;
        _lastRollTime = Time.time;

        float elapsed = 0f;
        while (elapsed < _rollDuration)
        {
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _rollCoroutine = null;
        _isRolling = false;
    }

    private bool IsGrounded()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(
                transform.position + (transform.forward * 0.2f) + (transform.up * 0.01f),
                Vector3.down
            ),
            new Ray(
                transform.position + (-transform.forward * 0.2f) + (transform.up * 0.01f),
                Vector3.down
            ),
            new Ray(
                transform.position + (transform.right * 0.2f) + (transform.up * 0.01f),
                Vector3.down
            ),
            new Ray(
                transform.position + (-transform.right * 0.2f) + (transform.up * 0.01f),
                Vector3.down
            ),
        };

        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], 0.1f, _groundLayerMask))
                return true;
        }

        return false;
    }

    private bool CanStandUp()
    {
        return !Physics.Raycast(transform.position, Vector3.up, _standHeight, _groundLayerMask);
    }

    private void FixedUpdate()
    {
        _rb.MoveRotation(Quaternion.Euler(0f, Yaw, 0f));

        if (_isCrouching)
        {
            _collider.height = _crouchHeight;
            _collider.center = new Vector3(0f, _crouchHeight / 2f, 0f);
        }
        else
        {
            _collider.height = _standHeight;
            _collider.center = new Vector3(0f, _standHeight / 2f, 0f);
        }

        float speed = _isCrouching ? _moveSpeed * _crouchSpeed : _moveSpeed;

        if (_isRolling)
        {
            Vector3 rollVelocity = _rollDirection * _rollSpeed;
            rollVelocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = rollVelocity;
        }
        else
        {
            Vector3 move =
                (transform.forward * _inputDir.y + transform.right * _inputDir.x) * speed;
            move.y = _rb.linearVelocity.y;
            _rb.linearVelocity = move;
        }
    }
}
