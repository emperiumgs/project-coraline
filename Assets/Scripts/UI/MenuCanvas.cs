using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuCanvas : MonoBehaviour
{
    public AudioMixerSnapshot silentSoundtrack;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    void FadeSoundtrack()
    {
        silentSoundtrack.TransitionTo(1);
    }

	public void Play()
    {
        GetComponent<AudioSource>().Play();
        anim.SetTrigger("play");
    }

    public void Quit()
    {
        Application.Quit();
    }
}