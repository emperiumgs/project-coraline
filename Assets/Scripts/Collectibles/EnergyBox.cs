using UnityEngine;
using System.Collections;

public class EnergyBox : Collectible
{
    public float energyAmount = 20f;

    protected override void Collect()
    {
        ltng.RechargeEnergy(energyAmount);
        Destroy(gameObject);
    }
}