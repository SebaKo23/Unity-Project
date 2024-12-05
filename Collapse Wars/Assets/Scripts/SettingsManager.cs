using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteMusicToggle;

    [Header("Audio Sources")]
    public AudioSource musicSource;

    private void Start()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (muteMusicToggle != null)
        {
            muteMusicToggle.isOn = PlayerPrefs.GetInt("MuteMusic", 0) == 1;
            muteMusicToggle.onValueChanged.AddListener(MuteMusic);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        MusicManager.instance.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void MuteMusic(bool mute)
    {
        MusicManager.instance.MuteMusic(mute);
        PlayerPrefs.SetInt("MuteMusic", mute ? 1 : 0);
        if (mute)
        {
            musicSource.volume = 0;
        }
        else
        {
            musicSource.volume = musicVolumeSlider.value;
        }
    }
}
