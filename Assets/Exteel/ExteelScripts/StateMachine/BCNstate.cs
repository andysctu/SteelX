using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCNstate : MechStateMachineBehaviour {


	public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if(mctrl == null)
			mctrl = animator.transform.parent.gameObject.GetComponent<MechController>();
		animator.SetBool ("OnBCN", true);
	}

	public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		mctrl.BCNPose ();
		animator.SetBool ("Boost", false);
		mctrl.Boost (false);
	}

	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		animator.SetBool ("OnBCN", false);
	}
		
}
