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

    ParticleSystem particles;
    RaycastHit hit;
    Transform target;
    Animator anim;
    float detectRange = 15f,
        shootDuration,
        maxAtkCd = 2f,
        minAtkCd = 1f,
        maxHealth = 50f,
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
        if (Physics.Raycast(centerPos, targetCenterPos - centerPos, out hit, detectRange))
        {
            if (hit.collider.tag == "Player")
                state = States.Shooting;
        }
    }

    void Shooting()
    {
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
        if (Vector3.Angle(transform.forward, dir) > 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSens);
            particles.transform.LookAt(targetCenterPos);
            //particles.transform.rotation = Quaternion.LookRotation(targetCenterPos - particles.transform.position);
            //particles.transform.rotation.SetLookRotation(targetCenterPos - particles.transform.position);
        }
        else if (ready && attackable)
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

        health -= damage;
        if (health <= 0)
        {
            state = States.Dying;
            //anim.SetTrigger("die");
            Die();
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