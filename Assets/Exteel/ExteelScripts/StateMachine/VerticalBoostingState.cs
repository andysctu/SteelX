using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalBoostingState : MechStateMachineBehaviour {
	static bool curBoostState = true;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (cc == null || !cc.enabled) return;
		mctrl.SetCanVerticalBoost(false);
		animator.SetBool (onSlash_id, false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled) return;
		mctrl.VerticalBoost();

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat(speed_id, speed);
		animator.SetFloat(direction_id, direction);

		if ( (mcbt.FuelEmpty() || !Input.GetKey(KeyCode.Space))) {
			mctrl.Boost (false);
			animator.SetFloat(speed_id, 0);
			animator.SetBool(boost_id, false);
		}
	}
		
}
	