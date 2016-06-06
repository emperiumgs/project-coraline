using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsControl : MonoBehaviour
{
    public SoundtrackController soundtrack;

    void PlaySoundtrack()
    {
        soundtrack.GetComponent<AudioSource>().Play();
    }

    void ChangeSoundtrack()
    {
        soundtrack.FadeAndChangeSong("credits_2", 2);
    }

    void FadeSoundtrack()
    {
        soundtrack.FadeOut(2);
    }

    void Complete()
    {
        SceneManager.LoadScene(0);
    }
}
