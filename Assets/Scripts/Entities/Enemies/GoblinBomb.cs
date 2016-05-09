using UnityEngine;
using System.Collections;

public class GoblinBomb : MonoBehaviour, IDamageable
{
    [HideInInspector]
    public Transform target;
    public LayerMask mask;

    ParticleSystem blast;
    NavMeshAgent agent;
    float explosionTime = 1.5f;
    float explosionDamage = 15f;
    float explosionRange = 1.5f;
    float explosionRadius = 3f;
    bool armed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        blast = GetComponentInChildren<ParticleSystem>(true);
    }

    void FixedUpdate()
    {
        if (!armed)
        {
            if (Physics.CheckSphere(transform.position, explosionRange, mask))
                Arm();
            else
                agent.SetDestination(target.position);
        }
    }

    void Arm()
    {
        armed = true;
        agent.Stop();
        StartCoroutine(ExplosionTimer());
    }

    public void TakeDamage(float damage)
    {
        Die();
    }

    public void Die()
    {
        blast.gameObject.SetActive(true);
        blast.transform.SetParent(null);
        blast.Play();
        blast.GetComponent<ParticleDestroy>().Destroy();
        if (Physics.CheckSphere(transform.position, explosionRadius, mask))
            target.GetComponent<IDamageable>().TakeDamage(explosionDamage);
        Destroy(gameObject);
    }

    IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(explosionTime);
        Die();
    }
}