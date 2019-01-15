using UnityEngine;

namespace StateMachine.MechMovement
{
    public class JumpedState : MechStateMachineBehaviour
    {
        public bool isFirstjump = false;

        private bool _playedOnLandingAction;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
            if (!cc.enabled) return;

            _playedOnLandingAction = false;

            if (isFirstjump){
                Mctrl.OnJumpAction();
            }

            if (Mctrl.GetOwner() != null && !Mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) return;

            //mctrl.Boost(false);
            animator.SetBool(AnimatorHashVars.GroundedHash, Mctrl.Grounded);
            animator.SetBool(AnimatorHashVars.BoostHash, Mctrl.IsBoosting); //avoid shift+space directly vertically boost
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (!cc.enabled) return;
            Mctrl.EnableBoostFlame(animator.GetBool(AnimatorHashVars.BoostHash));

            UpdateAnimatorParameters(animator);

            if (!Mctrl.IsJumping && Mctrl.Grounded){
                animator.SetBool(AnimatorHashVars.JumpHash, false);
                animator.SetBool(AnimatorHashVars.GroundedHash, true);
                return;
            }

            if (!isFirstjump && animator.GetBool(AnimatorHashVars.GroundedHash)){
                //TODO : consider move this part to grounded state
                if (!_playedOnLandingAction){
                    _playedOnLandingAction = true;
                    Mctrl.OnLandingAction();
                }
            }

            if (Mctrl.GetOwner() != null && !Mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) return;

            if (Mctrl.IsBoosting){
                animator.SetBool(AnimatorHashVars.BoostHash, true);
            }
        }
    }
}