﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalBoostingState : MechStateMachineBehaviour {
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if ( cc == null || !cc.enabled || !cc.isGrounded) return;

		mcbt.CanSlash = true;
		mcbt.SetReceiveNextSlash (1);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled || !cc.isGrounded)
			return;

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat(speed_id, speed);
		animator.SetFloat(direction_id, direction);

		if (!Input.GetKey (KeyCode.LeftShift) || animator.GetBool(jump_id) ||animator.GetBool(onSlash_id) || !mcbt.IsFuelAvailable ()) {
			Sounds.StopBoostLoop ();
			animator.SetBool (boost_id, false);
			mctrl.Boost (false);
			return;
		} else {
			if (mctrl.grounded)
				mctrl.Boost (true);
		}
			
		if (Input.GetKey(KeyCode.Space) && !animator.GetBool(onSlash_id)) {	
			Sounds.StopBoostLoop ();
			mctrl.Boost (false);
			mctrl.SetCanVerticalBoost(true);
			mctrl.Jump();
			animator.SetBool(boost_id, false);
			animator.SetBool(grounded_id, false);
			mctrl.grounded = false;
			animator.SetBool(jump_id, true);
		}
	}
}