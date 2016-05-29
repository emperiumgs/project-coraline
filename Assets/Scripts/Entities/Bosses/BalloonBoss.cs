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
    Transform target,
        nose,
        rHand,
        lHand,
        flower,
		atkTransform;
    Coroutine damageDeal;
    Animator anim;
    float detectRange = 25f,
        rangedRange = 20f,
        meleeRange = 10f,
        explosionRange = 7f,
        handRange = 2f,
        noseRange = 4f,
        preWaterDuration = 1.5f,
        waterDuration = 3f,
        minAtkCd = 2f,
        maxAtkCd = 3f,
        minBalloonRange = 10f,
        maxBalloonRange = 25f,
        chaseSens = 3.75f,
        maxFlowerRot = 30f,
		maxHealth = 200,
		health,
		meleeDamage = 10f,
		noseDamage = 20f;
    bool attackable = true;
    int rotationSens = 10,
		balloonCount = 15;

    Vector3 centerPos
    {
        get { return transform.position + Vector3.up * 3; }
    }
    Vector3 targetCenterPos
    {
        get { return target.position + Vector3.up; }
    }

    void Awake()
    {
        pc = FindObjectOfType<PlayerCombat>();
        target = pc.transform;
        water = GetComponentInChildren<ParticleSystem>();
        nose = transform.FindChild("Nose");
        rHand = transform.FindChild("RHand");
        lHand = transform.FindChild("LHand");
        flower = transform.FindChild("Flower");
        anim = GetComponent<Animator>();
        health = maxHealth;
        SpawnBalloons();
    }

    void FixedUpdate()
    {
        if (state != States.Dying && state != States.Shooting)
        {
            Vector3 dir = targetCenterPos - centerPos;
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
        if (Physics.Raycast(centerPos, targetCenterPos - centerPos, out hit, detectRange, ~(1 << gameObject.layer)))
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
        {
            pc.TakeDamage(noseDamage);
            pc.Knockup(target.position - transform.position, 15);
        }
    }

    void EndAttack()
    {
        StartCoroutine(AttackCooldown());
        state = States.Engaging;
    }

    void SpawnBalloons()
    {
        GameObject prefab = null;
        Vector3 pos;
        int r;
        for (int i = 0; i < balloonCount; i++)
        {
            r = Random.Range(0, 8);
            if (r < 2)
                prefab = balloons[0];
            else if (r < 5)
                prefab = balloons[1];
            else if (r < 8)
                prefab = balloons[2];
            pos = Random.insideUnitSphere * Random.Range(minBalloonRange, maxBalloonRange) + transform.position;
            pos.y = 2;
            (Instantiate(prefab, centerPos, Quaternion.identity) as GameObject).GetComponent<Balloon>().Spawn(pos);
        }
    }

    void FaceWaterPlayer()
    {
        float dirY;
        Quaternion refRot;
        Vector3 dir = targetCenterPos - water.transform.position;
        dirY = dir.y;
        dir.y = 0;
        refRot = water.transform.rotation;
        refRot.eulerAngles = new Vector3(0, refRot.eulerAngles.y);
        transform.rotation = Quaternion.Slerp(refRot, Quaternion.LookRotation(dir), Time.fixedDeltaTime * chaseSens);
        dir.y = dirY;
        water.transform.rotation = Quaternion.Slerp(water.transform.rotation, Quaternion.LookRotation(dir), Time.fixedDeltaTime * chaseSens);
        water.transform.localEulerAngles = new Vector3(water.transform.localEulerAngles.x, 0);
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
        float time = 0,
            flowerRot;
        while (time < preWaterDuration)
        {
            time += Time.fixedDeltaTime;
            flowerRot = time / preWaterDuration * maxFlowerRot;
            flower.Rotate(Vector3.forward * flowerRot);
            FaceWaterPlayer();
            yield return new WaitForFixedUpdate();
        }
        time = 0;
        water.Play();
        while (time < waterDuration)
        {
            time += Time.fixedDeltaTime;
            flower.Rotate(Vector3.forward * maxFlowerRot);
            FaceWaterPlayer();
            yield return new WaitForFixedUpdate();
        }
        water.Stop();
        StartCoroutine(FlowerFade());
        EndAttack();
    }

    IEnumerator FlowerFade()
    {
        float time = 0,
            flowerRot;
        while (time < minAtkCd)
        {
            time += Time.fixedDeltaTime;
            flowerRot = (1 - time / minAtkCd) * maxFlowerRot;
            flower.Rotate(Vector3.forward * flowerRot);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(Random.Range(minAtkCd, maxAtkCd));
        attackable = true;
    }
}