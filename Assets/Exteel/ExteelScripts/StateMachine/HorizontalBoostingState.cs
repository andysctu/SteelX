using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalBoostingState : MechStateMachineBehaviour {

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled || !cc.isGrounded) return;
		mctrl.Boost();

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat("Speed", speed);
		animator.SetFloat("Direction", direction);

		if (mcbt.FuelEmpty() || !Input.GetKey(KeyCode.LeftShift)) {
			animator.SetBool("Boost", false);
			return;
		}

		if (Input.GetKey(KeyCode.Space)) {
			animator.SetBool("Boost", false);
			animator.SetBool("Grounded", false);
			animator.SetBool("Jump", true);
		}
	}
}
