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

            mctrl.EnableBoostFlame(animator.GetBool("Boost"));

            if ((mctrl.GetOwner() != null && !mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

            UpdateAnimatorParameters(animator);

            if (!mctrl.Grounded){
                animator.SetBool(GroundedHash, false);
            }

            if (mctrl.IsJumping){
                animator.SetBool(JumpHash, true);
            }

            animator.SetBool(BoostHash, mctrl.IsBoosting);
        }
    }
}