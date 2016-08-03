using UnityEngine;

public class PlayerCombatComboState : PlayerCombatStateBase
{
    public override void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(anim, stateInfo, layerIndex);
        pc.ToggleAttack();
    }
}
