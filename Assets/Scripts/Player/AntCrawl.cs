using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Animator))]
public class AntCrawl : MonoBehaviour
{
    [Header("감지 설정")]
    [Tooltip("벽타기 가능한 레이어 (건물 벽, 장애물 등)")]
    [SerializeField] private LayerMask _crawlMask;

    [Tooltip("진입용 벽 감지 거리 (이동 방향 SphereCast)")]
    [SerializeField] private float _detectRange = 0.3f;

    [Tooltip("크롤 중 표면 유지 감지 거리 — _detectRange + _checkRadius 이상이어야 함")]
    [SerializeField] private float _surfaceHoldRange = 0.6f;

    [Tooltip("SphereCast 반경 (캡슐 반지름보다 작게)")]
    [SerializeField] private float _checkRadius = 0.1f;

    [Header("크롤 이동")]
    [SerializeField] private float _crawlSpeed = 1.0f;

    [Tooltip("표면 법선 방향으로 정렬하는 보간 속도")]
    [SerializeField] private float _alignSpeed = 10f;

    [Tooltip("표면과의 유지 거리 — CapsuleCollider 반지름과 맞춰야 함")]
    [SerializeField] private float _snapOffset = 0.25f;

    [Header("이탈 점프")]
    [Tooltip("Space 이탈 시 표면 법선 방향으로 가해지는 힘")]
    [SerializeField] private float _exitJumpForce = 4f;

    [Tooltip("이탈 직후 재진입을 막는 쿨다운 (초)")]
    [SerializeField] private float _exitCooldown = 0.5f;

    [Header("이탈 버퍼")]
    [Tooltip("표면 감지 실패가 몇 프레임 연속되면 이탈할지 (모서리·요철 맵에서는 5~10 권장)")]
    [SerializeField] private int _lostSurfaceThreshold = 5;

    [Tooltip("표면 snap velocity 최대치 — 너무 크면 진동 발생")]
    [SerializeField] private float _snapVelMax = 2f;

    public bool IsCrawling { get; private set; }

    private static readonly int k_SpeedHash = Animator.StringToHash("Speed");
    private static readonly int k_IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int k_IsRollingHash = Animator.StringToHash("IsRolling");
    private static readonly int k_IsCrouchHash = Animator.StringToHash("isCrouching");
    private static readonly int k_IsCrawlHash = Animator.StringToHash("IsCrawling");

    private PlayerMovement _playerMovement;
    private Rigidbody _rb;
    private Animator _animator;
    private RaycastHit _lastSurfaceHit;
    private float _exitCrawlTime = -999f;
    private int _lostSurfaceFrames = 0;
    private bool _pendingSnapOnEntry = false;

    private Vector2 InputDir => _playerMovement.InputDir;


    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!IsCrawling)
            TryEnterCrawl();
    }

    private void FixedUpdate()
    {
        if (!IsCrawling)
            return;

        if (_pendingSnapOnEntry)
        {
            _pendingSnapOnEntry = false;
            float dist = Vector3.Dot(transform.position - _lastSurfaceHit.point, _lastSurfaceHit.normal);
            float err = dist - _snapOffset;
            _rb.MovePosition(transform.position - _lastSurfaceHit.normal * err);
            return;
        }

        if (!DetectSurface(out RaycastHit hit))
        {
            if (++_lostSurfaceFrames >= _lostSurfaceThreshold)
            {
                _lostSurfaceFrames = 0;
                ExitCrawl(Vector3.zero);
            }
            return;
        }

        _lostSurfaceFrames = 0;
        _lastSurfaceHit = hit;
        AlignToSurface(hit.normal);

        float distToWall = Vector3.Dot(transform.position - hit.point, hit.normal);
        float snapError = distToWall - _snapOffset;
        float snapMag = Mathf.Clamp(-snapError * _alignSpeed, -_snapVelMax, _snapVelMax);
        Vector3 snapVel = hit.normal * snapMag;

        Vector3 moveVel = GetMoveVelocity(hit.normal);
        _rb.linearVelocity = snapVel + moveVel;

        UpdateAnimator(moveVel.magnitude);
    }

    private void TryEnterCrawl()
    {
        if (Time.time - _exitCrawlTime < _exitCooldown)
        {
            Debug.Log($"[AntCrawl] 이탈 쿨다운 중: {_exitCooldown - (Time.time - _exitCrawlTime):F2}s 남음");
            return;
        }

        if (InputDir.magnitude < 0.1f)
            return;

        Vector3 moveDir = (transform.forward * InputDir.y + transform.right * InputDir.x).normalized;

        Debug.DrawRay(transform.position, moveDir * _detectRange, Color.cyan);
        Debug.Log($"[AntCrawl] SphereCast 시도: origin={transform.position}, dir={moveDir:F2}, range={_detectRange}, mask={_crawlMask.value}");

        if (!Physics.SphereCast(transform.position, _checkRadius, moveDir,
                                out RaycastHit hit, _detectRange, _crawlMask))
        {
            Debug.Log("[AntCrawl] ❌ 벽 감지 실패 — Crawl Mask 레이어 또는 detectRange 확인");
            return;
        }

        Debug.DrawRay(hit.point, hit.normal * 0.3f, Color.yellow);
        float normalDotUp = Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up));
        Debug.Log($"[AntCrawl] ✅ 벽 감지 성공: normal={hit.normal:F2}, normalDotUp={normalDotUp:F2} (0.5 미만이면 진입)");

        if (normalDotUp < 0.5f)
            EnterCrawl(hit);
        else
            Debug.Log("[AntCrawl] ❌ 바닥/천장으로 판정되어 진입 거부");
    }

    private void EnterCrawl(RaycastHit hit)
    {
        IsCrawling = true;
        _lastSurfaceHit = hit;
        _lostSurfaceFrames = 0;
        _pendingSnapOnEntry = true;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.ProjectOnPlane(_rb.linearVelocity, hit.normal);

        Vector3 wallUp = Vector3.ProjectOnPlane(Vector3.up, hit.normal).normalized;
        if (wallUp.sqrMagnitude < 0.001f)
            wallUp = Vector3.ProjectOnPlane(Vector3.forward, hit.normal).normalized;
        if (wallUp.sqrMagnitude > 0.001f)
            _rb.MoveRotation(Quaternion.LookRotation(wallUp, hit.normal));

        _playerMovement.SetCrawling(true);
    }


    private void ExitCrawl(Vector3 exitVelocity)
    {
        IsCrawling = false;
        _exitCrawlTime = Time.time;
        _rb.useGravity = true;
        _playerMovement.SetCrawling(false);
        _rb.MoveRotation(Quaternion.Euler(0f, _playerMovement.Yaw, 0f));

        if (exitVelocity != Vector3.zero)
            _rb.linearVelocity = exitVelocity;

        UpdateAnimator(0f);
    }

    private bool DetectSurface(out RaycastHit hit)
    {
        Vector3 origin = transform.position + _lastSurfaceHit.normal * _checkRadius;
        return Physics.SphereCast(
            origin,
            _checkRadius,
            -_lastSurfaceHit.normal,
            out hit,
            _surfaceHoldRange + _checkRadius,
            _crawlMask
        );
    }

    private void AlignToSurface(Vector3 surfaceNormal)
    {
        Vector3 wallUp = Vector3.ProjectOnPlane(Vector3.up, surfaceNormal).normalized;

        if (wallUp.sqrMagnitude < 0.001f)
            wallUp = Vector3.ProjectOnPlane(
                Quaternion.Euler(0f, _playerMovement.Yaw, 0f) * Vector3.forward,
                surfaceNormal).normalized;

        if (wallUp.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(wallUp, surfaceNormal);
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, _alignSpeed * Time.fixedDeltaTime));
    }

    private Vector3 GetMoveVelocity(Vector3 surfaceNormal)
    {
        Vector2 clampedInput = Vector2.ClampMagnitude(InputDir, 1f);

        Vector3 wallUp = Vector3.ProjectOnPlane(Vector3.up, surfaceNormal).normalized;
        if (wallUp.sqrMagnitude < 0.001f)
            wallUp = Vector3.ProjectOnPlane(transform.forward, surfaceNormal).normalized;

        Vector3 wallRight = Vector3.Cross(surfaceNormal, wallUp).normalized;

        return (wallUp * clampedInput.y + wallRight * clampedInput.x) * _crawlSpeed;
    }

    private void UpdateAnimator(float speed)
    {
        if (_animator == null) return;
        _animator.SetFloat(k_SpeedHash, speed);
        _animator.SetBool(k_IsGroundedHash, true);
        _animator.SetBool(k_IsRollingHash, false);
        _animator.SetBool(k_IsCrouchHash, false);
        _animator.SetBool(k_IsCrawlHash, IsCrawling);
    }

    private void OnJump(InputValue value)
    {
        if (!value.isPressed || !IsCrawling)
            return;

        ExitCrawl(_lastSurfaceHit.normal * _exitJumpForce);
    }
}
