using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Audio Source (SFX)")]
    [SerializeField] public AudioSource sfxSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    private const string SFX_VOLUME_PARAM = "SFXVolume";
    private const string PREF_SFX_VOLUME = "SFXVol";

    private float defaultSFXValue = 0.75f;
    public float CurrentSFXVolume { get; private set; }

    private void Awake()
    {
        // Singleton Kurulumu
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CurrentSFXVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, defaultSFXValue);
        SetSFXVolume(CurrentSFXVolume);
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
    public void SetSFXVolume(float linearValue)
    {
        CurrentSFXVolume = linearValue;

        if (linearValue <= 0.0001f)
        {
            masterMixer.SetFloat(SFX_VOLUME_PARAM, -80f);
        }
        else
        {
            float dB = Mathf.Log10(linearValue) * 20f;
            masterMixer.SetFloat(SFX_VOLUME_PARAM, dB);
        }

        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, CurrentSFXVolume);
        PlayerPrefs.Save();
    }
}
