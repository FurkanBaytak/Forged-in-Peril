using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        if (MusicManager.Instance != null)
            musicSlider.value = MusicManager.Instance.CurrentMusicVolume;

        if (SFXManager.Instance != null)
            sfxSlider.value = SFXManager.Instance.CurrentSFXVolume;
    }

    public void OnMusicSliderChanged(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXSliderChanged(float value)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.SetSFXVolume(value);
    }
}
