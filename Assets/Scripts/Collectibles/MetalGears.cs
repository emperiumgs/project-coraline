using UnityEngine;

public class MetalGears : Collectible
{
    public float healingAmount = 20f;

    protected override void Collect()
    {
        pc.RechargeLife(healingAmount);
        Destroy(gameObject);
    }
}