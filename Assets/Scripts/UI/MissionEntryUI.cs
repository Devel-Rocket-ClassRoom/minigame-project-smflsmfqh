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

    public string ItemName => _item.itemName;

    private ItemData _item;

    public void Init(ItemData item)
    {
        _item = item;
        _missionNameText.text = item.itemName;
        _checkIcon.gameObject.SetActive(true);
        _strikeThrough.SetActive(false);
    }

    public void Complete()
    {
        _checkIcon.color = Color.red;
        _strikeThrough.SetActive(true);
    }
}
