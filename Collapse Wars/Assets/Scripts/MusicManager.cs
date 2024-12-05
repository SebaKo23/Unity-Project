using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    private AudioSource audioSource;
    public float SFXVolume = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMusicVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;
    }

    public void MuteMusic(bool mute)
    {
        if (audioSource != null)
            audioSource.mute = mute;
    }

    internal void SetSFXVolume(float volume)
    {
        SFXVolume = volume;
    }
}