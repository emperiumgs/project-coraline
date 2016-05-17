using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightningAttack : MonoBehaviour
{
    public Slider slider;
    public Text text;
    public Transform lightningPivot;
    public LineRenderer[] lightning,
        branches;
    public Light[] lights;
    public LayerMask mask;
    public AudioController audioCtrl;
    public bool striking { get; private set; }

    public static int MAX_RANGE = 13;
    public int MAX_BRANCH_SIZE = 5;
    public float RANDOM_THRESHOLD = 0.3f,
        CAST_TIME = 1f;
    
    Coroutine strike;
    Vector3 camPoint;
    Camera cam;
    public float damage = 2.5f,
        blastInterval = 0.05f,
        manaCost = 1,
        maxMana = 100f;
    float mana;
    public int maxBlasts = 20;
    int activeBranches;

	void Awake()
    {
        mana = maxMana;
        UpdateMana();
        lights[1].transform.SetParent(null);
        activeBranches = 0;
        cam = Camera.main;
        camPoint = new Vector3(Screen.width / 2, Screen.height / 2, MAX_RANGE + Mathf.Abs(cam.transform.localPosition.z) + lightningPivot.localPosition.z);
    }

    public void Strike()
    {
        striking = true;
        for (int i = 0; i < lightning.Length; i++)
            lightning[i].enabled = true;
        lights[0].enabled = true;
        strike = StartCoroutine(ContinuousStrike());
        //audioCtrl.PlayClip("blast");
    }

    public void AbortStrike()
    {
        DisableStrike();
        StopCoroutine(strike);
        //audioCtrl.StopClip();
    }
    
    public void RechargeEnergy(float amount)
    {
        mana += amount;
        if (mana > maxMana)
            mana = maxMana;
        UpdateMana();
    }

    void UpdateMana()
    {
        slider.value = mana / maxMana;
        text.text = "Mana: " + mana;
    }

    void DisableStrike()
    {
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

    IEnumerator ContinuousStrike()
    {
        int blasts = 0;
        Vector3 target, dir, camPos, ltngPos;
        RaycastHit hit;
        float maxDist;
        while (blasts < maxBlasts && mana > manaCost)
        {
            blasts++;
            mana -= manaCost;
            UpdateMana();
            camPos = cam.transform.position;
            ltngPos = lightningPivot.position;
            // Camera Raycasting
            target = cam.ScreenToWorldPoint(camPoint);
            dir = (target - camPos).normalized;
            maxDist = Vector3.Distance(camPos, target);
            if (Physics.Raycast(camPos, dir, out hit, maxDist, mask))
                target = hit.point;
            // Hand Raycasting
            dir = (target - ltngPos).normalized;
            maxDist = Vector3.Distance(ltngPos, target);
            if (Physics.Raycast(ltngPos, dir, out hit, maxDist, mask))
                target = hit.point;
            dir = (target - ltngPos).normalized;
            // Deal Damage
            if (hit.collider != null)
            {
                IDamageable col = hit.collider.GetComponent<IDamageable>();
                if (col != null)
                    col.TakeDamage(damage);
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
}