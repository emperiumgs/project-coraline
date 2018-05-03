using UnityEngine;
using System.Collections;

public class GoblinBomb : MonoBehaviour, IDamageable
{
    [HideInInspector]
    public Transform target;
    public LayerMask mask;

    ParticleSystem blast;
    UnityEngine.AI.NavMeshAgent agent;
    Animator anim;
    float explosionTime = 1.5f;
    float explosionDamage = 15f;
    float explosionRange = 1.5f;
    float explosionRadius = 3f;
    bool armed;

    Vector3 centerPos
    {
        get { return transform.position + Vector3.up; }
    }

    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        blast = GetComponentInChildren<ParticleSystem>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!armed)
        {
            if (target == null || Physics.CheckSphere(centerPos, explosionRange, mask))
                Arm();
            else
                agent.SetDestination(target.position);
        }
    }

    void Arm()
    {
        armed = true;
        agent.Stop();
        anim.enabled = false;
        StartCoroutine(ExplosionTimer());
    }

    public void TakeDamage(float damage)
    {
        Die();
    }

    public void Die()
    {
        blast.transform.SetParent(null);
        blast.Play();
        blast.GetComponent<ParticleDestroy>().Destroy();
        blast.GetComponent<AudioSource>().Play();
        if (target != null && Physics.CheckSphere(transform.position, explosionRadius, mask))
            target.GetComponent<IDamageable>().TakeDamage(explosionDamage);
        Destroy(gameObject);
    }

    IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(explosionTime);
        Die();
    }
}