using UnityEngine;

namespace StateMachine.Attack
{
    public class SlashState : MechStateMachineBehaviour
    {
        private float threshold = 0.95f;//todo :check this
        private bool _isInAir;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);

            if (cc == null || !cc.enabled) return;

            _isInAir = !Mctrl.Grounded;
            animator.SetBool("CanExit", false);

            animator.SetBool(AnimatorHashVars.SlashLHash, false);
            animator.SetBool(AnimatorHashVars.SlashRHash, false);
            animator.SetBool(AnimatorHashVars.BoostHash, false);

            Mctrl.EnableBoostFlame(_isInAir);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            if (stateInfo.normalizedTime > threshold && !animator.IsInTransition(3)){
                animator.SetBool("CanExit", true);
            }

            Mctrl.LockMechRot(!animator.IsInTransition(3));//todo : check this

            if (_isInAir &&  stateInfo.normalizedTime > 0.7f){
                Mctrl.EnableBoostFlame(false);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            animator.SetBool("CanExit", false);
            Mctrl.LockMechRot(false);
            if (_isInAir)Mctrl.EnableBoostFlame(false);
        }
    }
}