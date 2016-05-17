using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HudController : MonoBehaviour
{
    LightningAttack ltng;
    Coroutine rangeCheck;
    Vector3 camPoint;
    Camera cam;
    Image crosshair;
    
    void Awake()
    {
        ltng = FindObjectOfType<LightningAttack>();
        crosshair = transform.FindChild("Crosshair").GetComponent<Image>();
        cam = Camera.main;
        camPoint = new Vector3(Screen.width / 2, Screen.height / 2, LightningAttack.MAX_RANGE + Mathf.Abs(cam.transform.localPosition.z) + ltng.lightningPivot.localPosition.z);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            Debug.Break();
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
                crosshair.color = new Color(1, 0, 0, crosshair.color.a);
            else
                crosshair.color = new Color(1, 1, 1, crosshair.color.a);
            yield return null;
        }
    }
}
