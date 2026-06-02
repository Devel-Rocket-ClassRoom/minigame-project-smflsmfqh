using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private int _score;
    public int Score => _score;

    private bool _isPaused;
    private float _playTime;

    [SerializeField]
    private PlayerHealth _playerHealth;

    [SerializeField]
    private AngerSystem _angerSystem;

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
        _score = 0;

        StringTableManager.Instance.SetLanguage(_language);
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
        {
            MissionManager.Instance.DebugCompleteAll();
            Debug.Log("[Debug] 모든 미션 즉시 완료");
        }
#endif
    }

    public void AddScore(int amount)
    {
        _score += amount;
    }

    public void GameOver(CauseDeath cause)
    {
        Debug.Log($"[게임 오버] {cause}가 죽임");
        if (cause == CauseDeath.Mission || cause == CauseDeath.Anger)
            Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UIManager.Instance.ShowGameOver(cause);
    }

    public void GameClear()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UIManager.Instance.ShowGameClear(Mathf.RoundToInt(_playTime));
    }

    public void Restart()
    {
        Debug.Log("[게임 오버] RESTART 버튼 눌림");
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isPaused;

        if (_isPaused) UIManager.Instance.ShowPause();
        else           UIManager.Instance.HidePause();
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
