using UnityEngine;
using System.Collections;

public class EnergyBox : Collectible
{
    float energyAmount = 50f;

    protected override void Collect()
    {
        pc.RechargeEnergy(energyAmount);
        Destroy(gameObject);
    }
}