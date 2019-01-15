using UnityEngine;

namespace StateMachine
{
    public class MechStateMachineBehaviour : StateMachineBehaviour
    {
        protected CharacterController cc;
        protected MechController Mctrl;
        protected MechIK MechIK;
        protected EffectController EffectController;

        private bool _isInit;

        public void Init(Animator animator){
            if (_isInit) return;

            cc = animator.GetComponent<CharacterController>();
            Mctrl = animator.GetComponent<MechController>();
            MechIK = animator.GetComponent<MechIK>();
            EffectController = animator.transform.root.GetComponentInChildren<EffectController>();

            _isInit = true;
        }

        public void UpdateAnimatorParameters(Animator animator) {
            if(!_isInit)return;

            animator.SetFloat(AnimatorHashVars.SpeedHash, Mathf.Lerp(animator.GetFloat(AnimatorHashVars.SpeedHash), Mctrl.Speed, Time.deltaTime * 15));
            animator.SetFloat(AnimatorHashVars.DirectionHash, Mathf.Lerp(animator.GetFloat(AnimatorHashVars.DirectionHash), Mctrl.Direction, Time.deltaTime * 15));
            animator.SetFloat(AnimatorHashVars.AngleHash, Mathf.Clamp(Mctrl.Angle/90,-1,1));
        }
    }
}