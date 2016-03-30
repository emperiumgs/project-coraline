using UnityEngine;
using System.Collections;

public class Lightning : MonoBehaviour
{
    const int MAX_RANGE = 10;
    const float RANDOM_THRESHOLD = 0.3f;
    const float CAST_TIME = 1f;

    LineRenderer lr;
    Camera cam;
    Vector3 camPoint;
    LayerMask mask;
    Coroutine strike;

	void Awake()
    {
        lr = GetComponent<LineRenderer>();
        cam = Camera.main;
        camPoint = new Vector3(Screen.width / 2, Screen.height / 2, MAX_RANGE - cam.transform.localPosition.z + transform.localPosition.z);
        mask = ~(1 << LayerMask.NameToLayer("Player"));
    }

    public void Strike()
    {
        lr.enabled = true;
        strike = StartCoroutine(ContinuousStrike());
    }

    public void AbortStrike()
    {
        lr.enabled = false;
        StopCoroutine(strike);
    }

    IEnumerator ContinuousStrike()
    {
        float time = 0;
        while (time < CAST_TIME)
        {
            time += Time.fixedDeltaTime;

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

            lr.SetVertexCount(nodes);
            Vector3[] nodesPos = new Vector3[nodes];
            for (int i = 0; i < nodes; i++)
            {
                if (i == 0)
                    nodesPos[i] = transform.position;
                else if (i == nodes - 1)
                    nodesPos[i] = target;
                else
                    nodesPos[i] = Random.insideUnitSphere * RANDOM_THRESHOLD + i * dir + transform.position;
            }
            lr.SetPositions(nodesPos);

            yield return new WaitForFixedUpdate();
        }

        lr.enabled = false;
    }
}
