using UnityEngine;
using UnityEngine.Animations;

public class HorizontalBoostingState : MechStateMachineBehaviour {
	
    private bool isBoostingUsingShift, doubleButtonDown;
    private float lastInputDownTime;
    private KeyCode lastInput = KeyCode.None;
    private const float DetectButtonDownInterval = 0.4f;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
        base.Init(animator);
        if (cc == null || !cc.enabled) return;
        isBoostingUsingShift = Input.GetKey(KeyCode.LeftShift);
        doubleButtonDown = !isBoostingUsingShift;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.Init(animator);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		EffectController.UpdateBoostingDust ();
		if (cc == null || !cc.enabled)
			return;

		animator.SetFloat(speed_id, mctrl.speed);
		animator.SetFloat(direction_id, mctrl.direction);

		if(animator.GetBool(jump_id)){
            return;
		}

		if(!mctrl.CheckIsGrounded()){//falling
			mctrl.SetCanVerticalBoost (true);
			mctrl.grounded = false;
			animator.SetBool (grounded_id, false);
			animator.SetBool (boost_id, false);//avoid dir go to next state (the transition interrupts by next state)
			mctrl.Boost (false);
			return;
		}

        if (Input.GetKeyUp(KeyCode.LeftShift)) {
            isBoostingUsingShift = false;
        } else if (Input.GetKeyDown(KeyCode.LeftShift)) {
            isBoostingUsingShift = true;
        }

        if (doubleButtonDown) {
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D)) {
                doubleButtonDown = false;
            }
        } else{
            CheckDoubleButtonDown();
        }               

        if (!gm.BlockInput && Input.GetKeyDown(KeyCode.Space)) {
			mctrl.SetCanVerticalBoost(true);
			animator.SetBool(jump_id, true);
		}

		if (gm.BlockInput || (!isBoostingUsingShift && !doubleButtonDown) || !mcbt.IsENAvailable ()) {
			mctrl.Run ();
			animator.SetBool (boost_id, false);
			mctrl.Boost (false);
			return;
		} else{
			animator.SetBool (boost_id, true);
			mctrl.Boost (true);
		}
	}

    //When in state transition , the update gets called 2 times in the same frame , thus the check (Time.time - lastInputDownTime) > 0.05f
    private void CheckDoubleButtonDown() {
        if (Input.GetKeyDown(KeyCode.W)) {
            if (Time.time - lastInputDownTime < DetectButtonDownInterval && Time.time - lastInputDownTime > 0.05f && lastInput == KeyCode.W) {
                doubleButtonDown = true;
            }
            lastInput = KeyCode.W;
            lastInputDownTime = Time.time;
        } else if (Input.GetKeyDown(KeyCode.A)) {
            if (Time.time - lastInputDownTime < DetectButtonDownInterval  && Time.time - lastInputDownTime > 0.05f && lastInput == KeyCode.A) {
                doubleButtonDown = true;
            }
            lastInput = KeyCode.A;
            lastInputDownTime = Time.time;            
        } else if (Input.GetKeyDown(KeyCode.S)) {
            if (Time.time - lastInputDownTime < DetectButtonDownInterval && Time.time - lastInputDownTime > 0.05f && lastInput == KeyCode.S) {
                doubleButtonDown = true;
            }
            lastInput = KeyCode.S;
            lastInputDownTime = Time.time;            
        } else if (Input.GetKeyDown(KeyCode.D)) {
            if (Time.time - lastInputDownTime < DetectButtonDownInterval && Time.time - lastInputDownTime > 0.05f && lastInput == KeyCode.D) {
                doubleButtonDown = true;
            }
            lastInput = KeyCode.D;
            lastInputDownTime = Time.time;            
        }
    }
}
