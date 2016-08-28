using UnityEngine;
using System.Collections;

public abstract class Balloon : MonoBehaviour, IDamageable
{
    public LayerMask mask;

    Coroutine activateDelayer;
    float activateDelay = 0.5f;
    float activateRange = 2f;
    float maxHealth = 5f;
    float health;
    bool active = false;
    
    void Awake()
    {
        health = maxHealth;
        active = true;
        StartCoroutine(Detect());
    }

    public void Spawn(Vector3 destiny)
    {
        StartCoroutine(GoToDestination(destiny));
    }

    public void ShowParticlesAndDie()
    {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        ps.GetComponent<AudioSource>().Play();
        ps.transform.SetParent(null);
        ps.Play();
        ps.GetComponent<ParticleDestroy>().Destroy();
        BalloonBoss.BossDeath -= ShowParticlesAndDie;
        Destroy(gameObject);
    }

    public abstract void Activate();

    public void TakeDamage(float damage)
    {
        if (!active)
            return;
        health -= damage;
        if (health <= 0 && activateDelayer == null)
            activateDelayer = StartCoroutine(TriggerActivation());
    }

    public void Die()
    {
        Activate();
    }

    IEnumerator GoToDestination(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = Vector3.Slerp(transform.position, destination, Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        active = true;
        StartCoroutine(Detect());
    }

    IEnumerator Detect()
    {
        while (active)
        {
            if (Physics.CheckSphere(transform.position, activateRange, mask) && activateDelayer == null)
            {
                activateDelayer = StartCoroutine(TriggerActivation());
                active = false;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator TriggerActivation()
    {
        yield return new WaitForSeconds(activateDelay);
        Activate();
    }
}
