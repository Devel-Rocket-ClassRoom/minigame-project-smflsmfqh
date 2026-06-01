using System.Collections;
using System.Collections.Generic;
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

    [Header("파티클")]
    [SerializeField]
    private ParticleSystem _spotParticle;

    [Header("드랍 쿨타임")]
    [SerializeField]
    private float _cooldown = 3f;

    private List<ItemData> _remainingItems = new();
    private Collider _collider;
    private Vector3 _spawnPos;

    private void Start()
    {
        _collider = GetComponent<Collider>();

        if (_shopData.dropItems != null)
            _remainingItems = new List<ItemData>(_shopData.dropItems);

        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionAssigned += HandleMissionAssigned;

        if (_spotParticle != null)
        {
            _spotParticle.transform.position = GetParticlePosition();
            StopParticle();
        }
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionAssigned -= HandleMissionAssigned;
    }

    private void HandleMissionAssigned(ItemData data)
    {
        if (_spotParticle == null)
            return;

        if (_remainingItems.Exists(item => item.itemName == data.itemName))
            PlayParticle();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_remainingItems.Count == 0)
        {
            Debug.LogWarning($"[ShopBuilding] {_shopData.shopName}: 드랍할 아이템이 없습니다.");
            return;
        }

        ItemData selected = _remainingItems[Random.Range(0, _remainingItems.Count)];

        if (
            selected.category == ItemCategory.Mission
            && (
                MissionManager.Instance == null
                || !MissionManager.Instance.IsMissionAssigned(selected.itemName)
            )
        )
            return;

        _remainingItems.Remove(selected);

        _spawnPos = GetFrontSpawnPosition();
        _spawnPos.y += selected.spawnHeightOffset;
        var rot = Quaternion.Euler(selected.spawnRotation);
        Instantiate(selected.dropPrefab, _spawnPos, rot);

        Debug.Log(
            $"[아이템 드랍] {_shopData.shopName}에서 {selected.itemName} 드랍! (남은 아이템: {_remainingItems.Count}개)"
        );

        if (!HasAvailableItem() && _spotParticle != null)
            StopParticle();

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(_cooldown);

        if (_remainingItems.Count > 0)
            _collider.enabled = true;
    }

    private bool HasAvailableItem()
    {
        foreach (var item in _remainingItems)
        {
            if (item.category != ItemCategory.Mission)
                return true;

            if (
                MissionManager.Instance != null
                && MissionManager.Instance.IsMissionAssigned(item.itemName)
            )
                return true;
        }

        return false;
    }

    private void PlayParticle()
    {
        _spotParticle.gameObject.SetActive(true);
        _spotParticle.Play();
    }

    private void StopParticle()
    {
        _spotParticle.gameObject.SetActive(false);
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

    private Vector3 GetParticlePosition()
    {
        var col = GetComponent<BoxCollider>();
        if (col == null)
            return transform.position;

        Vector3 center = transform.TransformPoint(col.center);
        Vector3 outDir = center - transform.position;
        outDir.y = 0f;
        outDir = outDir.sqrMagnitude > 0.001f ? outDir.normalized : transform.forward;

        Vector3 scale = transform.lossyScale;
        float edgeDist =
            Mathf.Abs(Vector3.Dot(transform.right, outDir)) * col.size.x * Mathf.Abs(scale.x) * 0.5f
            + Mathf.Abs(Vector3.Dot(transform.forward, outDir))
                * col.size.z
                * Mathf.Abs(scale.z)
                * 0.5f;

        return center + outDir * edgeDist;
    }
}
