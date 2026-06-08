using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionEntryUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _missionNameText;

    [SerializeField]
    private Image _checkIcon;

    [SerializeField]
    private GameObject _strikeThrough;

    public string ItemName => _item?.itemName;

    private ItemData _item;

    private void OnEnable()
    {
        StringTableManager.Instance.OnLanguageChanged += RefreshText;
    }

    private void OnDisable()
    {
        StringTableManager.Instance.OnLanguageChanged -= RefreshText;
    }

    public void Init(ItemData item)
    {
        _item = item;
        RefreshText();
        _checkIcon.gameObject.SetActive(true);
        _strikeThrough.SetActive(false);
    }

    private void RefreshText()
    {
        if (_item == null)
            return;
        _missionNameText.text = StringTableManager.Instance.GetItemDisplayName(_item.itemName);
    }

    public void Complete()
    {
        _checkIcon.color = Color.red;
        _strikeThrough.SetActive(true);
    }
}
