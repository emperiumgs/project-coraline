using UnityEngine;
using System.Linq;
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
        foreach (AudioClip clip in assets)
            clips.Add(clip.name, clip);
    }

    public void PlayClip(string name)
    {
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(clips[name], Random.Range(0.9f, 1));
    }
    
    public void PlayClipRandomized(string name)
    {
        int amount = clips.Keys.ToArray().Where(s => s.Contains(name)).Count();
        PlayClip(name + (Random.Range(1, amount + 1)));
    }

    public void PlayStepSound()
    {
        PlayClipRandomized("step");
    }

    public void StopClip()
    {
        if (source.loop)
            source.loop = false;
        source.Stop();
    }
}
