using System;
using System.Collections;
using System.Collections.Generic;
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
    private float _displayDuration = 7f;

    [SerializeField]
    private AngerSystem _angerSystem;

    // 슬라이드 완료 직후 발화 — 미션 메시지일 때만 ItemData가 non-null
    public event Action<ItemData> OnSlidedIn;

    private readonly struct MessageData
    {
        public readonly string Message;
        public readonly string Sender;
        public readonly ItemData Item; // 분노·힌트·독백 메시지는 null

        public MessageData(string message, string sender, ItemData item = null)
        {
            Message = message;
            Sender = sender;
            Item = item;
        }
    }

    private float _panelWidth;
    private Coroutine _slideCo;
    private readonly Queue<MessageData> _queue = new();

    private void Awake()
    {
        _panelWidth = _panel.sizeDelta.x;
        _panel.anchoredPosition = new Vector2(-_panelWidth, _panel.anchoredPosition.y);
    }

    private void OnEnable()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionAssigned += HandleMissionAssigned;
            MissionManager.Instance.OnHintAssigned += HandleHint;
            MissionManager.Instance.OnOptionalMissionUnlocked += HandleMonologue;
        }
        if (_angerSystem != null)
            _angerSystem.OnMessasgeTriggered += HandleAngerMessage;
    }

    private void OnDisable()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionAssigned -= HandleMissionAssigned;
            MissionManager.Instance.OnHintAssigned -= HandleHint;
            MissionManager.Instance.OnOptionalMissionUnlocked -= HandleMonologue;
        }
        if (_angerSystem != null)
            _angerSystem.OnMessasgeTriggered -= HandleAngerMessage;
    }

    private void HandleMissionAssigned(ItemData itemData)
    {
        var (message, sender) = StringTableManager.Instance.GetMissionMessage(itemData.itemName);
        if (string.IsNullOrEmpty(message))
            return; // 버섯 등 메시지 없는 미션은 무시
        Enqueue(new MessageData(message, sender, itemData));
    }

    private void HandleHint(string message, string sender)
    {
        if (string.IsNullOrEmpty(message))
            return;
        Enqueue(new MessageData(message, sender));
    }

    private void HandleMonologue()
    {
        var (homeMsg, homeSender) = StringTableManager.Instance.GetMessage("MONOLOGUE_HOME");
        if (!string.IsNullOrEmpty(homeMsg))
            Enqueue(new MessageData(homeMsg, homeSender));

        var (waitMsg, waitSender) = StringTableManager.Instance.GetMessage("MONOLOGUE_WAIT");
        if (!string.IsNullOrEmpty(waitMsg))
            Enqueue(new MessageData(waitMsg, waitSender));

        var (flowerMsg, flowerSender) = StringTableManager.Instance.GetMessage("MONOLOGUE_FLOWER");
        if (!string.IsNullOrEmpty(flowerMsg))
            Enqueue(new MessageData(flowerMsg, flowerSender));
    }

    private void HandleAngerMessage((string, string) data)
    {
        Enqueue(new MessageData(data.Item1, data.Item2));
    }

    private void Enqueue(MessageData data)
    {
        _queue.Enqueue(data);
        if (_slideCo == null)
            _slideCo = StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        while (_queue.Count > 0)
        {
            var data = _queue.Dequeue();

            _message.text = data.Message;
            _senderName.text = data.Sender;
            if (!string.IsNullOrEmpty(data.Sender))
                _icon.sprite = Resources.Load<Sprite>(data.Sender);

            yield return StartCoroutine(Slide(-_panelWidth, 10f, 0.3f));
            OnSlidedIn?.Invoke(data.Item);
            yield return new WaitForSeconds(_displayDuration);
            yield return StartCoroutine(Slide(0f, -_panelWidth, 0.3f));
        }

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
