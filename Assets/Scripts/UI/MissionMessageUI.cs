using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MissionMessageUI : MonoBehaviour
{
    [SerializeField]
    private Image _icon;

    [SerializeField]
    private TextMeshProUGUI _senderName;

    [SerializeField]
    private TextMeshProUGUI _message;

    [SerializeField]
    private RectTransform _panel;

    [SerializeField]
    private float _displayDuration = 5f;

    private float _panelWidth;
    private Coroutine _slideCo;
    private ItemData _currentItem;
    public event Action<ItemData> OnSlidedOut;

    private void Awake()
    {
        _panelWidth = _panel.sizeDelta.x;
        _panel.anchoredPosition = new Vector2(-_panelWidth, _panel.anchoredPosition.y);
    }

    private void OnEnable()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionAssigned += HandleMissionAssigned;
    }

    private void OnDisable()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionAssigned -= HandleMissionAssigned;
    }

    private void HandleMissionAssigned(ItemData itemData)
    {
        _currentItem = itemData;
        var (message, sender) = StringTableManager.Instance.GetMissionMessage(itemData.itemName);

        _message.text = message;
        _senderName.text = sender;
        if (!string.IsNullOrEmpty(sender))
            _icon.sprite = Resources.Load<Sprite>(sender);

        if (_slideCo != null)
        {
            StopCoroutine(_slideCo);
        }
        _slideCo = StartCoroutine(SlideCoroutine());
    }

    private IEnumerator SlideCoroutine()
    {
        yield return StartCoroutine(Slide(-_panelWidth, 0f, 0.3f));
        OnSlidedOut?.Invoke(_currentItem);
        yield return new WaitForSeconds(_displayDuration);
        yield return StartCoroutine(Slide(0f, -_panelWidth, 0.3f));

        _slideCo = null;
    }

    private IEnumerator Slide(float fromX, float toX, float duration)
    {
        float elapsed = 0f;
        float y = _panel.anchoredPosition.y;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _panel.anchoredPosition = new Vector2(Mathf.Lerp(fromX, toX, t), y);
            yield return null;
        }

        _panel.anchoredPosition = new Vector2(toX, y);
    }
}
