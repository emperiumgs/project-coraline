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
    bool attackable = true;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        pc = target.GetComponent<PlayerCombat>();
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
                attackable = false;
                state = States.Fighting;                
                anim.SetTrigger("atk");
            }
        }
        else if (dist <= chaseRange)
            agent.SetDestination(target.position);
        else
            state = States.Idle;
    }

    void EndAttack()
    {
        state = States.Chasing;
    }

    public void TakeDamage(float damage)
    {
        print("Took " + damage + " damage");
    }

    public void Die()
    {

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