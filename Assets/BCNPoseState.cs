using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCNPoseState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if ( cc == null || !cc.enabled) return;
		mcbt.isOnBCNPose = true;
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if ( cc == null || !cc.enabled) return;
		mcbt.isOnBCNPose = false;
	}
}
