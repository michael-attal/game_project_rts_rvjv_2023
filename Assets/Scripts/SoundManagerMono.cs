using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct AudioClipWithTitle
{
    public string title;
    public AudioClip clip;
}

public class SoundManagerMono : MonoBehaviour
{
    [SerializeField] private List<AudioClipWithTitle> audioClipsList;

    [SerializeField] [Range(0f, 1f)] public float Volume;

    [SerializeField] [Range(0f, 1f)] public float BackgroundVolume;

    private AudioSource backgroundMusicSource;
    private Dictionary<string, AudioClip> soundDictionary;

    private void Start()
    {
        soundDictionary = new Dictionary<string, AudioClip>();

        foreach (var item in audioClipsList)
        {
            soundDictionary.Add(item.title, item.clip);
        }

        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        backgroundMusicSource.loop = true;
    }

    public AudioClip GetAudioClipByTitle(string title)
    {
        if (soundDictionary.ContainsKey(title))
        {
            return soundDictionary[title];
        }

        Debug.LogWarning("Title not found: " + title);
        return null;
    }

    // NOTE: Method for playing a sound based on EntityType and SoundType
    public void PlaySound(EntityType entityType, SoundType soundType, Vector3? fromPosition = null)
    {
        var position = fromPosition ?? transform.position;
        var soundTitle = entityType + soundType.ToString();
        var clip = GetAudioClipByTitle(soundTitle);

        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, Volume);
        }
    }

    public void PlayBackgroundMusic(string title)
    {
        var clip = GetAudioClipByTitle(title);

        if (clip != null)
        {
            backgroundMusicSource.clip = clip;
            backgroundMusicSource.volume = BackgroundVolume;
            backgroundMusicSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music not found: " + title);
        }
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }
    }

    public void SetVolume(float newVolume)
    {
        Volume = Mathf.Clamp(newVolume, 0f, 1f); // NOTE: Make sure the volume is between 0 and 1
    }

    public void SetBackgroundVolume(float newVolume)
    {
        BackgroundVolume = Mathf.Clamp(newVolume, 0f, 1f);
        backgroundMusicSource.volume = BackgroundVolume;
    }
}