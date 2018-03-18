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
		if (mcbt != null) {
			mcbt.ShowTrailL (false);
			mcbt.ShowTrailR (false);
		}

		if ( cc == null || !cc.enabled) return;
		animator.SetBool (onSlash_id, false);

		mcbt.isRSlashPlaying = 0;
		mcbt.isLSlashPlaying = 0;
		mcbt.SetReceiveNextSlash (1);
	}

	public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		mcbt.CanSlash = mctrl.CheckIsGrounded ();
	}

	public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (cc == null)
			return;

		if (!cc.enabled) {
			if (!animator.GetBool (slashR2_id) && !animator.GetBool (slashL2_id) && !animator.GetBool (slashR3_id) && !animator.GetBool (slashL3_id)) {//exit slash state not through stateMachineExit ( directly go to falling state )
				mcbt.ShowTrailL (false);
				mcbt.ShowTrailR (false);
			}
		}else{
			if (!animator.GetBool (slashR2_id) && !animator.GetBool (slashL2_id) && !animator.GetBool (slashR3_id) && !animator.GetBool (slashL3_id)) {
				mcbt.ShowTrailL (false);
				mcbt.ShowTrailR (false);
				mcbt.isRSlashPlaying = 0;
				mcbt.isLSlashPlaying = 0;
				animator.SetBool (onSlash_id, false);
			}
		}

		animator.SetBool (boost_id, false);
		mctrl.SetCanVerticalBoost (false);
		mctrl.Boost (false);
	}
}
