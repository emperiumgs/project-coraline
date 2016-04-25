using UnityEngine;
using System.Collections;

public class GoblinRocket : MonoBehaviour, IDamageable
{
    public LayerMask mask;
    public Transform bombPivot;
    public GameObject bombPrefab;

	enum States
    {
        Idle,
        Bombing,
        Rocketing,
        Dying
    }
    States state = States.Idle;

    ParticleSystem blast;
    NavMeshAgent agent;
    Transform target;
    Animator anim;
    float detectRange = 10f;
    float suicideRange = 5f;
    float explosionRange = 1.5f;
    float explosionRadius = 3f;
    float explosionDamage = 20f;
    float maxAtkCd = 1.5f;
    float minAtkCd = 0.5f;
    float maxHealth = 30f;
    float health;
    float rocketTimer = 4f;
    bool attackable = true;
    int rotationSens = 10;

    void Awake()
    {
        target = FindObjectOfType<PlayerPhysics>().transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        blast = GetComponentInChildren<ParticleSystem>(true);
        health = maxHealth;
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case States.Idle:
                Idle();
                break;
            case States.Bombing:
                Bombing();
                break;
            case States.Rocketing:
                Rocketing();
                break;
        }
    }

    void Idle()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, target.position - transform.position, out hit, detectRange))
        {
            if (hit.collider.tag == "Player")
                state = States.Bombing;
        }
    }

    void Bombing()
    {
        Vector3 dir = target.position - transform.position;
        if (Vector3.Angle(transform.forward, dir) > 1)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSens);
        else if (attackable)
        {
            attackable = false;
            anim.SetTrigger("bomb");
        }

        if (Vector3.Distance(transform.position, target.position) <= suicideRange)
            EngageRocket();
        if (Vector3.Distance(transform.position, target.position) > detectRange)
            state = States.Idle;        
    }

    void Rocketing()
    {
        if (attackable)
        {
            agent.SetDestination(target.position);
            if (Physics.CheckSphere(transform.position, explosionRange, mask))
                Die();
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
            Die();
        else if (health <= maxHealth / 4)
            EngageRocket();
    }

    public void Die()
    {
        state = States.Dying;
        blast.gameObject.SetActive(true);
        blast.transform.SetParent(null);
        blast.Play();
        blast.GetComponent<ParticleDestroy>().Destroy();
        if (Physics.CheckSphere(transform.position, explosionRadius, mask))
            target.GetComponent<IDamageable>().TakeDamage(explosionDamage);
        Destroy(gameObject);
    }

    void PlaceBomb()
    {
        GoblinBomb bomb = (Instantiate(bombPrefab, bombPivot.position, bombPivot.rotation) as GameObject).GetComponent<GoblinBomb>();
        bomb.target = target;
    }

    void EndBombing()
    {
        StartCoroutine(BombCooldown());
    }

    void EngageRocket()
    {
        anim.SetTrigger("rocket");
        state = States.Rocketing;
        attackable = false;
    }

    void LightRocket()
    {
        attackable = true;
        StartCoroutine(ExplosionTimer());
    }

    IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(rocketTimer);
        Die();
    }

    IEnumerator BombCooldown()
    {
        yield return new WaitForSeconds(Random.Range(minAtkCd, maxAtkCd));
        attackable = true;
    }
}
