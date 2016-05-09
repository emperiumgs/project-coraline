using UnityEngine;
using System.Collections;

public interface IMeleeAttackable
{
    void OpenDamage();
    void CloseDamage();
    IEnumerator DamageDealing();
}
