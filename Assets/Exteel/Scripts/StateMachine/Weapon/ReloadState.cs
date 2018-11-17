using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadState : MechStateMachineBehaviour {
    public int hand = 0;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);
        mcbt.OnWeaponStateCallBack<RangedWeapon>(hand, this, (int)RangedWeapon.StateCallBackType.ReloadStateEnter);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);
        mcbt.OnWeaponStateCallBack<RangedWeapon>(hand, this, (int)RangedWeapon.StateCallBackType.ReloadStateExit);
    }
}
