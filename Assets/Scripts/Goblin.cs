using UnityEngine;
using System.Collections;

public class Goblin : MonoBehaviour
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
    float detectRange = 8f;
    float chaseRange = 13f;
    float atkRange = 2.5f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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
            case States.Fighting:
                Fight();
                break;
            case States.Hurting:
                Hurt();
                break;
            case States.Dying:
                Die();
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
            agent.Stop();
        }
        else if (dist <= chaseRange)
            agent.SetDestination(target.position);
        else
            state = States.Idle;
    }

    void Fight()
    {
        if (Vector3.Distance(transform.position, target.position) <= atkRange)
        {

        }
        else
            state = States.Chasing;
    }

    void Hurt()
    {

    }

    void Die()
    {

    }
}