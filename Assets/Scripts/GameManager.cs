using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private int _score;
    public int Score => _score;

    [SerializeField]
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _score = 0;
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

    public void GameOver()
    {
        UIManager.Instance.ShowGameOver();
    }

    public void GameClear() { }

    public void Restart() { }
}
