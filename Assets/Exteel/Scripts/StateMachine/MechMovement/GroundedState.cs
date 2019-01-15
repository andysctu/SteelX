using UnityEngine;

namespace StateMachine.MechMovement
{
    public class GroundedState : MechStateMachineBehaviour {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.Init(animator);

            if ((Mctrl.GetOwner() != null && !Mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

            animator.SetBool(AnimatorHashVars.GroundedHash, true);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Mctrl.EnableBoostFlame(animator.GetBool(AnimatorHashVars.BoostHash));

            if ((Mctrl.GetOwner() != null && !Mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

            UpdateAnimatorParameters(animator);

            if (Mctrl.IsJumping && !Mctrl.Grounded) {//TODO : condition : you can't jump if playing melee attack
                animator.SetBool(AnimatorHashVars.GroundedHash, false);
                animator.SetBool(AnimatorHashVars.JumpHash, true);
                return;
            }

            if (!Mctrl.Grounded) {//check not jumping but is falling
                animator.SetBool(AnimatorHashVars.GroundedHash, false);
                return;
            }

            if (Mctrl.IsBoosting) {
                animator.SetBool(AnimatorHashVars.BoostHash, true);
            }
        }
    }
}
