using UnityEngine;
using System.Collections;

public class ClownShooter : MonoBehaviour, IDamageable
{
    public LayerMask mask;
    public ParticleSystem particles;

    enum States
    {
        Idle,
        Shooting,
        Dying
    }
    States state = States.Idle;

    ParticleSystem.Particle[] parts;
    ParticleSystem.Particle projectile;
    RaycastHit hit;
    Transform target;
    Transform hand;
    Animator anim;
    float detectRange = 15f;
    float shootDuration;
    float maxAtkCd = 1f;
    float minAtkCd = 0.5f;
    float maxHealth = 50f;
    float health;
    float damage = 15f;
    bool attackable = true;
    int rotationSens = 10;

    void Awake()
    {
        target = FindObjectOfType<PlayerPhysics>().transform;
        health = maxHealth;
        anim = GetComponent<Animator>();
        particles = GetComponentInChildren<ParticleSystem>(true);
        parts = new ParticleSystem.Particle[particles.maxParticles];
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
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSens);
            hand.rotation = Quaternion.Slerp(hand.rotation, Quaternion.LookRotation(target.position - hand.position), Time.deltaTime * rotationSens);
        }
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
        Vector3 shotOffset = hand.TransformDirection(Vector3.forward);
        float time = 0;
        bool damaged = false;
        while(time < shootDuration)
        {
            time += Time.deltaTime;
            yield return null;
            particles.GetParticles(parts);
            projectile = parts[0];
            if (!damaged)
            {
                Debug.DrawLine(projectile.position + shotOffset, projectile.position + shotOffset + Vector3.up, Color.red);
                if (Physics.CheckSphere(projectile.position + shotOffset, particles.startSize, mask))
                {
                    target.GetComponent<IDamageable>().TakeDamage(damage);
                    damaged = true;
                }
            }
        }
        particles.Stop(true);
        particles.gameObject.SetActive(false);
        yield return new WaitForSeconds(Random.Range(minAtkCd, maxAtkCd));
        attackable = true;
    }
}