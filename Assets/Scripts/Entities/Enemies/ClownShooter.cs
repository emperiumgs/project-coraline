using UnityEngine;
using System.Collections;

public class ClownShooter : MonoBehaviour, IDamageable
{
    public LayerMask mask;

    enum States
    {
        Idle,
        Shooting,
        Dying
    }
    States state = States.Idle;
    
    AudioController audioCtrl;
    ParticleSystem particles;
    RaycastHit hit;
    Transform target;
    Animator anim;
    float detectRange = 15f,
        shootDuration,
        maxAtkCd = 2f,
        minAtkCd = 1f,
        maxHealth = 40f,
        health;
    bool attackable = true,
        ready;
    int rotationSens = 10;

    Vector3 centerPos
    {
        get { return transform.position + Vector3.up; }
    }
    Vector3 targetCenterPos
    {
        get { return target.position + Vector3.up; }
    }

    void Awake()
    {
        audioCtrl = GetComponent<AudioController>();
        target = FindObjectOfType<PlayerPhysics>().transform;
        health = maxHealth;
        anim = GetComponent<Animator>();
        particles = GetComponentInChildren<ParticleSystem>(true);
        shootDuration = particles.startLifetime;
    }

    void FixedUpdate()
    {
        if (state == States.Idle)
            Idle();
        else if (state == States.Shooting)
            Shooting();
    }

    void Idle()
    {
        Collider[] cols;
        cols = Physics.OverlapSphere(centerPos, detectRange, mask);
        if (cols.Length != 0)
        {
            target = cols[0].transform;
            RaycastHit hit;
            if (Physics.Raycast(centerPos, targetCenterPos - centerPos, out hit, detectRange))
            {
                if (hit.collider.tag == "Player")
                    state = States.Shooting;
            }
        }
        else
            target = null;
    }

    void Shooting()
    {
        if (!Physics.CheckSphere(centerPos, detectRange, mask))
        {
            ToIdle();
            return;
        }

        Vector3 dir = targetCenterPos - centerPos;
        if (Physics.Raycast(centerPos, dir, out hit, detectRange))
        {
            if (hit.collider.tag != "Player")
            {
                ToIdle();
                return;
            }
        }
        else
        {
            ToIdle();
            return;
        }
        anim.SetBool("aiming", true);
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSens);
        particles.transform.LookAt(targetCenterPos);

        if (ready && attackable)
            Shoot();
    }

    void Shoot()
    {
        anim.SetBool("aiming", false);
        anim.SetTrigger("shoot");
        StartCoroutine(ShootCycle());
    }

    void Ready()
    {
        ready = true;
    }

    void Unready()
    {
        ready = false;
    }

    void ToIdle()
    {
        state = States.Idle;
        anim.SetBool("aiming", false);
    }

    public void TakeDamage(float damage)
    {
        if (state == States.Dying)
            return;
        
        anim.SetBool("aiming", false);
        ready = false;
        anim.SetTrigger("takeDamage");
        if (damage > 2.5f)
            audioCtrl.PlayClip("takeDamage");
        health -= damage;
        if (health <= 0)
        {
            state = States.Dying;
            anim.SetTrigger("death");
        }
    }

    public void Die()
    {
        Destroy(gameObject, 0.5f);
    }
    
    IEnumerator ShootCycle()
    {
        attackable = false;
        particles.Play();
        yield return new WaitForSeconds(shootDuration);
        particles.Stop(true);
        yield return new WaitForSeconds(Random.Range(minAtkCd, maxAtkCd));
        attackable = true;
    }
}