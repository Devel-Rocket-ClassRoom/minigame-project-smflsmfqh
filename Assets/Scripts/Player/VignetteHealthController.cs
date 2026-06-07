using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteHealthController : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Volume _volume;
    [SerializeField] private float _maxIntensity = 0.6f;

    private Vignette _vignette;

    private void Awake()
    {
        EnsurePostProcessingEnabled();
        SetupVignette();
    }

    private void EnsurePostProcessingEnabled()
    {
        var cam = Camera.main;
        if (cam == null) return;
        var urpData = cam.GetComponent<UniversalAdditionalCameraData>();
        if (urpData != null)
            urpData.renderPostProcessing = true;
    }

    private void SetupVignette()
    {
        // Inspector에서 직접 연결한 경우
        if (_volume != null)
        {
            if (!_volume.profile.TryGet(out _vignette))
                _vignette = _volume.profile.Add<Vignette>(true);
            return;
        }

        // 씬에서 글로벌 Volume 탐색
        var found = FindFirstObjectByType<Volume>();
        if (found != null)
        {
            _volume = found;
            if (!_volume.profile.TryGet(out _vignette))
                _vignette = _volume.profile.Add<Vignette>(true);
            return;
        }

        // 없으면 런타임에 글로벌 Volume 생성
        var go = new GameObject("GlobalVolume_Vignette");
        _volume = go.AddComponent<Volume>();
        _volume.isGlobal = true;
        _volume.priority = 10;
        _vignette = _volume.profile.Add<Vignette>(true);
        _vignette.active = true;
        _vignette.intensity.Override(0f);
    }

    private void OnEnable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float hpRatio)
    {
        if (_vignette == null) return;

        if (hpRatio > 0.5f)
        {
            _vignette.intensity.Override(0f);
            return;
        }

        // 50%→0% 구간을 10% 단위 5계단으로 나눔
        // 50~41% → 1단계, 40~31% → 2단계, ..., 10~0% → 5단계
        int stage = Mathf.FloorToInt((0.5f - hpRatio) * 10f) + 1;
        stage = Mathf.Clamp(stage, 1, 5);
        _vignette.intensity.Override((stage / 5f) * _maxIntensity);
    }
}
