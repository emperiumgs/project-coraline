using UnityEngine;
using System.Collections;

public class GoblinBomb : MonoBehaviour
{
    [HideInInspector]
    public Transform target;

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void FixedUpdate()
    {

    }
}