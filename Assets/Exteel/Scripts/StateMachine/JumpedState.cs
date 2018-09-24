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
			animator.SetBool (BoostHash, false);//avoid shift+space directly vertically boost
			jumpReleased = false;
			mctrl.grounded = false;
			animator.SetBool (GroundedHash, false);
		}else if(!animator.GetBool(JumpHash)){//dir falling
			mctrl.Boost (false);
			animator.SetBool (BoostHash, false);
			animator.SetBool (JumpHash, true);
			animator.SetBool (GroundedHash, false);
			jumpReleased = true;
			mctrl.grounded = false;
		}
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled)return;

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat(SpeedHash, speed);
		animator.SetFloat(DirectionHash, direction);

		if (!gm.BlockInput && Input.GetKeyUp(KeyCode.Space)) {
			jumpReleased = true;
		}

		if (!isFirstjump && mctrl.CheckIsGrounded() && !animator.GetBool(BoostHash)) {//the first jump is on ground
            if (!mctrl.grounded) {
                Debug.Log("On Landing action");
                mctrl.OnLandingAction();
            }

			mctrl.grounded = true;
			animator.SetBool(GroundedHash, true);
			animator.SetBool (JumpHash, false);
			mctrl.SetCanVerticalBoost (false);
			return;
		}else{
			mctrl.JumpMoveInAir ();
		}

		if (!gm.BlockInput && Input.GetKey(KeyCode.Space) && jumpReleased && mctrl.CanVerticalBoost()) {
			mctrl.SetCanVerticalBoost (false);
			jumpReleased = false;
			animator.SetBool(BoostHash, true);
			mctrl.Boost (true);
		}
	}

}
