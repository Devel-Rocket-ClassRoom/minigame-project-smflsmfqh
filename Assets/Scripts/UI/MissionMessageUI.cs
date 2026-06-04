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

    [Header("오디오")]
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private AudioClip _slideInSound;

    // 슬라이드 완료 직후 발화 — 미션 메시지일 때만 ItemData가 non-null
    public event Action<ItemData> OnSlidedIn;

    private readonly struct MessageData
    {
        public readonly string Message;
        public readonly string Sender;
        public readonly ItemData Item; // 분노·힌트·독백 메시지는 null
        public readonly System.Action OnSlidedInCallback;

        public MessageData(string message, string sender, ItemData item = null, System.Action onSlidedInCallback = null)
        {
            Message = message;
            Sender = sender;
            Item = item;
            OnSlidedInCallback = onSlidedInCallback;
        }
    }

    private float _panelWidth;
    private float _currentSlideDuration = 0.3f;
    private Coroutine _slideCo;
    private readonly Queue<MessageData> _queue = new();

    public void SetSlideDuration(float duration) => _currentSlideDuration = duration;

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
            return;
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
        {
            var flowerItem = MissionManager.Instance?.OptionalMission;
            Debug.Log($"[Flower] HandleMonologue: flowerItem={flowerItem?.itemName ?? "NULL"}");
            Enqueue(new MessageData(flowerMsg, flowerSender, flowerItem));
        }
    }

    private void HandleAngerMessage((string, string) data)
    {
        Enqueue(new MessageData(data.Item1, data.Item2));
    }

    public void EnqueueTutorialMessage(string csvKey, System.Action onSlidedIn = null)
    {
        var (message, sender) = StringTableManager.Instance.GetMessage(csvKey);
        if (string.IsNullOrEmpty(message)) return;
        Enqueue(new MessageData(message, sender, null, onSlidedIn));
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
            {
                string imageKey = StringTableManager.Instance.GetImageKeyBySender(data.Sender);
                _icon.sprite = Resources.Load<Sprite>(imageKey);
            }

            if (_audioSource != null && _slideInSound != null)
                _audioSource.PlayOneShot(_slideInSound);
            yield return StartCoroutine(Slide(-_panelWidth, 10f, _currentSlideDuration));
            OnSlidedIn?.Invoke(data.Item);
            data.OnSlidedInCallback?.Invoke();
            if (data.Item != null)
                MissionManager.Instance?.NotifyMissionDisplayed(data.Item);
            yield return new WaitForSecondsRealtime(_displayDuration);
            yield return StartCoroutine(Slide(0f, -_panelWidth, _currentSlideDuration));
        }

        _slideCo = null;
    }

    private IEnumerator Slide(float fromX, float toX, float duration)
    {
        float elapsed = 0f;
        float y = _panel.anchoredPosition.y;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _panel.anchoredPosition = new Vector2(Mathf.Lerp(fromX, toX, t), y);
            yield return null;
        }

        _panel.anchoredPosition = new Vector2(toX, y);
    }
}
