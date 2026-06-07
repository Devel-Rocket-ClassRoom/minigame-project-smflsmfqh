using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum Language
{
    En,
    Ko,
}

public class StringTableManager
{
    private static StringTableManager _instance;
    public static StringTableManager Instance => _instance ??= new StringTableManager();

    private static readonly System.Collections.Generic.Dictionary<Language, string> _languageFiles =
        new() { { Language.En, "en" }, { Language.Ko, "ko" } };

    private Dictionary<string, string> _stringTable = new Dictionary<string, string>();
    private Dictionary<string, string> _senderTable = new Dictionary<string, string>();
    private Dictionary<string, string> _senderImageMap = new Dictionary<string, string>();

    public Language CurrentLanguage { get; private set; } = Language.En;
    public event Action OnLanguageChanged;

    public void SetLanguage(Language language)
    {
        string fileName = _languageFiles[language];
        var csv = Resources.Load<TextAsset>($"Data/{fileName}");
        if (csv == null)
            return;
        Load(csv);
        CurrentLanguage = language;
        OnLanguageChanged?.Invoke();
    }

    public void Load(TextAsset csv)
    {
        _stringTable.Clear();
        _senderTable.Clear();
        _senderImageMap.Clear();

        var lines = csv.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            var match = Regex.Match(line, @"^([^,]+),""([^""]*)""(?:,([^,]*)(?:,(.*))?)?$");
            if (match.Success)
            {
                string key = match.Groups[1].Value;
                _stringTable[key] = match.Groups[2].Value;

                if (match.Groups[3].Success)
                {
                    string sender = match.Groups[3].Value.Trim();
                    if (!string.IsNullOrEmpty(sender))
                    {
                        _senderTable[key] = sender;
                        if (match.Groups[4].Success)
                        {
                            string imageKey = match.Groups[4].Value.Trim();
                            if (!string.IsNullOrEmpty(imageKey))
                                _senderImageMap[sender] = imageKey;
                        }
                    }
                }
            }
        }
    }

    public string GetImageKeyBySender(string sender)
    {
        return _senderImageMap.TryGetValue(sender, out var key) ? key : sender;
    }

    public string GetDeathMessage(CauseDeath cause)
    {
        string prefix = $"DEATH_{cause.ToString().ToUpper()}";
        var candidates = new List<string>();

        foreach (var kv in _stringTable)
        {
            if (kv.Key.StartsWith(prefix))
                candidates.Add(kv.Value);
        }

        if (candidates.Count == 0)
            return string.Empty;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    public (string message, string sender) GetMissionMessage(string itemName)
    {
        string key = $"MISSION_{itemName}";
        string message = _stringTable.TryGetValue(key, out var msg) ? msg : string.Empty;
        string sender = _senderTable.TryGetValue(key, out var s) ? s : string.Empty;
        return (message, sender);
    }

    public (string message, string sender) GetMessage(string key)
    {
        string message = _stringTable.TryGetValue(key, out var msg) ? msg : string.Empty;
        string sender = _senderTable.TryGetValue(key, out var s) ? s : string.Empty;
        return (message, sender);
    }

    public (string message, string sender) GetAngerMessage(float thresholdPercent)
    {
        string key = $"ANGER_{(int)thresholdPercent}";
        string message = _stringTable.TryGetValue(key, out var msg) ? msg : string.Empty;
        string sender = _senderTable.TryGetValue(key, out var s) ? s : string.Empty;
        return (message, sender);
    }

    public string GetItemDisplayName(string itemName)
    {
        string key = $"ITEM_{itemName.ToUpper()}";
        return _stringTable.TryGetValue(key, out var name) ? name : itemName;
    }

    public string GetPreMonologueKey(string itemName)
    {
        string key = $"MONOLOGUE_{itemName.ToUpper()}";
        return _stringTable.ContainsKey(key) ? key : string.Empty;
    }
}
