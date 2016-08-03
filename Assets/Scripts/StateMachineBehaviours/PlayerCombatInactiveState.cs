using UnityEngine;

public class PlayerCombatInactiveState : PlayerCombatStateBase
{
    public override void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(anim, stateInfo, layerIndex);
        pc.ClearCombo();
    }
}
