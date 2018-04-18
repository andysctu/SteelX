using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCNstate : MechStateMachineBehaviour {


	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);
		if ( cc == null || !cc.enabled) return;
		animator.SetBool ("OnBCN", true);
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( cc == null || !cc.enabled) return;

		if(animator.GetBool("OnBCN"))
			mctrl.BCNPose ();
		
		animator.SetBool (boost_id, false);

		if(!animator.IsInTransition(0))
			mctrl.Boost (false);
	}

	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		if ( cc == null || !cc.enabled) return;
		animator.SetBool ("OnBCN", false);
	}
		
}
