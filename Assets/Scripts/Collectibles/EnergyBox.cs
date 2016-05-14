using UnityEngine;
using System.Collections;

public class EnergyBox : Collectible
{
    float energyAmount = 20f;

    protected override void Collect()
    {
        pc.RechargeEnergy(energyAmount);
        Destroy(gameObject);
    }
}