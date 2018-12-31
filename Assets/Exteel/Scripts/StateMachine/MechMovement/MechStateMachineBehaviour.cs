using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class MechStateMachineBehaviour : StateMachineBehaviour
    {
        protected AnimatorVars animatorVars;
        protected AnimationEventController AnimationEventController;
        protected CharacterController cc;
        protected MechController mctrl;
        protected MechCombat mcbt;
        protected Sounds Sounds;
        protected MechIK mechIK;
        protected EffectController EffectController;
        protected HandleInputs HandleInputs;

        protected int BoostHash;
        protected int GroundedHash;
        protected int JumpHash;
        protected int SpeedHash;
        protected int DirectionHash;

        private bool _isInit = false;

        public void Init(Animator animator){
            animatorVars = animator.GetComponent<AnimatorVars>();

            if (_isInit) return;

            cc = animatorVars.cc;
            mctrl = animatorVars.Mctrl;
            mcbt = animatorVars.Mcbt;
            AnimationEventController = animator.GetComponent<AnimationEventController>();
            Sounds = animatorVars.Sounds;
            mechIK = animatorVars.MechIK;
            EffectController = animatorVars.EffectController;
            HandleInputs = animatorVars.HandleInputs;

            BoostHash = animatorVars.BoostHash;
            GroundedHash = animatorVars.GroundedHash;
            JumpHash = animatorVars.JumpHash;
            DirectionHash = animatorVars.DirectionHash;
            SpeedHash = animatorVars.SpeedHash;

            _isInit = true;
        }
    }
}