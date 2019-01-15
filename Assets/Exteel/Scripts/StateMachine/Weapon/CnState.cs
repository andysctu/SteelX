using UnityEngine;

namespace StateMachine.Attack
{
    public class CnState : MechStateMachineBehaviour
    {

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
            if (cc == null || !cc.enabled) return;
            animator.SetBool(AnimatorHashVars.JumpHash, false);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;

            //Fix position
            if (animator.GetBool(AnimatorHashVars.CnPoseHash) || (animator.GetBool(AnimatorHashVars.CnShootHash) && !animator.IsInTransition(3))) Mctrl.CnPose();

            //shut down boost
            if (animator.GetBool(AnimatorHashVars.BoostHash) && !animator.IsInTransition(0)){
                animator.SetBool(AnimatorHashVars.BoostHash, false);
                //mctrl.Boost (false);
            }
        }
    }
}