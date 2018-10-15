using UnityEngine;

public class JumpedState : MechStateMachineBehaviour {
    public static bool jumpReleased = false;
    public bool isFirstjump = false;

    private bool _playedOnLandingAction;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);
        if (!cc.enabled)return;

        _playedOnLandingAction = false;

        if (isFirstjump) {
            mctrl.OnJumpAction();
        } else if (!animator.GetBool("Jump")) {//dir falling
            jumpReleased = true;
        }

        if (!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) return;

        //mctrl.Boost(false);
        animator.SetBool(GroundedHash, false);
        animator.SetBool(BoostHash, false);//avoid shift+space directly vertically boost

        if (isFirstjump) {
            jumpReleased = false;
        } else if (!animator.GetBool(JumpHash)) {//dir falling
            animator.SetBool(JumpHash, true);
            jumpReleased = true;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!cc.enabled) return;

        if (!isFirstjump && animator.GetBool("Grounded")) {
            if (!_playedOnLandingAction) {
                _playedOnLandingAction = true;
                mctrl.OnLandingAction();
            }
        }

        if (!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) return;

        if (!mctrl.Buttons[(int)HandleInputs.Button.Space]) {
            jumpReleased = true;
        }

        if (!isFirstjump && mctrl.CheckIsGrounded() && !animator.GetBool(BoostHash)) {//the first jump is on ground
            mctrl.grounded = true;
            animator.SetBool(GroundedHash, true);
            animator.SetBool(JumpHash, false);
            mctrl.SetCanVerticalBoost(false);
            return;
        } else {
            //mctrl.JumpMoveInAir();
        }

        if (mctrl.Buttons[(int)HandleInputs.Button.Space] && jumpReleased && mctrl.CanVerticalBoost()) {
            mctrl.SetCanVerticalBoost(false);
            jumpReleased = false;
            animator.SetBool(BoostHash, true);
            //mctrl.Boost(true);
        }
    }
}