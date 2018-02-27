using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginJumpState : MechStateMachineBehaviour {

	/*public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter (animator, stateInfo, layerIndex);
		if ( cc == null || !cc.enabled) return;
		animator.SetBool (SpaceUp_id, false);

		if(Input.GetKeyUp(KeyCode.Space)){
			animator.SetBool (SpaceUp_id, true);
		}
	}

	public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( cc == null || !cc.enabled) return;

		if(Input.GetKeyUp(KeyCode.Space)){
			animator.SetBool (SpaceUp_id, true);
		}

		if(!animator.GetBool(SpaceUp_id)){
			mctrl.Boost (false);
			animator.SetBool (boost_id, false);
		}
	}

	public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if ( cc == null || !cc.enabled) return;

		animator.SetBool (SpaceUp_id, false);
	}*/
}
