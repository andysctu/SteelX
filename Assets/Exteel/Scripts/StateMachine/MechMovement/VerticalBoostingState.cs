using UnityEngine;

namespace StateMachine.MechMovement
{
    public class VerticalBoostingState : MechStateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            Mctrl.EnableBoostFlame(Mctrl.IsBoosting);

            UpdateAnimatorParameters(animator);

            animator.SetBool(AnimatorHashVars.BoostHash, Mctrl.IsBoosting);

            animator.SetBool(AnimatorHashVars.GroundedHash, Mctrl.Grounded);
        }
    }
}
	