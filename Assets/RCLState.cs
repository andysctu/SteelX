using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCLState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (mcbt == null)return;
		mechIK.SetIK (true, 2, 0);
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		if (mcbt == null)return;
		mechIK.SetIK (false, 2, 0);
	}

}
