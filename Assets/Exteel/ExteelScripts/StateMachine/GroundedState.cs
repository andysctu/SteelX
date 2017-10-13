using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedState : MechStateMachineBehaviour {

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled || !cc.isGrounded) return;
		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		if (Input.GetKey(KeyCode.Space)) {
			animator.SetBool("Grounded", false);
			animator.SetBool("Jump", true);
		}

		if (speed > 0 || speed < 0 || direction > 0 || direction < 0) {
			mctrl.Run();

			if (Input.GetKey(KeyCode.LeftShift) && mcbt.EnoughFuelToBoost()) {
				animator.SetBool("Boost", true);
				mctrl.Boost();
			}
		}
			
		animator.SetFloat("Speed", speed);
		animator.SetFloat("Direction", direction);
	}
}
