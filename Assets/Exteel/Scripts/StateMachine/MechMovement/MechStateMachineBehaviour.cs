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
        protected int AngleHash;

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
            AngleHash = animatorVars.AngleHash;

            _isInit = true;
        }

        public void UpdateAnimatorParameters(Animator animator) {
            if(!_isInit)return;

            animator.SetFloat(SpeedHash, Mathf.Lerp(animator.GetFloat(SpeedHash), mctrl.Speed, Time.deltaTime * 15));
            animator.SetFloat(DirectionHash, Mathf.Lerp(animator.GetFloat(DirectionHash), mctrl.Direction, Time.deltaTime * 15));
            animator.SetFloat(AngleHash, Mathf.Clamp(mctrl.Angle/90,-1,1));
        }
    }
}