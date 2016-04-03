using UnityEngine;
using System.Collections;

public class Lightning : MonoBehaviour
{
    const int MAX_RANGE = 10;
    const int MAX_BRANCH_SIZE = 5;
    const float RANDOM_THRESHOLD = 0.3f;
    const float CAST_TIME = 1f;

    LineRenderer[] lightning;
    LineRenderer[] branches;
    Camera cam;
    Vector3 camPoint;
    LayerMask mask;
    Coroutine strike;
    int activeBranches;

    Light[] lights;

	void Awake()
    {
        lightning = transform.GetChild(0).GetComponentsInChildren<LineRenderer>(true);
        branches = transform.GetChild(1).GetComponentsInChildren<LineRenderer>(true);
        activeBranches = 0;
        lights = GetComponentsInChildren<Light>();
        cam = Camera.main;
        camPoint = new Vector3(Screen.width / 2, Screen.height / 2, MAX_RANGE - cam.transform.localPosition.z + transform.localPosition.z);
        mask = ~(1 << LayerMask.NameToLayer("Player"));
    }

    public void Strike()
    {
        for (int i = 0; i < lightning.Length; i++)
            lightning[i].enabled = true;
        lights[0].enabled = true;
        strike = StartCoroutine(ContinuousStrike());
    }

    public void AbortStrike()
    {
        DisableStrike();
        StopCoroutine(strike);
    }

    void DisableStrike()
    {
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
        float time = 0;
        while (time < CAST_TIME)
        {
            time += 0.05f;

            Vector3 target = cam.ScreenToWorldPoint(camPoint);
            Vector3 dir = (target - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, MAX_RANGE, mask))
            {
                target = hit.point;
                dir = (target - transform.position).normalized;
            }

            float dist = Vector3.Distance(transform.position, target);
            int nodes = Mathf.CeilToInt(dist);
            Vector3[] nodePos = new Vector3[nodes];

            for (int l = 0; l < lightning.Length; l++)
            {
                lightning[l].SetVertexCount(nodes);
                for (int i = 0; i < nodes; i++)
                {
                    if (i == 0)
                        nodePos[i] = transform.position;
                    else if (i == nodes - 1)
                        nodePos[i] = target;
                    else
                    {
                        nodePos[i] = Random.insideUnitSphere * RANDOM_THRESHOLD * (1 + l) + i * dir + transform.position;
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

            yield return new WaitForSeconds(0.05f);
        }

        DisableStrike();
    }
}