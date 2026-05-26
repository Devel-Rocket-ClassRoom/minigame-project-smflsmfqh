using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CatMovement : MonoBehaviour
{
    [Header("추격 대상")]
    [SerializeField]
    private PlayerMovement _player;

    [Header("가시 범위")]
    [SerializeField]
    private float _detectionRadius = 8f;

    [SerializeField]
    private LayerMask _playerLayer;

    [Header("배회 설정")]
    [SerializeField]
    private float _wanderRadius = 10f;

    [SerializeField]
    private float _wanderSpeed = 2f;

    [SerializeField]
    private float _wanderWaitTime = 1f;

    [Header("추격 설정")]
    [SerializeField]
    private float _runSpeed = 4f;

    [SerializeField]
    private float _stopDistance = 1.5f;

    [Header("공격 설정")]
    [SerializeField]
    private float _attackRadius = 2.5f;

    [SerializeField]
    private float _attackCoolDown = 5f;

    private const float _damage = 10f;
    private float _lastAttackTime = -999f;
    private float _attackTimer;

    [Header("NavMesh 영역 설정")]
    private NavMeshAgent _agent;
    private Animator _animator;

    private enum State
    {
        Wander,
        Chase,
    }

    private State _state = State.Wander;
    private string _excludeArea = "Road";
    private int _walkableMask;
    private bool _isWaiting;
    private bool _isFound = false;
    private float _waitTimer;
    private bool _initialized;
    private bool _hasDestination = false;
    private string _currentAnim = string.Empty;
    private string _currentFace = string.Empty;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Setup();
        SetWanderTarget();
    }

    private void Update()
    {
        if (!_initialized)
            return;

        bool detected = Physics.CheckSphere(transform.position, _detectionRadius, _playerLayer);

        if (detected && _state != State.Chase)
            EnterChase();
        if (!detected && _state != State.Wander)
            EnterWander();

        switch (_state)
        {
            case State.Chase:
                UpdateChase();
                break;
            case State.Wander:
                UpdateWander();
                break;
        }

        UpdateAnimation();
    }

    private void Setup()
    {
        _initialized = true;
        int roadAreaIndex = NavMesh.GetAreaFromName(_excludeArea);
        // Road 영역을 제외한 전체 영역을 허용
        _walkableMask =
            roadAreaIndex >= 0 ? NavMesh.AllAreas & ~(1 << roadAreaIndex) : NavMesh.AllAreas;

        _agent.areaMask = _walkableMask;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    private void EnterChase()
    {
        _state = State.Chase;
        _agent.speed = _runSpeed;
        _agent.stoppingDistance = _stopDistance;
        _hasDestination = false;
        _isFound = true;
        _isWaiting = false;
    }

    private void EnterWander()
    {
        _state = State.Wander;
        _agent.speed = _wanderSpeed;
        _agent.stoppingDistance = 0f;

        _isFound = false;

        SetWanderTarget();
    }

    private void UpdateChase()
    {
        if (_player == null || !_agent.isOnNavMesh)
            return;

        _agent.SetDestination(_player.transform.position);
    }

    private void UpdateWander()
    {
        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                SetWanderTarget();
                return;
            }
        }

        if (
            _hasDestination
            && _agent.isOnNavMesh
            && !_agent.pathPending
            && _agent.remainingDistance <= _agent.stoppingDistance
        )
        {
            _hasDestination = false;
            _isWaiting = true;
            _waitTimer = _wanderWaitTime;
        }
    }

    private void SetWanderTarget()
    {
        int maxCount = 10;
        for (int i = 0; i < maxCount; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * _wanderRadius;
            if (
                _agent.isOnNavMesh
                && NavMesh.SamplePosition(
                    randomPos,
                    out NavMeshHit hit,
                    _wanderRadius,
                    _walkableMask
                )
            )
            {
                _agent.SetDestination(hit.position);
                _hasDestination = true;
                return;
            }
        }
    }

    private void UpdateAnimation()
    {
        string targetMove;
        string targetFace;
        if (_state == State.Chase)
        {
            targetMove = "Run";
            targetFace = "Eyes_Excited";
        }
        else if (_isWaiting || _agent.velocity.magnitude < 0.1f)
        {
            targetMove = "Idle_A";
            targetFace = "Eyes_Blink";
        }
        else
            targetMove = "Walk";
        targetFace = "Eyes_Blink";

        if (_currentAnim != targetMove)
        {
            _currentAnim = targetMove;
            _currentFace = targetFace;
            _animator.Play(targetMove);
            _animator.SetBool("isFound", _isFound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}
