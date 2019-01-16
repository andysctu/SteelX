using UnityEngine;

namespace StateMachine.MechMovement {
    public class GroundedState : MechStateMachineBehaviour {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.Init(animator);

            if (cc == null || !cc.enabled) return;

            animator.SetBool(AnimatorHashVars.GroundedHash, true);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Mctrl.EnableBoostFlame(Mctrl.IsBoosting);

            if (cc == null || !cc.enabled) return;

            UpdateAnimatorParameters(animator);

            if (Mctrl.IsJumping) {
                animator.SetBool(AnimatorHashVars.GroundedHash, false);
                animator.SetBool(AnimatorHashVars.JumpHash, true);
                return;
            }

            if (!Mctrl.Grounded) {//falling
                animator.SetBool(AnimatorHashVars.GroundedHash, false);
                return;
            }

            if (Mctrl.IsBoosting) {
                animator.SetBool(AnimatorHashVars.BoostHash, true);
            }
        }
    }
}