using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedState : MechStateMachineBehaviour {

	public static bool jumpReleased = false;
	public bool isFirstjump = false;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init (animator);
		if (cc == null || !cc.enabled)return;

		if(isFirstjump){
			mctrl.Boost (false);
			animator.SetBool (boost_id, false);//avoid shift+space directly vertically boost
			jumpReleased = false;
			mctrl.grounded = false;
			animator.SetBool (grounded_id, false);
		}else if(!Input.GetKey(KeyCode.Space)){//dir falling
			animator.SetBool (jump_id, true);
			jumpReleased = true;
			mctrl.grounded = false;
			animator.SetBool (grounded_id, false);
		}
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled)return;

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat(speed_id, speed);
		animator.SetFloat(direction_id, direction);

		if (Input.GetKeyUp(KeyCode.Space)) {
			jumpReleased = true;
		}

		if (!isFirstjump && mctrl.CheckIsGrounded() && !animator.GetBool(boost_id)) {
			mctrl.grounded = true;
			animator.SetBool(grounded_id, true);
			animator.SetBool (jump_id, false);
			mctrl.SetCanVerticalBoost (false);
			return;
		}else{
			mctrl.JumpMoveInAir ();
		}

		if (Input.GetKey(KeyCode.Space) && jumpReleased && mctrl.CanVerticalBoost()) {
			mctrl.SetCanVerticalBoost (false);
			jumpReleased = false;
			animator.SetBool(boost_id, true);
			mctrl.Boost (true);
		}
	}

}
