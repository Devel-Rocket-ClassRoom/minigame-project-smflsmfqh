using UnityEngine;

public class CatSpawnZone : MonoBehaviour
{
    private BoxCollider _collider;

    private BoxCollider GetCollider()
    {
        if (_collider == null)
            _collider = GetComponent<BoxCollider>();
        return _collider;
    }
}
