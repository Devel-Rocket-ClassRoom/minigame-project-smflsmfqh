using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour
{
    public enum NpcType
    {
        Patrol,
        Runner,
    }

    [Header("NPC 타입")]
    [SerializeField]
    private NpcType _Type = NpcType.Patrol;

    [Header("이동 범위")]
    [SerializeField]
    private float _RectWidth = 20f;

    [SerializeField]
    private float _RectDepth = 10f;

    [Header("Patrol 설정")]
    [SerializeField]
    private float _PatrolSpeed = 2.5f;

    [SerializeField]
    private float _PatrolWaitTime = 1f;

    [Header("Runner 설정")]
    [SerializeField]
    private float _RunSpeed = 5f;

    [SerializeField]
    private float _RunWaitTime = 0.5f;

    [Header("NavMesh 영역 설정")]
    [SerializeField]
    private string[] _ExcludeAreas = { "Road", "Not Walkable" };

    private NavMeshAgent _Agent;
    private Animator _Animator;

    private Vector3 _Origin;
    private int _AreaMask;
    private float _WaitTimer;
    private bool _Waiting;
    private bool _Initialized;
    private float _AnimVert;
    private float _CurrentWaitTime;

    private Vector3 _PointA;
    private Vector3 _PointB;
    private bool _GoingToB;
    private float _minDistance = 5f;

    private static readonly int k_HorID = Animator.StringToHash("Hor");
    private static readonly int k_VertID = Animator.StringToHash("Vert");
    private static readonly int k_StateID = Animator.StringToHash("State");

    private void Awake()
    {
        _Agent = GetComponent<NavMeshAgent>();
        _Animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!_Initialized)
        {
            _Origin = transform.position;
            Setup();
        }
    }

    private void Update()
    {
        if (!_Initialized)
            return;
        UpdateMovement();
        UpdateAnimator();
    }

    public void SetupPatrol(
        Vector3 zoneCenter,
        float rectWidth = 20f,
        float rectDepth = 10f,
        float speed = 2.5f,
        float waitTime = 1f
    )
    {
        _Origin = zoneCenter;
        _Type = NpcType.Patrol;
        _RectWidth = rectWidth;
        _RectDepth = rectDepth;
        _PatrolSpeed = speed;
        _PatrolWaitTime = waitTime;
        Setup();
    }

    public void SetupRunner(
        Vector3 zoneCenter,
        float rectWidth = 20f,
        float rectDepth = 10f,
        float speed = 5f,
        float waitTime = 0.5f
    )
    {
        _Origin = zoneCenter;
        _Type = NpcType.Runner;
        _RectWidth = rectWidth;
        _RectDepth = rectDepth;
        _RunSpeed = speed;
        _RunWaitTime = waitTime;
        Setup();
    }

    private void Setup()
    {
        _Initialized = true;
        _AreaMask = BuildAreaMask();

        _Agent.areaMask = _AreaMask;
        _Agent.stoppingDistance = 0.3f;
        _Agent.speed = _Type == NpcType.Patrol ? _PatrolSpeed : _RunSpeed;
        _CurrentWaitTime = _Type == NpcType.Patrol ? _PatrolWaitTime : _RunWaitTime;

        SetupLinearPoints();
    }

    private int BuildAreaMask()
    {
        int mask = NavMesh.AllAreas;
        foreach (var areaName in _ExcludeAreas)
        {
            int area = NavMesh.GetAreaFromName(areaName);
            if (area >= 0)
                mask &= ~(1 << area);
            else
                Debug.LogWarning($"[NPCMovement] NavMesh area '{areaName}'를 찾을 수 없습니다.");
        }
        return mask;
    }

    private void SetupLinearPoints()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

        float halfW = _RectWidth * 0.5f;
        float halfD = _RectDepth * 0.5f;

        float tX = Mathf.Abs(dir.x) > 0.001f ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
        float tZ = Mathf.Abs(dir.z) > 0.001f ? halfD / Mathf.Abs(dir.z) : float.MaxValue;
        float t = Mathf.Min(tX, tZ);

        Vector3 candidateA = _Origin + dir * t;
        Vector3 candidateB = _Origin - dir * t;

        NavMeshHit hit;
        _PointA = NavMesh.SamplePosition(candidateA, out hit, 3f, _AreaMask)
            ? hit.position
            : _Origin;
        _PointB = NavMesh.SamplePosition(candidateB, out hit, 3f, _AreaMask)
            ? hit.position
            : _Origin;

        if (Vector3.Distance(_PointA, _PointB) < _minDistance)
        {
            _PointA = _Origin;
            _PointB = _Origin;
            _Agent.isStopped = true;
            _Waiting = true;
            _WaitTimer = _CurrentWaitTime;
            return;
        }

        _GoingToB = true;
        _Agent.SetDestination(_PointB);
    }

    private void UpdateMovement()
    {
        if (_Waiting)
        {
            _WaitTimer -= Time.deltaTime;
            if (_WaitTimer <= 0f)
            {
                Debug.Log("[NPC Movement] 웨이팅 타이머 종료");
                _Waiting = false;
                _Agent.isStopped = false;
                _GoingToB = !_GoingToB;
                Debug.Log($"[NPC Movement] _GointToB: {_GoingToB}");
                _Agent.SetDestination(_GoingToB ? _PointB : _PointA);
            }
            return;
        }

        if (HasArrived())
        {
            _Agent.isStopped = true;
            _Waiting = true;
            _WaitTimer = _CurrentWaitTime;
        }
    }

    private void UpdateAnimator()
    {
        var moving = !_Agent.isStopped && _Agent.velocity.sqrMagnitude > 0.01f;
        _AnimVert = Mathf.MoveTowards(_AnimVert, moving ? 1f : 0f, Time.deltaTime * 4.5f);

        _Animator.SetFloat(k_HorID, 0f);
        _Animator.SetFloat(k_VertID, _AnimVert);
        _Animator.SetFloat(k_StateID, _Type == NpcType.Runner ? 1f : 0f);
    }

    private bool HasArrived()
    {
        return !_Agent.pathPending
            && _Agent.hasPath
            && _Agent.remainingDistance <= _Agent.stoppingDistance;
    }

    private void OnDrawGizmosSelected()
    {
        var center = Application.isPlaying ? _Origin : transform.position;
        Gizmos.color =
            _Type == NpcType.Patrol ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0.3f, 0f, 0.4f);
        Gizmos.DrawWireCube(center, new Vector3(_RectWidth, 0.1f, _RectDepth));

        if (Application.isPlaying)
        {
            Gizmos.DrawSphere(_PointA, 0.2f);
            Gizmos.DrawSphere(_PointB, 0.2f);
            Gizmos.DrawLine(_PointA, _PointB);
        }
    }
}
