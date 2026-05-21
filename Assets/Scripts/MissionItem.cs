using UnityEngine;

public class MissionItem : MonoBehaviour, IInteractive
{
    [SerializeField]
    private ItemData _itemData;

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
