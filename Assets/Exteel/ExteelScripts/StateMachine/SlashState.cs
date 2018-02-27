using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashState : MechStateMachineBehaviour {

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
	//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called before OnStateExit is called on any state inside this state machine
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called before OnStateMove is called on any state inside this state machine
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called before OnStateIK is called on any state inside this state machine
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	//override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
	//
	//}
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		animator.SetBool ("OnSlash", true);
	}

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		animator.SetBool ("OnSlash", false);
		if(mcbt==null)mcbt = animator.transform.parent.gameObject.GetComponent<MechCombat>();//avoid null reference bug
		mcbt.isRSlashPlaying = 0;
		mcbt.isLSlashPlaying = 0;
		mcbt.ShowTrailL (false);
		mcbt.ShowTrailR (false);
		mcbt.SetReceiveNextSlash (1);
	}
}
