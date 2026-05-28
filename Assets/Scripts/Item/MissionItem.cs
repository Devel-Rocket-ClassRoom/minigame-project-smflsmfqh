using System;
using System.Collections;
using UnityEngine;

public class MissionItem : MonoBehaviour, IInteractive
{
    [SerializeField]
    private ItemData _itemData;

    [HideInInspector]
    public bool IsPooled = false;

    [HideInInspector]
    public float LifeTime = 0f;
    public event Action<MissionItem> OnDeactivated;

    private PlayerMovement _playerMovement;
    private FollowCamera _camera;
    private Coroutine _lifeTimer;

    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        _playerMovement = player.GetComponent<PlayerMovement>();
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowCamera>();

        if (_itemData != null)
            SetupMinimapMarker();
    }

    public void Init(ItemData data)
    {
        _itemData = data;
        SetupMinimapMarker();
    }

    private void SetupMinimapMarker()
    {
        if (!TryGetComponent<MinimapMarker>(out MinimapMarker marker))
        {
            marker = gameObject.AddComponent<MinimapMarker>();
            marker.type = MinimapMarker.MarkerType.Item;
        }

        marker.colorOverride =
            _itemData.category == ItemCategory.Mission ? Color.red : Color.magenta;
    }

    private void OnEnable()
    {
        if (IsPooled && LifeTime > 0f)
            _lifeTimer = StartCoroutine(AutoDeactivate());
    }

    private void OnDisable()
    {
        if (_lifeTimer != null)
        {
            StopCoroutine(_lifeTimer);
            _lifeTimer = null;
        }
    }

    private IEnumerator AutoDeactivate()
    {
        yield return new WaitForSeconds(LifeTime);
        Deactivate();
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

        if (IsPooled)
            Deactivate();
        else
            Destroy(gameObject);
    }

    public string GetItemName() => _itemData.itemName;

    private void Deactivate()
    {
        OnDeactivated?.Invoke(this);
        gameObject.SetActive(false);
    }
}
