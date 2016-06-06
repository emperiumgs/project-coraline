using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class SoundtrackController : MonoBehaviour
{
    public AudioMixerSnapshot defaultSnapshot,
        silentSnapshot;

    AudioSource source;

	void Awake()
    {
        FadeIn(0.1f);
        source = GetComponent<AudioSource>();
    }

    public void ChangeSong(string name)
    {
        source.clip = Resources.Load<AudioClip>("Sounds/Soundtrack/" + name);
    }

    public void FadeOut(float time)
    {
        silentSnapshot.TransitionTo(time);
    }

    public void FadeIn(float time)
    {
        defaultSnapshot.TransitionTo(time);
    }

    public void FadeAndChangeSong(string name, float time)
    {
        StartCoroutine(ChangeSongProcess(name, time));
    }

    IEnumerator ChangeSongProcess(string name, float transitionTime)
    {
        FadeOut(transitionTime);
        yield return new WaitForSeconds(transitionTime);
        ChangeSong(name);
        source.Play();
        FadeIn(transitionTime);
    }
}
