using UnityEngine;
using Weapons;

namespace StateMachine.Attack
{
    public class SmashState : MechStateMachineBehaviour
    {
        private bool _inAir, _detectedGrounded;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);

            if (cc == null || !cc.enabled) return;

            animator.SetBool(animatorVars.SmashRHash, false);
            animator.SetBool(animatorVars.SmashLHash, false);
            animator.SetBool(BoostHash, false);
            _inAir = mctrl.IsJumping;

            _detectedGrounded = false;

            mcbt.CanMeleeAttack = !animator.GetBool(JumpHash);
            mctrl.ResetCurBoostingSpeed();

            if (_inAir){
                //mctrl.Boost(true);
            } else{
                //mctrl.Boost(false);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            mctrl.LockMechRot(!animator.IsInTransition(3));//todo : check this

            if (stateInfo.normalizedTime > 0.5f && !_detectedGrounded){
                //if (b){
                    //mctrl.Boost(false);
                //}

                if (mctrl.Grounded) {
                    _detectedGrounded = true;
                    //mctrl.Boost(false);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            mctrl.LockMechRot(false);
        }
    }
}