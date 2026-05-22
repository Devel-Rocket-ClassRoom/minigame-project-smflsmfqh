using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCheckListUI : MonoBehaviour
{
    [SerializeField]
    private MissionMessageUI _missionMessageUI;

    [SerializeField]
    private GameObject _listPanel;

    [SerializeField]
    private GameObject _entryPrefab;

    [SerializeField]
    private Transform _entryContainer;

    private readonly List<MissionEntryUI> _entries = new();
    private bool _isOpen = false;
    private Coroutine _peekCo;

    private void Awake()
    {
        _listPanel.SetActive(false);
    }

    private void OnEnable()
    {
        _missionMessageUI.OnSlidedOut += HandleSlidedOut;
        MissionManager.Instance.OnMissionCompleted += HandleMissionCompleted;
        PlayerController.OnMissionTogglePressed += ToggleList;
    }

    private void OnDisable()
    {
        _missionMessageUI.OnSlidedOut -= HandleSlidedOut;
        MissionManager.Instance.OnMissionCompleted -= HandleMissionCompleted;
        PlayerController.OnMissionTogglePressed -= ToggleList;
    }

    private void HandleSlidedOut(ItemData item)
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
        {
            _listPanel.SetActive(false);
        }
    }
}
