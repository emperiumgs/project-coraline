using UnityEngine;

public class PlayerCombatFinalComboState : PlayerCombatComboState
{
    public override void OnStateExit(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(anim, stateInfo, layerIndex);
        pc.ToggleAttack();
    }
}
