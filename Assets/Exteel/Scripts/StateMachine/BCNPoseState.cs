using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCNPoseState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);
		if (mcbt == null)return;
		mechIK.SetIK (true, 1, 0);
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (mcbt == null)return;

		if(!mcbt.isOnBCNPose && !animator.IsInTransition(0))
			mcbt.isOnBCNPose = true;
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (mcbt == null)return;
		mcbt.isOnBCNPose = false;

		mechIK.SetIK (false, 1, 0);
	}
}
