using UnityEngine;

public class PatrolCharacter : MonoBehaviour
{
    [SerializeField] private Transform m_PointA;
    [SerializeField] private Transform m_PointB;
    [SerializeField] private float m_Speed = 2f;
    [SerializeField] private float m_WaitTime = 1f;

    private Transform m_Current;
    private Transform m_Next;
    private float m_WaitTimer;
    private bool m_Waiting;

    private void Start()
    {
        if (m_PointA == null || m_PointB == null)
        {
            Debug.LogWarning($"[PatrolCharacter] {name}: PointA 또는 PointB가 설정되지 않았습니다.");
            enabled = false;
            return;
        }

        transform.position = m_PointA.position;
        m_Current = m_PointA;
        m_Next = m_PointB;
    }

    private void Update()
    {
        if (m_Waiting)
        {
            m_WaitTimer -= Time.deltaTime;
            if (m_WaitTimer <= 0f)
                m_Waiting = false;
            return;
        }

        MoveToward(m_Next.position);
    }

    private void MoveToward(Vector3 target)
    {
        var direction = (target - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            transform.position = new Vector3(target.x, transform.position.y, target.z);
            SwapTarget();
            return;
        }

        transform.position += direction.normalized * m_Speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void SwapTarget()
    {
        (m_Current, m_Next) = (m_Next, m_Current);
        m_Waiting = true;
        m_WaitTimer = m_WaitTime;
    }

    private void OnDrawGizmosSelected()
    {
        if (m_PointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_PointA.position, 0.2f);
        }
        if (m_PointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_PointB.position, 0.2f);
        }
        if (m_PointA != null && m_PointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_PointA.position, m_PointB.position);
        }
    }
}
