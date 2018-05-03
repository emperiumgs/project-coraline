using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HudController : MonoBehaviour
{
    public Sprite defCrosshair,
        hitCrosshair;
    public GameObject cinematicBars;

    static bool first;

    LightningAttack ltng;
    Coroutine rangeCheck;
    Vector3 camPoint;
    Camera cam;
    Image crosshair;
    Image fader;
    
    public void Awake()
    {
        ltng = FindObjectOfType<LightningAttack>();
        crosshair = transform.Find("Crosshair").GetComponent<Image>();
        fader = transform.Find("Fader").GetComponent<Image>();
        if (!first)
        {
            StartCoroutine(FadeIn(2));
            first = true;
        }
        cam = Camera.main;
        camPoint = new Vector3(Screen.width / 2, Screen.height / 2, LightningAttack.MAX_RANGE + Mathf.Abs(cam.transform.localPosition.z) + ltng.lightningPivot.localPosition.z);
    }

    public void EnableCrosshair()
    {
        crosshair.enabled = true;
        rangeCheck = StartCoroutine(LightningRange());
    }

    public void DisableCrosshair()
    {
        crosshair.enabled = false;
        StopCoroutine(rangeCheck);
    }

    public void EnableCinematic()
    {
        cinematicBars.SetActive(true);
        FindObjectOfType<PlayerCombat>().DisablePlayer();
        FindObjectOfType<CameraController>().Lock();
    }

    public void DisableCinematic()
    {
        cinematicBars.SetActive(false);
        FindObjectOfType<PlayerCombat>().EnablePlayer();
        FindObjectOfType<CameraController>().Unlock();
    }

    public void ToCredits()
    {
        StartCoroutine(End());
    }

    public IEnumerator FadeInOut(System.Action onBlack, System.Action onComplete)
    {
        yield return new WaitForSeconds(1);
        float time = 0,
            halfTime = 2;
        while (time < halfTime)
        {
            time += Time.deltaTime;
            fader.color = Color.black * time / halfTime;
            yield return null;
        }
        fader.color = Color.black;
        onBlack();
        yield return new WaitForSeconds(1);
        time = 0;
        while (time < halfTime)
        {
            time += Time.deltaTime;
            fader.color = Color.black * (1 - time / halfTime);
            yield return null;
        }
        fader.color = Color.clear;
        onComplete();
    }

    public IEnumerator FadeIn(float fadeTime)
    {
        float time = 0;
        fader.color = Color.black;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            fader.color = Color.black * (1 - time / fadeTime);
            yield return null;
        }
        fader.color = Color.clear;
    }

    public IEnumerator FadeOut(float fadeTime)
    {
        float time = 0;
        fader.color = Color.clear;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            fader.color = Color.black * time / fadeTime;
            yield return null;
        }
        fader.color = Color.black;
    }

    IEnumerator LightningRange()
    {
        RaycastHit hit;
        Vector3 target, dir, camPos, ltngPos;
        float maxDist;
        bool hitted;
        while (true)
        {
            hitted = false;
            crosshair.rectTransform.localPosition = Vector3.zero;
            camPos = cam.transform.position;
            ltngPos = ltng.lightningPivot.position;
            // Camera Raycasting
            target = cam.ScreenToWorldPoint(camPoint);
            dir = (target - camPos).normalized;
            maxDist = Vector3.Distance(camPos, target);
            if (Physics.Raycast(camPos, dir, out hit, maxDist))
            {
                target = hit.point;
                hitted = true;
            }
            // Hand Raycasting
            dir = (target - ltngPos).normalized;
            maxDist = Vector3.Distance(ltngPos, target);
            if (Physics.Raycast(ltngPos, dir, out hit, maxDist))
            {
                target = hit.point;
                Vector3 point = cam.WorldToScreenPoint(hit.point);
                point.z = 0;
                crosshair.rectTransform.localPosition = point - new Vector3(Screen.width / 2, Screen.height / 2);
                hitted = true;
            }
            if (hitted)
                crosshair.sprite = hitCrosshair;
            else
                crosshair.sprite = defCrosshair;
            yield return null;
        }
    }

    IEnumerator End()
    {
        yield return new WaitForSeconds(3);
        StartCoroutine(FadeOut(3));
        FindObjectOfType<SoundtrackController>().FadeOut(3);
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(2);
    }
}
