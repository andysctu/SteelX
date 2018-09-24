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

	protected int BoostHash;
	protected int GroundedHash;
	protected int JumpHash;
	protected int SpeedHash;
	protected int DirectionHash;

	public void Init(Animator animator){
		animatorVars = animator.GetComponent<AnimatorVars> ();
		if (animatorVars == null)//find too slow ?
			return;

		if (mctrl != null)//already init ( every state need to be assigned only one time )  ; cc is null if it's not mine
			return;

        gm = FindObjectOfType<GameManager>();

        cc = animatorVars.cc;
		mctrl = animatorVars.Mctrl;
		mcbt = animatorVars.Mcbt;
        AnimationEventController = animator.GetComponent<AnimationEventController>();
        Sounds = animatorVars.Sounds;
		mechIK = animatorVars.MechIK;
		EffectController = animatorVars.EffectController;

		BoostHash = animatorVars.BoostHash;
		GroundedHash = animatorVars.GroundedHash;
		JumpHash = animatorVars.JumpHash;
		DirectionHash = animatorVars.DirectionHash;
		SpeedHash = animatorVars.SpeedHash;
	}
}
