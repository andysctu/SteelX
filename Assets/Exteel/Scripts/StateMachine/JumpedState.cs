using UnityEngine;

public class JumpedState : MechStateMachineBehaviour {
    public static bool jumpReleased = false;
    public bool isFirstjump = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);
        if (!PhotonNetwork.isMasterClient || !cc.enabled){
            if (isFirstjump) {
                mctrl.OnJumpAction();
                mctrl.grounded = false;
            } else if (!animator.GetBool("Jump")) {//dir falling
                jumpReleased = true;
                mctrl.grounded = false;
            }
            return;
        }

        if (isFirstjump) {
            mctrl.OnJumpAction();

            mctrl.Boost(false);
            animator.SetBool(BoostHash, false);//avoid shift+space directly vertically boost
            jumpReleased = false;
            mctrl.grounded = false;
            animator.SetBool(GroundedHash, false);
        } else if (!animator.GetBool(JumpHash)) {//dir falling
            mctrl.Boost(false);
            animator.SetBool(BoostHash, false);
            animator.SetBool(JumpHash, true);
            animator.SetBool(GroundedHash, false);
            jumpReleased = true;
            mctrl.grounded = false;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!PhotonNetwork.isMasterClient || !cc.enabled){
            if (!isFirstjump && animator.GetBool("Grounded")){
                if (!mctrl.grounded){
                    mctrl.OnLandingAction();
                    mctrl.grounded = true;
                }
            }
            return;
        }

        //float speed = Input.GetAxis("Vertical");
        //float direction = Input.GetAxis("Horizontal");

        //animator.SetFloat(SpeedHash, speed);
        //animator.SetFloat(DirectionHash, direction);

        if (!HandleInputs.CurUserCmd.Buttons[(int)HandleInputs.Button.Space]) {
            jumpReleased = true;
        }

        if (!isFirstjump && mctrl.CheckIsGrounded() && !animator.GetBool(BoostHash)) {//the first jump is on ground
            if (!mctrl.grounded) {
                mctrl.OnLandingAction();
            }

            mctrl.grounded = true;
            animator.SetBool(GroundedHash, true);
            animator.SetBool(JumpHash, false);
            mctrl.SetCanVerticalBoost(false);
            return;
        } else {
            mctrl.JumpMoveInAir();
        }

        if (HandleInputs.CurUserCmd.Buttons[(int)HandleInputs.Button.Space] && jumpReleased && mctrl.CanVerticalBoost()) {
            mctrl.SetCanVerticalBoost(false);
            jumpReleased = false;
            animator.SetBool(BoostHash, true);
            mctrl.Boost(true);
        }
    }
}