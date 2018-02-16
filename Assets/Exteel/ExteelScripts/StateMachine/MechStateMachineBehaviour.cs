using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechStateMachineBehaviour : StateMachineBehaviour {

	protected CharacterController cc;
	protected MechController mctrl;
	protected MechCombat mcbt;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		cc = animator.transform.parent.gameObject.GetComponent<CharacterController>();
		mctrl = animator.transform.parent.gameObject.GetComponent<MechController>();
		mcbt = animator.transform.parent.gameObject.GetComponent<MechCombat>();
	}
}
