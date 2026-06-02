using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _titlePanel;
    [SerializeField] private GameObject _videoPanel;

    [Header("Video Settings")]
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private string _nextSceneName = "AntDemoScene";

    [Header("Buttons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _skipButton;

    private void Awake()
    {
        if (_startButton != null)
            _startButton.onClick.AddListener(StartGameSequence);

        if (_skipButton != null)
            _skipButton.onClick.AddListener(SkipVideo);

        if (_videoPlayer != null)
            _videoPlayer.loopPointReached += OnVideoFinished;

        // Ensure correct initial state
        if (_titlePanel != null) _titlePanel.SetActive(true);
        if (_videoPanel != null) _videoPanel.SetActive(false);
    }

    private void StartGameSequence()
    {
        if (_videoPlayer != null && _videoPlayer.clip != null)
        {
            if (_titlePanel != null) _titlePanel.SetActive(false);
            if (_videoPanel != null) _videoPanel.SetActive(true);
            _videoPlayer.Play();
        }
        else
        {
            Debug.LogWarning("VideoPlayer or VideoClip is missing. Skipping to game.");
            SkipVideo();
        }
    }

    private void SkipVideo()
    {
        if (_videoPlayer != null)
            _videoPlayer.Stop();

        LoadGameScene();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(_nextSceneName))
        {
            SceneManager.LoadScene(_nextSceneName);
        }
        else
        {
            Debug.LogError("Next scene name is not specified!");
        }
    }
}
