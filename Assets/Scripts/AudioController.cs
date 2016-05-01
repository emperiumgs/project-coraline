using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    public string assetPath;

    Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        AudioClip[] assets = Resources.LoadAll<AudioClip>("Sounds/" + assetPath);
        print(assets.Length);
        foreach (AudioClip clip in assets)
            clips.Add(clip.name, clip);
    }

    public void PlayClip(string name)
    {
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(clips[name], Random.Range(0.9f, 1));
    }

    public void StopClip()
    {
        source.Stop();
    }
}
