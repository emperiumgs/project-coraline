using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Goblin : MonoBehaviour, IDamageable, IMeleeAttackable, IStunnable
{
    public LayerMask mask;
    public Slider slider;
    public Text text;

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
    public float detectRange = 10f,
        chaseRange = 15f,
        atkRange = 1.85f,
        maxAtkCd = 1.5f,
        minAtkCd = 0.5f,
        damage = 10f,
        stunDamage = 10f,
        stunTime = 1f,
        maxHealth = 40f;
    float health;
    bool attackable = true,
        provoked,
        stunAttack;

    void Awake()
    {
        pc = FindObjectOfType<PlayerCombat>();
        target = pc.transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        health = maxHealth;
        UpdateHealth();
    }

    void FixedUpdate()
    {
        if (state == States.Idle)
            Idle();
        else if (state == States.Chasing)
            Chase();
    }

    void UpdateHealth()
    {
        slider.value = health / maxHealth;
        text.text = "Health: " + health;
    }

    void Idle()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, target.position - transform.position, out hit, detectRange))
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

    public void Stun(float time)
    {
        agent.SetDestination(transform.position);
        anim.SetTrigger("hurt");
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
            health = 0;
            agent.SetDestination(transform.position);
            anim.SetTrigger("die");
            state = States.Dying;
        }
        else if (state == States.Idle)
        {
            provoked = true;
            state = States.Chasing;
        }
        UpdateHealth();
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void OpenDamage()
    {
        stunAttack = Random.Range(0, 10) < 2 ? true : false;
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