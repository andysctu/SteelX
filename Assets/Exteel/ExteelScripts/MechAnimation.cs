using UnityEngine;
using System.Collections;

public class MechAnimation : MonoBehaviour {

	private Animator animator;
	// Use this for initialization

	static int idleState = Animator.StringToHash("Base Layer.Idle");	
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");			// these integers are references to our animator's states
//	static int jumpState = Animator.StringToHash("Base Layer.Jump");				// and are used to check state for various actions to occur
//	static int jumpDownState = Animator.StringToHash("Base Layer.JumpDown");		// within our FixedUpdate() function below
//	static int fallState = Animator.StringToHash("Base Layer.Fall");
//	static int rollState = Animator.StringToHash("Base Layer.Roll");
//	static int waveState = Animator.StringToHash("Layer2.Wave");

	void Start () {
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float h = Input.GetAxis("Horizontal");				// setup h variable as our horizontal input axis
		float v = Input.GetAxis("Vertical");				// setup v variables as our vertical input axis
		Debug.Log ("Speed is : " + v.ToString());
		Debug.Log ("Direction is : " + h.ToString());
		animator.SetFloat("Speed", v);							// set our animator's float parameter 'Speed' equal to the vertical input axis				
		animator.SetFloat("Direction", h); 						// set our animator's float parameter 'Direction' equal to the horizontal input axis		
		//animator.SetLayerWeight(0,1);
	}
}
