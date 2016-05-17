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
    Transform target,
        hand;
    Animator anim;
    public float detectRange = 15f,
        shootDuration,
        maxAtkCd = 1f,
        minAtkCd = 0.5f,
        maxHealth = 50f;
    float health;
    bool attackable = true;
    public int rotationSens = 10;

    void Awake()
    {
        target = FindObjectOfType<PlayerPhysics>().transform;
        health = maxHealth;
        anim = GetComponent<Animator>();
        particles = GetComponentInChildren<ParticleSystem>(true);
        shootDuration = particles.startLifetime;
        hand = transform.FindChild("RHand");
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
        if (Physics.Raycast(transform.position, target.position - transform.position, out hit, detectRange))
        {
            if (hit.collider.tag == "Player")
                state = States.Shooting;
        }
    }

    void Shooting()
    {
        Vector3 dir = target.position - transform.position;
        if (Physics.Raycast(transform.position, dir, out hit, detectRange))
        {
            if (hit.collider.tag != "Player")
            {
                state = States.Idle;
                return;
            }
        }
        else
        {
            state = States.Idle;
            return;
        }

        dir.y = 0;        
        if (Vector3.Angle(transform.forward, dir) > 1)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSens);
        dir = target.position - hand.position;
        if (Vector3.Angle(hand.forward, dir) > 1)
            hand.rotation = Quaternion.Slerp(hand.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSens);
        else if (attackable)
            Shoot();
    }

    void Shoot()
    {
        anim.SetTrigger("shoot");
        StartCoroutine(ShootCycle());
    }

    public void TakeDamage(float damage)
    {
        if (state == States.Dying)
            return;

        health -= damage;
        if (health <= 0)
        {
            state = States.Dying;
            anim.SetTrigger("die");
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
    
    IEnumerator ShootCycle()
    {
        attackable = false;
        particles.gameObject.SetActive(true);
        particles.Play();
        yield return new WaitForSeconds(shootDuration);
        particles.Stop(true);
        particles.gameObject.SetActive(false);
        yield return new WaitForSeconds(Random.Range(minAtkCd, maxAtkCd));
        attackable = true;
    }
}