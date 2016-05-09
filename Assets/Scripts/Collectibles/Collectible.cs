using UnityEngine;
using System.Collections;

public abstract class Collectible : MonoBehaviour
{
    public LayerMask mask;

    protected PlayerCombat pc;

    float getRange = 1.5f;

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
            if (Physics.CheckSphere(transform.position, getRange, mask))
            {
                collected = true;
                Collect();
            }
            yield return null;
        }
    }
}