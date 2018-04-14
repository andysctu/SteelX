using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingState : MechStateMachineBehaviour {

	public int hand = 0;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (mcbt == null)return;
		mcbt.isOnBCNPose = true;
		mechIK.SetIK (true, 0, hand);
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (mcbt == null)return;
		mcbt.isOnBCNPose = false;

		mechIK.SetIK (false, 0, hand);
	}
}	
