using UnityEngine;

namespace StateMachine.Attack
{
    public class CnState : MechStateMachineBehaviour
    {

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
            if (cc == null || !cc.enabled) return;
            animator.SetBool(JumpHash, false);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            //Fix position
            if (animator.GetBool(animatorVars.CnPoseHash) || (animator.GetBool(animatorVars.CnShootHash) && !animator.IsInTransition(0))) mctrl.CnPose();

            //shut down boost
            if (animator.GetBool(BoostHash) && !animator.IsInTransition(0)){
                animator.SetBool(BoostHash, false);
                //mctrl.Boost (false);
            }
        }
    }
}