using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private int _score;
    public int Score => _score;

    private bool _isPaused;
    private float _playTime;
    private DatabaseReference _playTimeRef;
    public float PlayTime => _playTime;
    private float _cachedBestPlaytime = 999f;
    public float CachedBestPlaytime => _cachedBestPlaytime;

    private bool _isGameClear;

    [SerializeField]
    private PlayerHealth _playerHealth;

    [SerializeField]
    private AngerSystem _angerSystem;

    [SerializeField]
    private MissionMessageUI _missionMessageUI;

    [SerializeField]
    private Language _language = Language.En;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Application.targetFrameRate = 60;
        _score = 0;

        if (PlayerPrefs.HasKey(TitleController.LanguagePrefKey))
            _language = (Language)PlayerPrefs.GetInt(TitleController.LanguagePrefKey);
        StringTableManager.Instance.SetLanguage(_language);
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

        _playTimeRef = FirebaseInitializer.Instance.Database.RootReference.Child("playtimes");
        Debug.Log("[Score] 파이어 베이스 초기화 완료");

        AuthManager.Instance.LogInStateChanged += OnLoginStatusChanged;

        if (AuthManager.Instance.IsLoggedIn)
            await SyncBestScoreToLeaderboardAsync();
    }
    private void OnLoginStatusChanged(bool signIn)
    {
        if (signIn)
        {
            SyncBestScoreToLeaderboardAsync().Forget();
        }
        else
        {
            _cachedBestPlaytime = 999f;
        }
    }

    private void OnEnable()
    {
        _playerHealth.OnDied += GameOver;
    }

    private void OnDisable()
    {
        _playerHealth.OnDied -= GameOver;
    }

    private void Update()
    {
        if (!_isPaused)
            _playTime += Time.deltaTime;

#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.f2Key.wasPressedThisFrame)
            MissionManager.Instance.DebugCompleteAll();
        if (UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame)
            MissionManager.Instance.DebugUnlockOptional();
        if (UnityEngine.InputSystem.Keyboard.current.f5Key.wasPressedThisFrame)
            GameClear();
        if (UnityEngine.InputSystem.Keyboard.current.f6Key.wasPressedThisFrame)
            _playerHealth.TakeDamage(20, CauseDeath.NPC);
        if (UnityEngine.InputSystem.Keyboard.current.f7Key.wasPressedThisFrame)
            _playerHealth.TakeDamage(20, CauseDeath.Car);
        if (UnityEngine.InputSystem.Keyboard.current.f8Key.wasPressedThisFrame)
            _playerHealth.TakeDamage(20, CauseDeath.Cat);
#endif
    }

    private async UniTask SyncBestScoreToLeaderboardAsync()
    {
        float best = await LoadBestScoreAsync();
        if (best > 0 && LeaderboardManager.Instance != null)
        {
             await LeaderboardManager.Instance.SaveToLeaderboardAsync(best);
             Debug.Log($"[Score] 기존 최고 기록 {best}점을 리더보드에 반영");
        }
    } 

    public async UniTask SavePlaytimeAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn || _playTimeRef == null)
        {
            Debug.Log("[GameManager] 로그인 안되어있음 or 플레이 시간 없음");
            return;
        }

        Debug.Log("[GameManager] 점수 저장 진입");


        string userId = AuthManager.Instance.UserId;
        float playTime = _playTime;
        bool gameclear = _isGameClear;
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int damagedNPC = _playerHealth.DamagedByNPC;
        int damagedCar = _playerHealth.DamagedByCar;
        int damagedCat = _playerHealth.DamagedByCat;

        try
        {
            Debug.Log($"[GameManager] 점수 저장 시도 중...{userId} / {gameclear} / {playTime} / {timestamp}");

            // 히스토리 기록
            string key = _playTimeRef.Child(userId).Child("history").Push().Key;
            await _playTimeRef.Child(userId).Child("history").Child(key)
                .SetRawJsonValueAsync(JsonUtility.ToJson(new ScoreData {

                    playtime = playTime, 
                    damagedNPC = damagedNPC,
                    damagedCar = damagedCar,
                    damagedCat = damagedCat,
                    gameclear = gameclear,
                    timestamp = timestamp }));

            // 최고 기록 갱신
            if (gameclear && playTime < _cachedBestPlaytime)
            {
                Debug.Log($"[GameManager] 최고 점수 갱신: {playTime}");
                await _playTimeRef.Child(userId).Child("bestplaytime").SetValueAsync(playTime);
                _cachedBestPlaytime = playTime;

                if (LeaderboardManager.Instance != null)
                    await LeaderboardManager.Instance.SaveToLeaderboardAsync(playTime);
            }

            Debug.Log($"[GameManager] 저장 완료: 플레이타임 - {playTime}초");
            Debug.Log($"[GameManager] 저장 완료: 데미지 횟수(NPC/Cat/Car) - {damagedNPC} / {damagedCat} / {damagedCar}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameManager] 플레이타임 저장 실패: {ex.Message}");
        }
    }

    public async UniTask<float> LoadBestScoreAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn || _playTimeRef == null)
        {
            Debug.Log("[GameManager] 플레이 점수 없음");
            return 0;
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            DataSnapshot snapshot = await _playTimeRef.Child(userId).Child("bestplaytime").GetValueAsync();
            _cachedBestPlaytime = snapshot.Exists ? FirebaseValue.ToInt(snapshot.Value) : 999f;

            Debug.Log($"[Score] 최고 점수 로드 성공: {_cachedBestPlaytime}");
            return _cachedBestPlaytime;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Score] 최고 점수 로드 실패: {ex.Message}");
            return 0;
        }
    }

    public async UniTask<List<ScoreData>> LoadHistoryAsync(int limit = 10)
    {
        if (!AuthManager.Instance.IsLoggedIn || _playTimeRef == null) // scoresRef == null 추가
        {
            Debug.Log("[Score] 로그인 안되어있음");
            return new List<ScoreData>();
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Query query = _playTimeRef.Child(userId).Child("history")
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

    public void AddScore(int amount)
    {
        _score += amount;
    }

    public void GameOver(CauseDeath cause)
    {
        if (cause == CauseDeath.Anger)
            Time.timeScale = 0f;

        _isGameClear = false;
        _angerSystem.Pause();
        MissionManager.Instance.PauseMissionAssignment();
        _missionMessageUI?.ClearQueue();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var cat in FindObjectsByType<CatMovement>(FindObjectsSortMode.None))
            cat.SetExternalPause(true);

        SavePlaytimeAsync().Forget();
        UIManager.Instance.ShowGameOver(cause, Mathf.RoundToInt(_playTime));
    }

    public void GameClear()
    {
        Time.timeScale = 0f;

        _isGameClear = true;
        MissionManager.Instance.PauseMissionAssignment();
        _missionMessageUI?.ClearQueue();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _playerHealth.GetComponent<UnityEngine.InputSystem.PlayerInput>()?.DeactivateInput();
        SavePlaytimeAsync().Forget();
        UIManager.Instance.ShowGameClear(Mathf.RoundToInt(_playTime));
    }

    public void GoToTitle()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("TitleScene");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerPrefs.SetInt("SkipTutorial", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isPaused;

        var playerInput = _playerHealth.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (_isPaused)
        {
            playerInput?.DeactivateInput();
            UIManager.Instance.ShowPause();
        }
        else
        {
            playerInput?.ActivateInput();
            UIManager.Instance.HidePause();
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ResetGame()
    {
        Debug.Log("[Game] 초기화");

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AuthManager.Instance.SignOut();

        // 씬 전환으로 GameManager, MissionManager, AngerSystem, PlayerHealth 등 씬 바운드 상태 자동 소멸
        SceneManager.LoadScene("TitleScene");
    }

    private void OnDestroy()
    {
        if (AuthManager.Instance != null)
            AuthManager.Instance.LogInStateChanged -= OnLoginStatusChanged;
    }
}
