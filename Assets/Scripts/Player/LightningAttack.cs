using UnityEngine;
using System.Collections;

public class LightningAttack : MonoBehaviour
{
    public Light manaLight;
    public Color charged,
        uncharged;
    public Transform lightningPivot;
    public LineRenderer[] lightning,
        branches;
    public Light[] lights;
    public LayerMask mask;
    public AudioController audioCtrl;
    public bool striking { get; private set; }

    public const int MAX_RANGE = 13;
    const int MAX_BRANCH_SIZE = 5,
        LOW_MANA_FLASHES = 4;
    const float RANDOM_THRESHOLD = 0.3f,
        CAST_TIME = 1f,
        LOW_MANA_FLASH_TIME = 0.1f;

    LayerMask ignoreLayer;
    Coroutine strike;
    Vector3 camPoint;
    Camera cam;
    float damage = 2.5f,
        blastInterval = 0.05f,
        maxMana = 100,
        mana,
        manaCost = 1f,
        maxManaIntensity = 4f;
    int maxBlasts = 20,
        activeBranches;

	void Awake()
    {
        mana = maxMana;
        ignoreLayer = LayerMask.NameToLayer("Ignore Lightning");
        AdjustManaFeedback();
        lights[1].transform.SetParent(null);
        activeBranches = 0;
        cam = Camera.main;
        camPoint = new Vector3(Screen.width / 2, Screen.height / 2, MAX_RANGE + Mathf.Abs(cam.transform.localPosition.z) + lightningPivot.localPosition.z);
    }

    public void Strike()
    {
        if (mana < manaCost)
        {
            StartCoroutine(FlashMana());
            return;
        }
        striking = true;
        for (int i = 0; i < lightning.Length; i++)
            lightning[i].enabled = true;
        lights[0].enabled = true;
        strike = StartCoroutine(ContinuousStrike());
        audioCtrl.PlayClip("blast");
    }

    public void AbortStrike()
    {
        DisableStrike();
        StopCoroutine(strike);
    }

    void DisableStrike()
    {
        audioCtrl.StopClip();
        if (mana < manaCost)
            audioCtrl.PlayClip("lowEnergy");
        striking = false;
        activeBranches = 0;
        for (int i = 0; i < branches.Length; i++)
        {
            branches[i].enabled = false;
            if (i < lightning.Length)
            {
                lights[i].enabled = false;
                lightning[i].enabled = false;
            }
        }
    }

    void DevelopBranch(Vector3 initPos)
    {
        activeBranches++;
        LineRenderer branch = branches[activeBranches - 1];
        branch.enabled = true;
        int nodes = Random.Range(2, MAX_BRANCH_SIZE);
        branch.SetVertexCount(nodes);
        Vector3[] nodePos = new Vector3[nodes];
        Vector3 dir = Random.insideUnitSphere;
        for (int i = 0; i < nodes; i++)
        {
            if (i == 0)
                nodePos[i] = initPos;
            else
            {
                nodePos[i] = Random.insideUnitSphere * RANDOM_THRESHOLD + i * dir + initPos;
                if (i == 1)
                    dir = (nodePos[i] - initPos) / 2;
            }
        }
        branch.SetPositions(nodePos);
    }

    void AdjustManaFeedback()
    {
        if (mana > 0)
            manaLight.intensity = mana / maxMana * maxManaIntensity + 1;
        else
            manaLight.color = uncharged;
    }

    public void RechargeEnergy(float amount)
    {
        if (mana <= 0)
            manaLight.color = charged;
        mana += amount;
        if (mana > maxMana)
            mana = maxMana;
        AdjustManaFeedback();
    }

    IEnumerator ContinuousStrike()
    {
        int blasts = 0;
        Vector3 target, dir, camPos, ltngPos;
        RaycastHit hit;
        float maxDist;
        while (blasts < maxBlasts && mana >= manaCost)
        {
            blasts++;
            mana -= manaCost;
            AdjustManaFeedback();
            camPos = cam.transform.position;
            ltngPos = lightningPivot.position;
            // Camera Raycasting
            target = cam.ScreenToWorldPoint(camPoint);
            dir = (target - camPos).normalized;
            maxDist = Vector3.Distance(camPos, target) + 1;
            if (Physics.Raycast(camPos, dir, out hit, maxDist, mask))
                target = hit.point;
            // Hand Raycasting
            dir = (target - ltngPos).normalized;
            maxDist = Vector3.Distance(ltngPos, target) + 1;
            if (Physics.Raycast(ltngPos, dir, out hit, maxDist, mask))
                target = hit.point;
            dir = (target - ltngPos).normalized;
            // Deal Damage
            if (hit.collider != null && hit.collider.gameObject.layer != ignoreLayer)
            {
                IDamageable col = hit.collider.GetComponentInParent<IDamageable>();
                if (col != null)
                {
                    audioCtrl.PlayClip("hit");
                    col.TakeDamage(damage);
                }
            }
            // Calculate node count
            float dist = Vector3.Distance(ltngPos, target);
            int nodes = Mathf.CeilToInt(dist);
            Vector3[] nodePos = new Vector3[nodes];
            // Calculate node position
            for (int l = 0; l < lightning.Length; l++)
            {
                lightning[l].SetVertexCount(nodes);
                for (int i = 0; i < nodes; i++)
                {
                    if (i == 0)
                        nodePos[i] = ltngPos;
                    else if (i == nodes - 1)
                        nodePos[i] = target;
                    else
                    {
                        nodePos[i] = Random.insideUnitSphere * RANDOM_THRESHOLD * (1 + l) + i * dir + ltngPos;
                        if (nodes > 2 && activeBranches < branches.Length && Random.Range(0, 10) < i)
                            DevelopBranch(nodePos[i]);
                    }
                }
                if (nodes > 2)
                {
                    lights[1].enabled = true;
                    lights[1].transform.position = nodePos[nodes - 2];
                }
                else
                    lights[1].enabled = false;

                lightning[l].SetPositions(nodePos);
            }
            if (activeBranches < branches.Length - 1)
            {
                for (int i = branches.Length - 1; i > activeBranches - 1; i--)
                    branches[i].enabled = false;
            }
            activeBranches = 0;
            yield return new WaitForSeconds(blastInterval);
        }
        DisableStrike();
    }

    IEnumerator FlashMana()
    {
        int flashes = 0;
        while (mana < manaCost && flashes < LOW_MANA_FLASHES)
        {
            manaLight.enabled = flashes % 2 == 0 ? false : true;
            flashes++;
            yield return new WaitForSeconds(LOW_MANA_FLASH_TIME); 
        }
    }
}