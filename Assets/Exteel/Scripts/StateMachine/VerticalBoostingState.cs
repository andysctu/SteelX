using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalBoostingState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.Init(animator);
		if ( cc == null || !cc.enabled) return;
		mctrl.SetCanVerticalBoost(false);
		animator.SetBool (animatorVars.OnMeleeHash, false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( cc == null || !cc.enabled) return;

		float speed = Input.GetAxis("Vertical");
		float direction = Input.GetAxis("Horizontal");

		animator.SetFloat(SpeedHash, speed);
		animator.SetFloat(DirectionHash, direction);

		if ( (mcbt.IsENEmpty() || !Input.GetKey(KeyCode.Space) || gm.BlockInput)) {
			mctrl.Boost (false);
			//animator.SetFloat(SpeedHash, 0);
			animator.SetBool(BoostHash, false);
		}else{
			mctrl.VerticalBoost();
		}
	}
}
	