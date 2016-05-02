using UnityEngine;
using UnityEngine.UI;
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
        camPoint = new Vector3(Screen.width / 2, Screen.height / 2, LightningAttack.MAX_RANGE + Mathf.Abs(cam.transform.localPosition.z) + ltng.transform.localPosition.z);
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
        Vector3 target, dir;
        while (true)
        {
            target = cam.ScreenToWorldPoint(camPoint);
            dir = (target - ltng.lightningPivot.position).normalized;
            if (Physics.Raycast(ltng.lightningPivot.position, dir, LightningAttack.MAX_RANGE))
                crosshair.color = new Color(1, 0, 0, crosshair.color.a);
            else
                crosshair.color = new Color(1, 1, 1, crosshair.color.a);
            yield return null;
        }
    }
}
