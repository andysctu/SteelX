using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechStateMachineBehaviour : StateMachineBehaviour {
	protected AnimatorVars animatorVars;
    protected AnimationEventController AnimationEventController;
    protected CharacterController cc;
	protected MechController mctrl;
	protected MechCombat mcbt;
	protected Sounds Sounds;
	protected MechIK mechIK;
	protected EffectController EffectController;
    protected GameManager gm;

	protected int boost_id;
	protected int grounded_id;
	protected int jump_id;
	protected int speed_id;
	protected int direction_id;
	protected int onMelee_id;

	protected int slash_id, finalSlash_id;

	protected int OnBCN_id;

	public void Init(Animator animator){
		animatorVars = animator.GetComponent<AnimatorVars> ();
		if (animatorVars == null)//find too slow ?
			return;

		if (mctrl != null)//already init ( every state need to be assigned only one time )  ; cc is null if it's not mine
			return;

        gm = FindObjectOfType<GameManager>();

        cc = animatorVars.cc;
		mctrl = animatorVars.mctrl;
		mcbt = animatorVars.mcbt;
        AnimationEventController = animator.GetComponent<AnimationEventController>();
        Sounds = animatorVars.Sounds;
		mechIK = animatorVars.mechIK;
		EffectController = animatorVars.EffectController;

		boost_id = animatorVars.boost_id;
		grounded_id = animatorVars.grounded_id;
		jump_id = animatorVars.jump_id;
		direction_id = animatorVars.direction_id;
		onMelee_id = animatorVars.onMelee_id;
		speed_id = animatorVars.speed_id;

        finalSlash_id = animatorVars.finalSlash_id;
        slash_id = animatorVars.slash_id;

		OnBCN_id = animatorVars.OnBCN_id;
	}
}
