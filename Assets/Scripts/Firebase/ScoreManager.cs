using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;
    public static ScoreManager Instance => instance;

    private DatabaseReference scoresRef;
    private float cachedBestScore = 999f;
    public float CachedBestScore => cachedBestScore;
    
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
            Debug.LogError("[Score] 파이어 베이스 초기화 실패");
            return;
        }

        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => ProfileManager.Instance.IsInitialized);

        scoresRef = FirebaseInitializer.Instance.Database.RootReference.Child("playtimes");
        Debug.Log("[Score] 파이어 베이스 초기화 완료");

        AuthManager.Instance.LogInStateChanged += OnLoginStatusChanged;

        if (AuthManager.Instance.IsLoggedIn)
            await SyncBestScoreToLeaderboardAsync();

        //await LoadBestScoreAsync();
    }

    private void OnLoginStatusChanged(bool signIn)
    {
        if (signIn)
        {
            SyncBestScoreToLeaderboardAsync().Forget();
        }
        else
        {
            cachedBestScore = 999f;
        }
    }


    private async UniTask SyncBestScoreToLeaderboardAsync()
    {
        await LoadBestScoreAsync();
    }


    public async UniTask<float> LoadBestScoreAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn || scoresRef == null)
        {
            Debug.Log("[Score] 로그인 안되어있거나 초기화 전");
            return 0;
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            DataSnapshot snapshot = await scoresRef.Child(userId).Child("bestplaytime").GetValueAsync();
            cachedBestScore = snapshot.Exists ? FirebaseValue.ToInt(snapshot.Value) : 0;

            Debug.Log($"[Score] 최고 점수 로드 성공: {cachedBestScore}");
            return cachedBestScore;
        }   
        catch (Exception ex)
        {
            Debug.LogError($"[Score] 최고 점수 로드 실패: {ex.Message}");
            return 0;
        }
    }

    public async UniTask<List<ScoreData>> LoadHistoryAsync(int limit = 10)
    {
        if (!AuthManager.Instance.IsLoggedIn || scoresRef == null) // scoresRef == null 추가
        {
            Debug.Log("[Score] 로그인 안되어있음");
            return new List<ScoreData>();
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Query query = scoresRef.Child(userId).Child("history")
                .OrderByChild("timestamp")
                .LimitToLast(limit);
            
            DataSnapshot snapshot = await query.GetValueAsync();
            List<ScoreData> historyList = new List<ScoreData>();

            if (snapshot.Exists)
            {
                foreach (DataSnapshot child in snapshot.Children)
                {
                    historyList.Add(JsonUtility.FromJson<ScoreData>(child.GetRawJsonValue()));
                }
                Debug.Log($"[Score] 히스토리 로드: {historyList.Count}");
                historyList.Reverse();
            } 
            return historyList;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Score] 히스토리 로드 실패: {ex.Message}");
            return new List<ScoreData>();
        }
    }

    // TODO
    private void OnDestroy()
    {
        if (AuthManager.Instance != null)
            AuthManager.Instance.LogInStateChanged -= OnLoginStatusChanged;
    }
}
