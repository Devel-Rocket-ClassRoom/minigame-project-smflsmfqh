using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField]
    private MissionMessageUI _messageUI;

    [SerializeField]
    private MissionCheckListUI _checkListUI;

    [SerializeField]
    private ParticleSystem _directionArrowParticle;

    [Header("약국")]
    [SerializeField]
    private ShopBuilding _pharmacyShop;

    [SerializeField]
    private MinimapMarker _pharmacyMarker;

    [Header("분노 시스템")]
    [SerializeField]
    private AngerSystem _angerSystem;

    [Header("튜토리얼 고양이")]
    [SerializeField]
    private CatMovement _catPrefab;

    [SerializeField]
    private PlayerHealth _player;

    [SerializeField]
    private float _catSpawnDistance = 6f;

    [SerializeField]
    private float _catLifetime = 8f;

    private bool _active = true;
    private Coroutine _mainCo;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        MissionManager.Instance.OnMissionAssigned  += HandleMissionAssigned;
        MissionManager.Instance.OnMissionDisplayed += HandleMissionDisplayed;
    }

    private void Update()
    {
        if (!_active)
            return;
        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
            Skip();
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionAssigned -= HandleMissionAssigned;
            MissionManager.Instance.OnMissionDisplayed -= HandleMissionDisplayed;
        }
        UnsubscribeShop();
    }

    private void HandleMissionDisplayed(ItemData item)
    {
        if (item == null || item.itemName.ToUpper() != "ENERGYDRINK")
            return;
        MissionManager.Instance.OnMissionDisplayed -= HandleMissionDisplayed;
        _mainCo = StartCoroutine(RunTutorial());
    }

    private void HandleMissionAssigned(ItemData item)
    {
        if (item == null || item.itemName.ToUpper() != "ENERGYDRINK")
            return;
        MissionManager.Instance.OnMissionAssigned -= HandleMissionAssigned;
        MissionManager.Instance.PauseMissionAssignment();
        if (_angerSystem != null)
            _angerSystem.Pause();
    }

    private IEnumerator RunTutorial()
    {
        if (_player != null)
            _player.SetTutorialInvincible(true);

        if (_pharmacyShop != null)
        {
            _pharmacyShop.OnPlayerEntered += HandlePharmacyEntered;
            _pharmacyShop.OnItemDropped += HandleItemDropped;
        }

        yield return EnqueueAndWait("TUT_MINIMAP1");

        if (_pharmacyMarker != null)
            MinimapUI.Instance?.PingMarker(_pharmacyMarker);
        yield return EnqueueAndWait("TUT_MINIMAP2");

        if (_directionArrowParticle != null)
        {
            _directionArrowParticle.gameObject.SetActive(true);
            _directionArrowParticle.Play();
        }
        yield return EnqueueAndWait("TUT_ARROW");

        yield return EnqueueAndWait("TUT_CONTROLS");
        if (_player != null)
            _player.SetTutorialInvincible(false);

        yield return EnqueueAndWait("TUT_CAT_WARNING");
        SpawnTutorialCat();

        _checkListUI?.Peek();
        yield return EnqueueAndWait("TUT_TOGGLE");
    }

    private void HandlePharmacyEntered()
    {
        if (!_active)
            return;
        _pharmacyShop.OnPlayerEntered -= HandlePharmacyEntered;
        _messageUI.EnqueueTutorialMessage("TUT_ITEM_GLOW");
    }

    private void HandleItemDropped(Transform droppedItem)
    {
        if (!_active)
            return;
        UnsubscribeShop();
        StartCoroutine(WaitNearItemThenPickupHint(droppedItem));
    }

    private IEnumerator WaitNearItemThenPickupHint(Transform item)
    {
        yield return new WaitUntil(() =>
            item != null && Vector3.Distance(_player.transform.position, item.position) < 1.5f
        );
        yield return EnqueueAndWait("TUT_ITEM_PICKUP");
        Complete();
    }

    private IEnumerator EnqueueAndWait(string csvKey)
    {
        bool done = false;
        _messageUI.EnqueueTutorialMessage(csvKey, () => done = true);
        yield return new WaitUntil(() => done);
    }

    private void SpawnTutorialCat()
    {
        if (_catPrefab == null || _player == null) return;

        Vector3 dir = (-_player.transform.forward + Random.insideUnitSphere * 0.5f).normalized;
        Vector3 origin = _player.transform.position + dir * _catSpawnDistance;
        origin.y = _player.transform.position.y;

        if (NavMesh.SamplePosition(origin, out NavMeshHit hit, _catSpawnDistance, NavMesh.AllAreas))
            origin = hit.position;

        var cat = Instantiate(_catPrefab, origin, Quaternion.identity);
        cat.SetPlayer(_player);
        StartCoroutine(DestroyCatAfter(cat));
    }

    private IEnumerator DestroyCatAfter(CatMovement cat)
    {
        yield return new WaitForSeconds(_catLifetime);
        if (cat != null)
            Destroy(cat.gameObject);
    }

    private void UnsubscribeShop()
    {
        if (_pharmacyShop == null)
            return;
        _pharmacyShop.OnPlayerEntered -= HandlePharmacyEntered;
        _pharmacyShop.OnItemDropped -= HandleItemDropped;
    }

    public void Skip()
    {
        if (_mainCo != null)
            StopCoroutine(_mainCo);
        UnsubscribeShop();
        Complete();
    }

    private void Complete()
    {
        _active = false;
        if (_player != null)
            _player.SetTutorialInvincible(false);
        if (MissionManager.Instance != null)
            MissionManager.Instance.ResumeMissionAssignment();
        if (_angerSystem != null)
            _angerSystem.Resume();
        if (_directionArrowParticle != null)
        {
            _directionArrowParticle.Stop();
            _directionArrowParticle.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
