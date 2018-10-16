using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalBoostingState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.Init(animator);
		if ( !PhotonNetwork.isMasterClient || !cc.enabled) return;
		//mctrl.SetCanVerticalBoost(false);
		//animator.SetBool (animatorVars.OnMeleeHash, false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!PhotonNetwork.isMasterClient || !cc.enabled) return;

		//animator.SetFloat(SpeedHash, mctrl.speed);
		//animator.SetFloat(DirectionHash, mctrl.direction);

		//if ( mcbt.IsENEmpty() || !HandleInputs.CurUserCmd.Buttons[(int)HandleInputs.Button.Space]) {
		//	//mctrl.Boost (false);
		//	//animator.SetFloat(SpeedHash, 0);
		//	animator.SetBool(BoostHash, false);
		//}else{
		//	mctrl.VerticalBoost();
		//}
	}
}
	