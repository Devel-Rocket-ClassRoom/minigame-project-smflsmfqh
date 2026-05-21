using System.Collections;
using GLTFast.Schema;
using TMPro;
using UnityEngine;

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

    private float _panelWidth;
    private bool _isPlaying;
    private Coroutine _slideCo;

    private void Awake()
    {
        _panelWidth = _panel.sizeDelta.x;
        _panel.anchoredPosition = new Vector2(0f, _panel.anchoredPosition.y);
    }

    private void OnEnable()
    {
        MissionManager.Instance.OnMissionAssigned += HandleMissionAssigned;
    }

    private void OnDisable()
    {
        MissionManager.Instance.OnMissionAssigned -= HandleMissionAssigned;
    }

    private void HandleMissionAssigned(ItemData itemData)
    {
        string message = StringTableManager.Instance.GetMissionMessage(itemData.itemName);

        _message.text = message;

        if (_slideCo != null)
        {
            StopCoroutine(_slideCo);
        }
        _slideCo = StartCoroutine(SlideCoroutine());
    }

    private IEnumerator SlideCoroutine()
    {
        yield return StartCoroutine(Slide(0f, -_panelWidth, 0.3f));
        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(Slide(-_panelWidth, 0f, 0.3f));
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
