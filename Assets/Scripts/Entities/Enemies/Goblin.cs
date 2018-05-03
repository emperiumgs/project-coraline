using UnityEngine;
using System.Collections;

public class Goblin : MonoBehaviour, IDamageable, IMeleeAttackable
{
    public LayerMask mask;
    public ParticleSystem stunParticles;

    enum States
    {
        Idle,
        Chasing,
        Fighting,
        Dying
    }
    States state = States.Idle;

    AudioController audioCtrl;
    UnityEngine.AI.NavMeshAgent agent;
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
        maxHealth = 30f,
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
        audioCtrl = GetComponent<AudioController>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
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
        Collider[] cols;
        cols = Physics.OverlapSphere(centerPos, detectRange, mask);
        if (cols.Length != 0)
        {
            target = cols[0].transform;
            pc = target.GetComponent<PlayerCombat>();
            RaycastHit hit;
            if (Physics.Raycast(centerPos, targetCenterPos - centerPos, out hit, detectRange))
            {
                if (hit.collider.tag == "Player")
                    state = States.Chasing;
            }
        }
        else
            target = null;
    }

    void Chase()
    {
        if (!Physics.CheckSphere(centerPos, chaseRange, mask))
        {
            provoked = false;
            anim.SetBool("walking", false);
            state = States.Idle;
            return;
        }
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= atkRange)
        {
            if (Physics.Raycast(centerPos, transform.forward, atkRange, mask))
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
            else
                agent.SetDestination(target.position);
        }
        else if (provoked || dist <= chaseRange)
        {
            anim.SetBool("walking", true);
            agent.SetDestination(target.position);
        }
    }

    void EndAttack()
    {
        state = States.Chasing;
    }

    public void TakeDamage(float damage)
    {
        if (state == States.Dying)
            return;

        provoked = false;
        health -= damage;
        if (damage > 2.5f)
            audioCtrl.PlayClip("takeDamage");
        anim.SetTrigger("takeDamage");
        CloseDamage();
        if (state == States.Fighting)
            EndAttack();
        if (health <= 0)
        {
            agent.SetDestination(transform.position);
            anim.SetTrigger("die");
            state = States.Dying;
        }
        else if (state == States.Idle)
        {
            provoked = true;
            target = FindObjectOfType<PlayerCombat>().transform;
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
                    audioCtrl.PlayClip("stun");
                }
                audioCtrl.PlayClip("damage");
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