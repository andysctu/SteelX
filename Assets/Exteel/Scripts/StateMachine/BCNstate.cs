using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCNstate : MechStateMachineBehaviour {


	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);
		if ( cc == null || !cc.enabled) return;
		animator.SetBool (OnBCN_id, true);
		animator.SetBool (jump_id, false);
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( cc == null || !cc.enabled) return;

		if(animator.GetBool(OnBCN_id))
			mctrl.BCNPose ();

		if (animator.GetBool(boost_id) && !animator.IsInTransition (0)) {
			animator.SetBool (boost_id, false);
			mctrl.Boost (false);
		}
	}

	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		if ( cc == null || !cc.enabled) return;
		animator.SetBool (OnBCN_id, false);
	}
		
}
