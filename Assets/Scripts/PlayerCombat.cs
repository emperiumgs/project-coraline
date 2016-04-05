using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    const float LTNG_CD = 1.5f;

    CameraController cam;    
    bool ltngMode;
    bool ltngEnabled = true;
    bool holdZoomOut;

    public Lightning ltng;

    void Awake()
    {
        cam = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            if (!holdZoomOut)
                ToggleZoom();
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            if (!ltngEnabled)
                holdZoomOut = true;
            else
                ToggleZoom();    
        }

        if (ltngMode)
        {
            if (ltngEnabled && Input.GetButtonDown("Fire1"))
            {
                ltng.Strike();
                ltngEnabled = false;
                StartCoroutine(LightningCD());
            }
        }
    }

    void ToggleZoom()
    {
        cam.Zoom(!ltngMode);
        ltngMode = !ltngMode;
    }

    IEnumerator LightningCD()
    {
        yield return new WaitForSeconds(LTNG_CD);
        if (holdZoomOut)
        {
            if (!Input.GetButton("Fire2"))
                ToggleZoom();
            holdZoomOut = false;
        }
        ltngEnabled = true;        
    }
}