using UnityEngine;

public class HorizontalBoostingState : MechStateMachineBehaviour {

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        base.Init(animator);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		EffectController.UpdateBoostingDust ();

		if ((!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) || !cc.enabled)return;

		animator.SetFloat(SpeedHash, mctrl.Speed);
		animator.SetFloat(DirectionHash, mctrl.Direction);

	    if (!mctrl.Grounded){
            animator.SetBool(GroundedHash, false);
	    }

	    if (mctrl.IsJumping){
            animator.SetBool(JumpHash, true);
	    }

	    animator.SetBool(BoostHash, mctrl.IsBoosting);
	}
}
