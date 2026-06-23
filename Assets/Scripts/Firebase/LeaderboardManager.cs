using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    private static LeaderboardManager instance;
    public static LeaderboardManager Instance => instance;

    private DatabaseReference leaderboardRef;
    private Query listenerQuery;

    private bool isListenerActive;
    public bool IsReady => leaderboardRef != null;
    public event Action<List<LeaderboardEntry>> OnLeaderboardUpdated;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async UniTaskVoid Start()
    {
        if (!await FirebaseInitializer.Instance.WaitForInitializationAsync())
        {
            Debug.LogError("[LeaderboardManager] 파이어베이스 초기화 실패...");
            return;
        }

        leaderboardRef = FirebaseInitializer.Instance.Database.RootReference.Child("leaderboard");

    }

    private void OnDestroy()
    {
        StopRealtimeListener();
    }

    public async UniTask<(bool success, string error)> SaveToLeaderboardAsync(float playtime)
    {
        if (!AuthManager.Instance.IsLoggedIn)
            return(false, "로그인이 필요합니다...");
        
        if (leaderboardRef == null)
            return (false, "[leaderboardRef]가 null");

        string userId = AuthManager.Instance.UserId;
        string nickname = ProfileManager.Instance.CachedProfile?.nickname ?? "익명";

        try
        {
            Debug.Log($"[LeaderboardManager] 저장 시도 중...");

            Dictionary<string, object> entryData = new Dictionary<string, object>
            {
                { "userId", userId },
                { "nickname", nickname },
                { "playtime", playtime },
                { "timestamp", ServerValue.Timestamp }
            };

            // UpdateChildrenAsync, 넘긴 딕셔너리에 키에 해당하는 부분만 새로 덮어써줌
            // Set..., 아예 새로 덮어씀
            await leaderboardRef.Child(userId).UpdateChildrenAsync(entryData); 
            Debug.Log($"[LeaderboardManager] 저장 성공");
            return (true, null);

        }
        catch (Exception ex)
        {
            Debug.LogError($"[LeaderboardManager] 저장 실패: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<List<LeaderboardEntry>> LoadLeaderboardAsync(int limit = 10)
    {
        if (leaderboardRef == null)
            return new List<LeaderboardEntry>();

        try
        {
            Debug.Log($"[LeaderboardManager] 로드 시도 중...");

            Query query = leaderboardRef
                .OrderByChild("playtime")
                .LimitToLast(limit);
            DataSnapshot snapshot = await query.GetValueAsync();
                
            List<LeaderboardEntry> leaderboard = ParseEntries(snapshot);

            Debug.Log($"[LeaderboardManager] 로드 성공");
            return leaderboard;
            
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LeaderboardManager] 로드 실패: {ex.Message}");
            return new List<LeaderboardEntry>();
        }
    }

    public List<LeaderboardEntry> ParseEntries(DataSnapshot snapshot)
    {
        List<LeaderboardEntry> list = new List<LeaderboardEntry>();

        if (snapshot.Exists)
        {
            foreach (DataSnapshot child in snapshot.Children)
                list.Add(LeaderboardEntry.FromJson(child.GetRawJsonValue()));
        }

        list.Sort((a, b) => a.playtime.CompareTo(b.playtime));
        return list;
    }

    public async UniTask StartRealtimeListener(int limit = 10)
    {
        if (isListenerActive || leaderboardRef == null)
        {
            Debug.LogError("[LeaderboardManager] 실시간 리스너 시작 및 초기화 실패");
            return;
        }

        Debug.Log("[LeaderboardManager] 실시간 리스너 시작");
        listenerQuery = leaderboardRef.OrderByChild("playtime").LimitToLast(limit);

        // 변경사항이 있을 때마다 갱신
        listenerQuery.ValueChanged += OnValueChanged;
        isListenerActive = true;
    }

    public void StopRealtimeListener()
    {
        if (isListenerActive && listenerQuery != null)
        {
            Debug.Log("[LeaderboardManager] 실시간 리스너 중지");
            listenerQuery.ValueChanged -= OnValueChanged;
            listenerQuery = null;
            isListenerActive = false;
        }
    }

    private void OnValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"[LeaderboardManager] 리스너 오류: {args.DatabaseError.Message}");
            return;
        }

        List<LeaderboardEntry> leaderboard = ParseEntries(args.Snapshot);
        DispatchUpdateAsync(leaderboard).Forget();
    } 

    private async UniTaskVoid DispatchUpdateAsync(List<LeaderboardEntry> leaderboard)
    {
        await UniTask.SwitchToMainThread();
        OnLeaderboardUpdated?.Invoke(leaderboard);
    }
}
