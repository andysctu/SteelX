using UnityEngine;
using Weapons;

namespace StateMachine.Attack
{
    public class SmashState : MechStateMachineBehaviour
    {
        private bool _isInAir;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);

            if (cc == null || !cc.enabled) return;

            animator.SetBool(AnimatorHashVars.SmashRHash, false);
            animator.SetBool(AnimatorHashVars.SmashLHash, false);
            animator.SetBool(AnimatorHashVars.BoostHash, false);
            _isInAir = Mctrl.IsJumping;

            Mctrl.ResetCurBoostingSpeed();

            Mctrl.EnableBoostFlame(_isInAir);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            Mctrl.LockMechRot(!animator.IsInTransition(3));//todo : check this

            if (_isInAir && stateInfo.normalizedTime > 0.7f) {
                Mctrl.EnableBoostFlame(false);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            Mctrl.LockMechRot(false);
            if (_isInAir) Mctrl.EnableBoostFlame(false);
        }
    }
}