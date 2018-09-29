using UnityEngine;

public class GroundedState : MechStateMachineBehaviour {
    private float lastInputDownTime;
    private bool doubleButtonDown = false, isBoosting = false;
    private KeyCode lastInput = KeyCode.None;
    private const float DetectButtonDownInterval = 0.4f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);
        if (cc == null || !cc.enabled) return;
        animator.SetBool(GroundedHash, true);
        animator.SetBool(animatorVars.OnMeleeHash, false);
        mctrl.grounded = true;
        mcbt.CanMeleeAttack = true;
        doubleButtonDown = false;
        isBoosting = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (cc == null || !cc.enabled)
            return;

        animator.SetFloat(SpeedHash, mctrl.speed);
        animator.SetFloat(DirectionHash, mctrl.direction);

        if (animator.GetBool(JumpHash)) {
            mctrl.Run();//not lose speed in air
            return;
        }

        if (!mctrl.CheckIsGrounded()) {//check not jumping but is falling
            mctrl.grounded = false;
            mctrl.SetCanVerticalBoost(true);
            animator.SetBool(GroundedHash, false);
            return;
        }

        CheckDoubleButtonDown();

        if (Input.GetKeyUp(KeyCode.LeftShift)) {
            isBoosting = false;
        } else if (Input.GetKeyDown(KeyCode.LeftShift)) {
            isBoosting = true;
        }

        if (!gm.BlockInput && Input.GetKeyDown(KeyCode.Space) && !animator.GetBool(animatorVars.OnMeleeHash)) {
            mctrl.SetCanVerticalBoost(true);
            mctrl.grounded = false;
            animator.SetBool(GroundedHash, false);
            animator.SetBool(JumpHash, true);
            return;
        }

        if (!gm.BlockInput && (Input.GetKey(KeyCode.LeftShift) || doubleButtonDown) && mcbt.EnoughENToBoost()) {
            if (mctrl.speed > 0 || mctrl.speed < 0 || mctrl.direction > 0 || mctrl.direction < 0) {
                animator.SetBool(BoostHash, true);
                mctrl.Boost(true);
            } else {
                mctrl.Boost(true);
            }
        } else if (!animator.GetBool(BoostHash)) {
            mctrl.Boost(false);
            mctrl.Run();
        }

        doubleButtonDown = false;
    }

    private void CheckDoubleButtonDown() {
        if (Input.GetKeyDown(KeyCode.W)) {
            if (Time.time - lastInputDownTime < DetectButtonDownInterval && lastInput == KeyCode.W) {
                doubleButtonDown = true;
            }
            lastInput = KeyCode.W;
            lastInputDownTime = Time.time;
        } else if (Input.GetKeyDown(KeyCode.A)) {
            if (Time.time - lastInputDownTime < DetectButtonDownInterval && lastInput == KeyCode.A) {
                doubleButtonDown = true;
            }
            lastInput = KeyCode.A;
            lastInputDownTime = Time.time;
        } else if (Input.GetKeyDown(KeyCode.S)) {
            if (Time.time - lastInputDownTime < DetectButtonDownInterval && lastInput == KeyCode.S) {
                doubleButtonDown = true;
            }
            lastInput = KeyCode.S;
            lastInputDownTime = Time.time;
        } else if (Input.GetKeyDown(KeyCode.D)) {
            if (Time.time - lastInputDownTime < DetectButtonDownInterval && lastInput == KeyCode.D) {
                doubleButtonDown = true;
            }
            lastInput = KeyCode.D;
            lastInputDownTime = Time.time;
        }
    }
}