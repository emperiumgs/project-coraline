using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    const float LTNG_CD = 1.5f;

    CameraController cam;
    Lightning ltng;
    bool ltngMode;
    bool ltngEnabled = true;

	void Awake()
    {
        cam = Camera.main.GetComponent<CameraController>();
        ltng = GetComponentInChildren<Lightning>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            ltngMode = true;
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            ltngMode = false;
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

    IEnumerator LightningCD()
    {
        yield return new WaitForSeconds(LTNG_CD);
        ltngEnabled = true;
    }
}