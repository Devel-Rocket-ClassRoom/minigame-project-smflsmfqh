using System;
using UnityEngine;

public class ProximityFeedback : MonoBehaviour
{
    [Header("감지 범위")]
    [SerializeField]
    private float _dangerRadius = 4f;

    [SerializeField]
    private float _panicRadius = 1f;

    [Header("카메라 흔들림")]
    [SerializeField]
    private FollowCamera _followCam;
    public event Action<float> OnIntensityChanged;

    public float Intensity { get; private set; }
    private GameObject[] _dangers = Array.Empty<GameObject>();
    private float _cacheTimer;
    private const float k_CacheInterval = 0.2f;

    private void Update()
    {
        RefreshDangerCache();
        float minDist = CalcMinDist();
        float newIntensity = CalcIntensity(minDist);

        _followCam?.SetShakeIntensity(minDist <= _panicRadius ? 1f : 0f);

        bool wasInDanger = Intensity > 0f;
        bool isInDanger  = newIntensity > 0f;

        if (wasInDanger == isInDanger && Mathf.Abs(newIntensity - Intensity) < 0.01f)
            return;

        Intensity = newIntensity;
        OnIntensityChanged?.Invoke(Intensity);
    }

    private void RefreshDangerCache()
    {
        _cacheTimer -= Time.deltaTime;
        if (_cacheTimer > 0f)
            return;

        _cacheTimer = k_CacheInterval;
        _dangers = GameObject.FindGameObjectsWithTag("Danger");
    }

    private float CalcMinDist()
    {
        float minDist = float.MaxValue;
        foreach (var obj in _dangers)
        {
            if (obj == null)
                continue;
            float d = Vector3.Distance(transform.position, obj.transform.position);
            if (d < minDist)
                minDist = d;
        }
        return minDist;
    }

    private float CalcIntensity(float minDist)
    {
        if (minDist == float.MaxValue)
            return 0f;

        return 1f - Mathf.Clamp01((minDist - _panicRadius) / (_dangerRadius - _panicRadius));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _panicRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _dangerRadius);
    }
}
