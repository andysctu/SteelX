﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCNstate : MechStateMachineBehaviour {


	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.OnStateEnter(animator, stateInfo, layerIndex);
		animator.SetBool ("OnBCN", true);
		if ( cc == null || !cc.enabled || !cc.isGrounded) return;
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( cc == null || !cc.enabled || !cc.isGrounded) return;
		mctrl.BCNPose ();
		animator.SetBool (boost_id, false);

		if(!animator.IsInTransition(0))
			mctrl.Boost (false);
	}

	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		animator.SetBool ("OnBCN", false);
		if ( cc == null || !cc.enabled || !cc.isGrounded) return;
	}
		
}
