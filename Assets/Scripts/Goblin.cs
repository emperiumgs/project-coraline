using UnityEngine;
using System.Collections;

public class Goblin : MonoBehaviour, IDamageable
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
    Animator anim;
    float detectRange = 8f;
    float chaseRange = 13f;
    float atkRange = 1.85f;
    float damage = 10f;

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
            state = States.Fighting;
            agent.SetDestination(transform.position);
            anim.SetTrigger("atk");
        }
        else if (dist <= chaseRange)
            agent.SetDestination(target.position);
        else
            state = States.Idle;
    }

    public void TakeDamage(float damage)
    {

    }

    public void Die()
    {

    }

    void Damage()
    {
        if (Physics.CheckBox(transform.position + transform.TransformDirection(Vector3.forward), Vector3.one / 2, Quaternion.identity, mask))
            pc.TakeDamage(damage);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue - Color.black / 2;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.forward, Vector3.one);
    }
}