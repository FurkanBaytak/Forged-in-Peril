using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioClip mapSceneMusic;
    [SerializeField] private AudioClip preparationSceneMusic;
    [SerializeField] private AudioClip battleSceneMusic;
    [SerializeField] public AudioClip victoryClip;
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string PREF_MUSIC_VOLUME = "MusicVol";
    private const string MUSIC_LOWPASS_PARAM = "MusicLowpassCutoff";
    private float defaultMusicValue = 0.75f;
    public float CurrentMusicVolume { get; private set; }
    public bool isMuffled = false;

    private void Awake()
    {
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
        CurrentMusicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, defaultMusicValue);
        SetMusicVolume(CurrentMusicVolume);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MapScene") PlayMusic(mapSceneMusic);
        else if (scene.name == "PreperationScene") PlayMusic(preparationSceneMusic);
        else if (scene.name == "BattleScene") PlayMusic(battleSceneMusic);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMuffled = !isMuffled;
            SetMusicMuffled(isMuffled);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void SetMusicVolume(float linearValue)
    {
        CurrentMusicVolume = linearValue;
        if (linearValue <= 0.0001f)
        {
            masterMixer.SetFloat(MUSIC_VOLUME_PARAM, -80f);
        }
        else
        {
            float dB = Mathf.Log10(linearValue) * 20f;
            masterMixer.SetFloat(MUSIC_VOLUME_PARAM, dB);
        }
        PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, CurrentMusicVolume);
        PlayerPrefs.Save();
    }

    private void SetMusicMuffled(bool muffled)
    {
        if (muffled) masterMixer.SetFloat(MUSIC_LOWPASS_PARAM, 500f);
        else masterMixer.SetFloat(MUSIC_LOWPASS_PARAM, 22000f);
    }
}
