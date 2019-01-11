﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalBoostingState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.Init(animator);
		if ( cc == null || !cc.enabled) return;
		mctrl.SetCanVerticalBoost(false);
		animator.SetBool (onMelee_id, false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( cc == null || !cc.enabled) return;
		//mctrl.VerticalBoost();

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat(speed_id, speed);
		animator.SetFloat(direction_id, direction);

		if ( (mcbt.IsENEmpty() || !Input.GetKey(KeyCode.Space) || gm.BlockInput)) {
			mctrl.Boost (false);
			animator.SetFloat(speed_id, 0);
			animator.SetBool(boost_id, false);
		}else{
			mctrl.VerticalBoost();
		}
	}
}
	