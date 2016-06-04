using UnityEngine;

public class BalloonElectric : Balloon
{
    float stunRange = 1.5f;
    float stunAmount = 1.25f;
    float damage = 5f;

    public override void Activate()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, stunRange, mask);
        if (cols.Length > 0)
        {
            IStunnable stun = cols[0].GetComponent<IStunnable>();
            if (stun != null)
                stun.Stun(stunAmount);
            IDamageable obj = cols[0].GetComponent<IDamageable>();
            if (obj != null)
                obj.TakeDamage(damage);
        }
        ShowParticlesAndDie();
    }
}