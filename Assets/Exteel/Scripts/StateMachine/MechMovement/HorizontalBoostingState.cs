using UnityEngine;

namespace StateMachine.MechMovement
{
    public class HorizontalBoostingState : MechStateMachineBehaviour
    {

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            EffectController.UpdateBoostingDust();

            Mctrl.EnableBoostFlame(animator.GetBool(AnimatorHashVars.BoostHash));

            if ((Mctrl.GetOwner() != null && !Mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

            UpdateAnimatorParameters(animator);

            if (!Mctrl.Grounded){
                animator.SetBool(AnimatorHashVars.GroundedHash, false);
            }

            if (Mctrl.IsJumping){
                animator.SetBool(AnimatorHashVars.JumpHash, true);
            }

            animator.SetBool(AnimatorHashVars.BoostHash, Mctrl.IsBoosting);
        }
    }
}