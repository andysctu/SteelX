﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashState : MechStateMachineBehaviour {

	private bool inAir = false;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);
		if ( cc == null || !cc.enabled ) return;
		animator.SetBool (onMelee_id, true);

		inAir = animator.GetBool (jump_id);

		animator.SetBool (boost_id, false);
		mctrl.Boost (false);

		if(inAir){
			mctrl.Boost (true);
		}

		if(mcbt.IsLMeleePlaying){
			mcbt.SlashDetect (0);
		}else{
			mcbt.SlashDetect (1);
		}
		mcbt.CanMeleeAttack = !animator.GetBool (jump_id);
        mctrl.ResetCurBoostingSpeed();
    }

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		if ( cc == null || !cc.enabled) return;
		animator.SetBool (onMelee_id, false);

		mcbt.IsRMeleePlaying = false;
		mcbt.IsLMeleePlaying = false;
		mcbt.CanMeleeAttack = true;
		mcbt.SetReceiveNextSlash (1);
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if ( cc == null || !cc.enabled) return;
		mcbt.CanMeleeAttack = !animator.GetBool (jump_id);

		if(inAir && !mcbt.IsLMeleePlaying && !mcbt.IsRMeleePlaying){
			mctrl.Boost (false);
			mctrl.JumpMoveInAir ();
		}

		mctrl.CallLockMechRot (!animator.IsInTransition (0));
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (cc == null || !cc.enabled)
			return;

		mctrl.SetCanVerticalBoost (false);
		//animator.SetBool (boost_id, false);
		//mctrl.Boost (false);

		if (inAir) {//exiting from jump melee attack
			animator.SetBool (onMelee_id, false);
			mcbt.IsRMeleePlaying = false;
			mcbt.IsLMeleePlaying = false;
			mcbt.SetReceiveNextSlash (1);
		}else{
			mcbt.CanMeleeAttack = true;
		}
	}
}
