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
    private NpcType m_Type = NpcType.Patrol;

    [Header("Patrol 설정")]
    [SerializeField]
    private float m_PatrolRadius = 10f; // 스폰 위치 기준 배회 반경

    [SerializeField]
    private float m_PatrolSpeed = 2.5f;

    [SerializeField]
    private float m_WaitTime = 1f; // 목적지 도착 후 대기 시간

    [Header("Runner 설정")]
    [SerializeField]
    private float m_RunRadius = 25f; // 스폰 위치 기준 이동 반경

    [SerializeField]
    private float m_RunSpeed = 5f;

    [Header("NavMesh 영역 설정")]
    [SerializeField]
    private string[] m_ExcludeAreas = { "Road" }; // 진입 금지 area 이름 목록

    private NavMeshAgent m_Agent;
    private Animator m_Animator;

    private Vector3 m_Origin; // 스폰 위치 (배회 중심)
    private int m_AreaMask;
    private float m_WaitTimer;
    private bool m_Waiting;
    private bool m_Initialized;
    private float m_AnimVert;

    // Animator 파라미터 — Character_Movement.controller 기준
    private static readonly int k_HorID = Animator.StringToHash("Hor");
    private static readonly int k_VertID = Animator.StringToHash("Vert");
    private static readonly int k_StateID = Animator.StringToHash("State");

    private void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!m_Initialized)
            Setup();
    }

    private void Update()
    {
        if (!m_Initialized)
            return;

        if (m_Type == NpcType.Patrol)
            UpdatePatrol();
        else
            UpdateRunner();

        UpdateAnimator();
    }

    // ---- 외부(SpawnManager)에서 런타임 세팅 ----

    public void SetupPatrol(float radius = 10f, float speed = 2.5f, float waitTime = 1f)
    {
        m_Type = NpcType.Patrol;
        m_PatrolRadius = radius;
        m_PatrolSpeed = speed;
        m_WaitTime = waitTime;
        Setup();
    }

    public void SetupRunner(float radius = 25f, float speed = 5f)
    {
        m_Type = NpcType.Runner;
        m_RunRadius = radius;
        m_RunSpeed = speed;
        Setup();
    }

    // ---- 초기화 ----

    private void Setup()
    {
        m_Initialized = true;
        m_Origin = transform.position;
        m_AreaMask = BuildAreaMask();

        m_Agent.areaMask = m_AreaMask;
        m_Agent.stoppingDistance = 0.3f;
        m_Agent.speed = m_Type == NpcType.Patrol ? m_PatrolSpeed : m_RunSpeed;

        MoveToNextDestination();
    }

    private int BuildAreaMask()
    {
        int mask = NavMesh.AllAreas;
        foreach (var areaName in m_ExcludeAreas)
        {
            int area = NavMesh.GetAreaFromName(areaName);
            if (area >= 0)
                mask &= ~(1 << area);
            else
                Debug.LogWarning($"[NPCMovement] NavMesh area '{areaName}'를 찾을 수 없습니다.");
        }
        return mask;
    }

    // ---- Patrol 업데이트 ----

    private void UpdatePatrol()
    {
        if (m_Waiting)
        {
            m_WaitTimer -= Time.deltaTime;
            if (m_WaitTimer <= 0f)
            {
                m_Waiting = false;
                m_Agent.isStopped = false;
                MoveToNextDestination();
            }
            return;
        }

        if (HasArrived())
        {
            m_Agent.isStopped = true;
            m_Waiting = true;
            m_WaitTimer = m_WaitTime;
        }
    }

    // ---- Runner 업데이트 ----

    private void UpdateRunner()
    {
        if (HasArrived())
            MoveToNextDestination();
    }

    // ---- NavMesh 랜덤 목적지 선택 ----

    private void MoveToNextDestination()
    {
        float radius = m_Type == NpcType.Patrol ? m_PatrolRadius : m_RunRadius;

        if (TryGetRandomNavMeshPoint(m_Origin, radius, out var dest))
        {
            m_Agent.SetDestination(dest);
        }
        else
        {
            // 유효한 점을 못 찾으면 잠시 후 재시도
            m_Waiting = true;
            m_WaitTimer = 0.5f;
        }
    }

    private bool TryGetRandomNavMeshPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++) // 최대 10회 시도
        {
            var candidate = center + Random.insideUnitSphere * radius;
            candidate.y = center.y;

            if (NavMesh.SamplePosition(candidate, out var hit, radius, m_AreaMask))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    // ---- Animator 구동 ----

    private void UpdateAnimator()
    {
        var moving = !m_Agent.isStopped && m_Agent.velocity.sqrMagnitude > 0.01f;
        m_AnimVert = Mathf.MoveTowards(m_AnimVert, moving ? 1f : 0f, Time.deltaTime * 4.5f);

        m_Animator.SetFloat(k_HorID, 0f);
        m_Animator.SetFloat(k_VertID, m_AnimVert);
        m_Animator.SetFloat(k_StateID, m_Type == NpcType.Runner ? 1f : 0f);
    }

    // ---- 도착 판정 ----

    private bool HasArrived()
    {
        return !m_Agent.pathPending
            && m_Agent.hasPath
            && m_Agent.remainingDistance <= m_Agent.stoppingDistance;
    }

    // ---- Gizmos ----

    private void OnDrawGizmosSelected()
    {
        var center = Application.isPlaying ? m_Origin : transform.position;
        var radius = m_Type == NpcType.Patrol ? m_PatrolRadius : m_RunRadius;

        Gizmos.color =
            m_Type == NpcType.Patrol ? new Color(0f, 1f, 0f, 0.2f) : new Color(1f, 0.3f, 0f, 0.2f);
        DrawWireCircle(center, radius);
    }

    private static void DrawWireCircle(Vector3 center, float radius, int segments = 32)
    {
        float step = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a0 = i * step * Mathf.Deg2Rad;
            float a1 = (i + 1) * step * Mathf.Deg2Rad;
            Gizmos.DrawLine(
                center + new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * radius,
                center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius
            );
        }
    }
}
