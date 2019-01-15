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

            Mctrl.EnableBoostFlame(animator.GetBool(AnimatorHashVars.BoostHash));

            if (Mctrl.GetOwner() != null && !Mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) return;

            UpdateAnimatorParameters(animator);

            animator.SetBool(AnimatorHashVars.BoostHash, Mctrl.IsBoosting);

            animator.SetBool(AnimatorHashVars.GroundedHash, Mctrl.Grounded);
        }
    }
}
	