using UnityEngine;
using System.Collections;

public class BalloonBoss : MonoBehaviour, IDamageable, IMeleeAttackable
{
    public LayerMask mask;
    public GameObject[] balloons;
    public ParticleSystem water;
    public Transform nose,
        rightHand,
        leftHand;

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

    public delegate void BossDeathHandler();
    public static event BossDeathHandler BossDeath;

    AudioController audioCtrl;
    PlayerCombat pc;
    AudioSource waterSource;
    RaycastHit hit;
    Transform target,
        atkTransform;
    Coroutine damageDeal;
    Animator anim;
    float detectRange = 25f,
        rangedRange = 20f,
        meleeRange = 10f,
        explosionRange = 5f,
        explosionPush = 5f,
        explosionKnock = 16f,
        handRange = 2f,
        noseRange = 3f,
        waterDuration = 3f,
        minAtkCd = 2f,
        maxAtkCd = 3f,
        minBalloonRange = 8f,
        maxBalloonRange = 20f,
        chaseSens = 3.75f,
        maxHealth = 200,
        health,
        meleeDamage = 10f,
        noseDamage = 20f;
    bool attackable = true,
        toSpawn = true,
        toDie;
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
        audioCtrl = GetComponent<AudioController>();
        waterSource = water.GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        health = maxHealth;
    }

    void FixedUpdate()
    {
        if (toDie)
        {
            state = States.Dying;
            anim.SetTrigger("death");
            toDie = false;
            FindObjectOfType<HudController>().EnableCinematic();
            return;
        }
        if (toSpawn)
        {
            anim.SetTrigger("balloons");
            toSpawn = false;
            return;
        }
        if (target != null && state != States.Dying && state != States.Shooting)
        {
            Vector3 dir = targetCenterPos - centerPos;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSens);
        }
        if (state == States.Idle)
            Idle();
        else if (state == States.Engaging)
            Attacking();
        else if (state == States.Shooting)
            FaceWaterPlayer();
    }

    void Idle()
    {
        Collider[] cols = Physics.OverlapSphere(centerPos, detectRange, mask);
        if (cols.Length != 0)
        {
            target = cols[0].transform;
            pc = target.GetComponent<PlayerCombat>();
            if (Physics.Raycast(centerPos, targetCenterPos - centerPos, out hit, detectRange, ~(1 << gameObject.layer)))
            {
                if (hit.collider.tag == "Player")
                    state = States.Engaging;
            }
        }
        else
            target = null;
    }

    void Attacking()
    {
        if (!attackable)
            return;
        if (!Physics.CheckSphere(centerPos, detectRange, mask))
        {
            state = States.Idle;
            return;
        }

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
            anim.SetBool("water", true);
            state = States.Shooting;
            attackable = false;
        }
    }

    void MeleeAttack(string attackOrient)
    {
        if (attackOrient == "r")
            atkTransform = rightHand;
        else if (attackOrient == "l")
            atkTransform = leftHand;
        else
            throw new System.Exception("Wrong attack orientation");

        OpenDamage();
    }

    void WaterStart()
    {
        damageDeal = StartCoroutine(WaterShooting());
    }

    void Explode()
    {
        if (Physics.CheckSphere(nose.position, noseRange, mask))
        {
            audioCtrl.PlayClip("noseExplode");
            pc.TakeDamage(noseDamage);
            pc.Knockup((target.position - transform.position).normalized * explosionPush, explosionKnock);
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
        Balloon b;
        for (int i = 0; i < balloonCount; i++)
        {
            r = Random.Range(0, 8);
            if (r < 2)
                prefab = balloons[0];
            else if (r < 5)
                prefab = balloons[1];
            else if (r < 8)
                prefab = balloons[2];
            pos = Random.insideUnitSphere * maxBalloonRange;
            pos += transform.position;
            pos.y = 0;
            if (Vector3.Distance(pos, transform.position) < minBalloonRange)
                pos = (pos - transform.position).normalized * minBalloonRange + transform.position;
            pos.y = 2;
            b = (Instantiate(prefab, centerPos, Quaternion.identity) as GameObject).GetComponent<Balloon>();
            b.Spawn(pos);
            BossDeath += b.ShowParticlesAndDie;
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
        if (health <= 0)
            return;

        float prevHealth = health;
        health -= damage;
        audioCtrl.PlayClip("takeDamage");
        if (prevHealth > maxHealth / 2 && health <= maxHealth / 2)
            toSpawn = true;
        else if (prevHealth > maxHealth / 4 && health <= maxHealth / 4)
            toSpawn = true;
        if (health <= 0)
            toDie = true;
    }

    public void Die()
    {
        FindObjectOfType<HudController>().ToCredits();
        if (BossDeath != null && BossDeath.GetInvocationList().Length > 0)
            BossDeath();
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
                audioCtrl.PlayClip("damage");
                hit = true;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator WaterShooting()
    {
        water.Play();
        waterSource.Play();
        yield return new WaitForSeconds(waterDuration);
        anim.SetBool("water", false);
        waterSource.Stop();
        water.Stop();
        EndAttack();
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(Random.Range(minAtkCd, maxAtkCd));
        attackable = true;
    }
}