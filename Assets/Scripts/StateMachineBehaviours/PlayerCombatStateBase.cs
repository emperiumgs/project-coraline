using UnityEngine;
using System.Collections;

public class PlayerCombatStateBase : StateMachineBehaviour
{
    protected PlayerCombat pc;

    public override void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = anim.GetComponent<PlayerCombat>();
    }
}
