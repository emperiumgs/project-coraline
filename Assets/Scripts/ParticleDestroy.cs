using UnityEngine;
using System.Collections;

public class ParticleDestroy : MonoBehaviour
{
    public void Destroy()
    {
        ParticleSystem particle = GetComponent<ParticleSystem>();
        float time = Mathf.Max(particle.startLifetime, particle.duration);
        Destroy(gameObject, time);
    }
}
