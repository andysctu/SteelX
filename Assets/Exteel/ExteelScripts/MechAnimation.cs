using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MechAnimation : NetworkBehaviour {

	[SyncVar (hook = "OnJumpChanged")] bool jump;

	private Animator animator;
	// Use this for initialization

	static int idleState = Animator.StringToHash("Base Layer.Idle");	
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");			// these integers are references to our animator's states
	static int jumpState = Animator.StringToHash("Base Layer.Jump");				// and are used to check state for various actions to occur
	static int backState = Animator.StringToHash("Base Layer.WalkBack");
//	static int jumpDownState = Animator.StringToHash("Base Layer.JumpDown");		// within our FixedUpdate() function below
//	static int fallState = Animator.StringToHash("Base Layer.Fall");
//	static int rollState = Animator.StringToHash("Base Layer.Roll");
//	static int waveState = Animator.StringToHash("Layer2.Wave");

	private AnimatorStateInfo currentBaseState;			// a reference to the current state of the animator, used for base layer

	void Start () {
		animator = GetComponent<Animator>();
//		if(animator.layerCount ==2)
//			animator.SetLayerWeight(1, 1);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!isLocalPlayer) return;
		float h = Input.GetAxis("Horizontal");				// setup h variable as our horizontal input axis
		float v = Input.GetAxis("Vertical");				// setup v variables as our vertical input axis
//		Debug.Log ("Speed is : " + v.ToString());
//		Debug.Log ("Direction is : " + h.ToString());
		animator.SetFloat("Speed", v);							// set our animator's float parameter 'Speed' equal to the vertical input axis				
		animator.SetFloat("Direction", h); 						// set our animator's float parameter 'Direction' equal to the horizontal input axis		
		//animator.SetLayerWeight(0,1);

		currentBaseState = animator.GetCurrentAnimatorStateInfo(0);	// set our currentState variable to the current state of the Base Layer (0) of animation

		// if we are currently in a state called Locomotion, then allow Jump input (Space) to set the Jump bool parameter in the Animator to true
		if (currentBaseState.fullPathHash == locoState || currentBaseState.fullPathHash == idleState || currentBaseState.fullPathHash == backState) {
			// Debug.Log ("idle or loco");
			if(Input.GetButtonDown("Jump")) {	
				// Debug.Log ("Jumping");
				animator.SetBool("Jump", true);
				jump = true;
			} else {
				animator.SetBool("Jump", false);
				jump = false;
			}

//			if (Input.GetKey("left shift")){
//				animator.SetBool ("Boost", true);
//			} else {
//				animator.SetBool("Boost", false);
//			}
		} else {
			Debug.Log ("Not in ");
		}
	}

	void OnJumpChanged(bool isJump){
		jump = isJump;
		animator.SetBool ("Jump", jump);
	}
}
