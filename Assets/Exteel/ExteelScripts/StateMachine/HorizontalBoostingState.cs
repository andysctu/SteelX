using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalBoostingState : MechStateMachineBehaviour {

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		mcbt.DecrementFuel();
		mctrl.Boost();

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat("Speed", speed);
		animator.SetFloat("Direction", direction);

		if (mcbt.FuelEmpty() || Input.GetKeyUp(KeyCode.LeftShift)) {
			animator.SetBool("Boost", false);
		}
	}

	//	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
