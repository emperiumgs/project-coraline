using UnityEngine;

public class BalloonElectric : Balloon
{
    float stunRange = 1.5f;
    float stunAmount = 1f;
    float damage = 5f;

    protected override void Activate()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, stunRange, mask);
        if (cols.Length > 0)
        {
            IDamageable obj = cols[0].GetComponent<IDamageable>();
            if (obj != null)
                obj.TakeDamage(damage);
            IStunnable stun = cols[0].GetComponent<IStunnable>();
            if (stun != null)
                stun.Stun(stunAmount);
        }
        ShowParticlesAndDie();
    }
}