using UnityEngine;

namespace StateMachine.MechMovement
{
    public class GroundedState : MechStateMachineBehaviour {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.Init(animator);

            if ((mctrl.GetOwner() != null && !mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

            animator.SetBool(GroundedHash, true);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            mctrl.EnableBoostFlame(animator.GetBool("Boost"));

            if ((mctrl.GetOwner() != null && !mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

            UpdateAnimatorParameters(animator);

            if (mctrl.IsJumping && !mctrl.Grounded) {//TODO : condition : you can't jump if playing melee attack
                animator.SetBool(GroundedHash, false);
                animator.SetBool(JumpHash, true);
                return;
            }

            if (!mctrl.Grounded) {//check not jumping but is falling
                animator.SetBool(GroundedHash, false);
                return;
            }

            if (mctrl.IsBoosting) {
                animator.SetBool(BoostHash, true);
            }
        }
    }
}
