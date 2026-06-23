using System;

[Serializable]
public class ScoreData
{
    public float playtime;
    public bool gameclear;
    public int damagedNPC;
    public int damagedCat;
    public int damagedCar;
    public long timestamp;

    public ScoreData() { }

    public ScoreData(float playtime, int damagedNPC, int damagedCar, int damagedCat, bool gameclear, long timestamp)
    {
        this.playtime = playtime;
        this.gameclear = gameclear;
        this.damagedNPC = damagedNPC;
        this.damagedCar = damagedCar;
        this.damagedCat = damagedCat;
        this.timestamp = timestamp;
    }

    public DateTime GetDateTime()
    {
        return TimeUtil.FromUnixMillis(timestamp);
    }

    public string GetDateString()
    {
        return TimeUtil.ToDateString(timestamp);
    }
}
