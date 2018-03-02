using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechStateMachineBehaviour : StateMachineBehaviour {

	protected AnimatorVars animatorVars;
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
		animatorVars = animator.GetComponent<AnimatorVars> ();
		if (animatorVars == null)//find too slow ?
			return;
		cc = animatorVars.cc;
		mctrl = animatorVars.mctrl;
		mcbt = animatorVars.mcbt;
		Sounds = animatorVars.Sounds;
	
		boost_id = animatorVars.boost_id;
		grounded_id = animatorVars.grounded_id;
		jump_id = animatorVars.jump_id;
		direction_id = animatorVars.direction_id;
		onSlash_id = animatorVars.onSlash_id;
		speed_id = animatorVars.speed_id;
	}
}
