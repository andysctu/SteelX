﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.Init(animator);
		if (cc == null || !cc.enabled) return;
		animator.SetBool (grounded_id, true);//in case respawn not grounded
		animator.SetBool (onMelee_id, false);
		mcbt.CanMeleeAttack = true;//CanMeleeAttack is to avoid multi-slash in air 
		mctrl.grounded = true;
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled)
			return;

		animator.SetFloat(speed_id, mctrl.speed);
		animator.SetFloat(direction_id, mctrl.direction);

		if(animator.GetBool(jump_id)){
			mctrl.Run ();//not lose speed in air
			return;
		}

		if(!mctrl.CheckIsGrounded()){//check not jumping but is falling
			mctrl.grounded = false;
			mctrl.SetCanVerticalBoost (true);
			animator.SetBool (grounded_id, false);
			return;
		}

		if (!gm.BlockInput && Input.GetKeyDown(KeyCode.Space) && !animator.GetBool(onMelee_id) ) {
			mctrl.SetCanVerticalBoost (true);
			mctrl.grounded = false;
			animator.SetBool(grounded_id, false);
			animator.SetBool(jump_id, true);
			return;
		}

		if (!gm.BlockInput && Input.GetKey(KeyCode.LeftShift) && mcbt.EnoughENToBoost()) {
			if (mctrl.speed > 0 || mctrl.speed < 0 || mctrl.direction > 0 || mctrl.direction < 0) {	
				animator.SetBool (boost_id, true);
				mctrl.Boost (true);
			}else{
				mctrl.Boost (true);
			}
		}else{
			mctrl.Boost (false);
			mctrl.Run();
		}
		
	}
}
