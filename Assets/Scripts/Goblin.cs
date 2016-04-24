using UnityEngine;
using System.Collections;

public class Goblin : MonoBehaviour, IDamageable, IMeleeAttackable
{
    public Transform target;
    public LayerMask mask;

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
    Animator anim;
    float detectRange = 8f;
    float chaseRange = 13f;
    float atkRange = 1.85f;
    float maxAtkCd = 1.5f;
    float minAtkCd = 0.5f;
    float damage = 10f;
    float stunDamage = 10f;
    float maxHealth = 40f;
    float health;
    bool attackable = true;
    bool provoked;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        pc = target.GetComponent<PlayerCombat>();
        health = maxHealth;
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case States.Idle:
                Idle();
                break;
            case States.Chasing:
                Chase();
                break;
        }
    }

    void Idle()
    {
        if (Vector3.Distance(transform.position, target.position) <= detectRange)
            state = States.Chasing;
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
                anim.SetTrigger("atk");
            }
        }
        else if (provoked || dist <= chaseRange)
            agent.SetDestination(target.position);
        else
        {
            provoked = false;
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

    public void TakeDamage(float damage)
    {
        provoked = false;
        health -= damage;
        if (health > 0 && damage > stunDamage)
        {
            anim.SetTrigger("hurt");            
            state = States.Hurting;
        }
        else if (health <= 0)
        {
            anim.SetTrigger("die");
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
        Destroy(gameObject);
    }

    public void OpenDamage()
    {
        damageDeal = StartCoroutine(DamageDealing());
    }

    public void CloseDamage()
    {        
        StopCoroutine(damageDeal);
        StartCoroutine(AttackCooldown());
    }

    public IEnumerator DamageDealing()
    {
        bool hit = false;
        while (!hit)
        {
            if (Physics.CheckBox(transform.position + transform.TransformDirection(Vector3.forward), Vector3.one / 2, Quaternion.identity, mask))
            {
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