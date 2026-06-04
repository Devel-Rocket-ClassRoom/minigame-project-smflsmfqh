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
    private MinimapMarker _pharmacyMarker;

    [SerializeField]
    private TriggerZone _pharmacyZone;

    [Header("분노 시스템")]
    [SerializeField]
    private AngerSystem _angerSystem;

    [Header("튜토리얼 고양이")]
    [SerializeField]
    private CatMovement _catPrefab;

    [SerializeField]
    private float _catSpawnDistance = 6f;

    [SerializeField]
    private float _catLifetime = 8f;

    [Header("플레이어")]
    [SerializeField]
    private PlayerHealth _player;

    [SerializeField]
    private SphereCollider _movementZone;

    public bool IsActive => _active;

    private bool _active = true;
    private Coroutine _mainCo;

    private static readonly float FastSlide   = 4f / 60f;
    private static readonly float NormalSlide = 0.3f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (_directionArrowParticle != null)
            _directionArrowParticle.gameObject.SetActive(false);

        // ANGER 메시지가 Start()에서 발화되기 전에 정지 + 빠른 슬라이드 설정
        Time.timeScale = 0f;
        _messageUI?.SetSlideDuration(FastSlide);
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
            MissionManager.Instance.OnMissionAssigned  -= HandleMissionAssigned;
            MissionManager.Instance.OnMissionDisplayed -= HandleMissionDisplayed;
        }
    }

    private void HandleMissionAssigned(ItemData item)
    {
        if (item == null || item.itemName.ToUpper() != "ENERGYDRINK")
            return;
        MissionManager.Instance.OnMissionAssigned -= HandleMissionAssigned;
        MissionManager.Instance.PauseMissionAssignment();
        if (_angerSystem != null)
            _angerSystem.Pause();
        if (_player != null)
            _player.SetTutorialInvincible(true);
    }

    private void HandleMissionDisplayed(ItemData item)
    {
        if (item == null || item.itemName.ToUpper() != "ENERGYDRINK")
            return;
        MissionManager.Instance.OnMissionDisplayed -= HandleMissionDisplayed;

        // 약국 미션 슬라이드 인 완료 후 시간 재개 (NPC 이동, AngerSystem/MissionManager는 Pause 유지)
        Time.timeScale = 1f;
        _messageUI?.SetSlideDuration(NormalSlide);
        _mainCo = StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        // 스킵 안내
        yield return EnqueueAndWait("TUT_SKIP");

        // 1. WASD 이동 설명
        yield return EnqueueAndWait("TUT_WASD");

        // 2. 이동 체험 — 구 콜라이더 반경 벗어나면 다음 단계
        Vector3 startPos = _player != null ? _player.transform.position : Vector3.zero;
        float radius = _movementZone != null
            ? _movementZone.radius * _movementZone.transform.lossyScale.x
            : 3f;
        yield return new WaitUntil(() =>
            _player != null
            && Vector3.Distance(_player.transform.position, startPos) >= radius
        );

        // 3. 마우스 회전 + Shift/Space/R 설명
        yield return EnqueueAndWait("TUT_MOUSE");
        yield return EnqueueAndWait("TUT_CONTROLS");

        // 4. CAT_WARNING 슬라이드 인 시점에 고양이 스폰
        bool catDone = false;
        _messageUI.EnqueueTutorialMessage("TUT_CAT_WARNING", () =>
        {
            SpawnTutorialCat();
            catDone = true;
        });
        yield return new WaitUntil(() => catDone);

        // 5. 맵 지도 + 화살표 설명
        if (_pharmacyMarker != null)
            MinimapUI.Instance?.PingMarker(_pharmacyMarker);
        yield return EnqueueAndWait("TUT_MAP");

        if (_directionArrowParticle != null)
        {
            _directionArrowParticle.gameObject.SetActive(true);
            _directionArrowParticle.Play();
        }
        yield return EnqueueAndWait("TUT_ARROW");

        // 6. 약국 앞 콜라이더 진입 대기 (이미 진입했으면 즉시 통과)
        if (_pharmacyZone != null && !_pharmacyZone.Triggered)
        {
            bool entered = false;
            _pharmacyZone.OnPlayerEntered += () => entered = true;
            yield return new WaitUntil(() => entered);
        }

        // 7. 약국 안내
        yield return EnqueueAndWait("TUT_ITEM_GLOW");
        yield return EnqueueAndWait("TUT_ITEM_PICKUP");
        _checkListUI?.Peek();
        yield return EnqueueAndWait("TUT_TOGGLE");
        yield return EnqueueAndWait("TUT_GOAL");

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
        if (_catPrefab == null || _player == null)
            return;

        Vector3 dir = (_player.transform.forward + Random.insideUnitSphere * 0.5f).normalized;
        Vector3 origin = _player.transform.position + dir * _catSpawnDistance;
        origin.y = _player.transform.position.y;

        if (NavMesh.SamplePosition(origin, out NavMeshHit hit, _catSpawnDistance, NavMesh.AllAreas))
            origin = hit.position;

        var cat = Instantiate(_catPrefab, origin, Quaternion.identity);
        cat.MarkAsTutorialCat();
        cat.SetPlayer(_player);
        StartCoroutine(DestroyCatAfter(cat));
    }

    private IEnumerator DestroyCatAfter(CatMovement cat)
    {
        yield return new WaitForSeconds(_catLifetime);
        if (cat != null)
            Destroy(cat.gameObject);
    }

    public void Skip()
    {
        if (_mainCo != null)
            StopCoroutine(_mainCo);
        Complete();
    }

    private void Complete()
    {
        _active = false;
        Time.timeScale = 1f;
        _messageUI?.SetSlideDuration(NormalSlide);
        if (_player != null)
            _player.SetTutorialInvincible(false);
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.ResumeMissionAssignment();
            MissionManager.Instance.ForceAssignNext();
        }
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
