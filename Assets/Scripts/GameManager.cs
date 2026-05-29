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
    private TextAsset _deathMessageCsv;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _score = 0;

        StringTableManager.Instance.Load(_deathMessageCsv);
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
