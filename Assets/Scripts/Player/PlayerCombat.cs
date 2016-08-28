using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour, IDamageable, IMeleeAttackable, IStunnable, IKnockable
{
    public Transform weapon;
    public LayerMask hittableLayers;
    public Vector3 weaponReachOffset,
        halfWeaponReach;
    public ParticleSystem damageFb,
        getHealth,
        getMana,
        threeQuarterHealthFb;
    public ParticleSystem[] halfHealthFbs,
        quarterHealthFbs;
    public AudioSource lowHealth;

    const float LTNG_CD = 1.5f;

    CheckpointZone zone;
    LightningAttack ltng;
    CameraController cam;
    AudioController audioCtrl;
    HudController hud;
    PlayerPhysics physics;
    Coroutine damageDeal;
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
        audioCtrl = GetComponent<AudioController>();
        ltng = GetComponent<LightningAttack>();
        anim = GetComponent<Animator>();
        hud = FindObjectOfType<HudController>();
    }

    void Update()
    {
        if (!physics.stunned)
        {
            if (attackable && Input.GetButton("Fire1"))
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
            audioCtrl.PlayClip("waterDamage");
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
        cam.onLightning = ltngMode;
        anim.SetBool("lightning", ltngMode);
    } 

    void FeedbacksControl()
    {
        if (health <= 3 * maxHealth / 4)
                threeQuarterHealthFb.Play();
        else
            threeQuarterHealthFb.Stop();

        if (health <= maxHealth / 2)
            ToggleParticles(halfHealthFbs, true);
        else
            ToggleParticles(halfHealthFbs, false);

        if (health <= maxHealth / 4)
        {
            lowHealth.Play();
            ToggleParticles(quarterHealthFbs, true);
        }
        else
        {
            lowHealth.Stop();
            ToggleParticles(quarterHealthFbs, false);
        }
    }

    void ToggleParticles(ParticleSystem[] particles, bool show)
    {
        foreach (ParticleSystem particle in particles)
        {
            if (show)
                particle.Play();
            else
                particle.Stop();
        }
    }

    public void ToggleAttack()
    {
        attackable = !attackable;
    }

    public void OpenDamage()
    {
        damageDeal = StartCoroutine(DamageDealing());
        audioCtrl.PlayClipRandomized("whoosh");
    }

    public void CloseDamage()
    {
        if (damageDeal != null)
            StopCoroutine(damageDeal);
    }

    public void ClearCombo()
    {
        comboNum = 0;
        anim.SetInteger("comboNum", comboNum);
    }

    public void TakeDamage(float damage)
    {
        if (!enabled)
            return;

        if (damage > 5)
            audioCtrl.PlayClip("takeDamage");
        damageFb.Play();
        if (!attackable)
            ToggleAttack();
        if (!physics.stunned)
            anim.SetTrigger("takeDamage");
        if (ltngMode)
        {
            if (ltng.striking)
                ltng.AbortStrike();
            ToggleZoom();
        }
        health -= damage;
        FeedbacksControl();
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    public void Die()
    {
        if (!enabled)
            return;

        anim.SetTrigger("death");
        physics.enabled = false;
        enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
        hud.StartCoroutine(hud.FadeInOut(zone.Respawn, zone.EnablePlayer));
    }

    public void Stun(float time)
    {
        if (health <= 0)
            return;
        
        physics.Stun(time);
    }

    public void Knockup(Vector3 dir, float strength)
    {
        if (health <= 0)
            return;

        physics.Knockup(dir, strength);
    }

    public void RechargeEnergy(float amount)
    {
        ltng.RechargeEnergy(amount);
        getMana.Play();
    }

    public void RechargeLife(float amount)
    {
        health += amount;
        if (health > maxHealth)
            health = maxHealth;
        FeedbacksControl();
        audioCtrl.PlayClip("healthUp");
        getHealth.Play();
    }

    public void UpdateCheckpoint(CheckpointZone zone)
    {
        if (this.zone != null)
            this.zone.Clear();
        this.zone = zone;
    }

    public void EnablePlayer()
    {
        enabled = true;
        physics.enabled = true;
    }

    public void DisablePlayer()
    {
        if (ltngMode)
        {
            if (ltng.striking)
                ltng.AbortStrike();
            ToggleZoom();
        }
        enabled = false;
        anim.SetFloat("movSpeed", 0);
        physics.enabled = false;
    }

    public IEnumerator DamageDealing()
    {
        Collider[] hits;
        List<IDamageable> hitted = new List<IDamageable>();
        while (comboNum != 0)
        {
            hits = Physics.OverlapBox(weapon.position + weapon.TransformDirection(weaponReachOffset), halfWeaponReach, weapon.rotation, hittableLayers);
            if (hits.Length > 0)
            {
                IDamageable dmg;
                for (int i = 0; i < hits.Length; i++)
                {
                    dmg = hits[i].GetComponentInParent<IDamageable>();
                    if (dmg != null && !hitted.Contains(dmg))
                    {
                        dmg.TakeDamage(comboDamage[comboNum - 1]);
                        hitted.Add(dmg);
                        audioCtrl.PlayClip("damage");
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
}