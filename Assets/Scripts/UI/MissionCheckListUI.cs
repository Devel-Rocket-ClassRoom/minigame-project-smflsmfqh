using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCheckListUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _listPanel;

    [SerializeField]
    private GameObject _entryPrefab;

    [SerializeField]
    private Transform _entryContainer;

    [SerializeField]
    private PlayerController _playerController;

    [SerializeField]
    private MissionMessageUI _missionMessageUI;

    private readonly List<MissionEntryUI> _entries = new();
    private bool _isOpen = false;
    private Coroutine _peekCo;

    private void Awake()
    {
        _listPanel.SetActive(false);
    }

    private void OnEnable()
    {
        MissionManager.Instance.OnMissionCompleted += HandleMissionCompleted;
        if (_playerController != null)
            _playerController.OnMissionTogglePressed += ToggleList;
        if (_missionMessageUI != null)
            _missionMessageUI.OnSlidedIn += HandleSlidedIn;
    }

    private void OnDisable()
    {
        MissionManager.Instance.OnMissionCompleted -= HandleMissionCompleted;
        if (_playerController != null)
            _playerController.OnMissionTogglePressed -= ToggleList;
        if (_missionMessageUI != null)
            _missionMessageUI.OnSlidedIn -= HandleSlidedIn;
    }

    // 미션 메시지가 슬라이딩 인 될 때 항목 추가 + 리스트 잠깐 표시
    private void HandleSlidedIn(ItemData item)
    {
        if (item == null)
            return; // 분노·힌트·독백 메시지는 무시
        AddEntry(item);
        Peek();
    }

    private void AddEntry(ItemData item)
    {
        var go = Instantiate(_entryPrefab, _entryContainer);
        var entry = go.GetComponent<MissionEntryUI>();
        entry.Init(item);
        _entries.Add(entry);
    }

    private void HandleMissionCompleted(ItemData item)
    {
        var entry = _entries.Find(e => e.ItemName == item.itemName);
        entry?.Complete();
        Peek();
    }

    public void Peek()
    {
        if (_peekCo != null)
            StopCoroutine(_peekCo);
        _peekCo = StartCoroutine(PeekCoroutine());
    }

    private void ToggleList()
    {
        _isOpen = !_isOpen;
        _listPanel.SetActive(_isOpen);
    }

    private IEnumerator PeekCoroutine()
    {
        _listPanel.SetActive(true);
        yield return new WaitForSeconds(3f);
        if (!_isOpen)
            _listPanel.SetActive(false);
    }
}
