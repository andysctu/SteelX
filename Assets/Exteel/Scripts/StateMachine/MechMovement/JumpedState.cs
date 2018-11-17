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
        animator.SetBool(GroundedHash, mctrl.Grounded);
        animator.SetBool(BoostHash, mctrl.IsBoosting);//avoid shift+space directly vertically boost

        if (!animator.GetBool(JumpHash)) {//dir falling
            animator.SetBool(JumpHash, mctrl.IsJumping);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!cc.enabled) return;
        mctrl.EnableBoostFlame(animator.GetBool("Boost"));

        animator.SetFloat(SpeedHash, Mathf.Lerp(animator.GetFloat(SpeedHash), mctrl.Speed, Time.deltaTime * 15));
        animator.SetFloat(DirectionHash, Mathf.Lerp(animator.GetFloat(DirectionHash), mctrl.Direction, Time.deltaTime * 15));

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

        if (mctrl.IsBoosting){
            animator.SetBool(BoostHash, true);
        }
    }
}