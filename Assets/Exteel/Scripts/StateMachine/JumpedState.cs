using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedState : MechStateMachineBehaviour {

	public static bool jumpReleased = false;
	public bool isFirstjump = false;

	override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
		base.Init (animator);
		if (cc == null || !cc.enabled)return;

		/*jumpReleased = false;
		mcbt.CanMeleeAttack = true;*/
	}
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init (animator);
		if (cc == null || !cc.enabled)return;
		animator.SetBool (onMelee_id, false);

		if(isFirstjump){
			mctrl.Boost (false);
			animator.SetBool (boost_id, false);
			jumpReleased = false;
			mcbt.CanMeleeAttack = true;
		}

		if(!Input.GetKey(KeyCode.Space)){
			jumpReleased = true;
		}
	}
	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled)return;

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat(speed_id, speed);
		animator.SetFloat(direction_id, direction);
		if (Input.GetKeyUp(KeyCode.Space)) {
			jumpReleased = true;
		}

		if (Input.GetKey(KeyCode.Space) && jumpReleased && mctrl.CanVerticalBoost()) {
			mctrl.SetCanVerticalBoost (false);
			jumpReleased = false;
			animator.SetBool(boost_id, true);
			mctrl.Boost (true);
		}

		if (!isFirstjump && cc.isGrounded && !animator.IsInTransition(0)) { //falling->end jump  but not jump slash -> falling
			animator.SetBool(grounded_id, true);
			animator.SetBool (jump_id, false);
			mctrl.grounded = true;
			mctrl.SetCanVerticalBoost (false);
			return;
		}else{
			mctrl.JumpMoveInAir ();
		}
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (cc == null || !cc.enabled)return;

		/*if (jumpFirstCall) {//call after the jump01 end
			jumpFirstCall = false;
			//mctrl.Jump ();
		}*/
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
