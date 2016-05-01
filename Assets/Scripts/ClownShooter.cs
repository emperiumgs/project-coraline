using UnityEngine;
using System.Collections;

public class ClownShooter : MonoBehaviour, IDamageable
{
    public LayerMask mask;

    enum States
    {
        Idle,
        Shooting,
        Dying
    }

    ParticleSystem projectile;
    Transform target;
    float detectRange = 15f;
    float maxHealth = 50f;
    float health;
    bool attackable = true;

    void Awake()
    {
        health = maxHealth;
    }

    public void TakeDamage(float damage)
    {

    }

    public void Die()
    {

    }
}