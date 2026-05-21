using UnityEngine;

public class ShopBuilding : MonoBehaviour
{
    [SerializeField]
    private ShopData _shopData;
    private Vector3 _spawnPos;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        _spawnPos = GetFrontSpawnPosition();
        foreach (var item in _shopData.dropItems)
            Instantiate(item.dropPrefab, _spawnPos, Quaternion.identity);
        Debug.Log(
            $"[아이템 드랍] {_shopData.shopName}에서 {_shopData.dropItems[0].itemName} 아이템 드랍!"
        );
    }

    private Vector3 GetFrontSpawnPosition()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (var r in GetComponentsInChildren<Renderer>())
            bounds.Encapsulate(r.bounds);

        Vector3 fwd = transform.forward;
        float halfExtent = Vector3.Dot(
            bounds.extents,
            new Vector3(Mathf.Abs(fwd.x), 0f, Mathf.Abs(fwd.z))
        );

        Vector3 frontEdge =
            new Vector3(bounds.center.x, 0.1f, bounds.center.z) + fwd * halfExtent;

        return frontEdge + fwd * Random.Range(0.5f, 1.5f) + transform.right * Random.Range(-1f, 1f);
    }
}
