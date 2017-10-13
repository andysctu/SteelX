using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedState : MechStateMachineBehaviour {

	private bool jumpReleased = false;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter(animator, stateInfo, layerIndex);
		jumpReleased = false;
		mctrl.Jump();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc.isGrounded) {
			animator.SetBool ("Jump", false);
			animator.SetBool ("Grounded", true);
		}

		if (Input.GetKeyUp (KeyCode.Space)) {
			jumpReleased = true;
		}

		if (jumpReleased && Input.GetKey(KeyCode.Space)) {
			animator.SetBool("Boost", true);
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
