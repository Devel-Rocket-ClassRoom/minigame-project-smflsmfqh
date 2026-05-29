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
    private MinimapMarker _minimapMarker;

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
        if (!TryGetComponent(out _minimapMarker))
        {
            _minimapMarker = gameObject.AddComponent<MinimapMarker>();
            _minimapMarker.type = MinimapMarker.MarkerType.Item;
        }

        _minimapMarker.colorOverride = _itemData.category switch
        {
            ItemCategory.Mission => Color.red,
            ItemCategory.Optional => Color.yellow,
            _ => Color.magenta,
        };

        // Optional 아이템은 미션 해제 전까지 미니맵에 표시하지 않음
        if (_itemData.category == ItemCategory.Optional)
            _minimapMarker.enabled = false;
    }

    private void OnEnable()
    {
        if (IsPooled && LifeTime > 0f)
            _lifeTimer = StartCoroutine(AutoDeactivate());

        if (_itemData != null && _itemData.category == ItemCategory.Optional
            && MissionManager.Instance != null)
            MissionManager.Instance.OnOptionalMissionUnlocked += ShowOnMinimap;
    }

    private void OnDisable()
    {
        if (_lifeTimer != null)
        {
            StopCoroutine(_lifeTimer);
            _lifeTimer = null;
        }

        if (_itemData != null && _itemData.category == ItemCategory.Optional
            && MissionManager.Instance != null)
            MissionManager.Instance.OnOptionalMissionUnlocked -= ShowOnMinimap;
    }

    private void ShowOnMinimap()
    {
        if (_minimapMarker != null)
            _minimapMarker.enabled = true;
    }

    private IEnumerator AutoDeactivate()
    {
        yield return new WaitForSeconds(LifeTime);
        Deactivate();
    }

    public void Interact(PlayerController player)
    {
        // Optional 미션은 독백(선택 미션 해제) 이후에만 픽업 가능
        if (_itemData.category == ItemCategory.Optional && !MissionManager.Instance.IsOptionalUnlocked)
            return;

        foreach (var effect in _itemData.effects)
        {
            effect.Apply(player);
            _camera.TriggerReactionCut(0.6f);
            _playerMovement.SetFaceHappy();
        }

        if (_itemData.category == ItemCategory.Mission)
        {
            MissionManager.Instance.ReportCollected(_itemData.itemName);
            GameManager.Instance.AddScore(100);
        }
        else if (_itemData.category == ItemCategory.Optional)
        {
            MissionManager.Instance.ReportCollected(_itemData.itemName);
            GameManager.Instance.AddScore(300);
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
