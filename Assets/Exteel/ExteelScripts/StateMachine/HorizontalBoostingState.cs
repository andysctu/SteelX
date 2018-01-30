using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalBoostingState : MechStateMachineBehaviour {
	static bool curBoostState = false;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (cc == null || !cc.enabled) return;

		if(curBoostState==false){
			curBoostState = true;
			mctrl.Boost (true);
			Debug.Log ("called set to true in horizontal boost.");
		}
		animator.SetBool ("OnSlash", false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled || !cc.isGrounded) return;

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat("Speed", speed);
		animator.SetFloat("Direction", direction);

		if ((mcbt.FuelEmpty() || !Input.GetKey(KeyCode.LeftShift)) && curBoostState == true) {
			animator.SetBool("Boost", false);
			mctrl.Boost (false);
			curBoostState = false;
			Debug.Log ("called set to false in horizontal boost.");
			return;
		}

		if (Input.GetKey(KeyCode.Space)) {

			if(curBoostState==true){
				mctrl.Boost (false);
				curBoostState = false;
			}
			Debug.Log ("called set to false in horizontal boost 2.");
			animator.SetBool("Boost", false);
			animator.SetBool("Grounded", false);
			animator.SetBool("Jump", true);
			mctrl.Jump();
		}
	}
}
