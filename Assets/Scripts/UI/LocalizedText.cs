using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string _csvKey;

    private TMP_Text _text;

    private void Awake() => _text = GetComponent<TMP_Text>();

    private void OnEnable()
    {
        StringTableManager.Instance.OnLanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        StringTableManager.Instance.OnLanguageChanged -= Refresh;
    }

    private void Refresh()
    {
        var (msg, _) = StringTableManager.Instance.GetMessage(_csvKey);
        if (!string.IsNullOrEmpty(msg))
            _text.text = msg;
    }
}
