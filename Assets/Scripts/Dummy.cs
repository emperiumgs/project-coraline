using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Dummy : MonoBehaviour, IDamageable
{
    [Range(20, 200)]
    public int maxHealth = 200;

    ParticleSystem destroy;
    float health;

    public Slider healthBar;
    public Text healthText;

    void Awake()
    {
        destroy = GetComponentInChildren<ParticleSystem>();
        health = maxHealth;
        healthBar.value = health / maxHealth;
        healthText.text = health + "/" + maxHealth;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        healthBar.value = health / maxHealth;
        healthText.text = health + "/" + maxHealth;
        if (health <= 0)
            Die();
    }

    public void Die()
    {        
        healthBar.enabled = false;
        destroy.Play();
        destroy.GetComponent<ParticleDestroy>().Destroy();
        destroy.transform.SetParent(null, true);        
        Destroy(gameObject);
    }
}
