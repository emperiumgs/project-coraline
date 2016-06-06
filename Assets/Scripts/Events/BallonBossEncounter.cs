using UnityEngine;
using System.Collections;

public class BallonBossEncounter : MonoBehaviour
{
    public GameObject spotL,
        spotR,
        cam,
        boss;

    static bool done = false;

    HudController hud;
    PlayerCombat pc;
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (done)
        {
            boss.SetActive(true);
            spotL.SetActive(true);
            spotR.SetActive(true);
        }
    }

    public void Activate()
    {
        hud = FindObjectOfType<HudController>();
        hud.StartCoroutine(hud.FadeOut(2));
        pc = FindObjectOfType<PlayerCombat>();
        pc.DisablePlayer();
        StartCoroutine(StartCutscene());
        done = true;
    }

    void Complete()
    {
        StartCoroutine(EndCutscene());
    }

    IEnumerator StartCutscene()
    {
        yield return new WaitForSeconds(2);
        cam.SetActive(true);
        hud.StartCoroutine(hud.FadeIn(1));
        yield return new WaitForSeconds(1);
        anim.enabled = true;
    }

    IEnumerator EndCutscene()
    {
        hud.StartCoroutine(hud.FadeOut(1));
        yield return new WaitForSeconds(1);
        cam.SetActive(false);
        hud.StartCoroutine(hud.FadeIn(1));
        pc.EnablePlayer();
    }
}