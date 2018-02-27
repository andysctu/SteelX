using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedState : MechStateMachineBehaviour {

	static public bool jumpReleased = false;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (cc == null || !cc.enabled) return;
		//jumpReleased = false;

		if(animator.GetBool (onSlash_id)){ //after slashing in air , shut the boost down , otherwise it will go to boost jump
			animator.SetBool (boost_id, false);
			animator.SetBool (onSlash_id, false);
			mctrl.SetCanVerticalBoost (false);
			mctrl.Boost (false);
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled) {
			return;
		}
			
		if (cc.isGrounded) {
			animator.SetBool(jump_id, false);
			animator.SetBool(grounded_id, true);
			mctrl.grounded = true;
			mctrl.SetCanVerticalBoost (false);
			jumpReleased = false;
			return;
		}

		if (Input.GetKeyUp(KeyCode.Space)) {
			jumpReleased = true;
		}
		if (Input.GetKey(KeyCode.Space) && jumpReleased && mctrl.CanVerticalBoost()) {
			jumpReleased = false;
			animator.SetBool(boost_id, true);
			mctrl.Boost (true);
			Sounds.PlayBoostStart ();
			Sounds.PlayBoostLoop ();
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
