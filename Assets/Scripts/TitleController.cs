using System.Collections;
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

    private bool _sceneLoading = false;

    private void Awake()
    {
        if (_startButton != null)
            _startButton.onClick.AddListener(StartGameSequence);

        if (_skipButton != null)
            _skipButton.onClick.AddListener(SkipVideo);

        if (_titlePanel != null) _titlePanel.SetActive(true);
        if (_videoPanel != null) _videoPanel.SetActive(false);
    }

    private void StartGameSequence()
    {
        if (_videoPlayer != null && _videoPlayer.clip != null)
        {
            if (_titlePanel != null) _titlePanel.SetActive(false);
            if (_videoPanel != null) _videoPanel.SetActive(true);

            _videoPlayer.prepareCompleted += OnVideoPrepared;
            _videoPlayer.Prepare();
        }
        else
        {
            Debug.LogWarning("VideoPlayer or VideoClip is missing. Skipping to game.");
            SkipVideo();
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnVideoPrepared;
        float duration = (float)vp.clip.length;
        Debug.Log($"[TitleController] 비디오 준비 완료 — 길이: {duration}s, 프레임수: {vp.clip.frameCount}");
        vp.Play();
        StartCoroutine(WaitForVideoEnd(duration));
    }

    private IEnumerator WaitForVideoEnd(float clipDuration)
    {
        // 타임스탬프 버그로 clip.length가 0 반환할 경우 최소 5초 보장
        float waitTime = Mathf.Max(clipDuration, 5f);
        Debug.Log($"[TitleController] {waitTime}초 대기 후 씬 전환");
        yield return new WaitForSeconds(waitTime);
        LoadGameScene();
    }

    private void SkipVideo()
    {
        StopAllCoroutines();
        if (_videoPlayer != null)
            _videoPlayer.Stop();

        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (_sceneLoading) return;
        _sceneLoading = true;

        if (!string.IsNullOrEmpty(_nextSceneName))
            SceneManager.LoadScene(_nextSceneName);
        else
            Debug.LogError("Next scene name is not specified!");
    }
}
