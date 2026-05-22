using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class StringTableManager
{
    private static StringTableManager _instance;
    public static StringTableManager Instance => _instance ??= new StringTableManager();

    private Dictionary<string, string> _stringTable = new Dictionary<string, string>();
    private Dictionary<string, string> _senderTable = new Dictionary<string, string>();

    public void Load(TextAsset csv)
    {
        _stringTable.Clear();
        _senderTable.Clear();
        
        var lines = csv.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            var match = Regex.Match(line, @"^([^,]+),""([^""]*)""(?:,(.+))?$");
            if (match.Success)
            {
                string key = match.Groups[1].Value;
                _stringTable[key] = match.Groups[2].Value;
                if (match.Groups[3].Success)
                    _senderTable[key] = match.Groups[3].Value.Trim();
            }
        }
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

        return candidates[Random.Range(0, candidates.Count)];
    }

    public (string message, string sender) GetMissionMessage(string itemName)
    {
        string key = $"MISSION_{itemName}";
        string message = _stringTable.TryGetValue(key, out var msg) ? msg : string.Empty;
        string sender = _senderTable.TryGetValue(key, out var s) ? s : string.Empty;
        return (message, sender);
    }

}
