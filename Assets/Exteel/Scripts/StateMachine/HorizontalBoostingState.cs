using UnityEngine;
using UnityEngine.Animations;

public class HorizontalBoostingState : MechStateMachineBehaviour {
	
    private bool isBoostingUsingShift, doubleButtonDown;
    private float lastInputDownTime;
    private KeyCode lastInput = KeyCode.None;
    private const float DetectButtonDownInterval = 0.4f;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
        base.Init(animator);
        if (!PhotonNetwork.isMasterClient || !cc.enabled) return;
        isBoostingUsingShift = HandleInputs.CurUserCmd.Buttons[(int)HandleInputs.Button.LeftShift];
        doubleButtonDown = !isBoostingUsingShift;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.Init(animator);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		EffectController.UpdateBoostingDust ();
		if (!PhotonNetwork.isMasterClient || !cc.enabled)return;

		animator.SetFloat(SpeedHash, mctrl.speed);
		animator.SetFloat(DirectionHash, mctrl.direction);

		if(animator.GetBool(JumpHash))return;

		if(!mctrl.CheckIsGrounded()){//falling
			mctrl.SetCanVerticalBoost (true);
			mctrl.grounded = false;
			animator.SetBool (GroundedHash, false);
			animator.SetBool (BoostHash, false);//avoid dir go to next state (the transition interrupts by next state)
			mctrl.Boost (false);
			return;
		}

        //if (Input.GetKeyUp(KeyCode.LeftShift)) {
        //    isBoostingUsingShift = false;
        //} else if (Input.GetKeyDown(KeyCode.LeftShift)) {
        //    isBoostingUsingShift = true;
        //}

	    isBoostingUsingShift = HandleInputs.CurUserCmd.Buttons[(int)HandleInputs.Button.LeftShift];

        //if (doubleButtonDown) {
        //    if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D)) {
        //        doubleButtonDown = false;
        //    }
        //} else{
        //    CheckDoubleButtonDown();
        //}               

        if (HandleInputs.CurUserCmd.Buttons[(int)HandleInputs.Button.Space]) {
            mctrl.grounded = false;
            animator.SetBool(GroundedHash, false);

            mctrl.SetCanVerticalBoost(true);
			animator.SetBool(JumpHash, true);
		}

		if ((!isBoostingUsingShift && !doubleButtonDown) || !mcbt.IsENAvailable ()) {
			//mctrl.Run ();
			animator.SetBool (BoostHash, false);
			mctrl.Boost (false);
			return;
		} else{
			animator.SetBool (BoostHash, true);
			mctrl.Boost (true);
		}
	}

    //When in state transition , the update gets called 2 times in the same frame , thus the check (Time.time - lastInputDownTime) > 0.05f
    //private void CheckDoubleButtonDown() {
    //    if (Input.GetKeyDown(KeyCode.W)) {
    //        if (Time.time - lastInputDownTime < DetectButtonDownInterval && Time.time - lastInputDownTime > 0.05f && lastInput == KeyCode.W) {
    //            doubleButtonDown = true;
    //        }
    //        lastInput = KeyCode.W;
    //        lastInputDownTime = Time.time;
    //    } else if (Input.GetKeyDown(KeyCode.A)) {
    //        if (Time.time - lastInputDownTime < DetectButtonDownInterval  && Time.time - lastInputDownTime > 0.05f && lastInput == KeyCode.A) {
    //            doubleButtonDown = true;
    //        }
    //        lastInput = KeyCode.A;
    //        lastInputDownTime = Time.time;            
    //    } else if (Input.GetKeyDown(KeyCode.S)) {
    //        if (Time.time - lastInputDownTime < DetectButtonDownInterval && Time.time - lastInputDownTime > 0.05f && lastInput == KeyCode.S) {
    //            doubleButtonDown = true;
    //        }
    //        lastInput = KeyCode.S;
    //        lastInputDownTime = Time.time;            
    //    } else if (Input.GetKeyDown(KeyCode.D)) {
    //        if (Time.time - lastInputDownTime < DetectButtonDownInterval && Time.time - lastInputDownTime > 0.05f && lastInput == KeyCode.D) {
    //            doubleButtonDown = true;
    //        }
    //        lastInput = KeyCode.D;
    //        lastInputDownTime = Time.time;            
    //    }
    //}
}
