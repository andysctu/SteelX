using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine.MechMovement
{
    public class VerticalBoostingState : MechStateMachineBehaviour
    {

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);

            if (!PhotonNetwork.isMasterClient || !cc.enabled) return;

            //animator.SetBool (animatorVars.OnMeleeHash, false);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (!cc.enabled) return;

            mctrl.EnableBoostFlame(animator.GetBool("Boost"));

            if (mctrl.GetOwner() != null && !mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) return;

            animator.SetFloat(SpeedHash, Mathf.Lerp(animator.GetFloat(SpeedHash), mctrl.Speed, Time.deltaTime * 15));
            animator.SetFloat(DirectionHash, Mathf.Lerp(animator.GetFloat(DirectionHash), mctrl.Direction, Time.deltaTime * 15));

            animator.SetBool(BoostHash, mctrl.IsBoosting);

            animator.SetBool(GroundedHash, mctrl.Grounded);
        }
    }
}
	