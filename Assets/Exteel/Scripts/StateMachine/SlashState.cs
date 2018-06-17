using UnityEngine;

public class SlashState : MechStateMachineBehaviour {

	private bool inAir = false, detectGrounded, leftHand;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);
        animator.SetFloat("slashTime", 0);
        animator.SetBool("CanExit", false);

        leftHand = stateInfo.IsTag("SlashL") || stateInfo.IsTag("SlashL2") || stateInfo.IsTag("SlashL3") || stateInfo.IsTag("SlashL4");

        if ( cc == null || !cc.enabled ) return;
		animator.SetBool (onMelee_id, true);

		inAir = animator.GetBool (jump_id);
		detectGrounded = false;

        animator.SetBool (boost_id, false);
		mctrl.Boost (false);

        if (inAir){
			mctrl.Boost (true);
		}

		if (!animator.GetBool (slashL4_id) && !animator.GetBool (slashR4_id)) {
			mcbt.SetReceiveNextSlash (1);
			if (animator.GetBool (slashL_id) || animator.GetBool (slashL2_id) || animator.GetBool (slashL3_id))
				mcbt.SetMeleePlaying(0,true);
			else
				mcbt.SetMeleePlaying(1, true);
        }

		if(mcbt.isLMeleePlaying){
			mcbt.SlashDetect (0);
		}else{
			mcbt.SlashDetect (1);
		}
		mcbt.CanMeleeAttack = !animator.GetBool (jump_id);

		mctrl.ResetCurBoostingSpeed ();
	}

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		if ( cc == null || !cc.enabled) return;

		animator.SetBool (onMelee_id, false);
		mcbt.SetMeleePlaying(1, false);
		mcbt.SetMeleePlaying(0, false);
        mcbt.CanMeleeAttack = true;
		mcbt.SetReceiveNextSlash (1);
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		animator.SetFloat ("slashTime", stateInfo.normalizedTime);

        if (stateInfo.normalizedTime > (leftHand ? mcbt.slashL_threshold : mcbt.slashR_threshold) && !animator.IsInTransition(0)) {
            animator.SetBool("CanExit", true);
        }
        if ( cc == null || !cc.enabled) return;

        bool b = (inAir && !mcbt.isLMeleePlaying && !mcbt.isRMeleePlaying);
		if (b) {
			mctrl.JumpMoveInAir ();
		}

		mctrl.CallLockMechRot (!animator.IsInTransition (0));

		if(stateInfo.normalizedTime>0.5f && !detectGrounded){
            if (b) {
                mctrl.Boost(false);
            }

            mcbt.CanMeleeAttack = !animator.GetBool (jump_id);
			if(mctrl.CheckIsGrounded()){
				detectGrounded = true;
				mctrl.grounded = true;
				mctrl.SetCanVerticalBoost (false);
				animator.SetBool (jump_id, false);
				animator.SetBool (grounded_id, true);
                mctrl.Boost(false);
            }
		}
    }

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        animator.SetFloat("slashTime", 0);
        animator.SetBool("CanExit", false);

        if (cc == null || !cc.enabled)
			return;
		mctrl.SetCanVerticalBoost (false);

		if (inAir) {//exiting from jump melee attack
			animator.SetBool (onMelee_id, false);
            mcbt.SetMeleePlaying(1, false);
            mcbt.SetMeleePlaying(0, false);
            mcbt.SetReceiveNextSlash (1);
		}else{
			mcbt.CanMeleeAttack = true;//sometimes OnstateMachineExit does not ensure canslash set to true ( called before update )
		}

        if(stateInfo.tagHash!=0)animator.SetBool(stateInfo.tagHash, false);
        mctrl.CallLockMechRot(false);
    }
}
