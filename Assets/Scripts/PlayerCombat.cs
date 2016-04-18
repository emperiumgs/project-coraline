using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{    
    public Transform weapon;
    public LayerMask hittableLayers;
    public Vector3 weaponReachOffset;
    public Vector3 halfWeaponReach;    

    const float LTNG_CD = 1.5f;

    LightningAttack ltng;
    CameraController cam;
    PlayerPhysics physics;
    Coroutine damageDeal;
    Animator anim;
    float[] comboDamage = { 8, 10, 15 };
    bool ltngMode;
    bool ltngEnabled = true;
    bool holdZoomOut;
    bool attackable = true;
    int comboNum;

    void Awake()
    {
        physics = GetComponent<PlayerPhysics>();
        cam = Camera.main.GetComponent<CameraController>();
        ltng = GetComponent<LightningAttack>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (attackable && Input.GetButtonDown("Fire1"))
        {
            // Reorient to face cam
            if (comboNum == 0)
                transform.localEulerAngles = new Vector3(0, cam.transform.eulerAngles.y);
            comboNum++;
            anim.SetInteger("comboNum", comboNum);
        }

        if (comboNum == 0)
        {
            if (Input.GetButtonDown("Fire2"))
            {
                if (!holdZoomOut)
                    ToggleZoom();
            }
            else if (ltngMode && Input.GetButtonUp("Fire2"))
            {
                if (!ltngEnabled)
                    holdZoomOut = true;
                else
                    ToggleZoom();
            }
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
        physics.camOrient = !ltngMode;
        attackable = ltngMode;
        ltngMode = !ltngMode;
    }

    void ToggleAttack()
    {
        attackable = !attackable;
    }

    void ClearCombo()
    {
        comboNum = 0;
        anim.SetInteger("comboNum", comboNum);
    }

    void OpenDamage()
    {
        damageDeal = StartCoroutine(DamageDealing());
    }

    void CloseDamage()
    {
        StopCoroutine(damageDeal);
    }

    IEnumerator DamageDealing()
    {
        Collider[] hits;
        List<IDamageable> hitted = new List<IDamageable>();
        while (true)
        {
            hits = Physics.OverlapBox(weapon.position + weapon.TransformDirection(weaponReachOffset), halfWeaponReach, weapon.rotation, hittableLayers);
            if (hits.Length > 0)
            {
                IDamageable dmg;
                for (int i = 0; i < hits.Length; i++)
                {
                    dmg = hits[i].GetComponent<IDamageable>();
                    if (dmg != null && !hitted.Contains(dmg))
                    {
                        dmg.TakeDamage(comboDamage[comboNum-1]);
                        hitted.Add(dmg);
                    }
                }
            }
            yield return new WaitForFixedUpdate();
        }
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green - Color.black / 2;
        Gizmos.matrix = Matrix4x4.TRS(weapon.position, weapon.rotation, Vector3.one);
        Gizmos.DrawCube(weaponReachOffset, halfWeaponReach * 2);
    }
}