using UnityEngine;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    public string assetPath;

    Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    AudioSource source;
    int stepSounds = 0;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        AudioClip[] assets = Resources.LoadAll<AudioClip>("Sounds/" + assetPath);
        foreach (AudioClip clip in assets)
        {
            clips.Add(clip.name, clip);
            if (clip.name.Contains("step"))
                stepSounds++;
        }
    }

    public void PlayClip(string name)
    {
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(clips[name], Random.Range(0.9f, 1));
    }

    public void PlayStepSound()
    {
        PlayClip("step" + (Random.Range(0, stepSounds) + 1));
    }

    public void StopClip()
    {
        if (source.loop)
            source.loop = false;
        source.Stop();
    }
}
