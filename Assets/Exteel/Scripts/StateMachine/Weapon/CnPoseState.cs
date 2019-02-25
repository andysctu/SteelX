using UnityEngine;
using Weapons;

namespace StateMachine.Attack
{
    public class CnPoseState : MechStateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            animator.SetBool(AnimatorHashVars.CnPoseHash, false);
        }
    }
}