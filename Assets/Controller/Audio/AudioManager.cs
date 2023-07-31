using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Audio Manager.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    private static AudioManager _instance;

    /// <summary>
    /// Declare the properties of the sounds.
    /// </summary>
    private void Awake()
    {
        
        if (_instance == null) _instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (var s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.playOnAwake = false;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    /// <summary>
    /// Update the audio clips properties of the sounds.
    /// </summary>
    private void Update()
    {
        foreach (var s in sounds)
        {
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    /// <summary>
    /// Beginning playing a chosen sound.
    /// </summary>
    /// <param name="soundName">The soundName of the sound.</param>
    public void Play(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found");
            return;
        }
        s.source.Play();
    }

    /// <summary>
    /// Get whether a sound is playing or not.
    /// </summary>
    /// <param name="soundName">The soundName of the sound.</param>
    /// <returns>Whether the sound is playing or not.</returns>
    public bool IsPlaying(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s != null) return (s.source.isPlaying);
        Debug.LogWarning("Sound: " + soundName + " not found");
        return false;
    }

    /// <summary>
    /// Play two sounds consecutively.
    /// </summary>
    /// <param name="soundName1">The soundName of the first sound to be played.</param>
    /// <param name="soundName2">The soundName of the sound to be player after the first has finished.</param>
    /// <returns></returns>
    public IEnumerator PlayQueued(string soundName1, string soundName2)
    {
        var s1 = Array.Find(sounds, sound => sound.name == soundName1);
        var s2 = Array.Find(sounds, sound => sound.name == soundName2);

        if (s1 == null)
        {
            Debug.LogWarning("Sound: " + soundName1 + " not found");
            yield return null;
        }
        if (s2 == null)
        {
            Debug.LogWarning("Sound: " + soundName2 + " not found");
            yield return null;
        }

        if ((s2 != null && s1 != null) && (s1.source.isPlaying | s2.source.isPlaying))
        {
            yield return null;
        }

        s1?.source.Play();
        if (s1 != null) yield return new WaitForSeconds(s1.clip.length);
        s2?.source.Play();
    }

    /// <summary>
    /// Stop playing a sound.
    /// </summary>
    /// <param name="soundName">The soundName of the sound.</param>
    public void Stop(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found");
            return;
        }
        s.source.Stop();
    }
}
