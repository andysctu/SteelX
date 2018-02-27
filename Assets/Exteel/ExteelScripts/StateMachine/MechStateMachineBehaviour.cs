using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechStateMachineBehaviour : StateMachineBehaviour {

	protected CharacterController cc;
	protected MechController mctrl;
	protected MechCombat mcbt;
	protected Sounds Sounds;

	protected int boost_id;
	protected int grounded_id;
	protected int jump_id;
	protected int speed_id;
	protected int direction_id;
	protected int onSlash_id;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		cc = animator.transform.parent.gameObject.GetComponent<CharacterController>();
		mctrl = animator.transform.parent.gameObject.GetComponent<MechController>();
		mcbt = animator.transform.parent.gameObject.GetComponent<MechCombat>();
		Sounds = animator.GetComponent<Sounds>();

		boost_id = Animator.StringToHash ("Boost");
		grounded_id = Animator.StringToHash ("Grounded");
		jump_id = Animator.StringToHash ("Jump");
		direction_id = Animator.StringToHash ("Direction");
		onSlash_id = Animator.StringToHash ("OnSlash");
		speed_id = Animator.StringToHash ("Speed");
	}
}
