using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Dummy : MonoBehaviour, IDamageable
{
    const int MAX_HEALTH = 200;
    float health;

    public Slider healthBar;

    void Awake()
    {
        health = MAX_HEALTH;
        healthBar.value = health / MAX_HEALTH;
    }

    public void TakeDamage(float damage)
    {
        print("<color=blue>Ouch! I took </color><color=red>" + damage + " damage</color><color=blue>! Now my health is </color><color=green>" + health + "</color>");
        health -= damage;
        healthBar.value = health / MAX_HEALTH;
        if (health <= 0)
            Die();
    }

    public void Die()
    {
        gameObject.SetActive(false);
        healthBar.enabled = false;
    }
}
