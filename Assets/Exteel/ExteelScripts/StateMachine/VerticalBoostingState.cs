using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalBoostingState : MechStateMachineBehaviour {

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		mctrl.VerticalBoost();

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat("Speed", speed);
		animator.SetFloat("Direction", direction);

		if (mcbt.FuelEmpty() || !Input.GetKey(KeyCode.Space)) {
			animator.SetBool("Boost", false);
		}
	}
}
	