using UnityEngine;

public class MetalGears : Collectible
{
    float healingAmount = 50f;

    protected override void Collect(PlayerCombat pc)
    {
        pc.RechargeLife(healingAmount);
        Destroy(gameObject);
    }
}