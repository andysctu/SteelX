using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedState : MechStateMachineBehaviour {

	private bool jumpReleased = false;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (cc == null || !cc.enabled) return;
		jumpReleased = false;
		//mctrl.SetCanVerticalBoost(true);  //CanVerticalBoost  is set to true when grounded

		if(animator.GetBool ("OnSlash")==true){
			animator.SetBool ("Boost", false);
			mctrl.SetCanVerticalBoost (false);
			mctrl.Boost (false);
		}

		animator.SetBool ("OnSlash", false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled) {
			return;
		}
			
		if (cc.isGrounded) {
			animator.SetBool("Jump", false);
			animator.SetBool("Grounded", true);
			mctrl.SetCanVerticalBoost(true);
		}

		if (Input.GetKeyUp(KeyCode.Space)) {
			jumpReleased = true;
		}

		if (jumpReleased && mctrl.CanVerticalBoost() && Input.GetKey(KeyCode.Space)) {
			animator.SetBool("Boost", true);
			mctrl.Boost (true);
			mctrl.GetComponentInChildren<Sounds> ().PlayBoostStart ();
			//animator.CrossFade ("BoostJump", 0.3f);//does not work as expected
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
