using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalBoostingState : MechStateMachineBehaviour {
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.Init(animator);
		if ( cc == null || !cc.enabled || !cc.isGrounded) return;

		mcbt.CanMeleeAttack = true;
		mcbt.SetReceiveNextSlash (1);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		EffectController.UpdateBoostingDust ();
		if (cc == null || !cc.enabled)
			return;

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat(speed_id, speed);
		animator.SetFloat(direction_id, direction);

		if(animator.GetBool(jump_id)){
            return;
		}

		if(!mctrl.CheckIsGrounded()){//falling
			mctrl.SetCanVerticalBoost (true);
			mctrl.grounded = false;
			animator.SetBool (grounded_id, false);
			animator.SetBool (boost_id, false);//avoid dir go to next state (the transition interrupts by next state)
			mctrl.Boost (false);
			return;
		}
        
		if (Input.GetKeyDown(KeyCode.Space)) {
			mctrl.SetCanVerticalBoost(true);
			animator.SetBool(jump_id, true);
		}

		if (!Input.GetKey (KeyCode.LeftShift) || !mcbt.IsFuelAvailable ()) {
			mctrl.Run ();
			animator.SetBool (boost_id, false);
			mctrl.Boost (false);
			return;
		} else {
			animator.SetBool (boost_id, true);
			mctrl.Boost (true);
		}
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( cc == null || !cc.enabled || !cc.isGrounded) return;

	}
}
