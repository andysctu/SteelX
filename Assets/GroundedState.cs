using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedState : StateMachineBehaviour {

	private CharacterController cc;
	private MechController mctrl;
	private MechCombat mcbt;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		cc = animator.transform.parent.gameObject.GetComponent<CharacterController>();
		mctrl = animator.transform.parent.gameObject.GetComponent<MechController>();
		mcbt = animator.transform.parent.gameObject.GetComponent<MechCombat>();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		if (cc.isGrounded) {
			if (Input.GetKey(KeyCode.Space)) {
				animator.SetBool("Jump", true);

				mctrl.ySpeed = mcbt.JumpPower();
				mctrl.UpdateSpeed(0);
			}
		} else {
			
		}

		animator.SetFloat("Speed", speed);
		animator.SetFloat("Direction", direction);
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
