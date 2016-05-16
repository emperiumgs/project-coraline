using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour, IDamageable, IMeleeAttackable, IStunnable
{
    public Transform weapon;
    public LayerMask hittableLayers;
    public Vector3 weaponReachOffset,
        halfWeaponReach;

    const float LTNG_CD = 1.5f;

    LightningAttack ltng;
    CameraController cam;
    HudController hud;
    PlayerPhysics physics;
    Coroutine damageDeal,
        stunRoutine;
    Animator anim;
    float[] comboDamage = { 8, 10, 15 };
    float maxHealth = 100,
        health;
    bool ltngMode,
        ltngEnabled = true,
        holdZoomOut,
        attackable = true;
    int comboNum;

    void Awake()
    {
        health = maxHealth;
        physics = GetComponent<PlayerPhysics>();
        cam = Camera.main.GetComponent<CameraController>();
        ltng = GetComponent<LightningAttack>();
        anim = GetComponent<Animator>();
        hud = FindObjectOfType<HudController>();
    }

    void Update()
    {
        if (!physics.stunned)
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
                    if (ltngMode)
                    {
                        if (!ltngEnabled)
                            holdZoomOut = true;
                        else
                            ToggleZoom();
                    }
                    else if (!holdZoomOut)
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
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Water")
        {
            float damage = float.Parse(other.name.Split('_')[1]);
            TakeDamage(damage);
        }
    }

    void ToggleZoom()
    {
        if (ltngMode)
            hud.DisableCrosshair();
        else
            hud.EnableCrosshair();
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

    public void OpenDamage()
    {
        damageDeal = StartCoroutine(DamageDealing());
    }

    public void CloseDamage()
    {
        StopCoroutine(damageDeal);
    }

    public void TakeDamage(float damage)
    {
        print("Took: " + damage);
        if (ltngMode)
        {
            if (ltng.striking)
                ltng.AbortStrike();
            ToggleZoom();
        }
    }

    public void Die()
    {

    }

    public void Stun(float time)
    {
        if (health <= 0)
            return;

        physics.stunned = true;
        stunRoutine = StartCoroutine(StunDuration(time));
        cam.Shake(time);
    }

    public void RechargeEnergy(float amount)
    {

    }

    public void RechargeLife(float amount)
    {
        health += amount;
        if (health > maxHealth)
            health = maxHealth;
    }

    public IEnumerator DamageDealing()
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
                        dmg.TakeDamage(comboDamage[comboNum - 1]);
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

    IEnumerator StunDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        physics.stunned = false;
    }
}