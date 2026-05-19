using UnityEngine;
using UnityEngine.AI;

public class NPCSpawnZone : MonoBehaviour
{
    private BoxCollider _collider;
    private const float _maxDistance = 2f;

    public Vector3 Center => _collider.bounds.center;
    public Vector3 Size => _collider.bounds.size;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    public bool TryGetRandomPoint(int areaMask, out Vector3 result, int maxAttempts = 10)
    {
        Bounds bounds = _collider.bounds;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _maxDistance, areaMask))
            {
                Debug.Log($"[NPC Random Point] {candidate} Spawned");
                result = hit.position;
                return true;
            }
        }
        result = transform.position;
        Debug.Log($"[NPC Random Point] Failed! {result}");

        return false;
    }

    public bool TryGetSpawnPoint(int areaMask, out Vector3 result)
    {
        return TryGetRandomPoint(areaMask, out result);
    }
}
