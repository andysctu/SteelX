using UnityEngine;

namespace StateMachine.Attack
{
    public class SlashState : MechStateMachineBehaviour
    {
        private bool inAir, detectedGrounded;
        private float threshold = 1;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);

            animator.SetFloat("slashTime", 0);
            animator.SetBool("CanExit", false);

            if (cc == null || !cc.enabled) return;

            inAir = mctrl.IsJumping;
            detectedGrounded = false;
            
            animator.SetBool(animatorVars.SlashLHash, false);
            animator.SetBool(animatorVars.SlashRHash, false);
            animator.SetBool(BoostHash, false);

            //Boost Effect
            if (inAir){
                //mctrl.Boost(true);
            } else{
                //mctrl.Boost(false);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            animator.SetFloat("slashTime", stateInfo.normalizedTime);

            if (stateInfo.normalizedTime > threshold && !animator.IsInTransition(0)){
                animator.SetBool("CanExit", true);
            }

            if (cc == null || !cc.enabled) return;

            mctrl.LockMechRot(!animator.IsInTransition(3));//todo : check this

            if (stateInfo.normalizedTime > 0.5f && !detectedGrounded){
                //if (b){
                    //mctrl.Boost(false);
                //}

                mcbt.CanMeleeAttack = !animator.GetBool(JumpHash);
                if (mctrl.Grounded) {
                    detectedGrounded = true;
                    //mctrl.Boost(false);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            animator.SetFloat("slashTime", 0);
            animator.SetBool("CanExit", false);
        }
    }
}