using UnityEngine;
using System.Collections;

public abstract class Collectible : MonoBehaviour
{
    public LayerMask mask;
    public Vector3 offsetPos;
    public float getRange = 1f;

    protected PlayerCombat pc;

    void Awake()
    {
        pc = FindObjectOfType<PlayerCombat>();
        StartCoroutine(CheckArea());
    }

    protected abstract void Collect();

    IEnumerator CheckArea()
    {
        bool collected = false;
        while (!collected)
        {
            if (Physics.CheckSphere(transform.position + offsetPos, getRange, mask))
            {
                collected = true;
                Collect();
            }
            yield return null;
        }
    }
}