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

            inAir = Mctrl.IsJumping;
            detectedGrounded = false;
            
            animator.SetBool(AnimatorHashVars.SlashLHash, false);
            animator.SetBool(AnimatorHashVars.SlashRHash, false);
            animator.SetBool(AnimatorHashVars.BoostHash, false);

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

            Mctrl.LockMechRot(!animator.IsInTransition(3));//todo : check this

            if (stateInfo.normalizedTime > 0.5f && !detectedGrounded){
                //if (b){
                    //mctrl.Boost(false);
                //}

                if (Mctrl.Grounded) {
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