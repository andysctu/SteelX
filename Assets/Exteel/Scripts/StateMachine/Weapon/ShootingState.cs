using UnityEngine;

namespace StateMachine.Attack
{
    public class ShootingState : MechStateMachineBehaviour
    {

        public int hand = 0;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
            //mechIK.SetIK (true, 0, hand);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            //mechIK.SetIK (false, 0, hand);
        }
    }
}
