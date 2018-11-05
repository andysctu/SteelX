using UnityEngine;

public class GroundedState : MechStateMachineBehaviour {
    //TODO : implement these elsewhere
    //private float lastInputDownTime;
    //private bool doubleButtonDown = false;
    //private KeyCode lastInput = KeyCode.None;
    //private const float DetectButtonDownInterval = 0.4f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);
        if ((!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

        animator.SetBool(GroundedHash, true);
        animator.SetBool(animatorVars.OnMeleeHash, false);
        
        //TODO : implement these elsewhere
        //doubleButtonDown = false;
        //mcbt.CanMeleeAttack = true;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if ((!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

        animator.SetFloat(SpeedHash, mctrl.Speed);
        animator.SetFloat(DirectionHash, mctrl.Direction);

        if (animator.GetBool(JumpHash)) {
            return;
        }

        if (!mctrl.Grounded) {//check not jumping but is falling
            animator.SetBool(GroundedHash, false);
            return;
        }

        if (mctrl.IsJumping){
            animator.SetBool(GroundedHash, false);
            animator.SetBool(JumpHash, true);
            return;
        }

        if (mctrl.IsBoosting){
            animator.SetBool(BoostHash,true);
        }

        //CheckDoubleButtonDown();

        //if (Input.GetKeyUp(KeyCode.LeftShift)) {
        //    isBoosting = false;
        //} else if (Input.GetKeyDown(KeyCode.LeftShift)) {
        //    isBoosting = true;
        //}

        //if (HandleInputs.Shift) {
        //    isBoosting = false;
        //} else{
        //    isBoosting = true;
        //}

        //if (HandleInputs.CurUserCmd.Buttons[(int)HandleInputs.Button.Space] && !animator.GetBool(animatorVars.OnMeleeHash)) {
        //    mctrl.SetCanVerticalBoost(true);
        //    mctrl.grounded = false;
        //    animator.SetBool(GroundedHash, false);
        //    animator.SetBool(JumpHash, true);
        //    return;
        //}

        //if ((HandleInputs.CurUserCmd.Buttons[(int)HandleInputs.Button.LeftShift] || doubleButtonDown) && mcbt.EnoughENToBoost()) {
        //    if (mctrl.speed > 0 || mctrl.speed < 0 || mctrl.direction > 0 || mctrl.direction < 0) {
        //        animator.SetBool(BoostHash, true);
        //        mctrl.Boost(true);
        //    } else {
        //        mctrl.Boost(true);
        //    }
        //} else if (!animator.GetBool(BoostHash)) {
        //    mctrl.Boost(false);
        //    //mctrl.Run();
        //}

        //doubleButtonDown = false;
    }

    //TODO : remake this using HandleInputs
    //private void CheckDoubleButtonDown() {
    //    if (Input.GetKeyDown(KeyCode.W)) {
    //        if (Time.time - lastInputDownTime < DetectButtonDownInterval && lastInput == KeyCode.W) {
    //            doubleButtonDown = true;
    //        }
    //        lastInput = KeyCode.W;
    //        lastInputDownTime = Time.time;
    //    } else if (Input.GetKeyDown(KeyCode.A)) {
    //        if (Time.time - lastInputDownTime < DetectButtonDownInterval && lastInput == KeyCode.A) {
    //            doubleButtonDown = true;
    //        }
    //        lastInput = KeyCode.A;
    //        lastInputDownTime = Time.time;
    //    } else if (Input.GetKeyDown(KeyCode.S)) {
    //        if (Time.time - lastInputDownTime < DetectButtonDownInterval && lastInput == KeyCode.S) {
    //            doubleButtonDown = true;
    //        }
    //        lastInput = KeyCode.S;
    //        lastInputDownTime = Time.time;
    //    } else if (Input.GetKeyDown(KeyCode.D)) {
    //        if (Time.time - lastInputDownTime < DetectButtonDownInterval && lastInput == KeyCode.D) {
    //            doubleButtonDown = true;
    //        }
    //        lastInput = KeyCode.D;
    //        lastInputDownTime = Time.time;
    //    }
    //}
}