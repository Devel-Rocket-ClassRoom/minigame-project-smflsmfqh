using System;

[Serializable]
public class LeaderboardEntry
{
    public string userId;
    public string nickname;
    public float playtime;
    public long timestamp;

    public LeaderboardEntry()
    {
    }

    public LeaderboardEntry(string userId, string nickname, float playtime, long timestamp)
    {
        this.userId = userId;
        this.nickname = nickname;
        this.playtime = playtime;
        this.timestamp = timestamp;
    }

    public string ToJson()
    {
        return UnityEngine.JsonUtility.ToJson(this);
    }

    public static LeaderboardEntry FromJson(string json)
    {
        return UnityEngine.JsonUtility.FromJson<LeaderboardEntry>(json);
    }
}
