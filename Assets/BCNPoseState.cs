using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCNPoseState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);
		if (mcbt == null)return;
		mcbt.isOnBCNPose = true;
		mechIK.SetIK (true, 1, 0);
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (mcbt == null)return;
		mcbt.isOnBCNPose = false;

		mechIK.SetIK (false, 1, 0);
	}
}
