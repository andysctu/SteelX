using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalBoostingState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (cc == null || !cc.enabled) return;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled || !cc.isGrounded) return;

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat("Speed", speed);
		animator.SetFloat("Direction", direction);
		if ((mcbt.FuelEmpty () || !Input.GetKey (KeyCode.LeftShift))) {
			animator.SetBool ("Boost", false);
			mctrl.Boost (false);
			return;
		} else {
			mctrl.Boost (true);
		}


		if (Input.GetKey(KeyCode.Space)) {
			mctrl.Boost (false);
			mctrl.SetCanVerticalBoost(true);
			mctrl.Jump();
			animator.SetBool("Boost", false);
			animator.SetBool("Grounded", false);
			animator.SetBool("Jump", true);
		}
	}
}
