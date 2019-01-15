using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine.Attack
{
    public class RCLState : MechStateMachineBehaviour
    {

        public int hand = 0;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
            MechIK.SetIK(true, 2, hand);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            MechIK.SetIK(false, 2, hand);
        }
    }
}