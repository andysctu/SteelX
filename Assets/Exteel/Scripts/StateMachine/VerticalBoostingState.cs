using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalBoostingState : MechStateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.Init(animator);
		if ( !PhotonNetwork.isMasterClient || !cc.enabled) return;
		//mctrl.SetCanVerticalBoost(false);
		//animator.SetBool (animatorVars.OnMeleeHash, false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!PhotonNetwork.isMasterClient || !cc.enabled) return;

	    if (!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) return;

	    animator.SetFloat(SpeedHash, mctrl.Speed);
	    animator.SetFloat(DirectionHash, mctrl.Direction);


	    animator.SetBool(BoostHash, mctrl.IsBoosting);

	    if (mctrl.Grounded){
            animator.SetBool(GroundedHash, true);
	    }

        //if ( mcbt.IsENEmpty() || !HandleInputs.CurUserCmd.Buttons[(int)HandleInputs.Button.Space]) {
        //	//mctrl.Boost (false);
        //	//animator.SetFloat(SpeedHash, 0);
        //	animator.SetBool(BoostHash, false);
        //}else{
        //	mctrl.VerticalBoost();
        //}
    }
}
	