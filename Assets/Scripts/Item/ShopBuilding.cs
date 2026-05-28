using UnityEngine;

public class ShopBuilding : MonoBehaviour
{
    [SerializeField]
    private ShopData _shopData;

    [SerializeField]
    private LayerMask _groundMask;

    [SerializeField]
    private float _spreadHalfAngle = 30f;

    [SerializeField]
    private float _dropDistanceMin = 0.4f;

    [SerializeField]
    private float _dropDistanceMax = 0.8f;

    private Vector3 _spawnPos;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_shopData.dropItems == null || _shopData.dropItems.Length == 0)
        {
            Debug.LogWarning($"[ShopBuilding] {_shopData.shopName}: dropItems가 비어있습니다.");
            return;
        }

        ItemData selected = _shopData.dropItems[Random.Range(0, _shopData.dropItems.Length)];
        _spawnPos = GetFrontSpawnPosition();
        _spawnPos.y += selected.spawnHeightOffset;
        var rot = Quaternion.Euler(selected.spawnRotation);
        Instantiate(selected.dropPrefab, _spawnPos, rot);

        Debug.Log($"[아이템 드랍] {_shopData.shopName}에서 {selected.itemName} 아이템 드랍!");
    }

    private Vector3 GetFrontSpawnPosition()
    {
        var col = GetComponent<BoxCollider>();
        Vector3 center = col != null ? transform.TransformPoint(col.center) : transform.position;

        Vector3 outDir = center - transform.position;
        outDir.y = 0f;
        outDir = outDir.sqrMagnitude > 0.001f ? outDir.normalized : transform.forward;

        float angle = Random.Range(-_spreadHalfAngle, _spreadHalfAngle);
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * outDir;
        Vector3 horizontal = center + dir * Random.Range(_dropDistanceMin, _dropDistanceMax);

        Vector3 origin = new Vector3(horizontal.x, center.y + 3f, horizontal.z);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 6f, _groundMask))
            return hit.point + Vector3.up * 0.01f;

        return new Vector3(horizontal.x, 0.01f, horizontal.z);
    }
}
