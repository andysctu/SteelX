using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingState : MechStateMachineBehaviour {

	public int hand = 0;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);
		if (mcbt == null)return;

		mechIK.SetIK (true, 0, hand);
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (mcbt == null)return;

		mechIK.SetIK (false, 0, hand);
	}
}	
