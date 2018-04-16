using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);
		if ( cc == null || !cc.enabled ) return;
		animator.SetBool (onMelee_id, true);


		Sounds.StopBoostLoop ();
		animator.SetBool (boost_id, false);
		mctrl.Boost (false);

		if (!animator.GetBool (slashL3_id) && !animator.GetBool (slashR3_id)) {
			mcbt.SetReceiveNextSlash (1);
			if (animator.GetBool (slashL_id) || animator.GetBool (slashL2_id))
				mcbt.isLMeleePlaying = 1;
			else
				mcbt.isRMeleePlaying = 1;
		}

		if(mcbt.isLMeleePlaying == 1){
			mcbt.SlashDetect (0);
		}else{
			mcbt.SlashDetect (1);
		}
	}

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		if ( cc == null || !cc.enabled) return;
		animator.SetBool (onMelee_id, false);

		mcbt.isRMeleePlaying = 0;
		mcbt.isLMeleePlaying = 0;
		mcbt.CanMeleeAttack = true;
		mcbt.SetReceiveNextSlash (1);
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if ( cc == null || !cc.enabled) return;
		mcbt.CanMeleeAttack = !animator.GetBool (jump_id);

		mctrl.CallLockMechRot (!animator.IsInTransition (0));
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (cc == null || !cc.enabled)
			return;

		mctrl.SetCanVerticalBoost (false);
		animator.SetBool (boost_id, false);
		mctrl.Boost (false);

		if (animator.GetBool (jump_id)) {//exiting from jump melee attack
			animator.SetBool (onMelee_id, false);
			mcbt.isRMeleePlaying = 0;
			mcbt.isLMeleePlaying = 0;
			mcbt.SetReceiveNextSlash (1);
		}
	}
}
