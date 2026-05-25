using UnityEngine;

public class MissionItem : MonoBehaviour, IInteractive
{
    [SerializeField]
    private ItemData _itemData;

    private PlayerMovement _playerMovement;
    private FollowCamera _camera;

    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        _playerMovement = player.GetComponent<PlayerMovement>();
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowCamera>();

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
            _camera.TriggerReactionCut(0.6f);
            _playerMovement.SetFaceHappy();

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
