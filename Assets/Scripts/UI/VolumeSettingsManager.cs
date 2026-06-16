using UnityEngine;
using UnityEngine.Audio;

public class VolumeSettingsManager : MonoBehaviour
{
    public static VolumeSettingsManager Instance { get; private set; }

    public const string BGMKey = "Vol_BGM";
    public const string SFXKey = "Vol_SFX";

    [SerializeField] private AudioMixer _mixer;

    public float BGMVolume { get; private set; }
    public float SFXVolume { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BGMVolume = PlayerPrefs.GetFloat(BGMKey, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFXKey, 1f);
    }

    private void Start()
    {
        _mixer?.SetFloat("BGMVolume", LinearToDb(BGMVolume));
        _mixer?.SetFloat("SFXVolume", LinearToDb(SFXVolume));
    }

    private static float LinearToDb(float linear) =>
        Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;

    public void SetBGMVolume(float value)
    {
        BGMVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BGMKey, BGMVolume);
        _mixer?.SetFloat("BGMVolume", LinearToDb(BGMVolume));
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SFXKey, SFXVolume);
        _mixer?.SetFloat("SFXVolume", LinearToDb(SFXVolume));
    }
}
