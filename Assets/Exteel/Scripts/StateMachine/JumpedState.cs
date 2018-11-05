using UnityEngine;

public class JumpedState : MechStateMachineBehaviour {
    public bool isFirstjump = false;

    private bool _playedOnLandingAction;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);
        if (!cc.enabled)return;

        _playedOnLandingAction = false;

        if (isFirstjump) {
            mctrl.OnJumpAction();
        }

        if (!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) return;

        //mctrl.Boost(false);
        animator.SetBool(GroundedHash, false);
        animator.SetBool(BoostHash, false);//avoid shift+space directly vertically boost

        if (!animator.GetBool(JumpHash)) {//dir falling
            animator.SetBool(JumpHash, true);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!cc.enabled) return;

        if (mctrl.Grounded){
            animator.SetBool(JumpHash, false);
            animator.SetBool(GroundedHash, true);
            return;
        }

        animator.SetBool(JumpHash, true);

        if (!isFirstjump && animator.GetBool("Grounded")) {//TODO : consider move this part to groundedstate
            if (!_playedOnLandingAction) {
                _playedOnLandingAction = true;
                mctrl.OnLandingAction();
            }
        }

        if (!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) return;

        animator.SetFloat(SpeedHash, mctrl.Speed);
        animator.SetFloat(DirectionHash, mctrl.Direction);

        if (mctrl.IsBoosting){
            animator.SetBool(BoostHash, true);
        }
    }
}