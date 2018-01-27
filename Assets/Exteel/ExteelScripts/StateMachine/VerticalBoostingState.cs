using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalBoostingState : MechStateMachineBehaviour {
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (cc == null || !cc.enabled) return;
		mctrl.SetCanVerticalBoost(false);
		mctrl.Boost (true);
		animator.SetBool ("OnSlash", false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled) return;
		mctrl.VerticalBoost();

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat("Speed", speed);
		animator.SetFloat("Direction", direction);

		if (mcbt.FuelEmpty() || !Input.GetKey(KeyCode.Space)) {
			mctrl.Boost (false);
			animator.SetFloat("Speed", 0);
			animator.SetBool("Boost", false);
		}
	}
		
}
	