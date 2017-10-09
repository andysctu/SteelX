using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// MechController controls the position of the player
public class MechController : Photon.MonoBehaviour {

	public CharacterController CharacterController;
	public Animator animator;

	[SerializeField] GameObject boostFlame;

	public float Gravity = 2.0f;
	private bool isHorizBoosting = false;
	private bool isVertBoosting = false;

	private float marginOfError = 0.1f;
	private float minDownSpeed = 0.0f;

	public bool jumped = false;

	public float TimeBetweenFire = 0.25f;
	public float xSpeed = 0f, ySpeed = 0f, zSpeed = 0f;

	private bool startBoosting = false;

	private Vector3 move = Vector3.zero;

	private bool ableToVertBoost = false;

	private MechCombat mechCombat;
	private Transform camTransform;

	private float characterControllerSpeed;

	// Animation
	private float speed;
	private float direction;
	private bool boost;
	private bool grounded;
	private bool jump;

	// Unused
	[SerializeField] Transform[] Legs;

	// Use this for initialization
	void Start () {
		initComponents();
		initTransforms();
	}

	void initComponents() {
		CharacterController = GetComponent<CharacterController> ();
		mechCombat = GetComponent<MechCombat>();
	}

	void initTransforms() {
		camTransform = transform.Find("Camera");
	}

	// Update is called once per frame
	void Update () {
		// Update the Vector3 move variable
		GetXZDirection();
	}

	void FixedUpdate() {
		// Do nothing if CharacterController not found
		if (CharacterController == null || !CharacterController.enabled){
			return;
		}
			
		if (!CharacterController.isGrounded) {
			ySpeed -= Gravity;
		} else {
			ySpeed = 0;
		}

		UpdateSpeed (0);
	}

	void LateUpdate() {

	}

	public void UpdateSpeed(float horizontalSpeed) {
		Debug.Log ("yspeed: " + ySpeed);
		move.x *= horizontalSpeed * Time.fixedDeltaTime;
		move.z *= horizontalSpeed * Time.fixedDeltaTime;
		move.y += ySpeed * Time.fixedDeltaTime;
		CharacterController.Move(move);
	}

	public void VerticalBoost() {
		ySpeed = mechCombat.MaxVerticalBoostSpeed();
	}

	[PunRPC]
	void Boost(bool boost) {
		boostFlame.SetActive(boost);
	}

	void GetXZDirection() {
		move = Vector3.zero;
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		if (v > marginOfError || v < -marginOfError) {
			move += new Vector3(0, 0, v);
		}

		if (h > marginOfError || h < -marginOfError) {
			move += new Vector3(h, 	0, 0);
		}

		if (move.magnitude > 1) {
			move = Vector3.Normalize (move);
		}

		speed = v;
		direction = h;

//		Debug.Log("Speed: " + v);
//		Debug.Log("Direc: " + h);
//		Debug.Log(h);
//		if (h < 0) {
//			Legs[0].localRotation = Quaternion.Euler(new Vector3(0, -90,0));
//			Legs[1].localRotation = Quaternion.Euler(new Vector3(0, -90,0));
//		} else if (h > 0) {
//			Legs[0].localRotation = Quaternion.Euler(new Vector3(0, 90,0));
//			Legs[1].localRotation = Quaternion.Euler(new Vector3(0, 90,0));
//		} else {
//			Legs[0].localRotation = Quaternion.Euler(new Vector3(0, 0,0));
//			Legs[1].localRotation = Quaternion.Euler(new Vector3(0, 0,0));
//		}
	}
}
