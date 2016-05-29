using UnityEngine;
using System.Collections;

public class GoblinRocket : MonoBehaviour, IDamageable
{
    public LayerMask mask;
    public Transform bombPivot;
    public GameObject bombPrefab;
    public Renderer bombRenderer;
    public Renderer rocketRenderer;
    public ParticleSystem rocketFire;

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
    float detectRange = 15f;
    float suicideRange = 5f;
    float explosionRange = 1.5f;
    float explosionRadius = 3f;
    float explosionDamage = 30f;
    float maxAtkCd = 1.5f;
    float minAtkCd = 0.5f;
    float maxHealth = 30f;
    float health;
    float rocketTimer = 4f;
    bool attackable = true;
    bool invulnerable;
    int rotationSens = 10;

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
        target = FindObjectOfType<PlayerPhysics>().transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        blast = GetComponentInChildren<ParticleSystem>();
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
        if (Physics.Raycast(centerPos, targetCenterPos - centerPos, out hit, detectRange))
        {
            if (hit.collider.tag == "Player")
                state = States.Bombing;
        }
    }

    void Bombing()
    {
        Vector3 dir = target.position - transform.position;
        if (Vector3.Angle(transform.forward, dir) > 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSens);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y);
        }
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
            if ((agent.velocity != Vector3.zero && agent.remainingDistance < 0.1f) || Physics.CheckSphere(centerPos, explosionRange, mask))
                Die();
    }

    public void TakeDamage(float damage)
    {
        if (invulnerable)
            return;

        health -= damage;
        if (health <= 0)
            Die();
        else if (health <= maxHealth / 4)
            EngageRocket();
    }

    public void Die()
    {
        if (state == States.Dying)
            return;
        state = States.Dying;
        blast.transform.SetParent(null);
        blast.Play();
        blast.GetComponent<ParticleDestroy>().Destroy();
        if (Physics.CheckSphere(centerPos, explosionRadius, mask))
            target.GetComponent<IDamageable>().TakeDamage(explosionDamage);
        rocketRenderer.enabled = false;
        rocketFire.Stop();
        anim.SetTrigger("die");
    }

    void Disappear()
    {
        Destroy(gameObject, 0.5f);
    }

    void PlaceBomb()
    {
        GoblinBomb bomb = (Instantiate(bombPrefab, bombPivot.position, bombPivot.rotation) as GameObject).GetComponent<GoblinBomb>();
        bomb.target = target;
        bombRenderer.enabled = false;
    }

    void EndBombing()
    {
        StartCoroutine(BombCooldown());
    }

    void EngageRocket()
    {
        attackable = false;
        invulnerable = true;
        anim.SetTrigger("rocket");
        state = States.Rocketing;
    }

    void LightRocket()
    {
        agent.SetDestination(target.position);
        StartCoroutine(ExplosionTimer());
        attackable = true;
        bombRenderer.enabled = false;
        rocketFire.Play();
    }

    IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(rocketTimer);
        Die();
    }

    IEnumerator BombCooldown()
    {
        yield return new WaitForSeconds(Random.Range(minAtkCd, maxAtkCd));
        bombRenderer.enabled = true;
        attackable = true;
    }
}