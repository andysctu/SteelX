using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalBoostingState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.Init(animator);

        if ( !PhotonNetwork.isMasterClient || !cc.enabled) return;

		//animator.SetBool (animatorVars.OnMeleeHash, false);
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!cc.enabled) return;

	    if (!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) return;

	    animator.SetFloat(SpeedHash, mctrl.Speed);
	    animator.SetFloat(DirectionHash, mctrl.Direction);

	    animator.SetBool(BoostHash, mctrl.IsBoosting);

	    animator.SetBool(GroundedHash, mctrl.Grounded);
    }
}
	