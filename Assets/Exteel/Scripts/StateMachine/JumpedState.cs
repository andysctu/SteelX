using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedState : MechStateMachineBehaviour {

	public static bool jumpReleased = false;
	bool jumpFirstCall = false;

	override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
		base.Init (animator);
		if (cc == null || !cc.enabled)return;

		jumpReleased = false;
		mcbt.CanSlash = true;
		jumpFirstCall = true;
	}
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init (animator);
		if (cc == null || !cc.enabled)return;
		animator.SetBool (onSlash_id, false);
	}
	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled)return;

		if (Input.GetKeyUp(KeyCode.Space)) {
			jumpReleased = true;
		}

		if (Input.GetKey(KeyCode.Space) && jumpReleased && mctrl.CanVerticalBoost()) {
			mctrl.SetCanVerticalBoost (false);
			jumpReleased = false;
			animator.SetBool(boost_id, true);
			mctrl.Boost (true);
		}

		if (!jumpFirstCall && cc.isGrounded) { //falling->end jump  
			animator.SetBool(grounded_id, true);
			animator.SetBool (jump_id, false);
			mctrl.grounded = true;
			mctrl.SetCanVerticalBoost (false);
			return;
		}else{
			mctrl.JumpMoveInAir ();
		}
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled)return;

		if (jumpFirstCall) {//call after the jump01 end
			jumpFirstCall = false;
			mctrl.Jump ();
		}
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
