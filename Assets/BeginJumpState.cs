using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginJumpState : MechStateMachineBehaviour {

	public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.SetBool ("SpaceUp", false);
	}

	public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if(Input.GetKeyUp(KeyCode.Space)){
			animator.SetBool ("SpaceUp", true);
		}

		if(!animator.GetBool("SpaceUp")){
			if(mctrl==null){
				mctrl = animator.transform.parent.gameObject.GetComponent<MechController>();
			}
			mctrl.Boost (false);
			animator.SetBool ("Boost", false);
		}
	}
}
