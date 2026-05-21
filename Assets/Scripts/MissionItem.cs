using UnityEngine;

public class MissionItem : MonoBehaviour, IInteractive
{
    [SerializeField]
    private ItemData _itemData;

    private void Awake()
    {
        // 프리팹에 MinimapMarker가 없을 경우 런타임에 자동 추가
        if (!TryGetComponent<MinimapMarker>(out _))
        {
            var marker = gameObject.AddComponent<MinimapMarker>();
            marker.type = MinimapMarker.MarkerType.Item;
        }
    }

    public void Interact(PlayerController player)
    {
        foreach (var effect in _itemData.effects)
        {
            effect.Apply(player);
            Debug.Log($"[아이템 효과 발동] 아이템: {_itemData.itemName}");
        }

        if (_itemData.category == ItemCategory.Mission)
        {
            MissionManager.Instance.ReportCollected(_itemData.itemName);
            GameManager.Instance.AddScore(100);
        }

        Destroy(gameObject);
    }

    public string GetItemName() => _itemData.itemName;
}
