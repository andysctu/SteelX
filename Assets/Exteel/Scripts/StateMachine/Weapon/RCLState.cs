using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCLState : MechStateMachineBehaviour {

    public int hand = 0;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);
        if (mcbt == null) return;
        mcbt.OnWeaponStateCallBack<RangedWeapon>(hand, this, (int)RangedWeapon.StateCallBackType.AttackStateEnter);
        mechIK.SetIK(true, 2, hand);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (mcbt == null) return;
        mcbt.OnWeaponStateCallBack<RangedWeapon>(hand, this, (int)RangedWeapon.StateCallBackType.AttackStateUpdate);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (mcbt == null) return;
        mechIK.SetIK(false, 2, hand);
        mcbt.OnWeaponStateCallBack<RangedWeapon>(hand, this, (int)RangedWeapon.StateCallBackType.AttackStateExit);
    }
}
