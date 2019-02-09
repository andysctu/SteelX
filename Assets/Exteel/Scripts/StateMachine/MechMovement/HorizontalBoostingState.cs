using UnityEngine;

namespace StateMachine.MechMovement
{
    public class HorizontalBoostingState : MechStateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            Mctrl.EnableBoostFlame(Mctrl.IsBoosting);

            UpdateAnimatorParameters(animator);

            if (!Mctrl.Grounded){
                animator.SetBool(AnimatorHashVars.GroundedHash, false);

                if (Mctrl.YSpeed > 0){
                    animator.SetBool(AnimatorHashVars.JumpHash, true);
                }
                return;
            }

            animator.SetBool(AnimatorHashVars.BoostHash, Mctrl.IsBoosting);
        }
    }
}