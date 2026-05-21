using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private int _score;
    public int Score => _score;

    [SerializeField]
    private PlayerHealth _playerHealth;

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

    public void AddScore(int amount)
    {
        _score += amount;
    }

    public void GameOver(CauseDeath cause)
    {
        Debug.Log($"[게임 오버] {cause}가 죽임");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UIManager.Instance.ShowGameOver(cause);
    }

    public void GameClear() { }

    public void Restart()
    {
        Debug.Log("[게임 오버] RESTART 버튼 눌림");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
