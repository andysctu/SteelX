using UnityEngine;

namespace StateMachine.MechMovement
{
    public class JumpedState : MechStateMachineBehaviour
    {
        public bool isFirstJump = false;//the first animation of jump . set in animator

        private bool _playedOnLandingAction;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);

            if (cc == null || !cc.enabled) return;

            _playedOnLandingAction = false;

            if (isFirstJump){
                Mctrl.OnJumpAction();
            }

            Mctrl.EnableBoostFlame(false);
            animator.SetBool(AnimatorHashVars.GroundedHash, Mctrl.Grounded);
            animator.SetBool(AnimatorHashVars.BoostHash, Mctrl.IsBoosting); //avoid shift+space directly vertically boost
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (!cc.enabled) return;

            UpdateAnimatorParameters(animator);

            if (Mctrl.Grounded){
                animator.SetBool(AnimatorHashVars.JumpHash, false);
                animator.SetBool(AnimatorHashVars.GroundedHash, true);
                return;
            }

            if (!isFirstJump && Mctrl.Grounded){
                if (!_playedOnLandingAction){
                    _playedOnLandingAction = true;
                    Mctrl.OnLandingAction();
                }
            }

            if (Mctrl.IsBoosting){
                animator.SetBool(AnimatorHashVars.BoostHash, true);
            }
        }
    }
}