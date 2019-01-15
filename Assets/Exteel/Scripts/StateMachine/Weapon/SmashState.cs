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

            animator.SetBool(AnimatorHashVars.SmashRHash, false);
            animator.SetBool(AnimatorHashVars.SmashLHash, false);
            animator.SetBool(AnimatorHashVars.BoostHash, false);
            _inAir = Mctrl.IsJumping;

            _detectedGrounded = false;

            Mctrl.ResetCurBoostingSpeed();

            if (_inAir){
                //mctrl.Boost(true);
            } else{
                //mctrl.Boost(false);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            Mctrl.LockMechRot(!animator.IsInTransition(3));//todo : check this

            if (stateInfo.normalizedTime > 0.5f && !_detectedGrounded){
                //if (b){
                    //mctrl.Boost(false);
                //}

                if (Mctrl.Grounded) {
                    _detectedGrounded = true;
                    //mctrl.Boost(false);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            Mctrl.LockMechRot(false);
        }
    }
}