using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Dummy : MonoBehaviour, IDamageable
{
    [Range(20, 200)]
    public int maxHealth = 200;

    ParticleSystem destroy;
    float health;
    
    public Text healthText;

    void Awake()
    {
        destroy = GetComponentInChildren<ParticleSystem>();
        health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
            Die();
    }

    public void Die()
    {        
        destroy.Play();
        destroy.GetComponent<ParticleDestroy>().Destroy();
        destroy.transform.SetParent(null, true);        
        Destroy(gameObject);
    }
}
