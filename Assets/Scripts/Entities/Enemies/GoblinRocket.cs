using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GoblinRocket : MonoBehaviour, IDamageable
{
    public LayerMask mask;
    public Slider slider;
    public Text text;
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
    public float detectRange = 10f,
        suicideRange = 5f,
        explosionRange = 1.5f,
        explosionRadius = 3f,
        explosionDamage = 20f,
        maxAtkCd = 1.5f,
        minAtkCd = 0.5f,
        maxHealth = 30f,
        rocketTimer = 4f;
    float health;
    bool attackable = true;
    bool invulnerable;
    public int rotationSens = 10;

    void Awake()
    {
        target = FindObjectOfType<PlayerPhysics>().transform;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        blast = GetComponentInChildren<ParticleSystem>(true);
        health = maxHealth;
        UpdateHealth();
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
            if ((agent.velocity != Vector3.zero && agent.remainingDistance < 0.1f) || Physics.CheckSphere(transform.position, explosionRange, mask))
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
        UpdateHealth();
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