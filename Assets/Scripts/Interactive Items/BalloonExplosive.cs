using UnityEngine;

public class BalloonExplosive : Balloon
{
    float explosionRange = 1.5f;
    float damage = 25f;

    public override void Activate()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, explosionRange, mask);
        if (cols.Length > 0)
        {
            IDamageable obj = cols[0].GetComponent<IDamageable>();
            obj.TakeDamage(damage);
        }
        ShowParticlesAndDie();
    }
}
