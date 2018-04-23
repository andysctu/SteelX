using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessSlashComboState : MechStateMachineBehaviour {

	public int hand, combo;//L1 => combo = 1

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);

	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (!cc.enabled) return;
		switch(combo){
		case 1:
			if(hand==0){
				if(animator.GetBool("SlashL2") && stateInfo.normalizedTime>0.75f && !animator.IsInTransition(0)){//if not get called
					animator.CrossFade ("SlashL2", 0.1f);
				}
			}else{
				if(animator.GetBool("SlashR2") && stateInfo.normalizedTime>0.75f && !animator.IsInTransition(0)){//if not get called
					animator.CrossFade ("SlashR2", 0.1f);
				}
			}
			break;
		case 2:
			if(hand==0){
				if(animator.GetBool("SlashL3") && stateInfo.normalizedTime>0.75f && !animator.IsInTransition(0)){//if not get called
					animator.CrossFade ("SlashL3", 0.1f);
				}
			}else{
				if(animator.GetBool("SlashR3") && stateInfo.normalizedTime>0.75f && !animator.IsInTransition(0)){//if not get called
					animator.CrossFade ("SlashR3", 0.1f);
				}
			}
			break;
		case 3:
			if(hand==0){
				if(animator.GetBool("SlashL4") && stateInfo.normalizedTime>0.75f && !animator.IsInTransition(0)){//if not get called
					animator.CrossFade ("SlashL4", 0.1f);
				}
			}else{
				if(animator.GetBool("SlashR4") && stateInfo.normalizedTime>0.75f && !animator.IsInTransition(0)){//if not get called
					animator.CrossFade ("SlashR4", 0.1f);
				}
			}
			break;
		}
	}
}
