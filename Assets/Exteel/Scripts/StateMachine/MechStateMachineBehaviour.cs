using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechStateMachineBehaviour : StateMachineBehaviour {
	protected AnimatorVars animatorVars;
	protected CharacterController cc;
	protected MechController mctrl;
	protected MechCombat mcbt;
	protected Sounds Sounds;
	protected MechIK mechIK;
	protected EffectController EffectController;

	protected int boost_id;
	protected int grounded_id;
	protected int jump_id;
	protected int speed_id;
	protected int direction_id;
	protected int onMelee_id;

	protected int slashL_id;
	protected int slashR_id;
	protected int slashL2_id;
	protected int slashR2_id;
	protected int slashL3_id;
	protected int slashR3_id;
	protected int slashL4_id;
	protected int slashR4_id;


	protected int OnBCN_id;
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	/*	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animatorVars = animator.GetComponent<AnimatorVars> ();
		if (animatorVars == null)//find too slow ?
			return;

		if (mctrl != null)//already init ( every state need to be assigned only one time )  ; cc is null if it's not mine
			return;
	}*/

	public void Init(Animator animator){
		animatorVars = animator.GetComponent<AnimatorVars> ();
		if (animatorVars == null)//find too slow ?
			return;

		if (mctrl != null)//already init ( every state need to be assigned only one time )  ; cc is null if it's not mine
			return;

		cc = animatorVars.cc;
		mctrl = animatorVars.mctrl;
		mcbt = animatorVars.mcbt;
		Sounds = animatorVars.Sounds;
		mechIK = animatorVars.mechIK;
		EffectController = animatorVars.EffectController;

		boost_id = animatorVars.boost_id;
		grounded_id = animatorVars.grounded_id;
		jump_id = animatorVars.jump_id;
		direction_id = animatorVars.direction_id;
		onMelee_id = animatorVars.onMelee_id;
		speed_id = animatorVars.speed_id;

		slashL_id = animatorVars.slashL_id;
		slashR_id = animatorVars.slashR_id;
		slashL2_id = animatorVars.slashL2_id;
		slashR2_id = animatorVars.slashR2_id;
		slashL3_id = animatorVars.slashL3_id;
		slashR3_id = animatorVars.slashR3_id;
		slashL4_id = animatorVars.slashL4_id;
		slashR4_id = animatorVars.slashR4_id;

		OnBCN_id = animatorVars.OnBCN_id;
	}
}
