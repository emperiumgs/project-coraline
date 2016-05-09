using UnityEngine;
using System.Collections;

public class BalloonBoss : MonoBehaviour, IDamageable, IMeleeAttackable
{
    public LayerMask mask;
    public GameObject[] balloons;

    enum States
    {
        Idle,
        Engaging,
        MeleeAttacking,
        Exploding,
        Shooting,
        Dying
    }
    States state = States.Idle;

    ParticleSystem water;
    PlayerCombat pc;
    RaycastHit hit;
    Transform target;
    Transform nose;
    Transform rHand;
    Transform lHand;
    Transform atkTransform;
    Coroutine damageDeal;
    Animator anim;
    float detectRange = 25f;
    float rangedRange = 20f;
    float meleeRange = 10f;
    float explosionRange = 7f;
    float handRange = 2f;
    float noseRange = 4f;
    float waterDuration = 3f;
    float minAtkCd = 2f;
    float maxAtkCd = 3f;
    float minBalloonRange = 10f;
    float maxBalloonRange = 20f;
    float maxHealth = 200;
    float health;
    float meleeDamage = 10f;
    float noseDamage = 20f;
    bool attackable = true;
    int rotationSens = 10;
    int balloonCount = 15;

    void Awake()
    {
        pc = FindObjectOfType<PlayerCombat>();
        target = pc.transform;
        water = GetComponentInChildren<ParticleSystem>();
        nose = transform.FindChild("Nose");
        rHand = transform.FindChild("RHand");
        lHand = transform.FindChild("LHand");
        anim = GetComponent<Animator>();
        health = maxHealth;
        SpawnBalloons();
    }

    void FixedUpdate()
    {
        if (state != States.Dying && state != States.Shooting)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSens);
        }
        if (state == States.Idle)
            Idle();
        else if (state == States.Engaging)
            Attacking();
    }

    void Idle()
    {
        if (Physics.Raycast(transform.position, target.position - transform.position, out hit, detectRange, ~(1 << gameObject.layer)))
        {
            if (hit.collider.tag == "Player")
                state = States.Engaging;
        }
    }

    void Attacking()
    {
        if (!attackable)
            return;

        float dist = Vector3.Distance(transform.position, target.position);        
        if (dist <= explosionRange)
        {
            anim.SetTrigger("explosion");
            state = States.Exploding;
            attackable = false;
        }
        else if (dist <= meleeRange)
        {
            anim.SetBool("altMelee", Random.Range(0, 2) > 0);
            anim.SetTrigger("melee");
            state = States.MeleeAttacking;
            attackable = false;
        }
        else if (dist <= rangedRange)
        {
            damageDeal = StartCoroutine(WaterShooting());
            state = States.Shooting;
            attackable = false;
        }
    }

    void MeleeAttack(string attackOrient)
    {
        if (attackOrient == "r")
            atkTransform = rHand;
        else if (attackOrient == "l")
            atkTransform = lHand;

        OpenDamage();
    }

    void Explode()
    {
        if (Physics.CheckSphere(nose.position, noseRange, mask))
            pc.TakeDamage(noseDamage);
    }

    void EndAttack()
    {
        StartCoroutine(AttackCooldown());
        state = States.Engaging;
    }

    void SpawnBalloons()
    {
        GameObject prefab;
        Vector3 pos;
        for (int i = 0; i < balloonCount; i++)
        {
            prefab = balloons[Random.Range(0, balloons.Length)];
            pos = Random.insideUnitSphere * Random.Range(minBalloonRange, maxBalloonRange) + transform.position;
            pos.y = 2;
            (Instantiate(prefab, transform.position, Quaternion.identity) as GameObject).GetComponent<Balloon>().Spawn(pos);
        }
    }

    public void TakeDamage(float damage)
    {
        float prevHealth = health;
        health -= damage;
        if (prevHealth > maxHealth / 2 && health <= maxHealth / 2)
            SpawnBalloons();
        else if (prevHealth > maxHealth / 4 && health <= maxHealth / 4)
            SpawnBalloons();
        if (health <= 0)
        {
            state = States.Dying;
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void OpenDamage()
    {
        damageDeal = StartCoroutine(DamageDealing());
    }

    public void CloseDamage()
    {
        if (damageDeal != null)
            StopCoroutine(damageDeal);
    }

    public IEnumerator DamageDealing()
    {
        bool hit = false;
        while (!hit)
        {
            if (Physics.CheckSphere(atkTransform.position, handRange, mask))
            {
                pc.TakeDamage(meleeDamage);
                hit = true;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator WaterShooting()
    {
        water.Play();
        float time = 0;
        while (time < waterDuration)
        {
            time += Time.fixedDeltaTime;
            Vector3 dir = target.position - water.transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(water.transform.rotation, Quaternion.LookRotation(dir), Time.fixedDeltaTime * rotationSens / 4);
            yield return new WaitForFixedUpdate();
        }
        water.Stop();
        EndAttack();
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(Random.Range(minAtkCd, maxAtkCd));
        attackable = true;
    }
}
