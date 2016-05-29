using UnityEngine;
using System.Collections;

public class Goblin : MonoBehaviour, IDamageable, IMeleeAttackable, IStunnable
{
    public LayerMask mask;
    public ParticleSystem stunParticles;

    enum States
    {
        Idle,
        Chasing,
        Fighting,
        Hurting,
        Dying
    }
    States state = States.Idle;

    NavMeshAgent agent;
    PlayerCombat pc;
    Coroutine damageDeal;
    Transform target;
    Animator anim;
    Vector3 weaponOffset,
        weaponExtents;
    float detectRange = 10f,
        chaseRange = 15f,
        atkRange = 1.2f,
        maxAtkCd = 1.5f,
        minAtkCd = 0.5f,
        damage = 10f,
        stunDamage = 10f,
        stunTime = 1f,
        maxHealth = 40f,
        health;
    bool attackable = true,
        provoked,
        stunAttack;

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
        pc = FindObjectOfType<PlayerCombat>();
        target = pc.transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        health = maxHealth;
        weaponOffset = new Vector3(0, 0, 0.75f);
        weaponExtents = new Vector3(1, 1, 0.75f);
    }

    void FixedUpdate()
    {
        if (state == States.Idle)
            Idle();
        else if (state == States.Chasing)
            Chase();
    }

    void Idle()
    {
        RaycastHit hit;
        if (Physics.Raycast(centerPos, targetCenterPos - centerPos, out hit, detectRange))
        {
            if (hit.collider.tag == "Player")
                state = States.Chasing;
        }
    }

    void Chase()
    {
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= atkRange)
        {
            agent.SetDestination(transform.position);
            if (attackable)
            {
                provoked = false;
                attackable = false;
                state = States.Fighting;
                anim.SetBool("walking", false);
                stunAttack = Random.Range(0, 10) < 2 ? true : false;
                if (stunAttack)
                    stunParticles.Play();
                anim.SetTrigger("atk");
            }
        }
        else if (provoked || dist <= chaseRange)
        {
            anim.SetBool("walking", true);
            agent.SetDestination(target.position);
        }
        else
        {
            provoked = false;
            anim.SetBool("walking", false);
            state = States.Idle;
        }
    }

    void EndAttack()
    {
        state = States.Chasing;
    }

    void EndHurt()
    {
        state = States.Idle;
    }

    public void Stun(float time)
    {
        agent.SetDestination(transform.position);
        anim.SetTrigger("hurt");
        CloseDamage();
        state = States.Hurting;
    }

    public void TakeDamage(float damage)
    {
        if (state == States.Dying)
            return;

        provoked = false;
        health -= damage;
        if (health > 0 && damage > stunDamage)
            Stun(stunTime);
        else if (health <= 0)
        {
            agent.SetDestination(transform.position);
            //anim.SetTrigger("die");
            Die();
            state = States.Dying;
        }
        else if (state == States.Idle)
        {
            provoked = true;
            state = States.Chasing;
        }
    }

    public void Die()
    {
        Destroy(gameObject, 0.5f);
    }

    public void OpenDamage()
    {
        damageDeal = StartCoroutine(DamageDealing());
    }

    public void CloseDamage()
    {
        if (damageDeal != null)
            StopCoroutine(damageDeal);
        if (stunParticles.isPlaying)
            stunParticles.Stop();
        StartCoroutine(AttackCooldown());
    }

    public IEnumerator DamageDealing()
    {
        bool hit = false;
        while (!hit)
        {
            if (Physics.CheckBox(centerPos + transform.TransformDirection(weaponOffset), weaponExtents / 2, Quaternion.identity, mask))
            {
                if (stunAttack)
                {
                    pc.Stun(stunTime);
                    stunAttack = false;
                }
                pc.TakeDamage(damage);
                hit = true;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(Random.Range(minAtkCd, maxAtkCd));
        attackable = true;
    }
}