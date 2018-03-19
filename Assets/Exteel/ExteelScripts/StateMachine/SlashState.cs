using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if ( cc == null || !cc.enabled ) return;
		animator.SetBool (onSlash_id, true);
	}

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		if ( cc == null || !cc.enabled) return;
		animator.SetBool (onSlash_id, false);

		mcbt.isRSlashPlaying = 0;
		mcbt.isLSlashPlaying = 0;
		mcbt.SetReceiveNextSlash (1);
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		mcbt.CanSlash = mctrl.CheckIsGrounded ();
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (cc == null || !cc.enabled)
			return;

		if (!animator.GetBool (slashR2_id) && !animator.GetBool (slashL2_id) && !animator.GetBool (slashR3_id) && !animator.GetBool (slashL3_id)) {//exit not through statemachineExit
			mcbt.isRSlashPlaying = 0;
			mcbt.isLSlashPlaying = 0;
			animator.SetBool (onSlash_id, false);
		}else{
			mctrl.SetCanVerticalBoost (false);
		}


		animator.SetBool (boost_id, false);
		mctrl.Boost (false);
	}
}
