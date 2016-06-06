using UnityEngine;
using System.Collections;

public abstract class Collectible : MonoBehaviour
{
    public LayerMask mask;
    public Vector3 offsetPos;
    public float getRange = 1f;

    Transform mesh;
    int rotation = 50;

    void Start()
    {
        mesh = transform.GetChild(0);
        StartCoroutine(CheckArea());
    }

    void Update()
    {
        mesh.Rotate(Vector3.up * rotation * Time.deltaTime);
    }

    protected abstract void Collect(PlayerCombat pc);

    IEnumerator CheckArea()
    {
        bool collected = false;
        Collider[] cols;
        while (!collected)
        {
            cols = Physics.OverlapSphere(transform.position + offsetPos, getRange, mask);
            if (cols.Length > 0)
            {
                collected = true;
                Collect(cols[0].GetComponent<PlayerCombat>());
            }
            yield return null;
        }
    }
}