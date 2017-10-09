using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

		// Decrease speed if walking backwards
		bool isWalkingBackwards = (Input.GetAxis ("Vertical") < 0) && !isHorizBoosting;

		// Transform direction from local space to world space
		move = transform.TransformDirection(move);

		ySpeed -= Gravity;

		ableToVertBoost = jump && (Input.GetKeyUp(KeyCode.Space) || !Input.GetKey(KeyCode.Space)) && mechCombat.EnoughFuelToBoost();

		if (CharacterController.isGrounded) {
			grounded = true;
			jump = false;

			if (Input.GetKey(KeyCode.Space)) {
				jump = true;
				grounded = false;

				ySpeed = mechCombat.JumpPower();
			}

			if (Input.GetKey(KeyCode.LeftShift)) {
				boost = true;

				mechCombat.DecrementFuel();
				characterControllerSpeed = mechCombat.BoostSpeed();
			} else {
				boost = false;

				mechCombat.IncrementFuel();
				characterControllerSpeed = mechCombat.MoveSpeed();
			}
		} else {
			Debug.Log("here able: " + ableToVertBoost);
			if (ableToVertBoost && Input.GetKey(KeyCode.Space)) {
				boost = true;

				ySpeed = mechCombat.VerticalBoostSpeed();
				mechCombat.DecrementFuel();
			} else {
				mechCombat.IncrementFuel();
			}
		}

		updateSpeed(characterControllerSpeed);

		CharacterController.Move(move);

//		// Handle movement in air
//		if (!CharacterController.isGrounded) {
//			if (!isVertBoosting) { // If we are in the air and not boosting
//				ySpeed -= Gravity; // Fall
//			} else { // If we are in the air and also boosting
//				ySpeed += mechCombat.VerticalBoostSpeed(); // Start vertical boosting if we've jumped
//				if (ySpeed > mechCombat.MaxVerticalBoostSpeed()) ySpeed = mechCombat.MaxVerticalBoostSpeed(); // Cap vertical speed
//			}
//		} else {
//			ySpeed = 0;
//			jumped = false;
//
//			// Can't vertical boost while on the ground
//			ableToVertBoost = false;
//			isVertBoosting = false;
//
//			// Animation
//			animator.SetBool("Grounded", true);
//		}
//			
//		if (Input.GetButton ("Jump") && !jumped) {
//			ySpeed = mechCombat.JumpPower();
//			jumped = true;
//
//			// Animation
//			animator.SetBool("Jump", true);
//			animator.SetBool("Grounded", false);
//		} else {
//			// Animation
//			animator.SetBool("Jump", false);
//		}

//		// We are only able to vertically boost if we've jumped, and if we're not already pressing space
//		if (!ableToVertBoost) {
//			ableToVertBoost = jumped && (Input.GetKeyUp("space") || !Input.GetKey("space")) && mechCombat.EnoughFuelToBoost();
//		}
//
//		// Prevent starting a boost when below min fuel
//		if (!isHorizBoosting) {
//			startBoosting = Input.GetKey ("left shift") && mechCombat.EnoughFuelToBoost();
//			isHorizBoosting = startBoosting;
//		} 
//			
//		// NOTE: ableToVertBoost and startBoosting depend on EnoughFuelToBoost, whereas if you are already boosting, you just need !FuelEmpty()
//
//		Vector3 curPos = camTransform.localPosition;
//
//		// Handle horizontal boosting and vertical boosting
//		if (startBoosting && Input.GetKey ("left shift") && !mechCombat.FuelEmpty()) {
//			isHorizBoosting = true;
//			animator.SetBool("Boost", true);
//
//			// Camera work
//			Vector3 newPos = camTransform.localPosition;
//			float h = Input.GetAxis("Horizontal");
//			if (h > 0) {
//				newPos = new Vector3(-7, curPos.y, curPos.z);
//			} else if (h < 0) {
//				newPos = new Vector3(7, curPos.y, curPos.z);
//			} else {
//				newPos = new Vector3(0, curPos.y, curPos.z);
//			}
//			camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);
//
//			// Use fuel while boosting
//			mechCombat.DecrementFuel();
//
//			updateSpeed(mechCombat.BoostSpeed());
//			photonView.RPC ("Boost", PhotonTargets.All, true);
//		} else if (ableToVertBoost && (Input.GetKey("space") || Input.GetKey("left shift")) && !mechCombat.FuelEmpty()) {
//			isVertBoosting = true;
//			animator.SetBool("Boost", true);
//
//			// Camera work
//			Vector3 newPos = camTransform.localPosition;
//			float h = Input.GetAxis("Horizontal");
//			if (h > 0) {
//				newPos = new Vector3(-7, curPos.y, curPos.z);
//			} else if (h < 0) {
//				newPos = new Vector3(7, curPos.y, curPos.z);
//			} else {
//				newPos = new Vector3(0, curPos.y, curPos.z);
//			}
//			camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);
//
//			// Use fuel while boosting
//			mechCombat.DecrementFuel();
//
//			updateSpeed(mechCombat.BoostSpeed());
//			photonView.RPC ("Boost", PhotonTargets.All, true);
//		}
//		// Handle walking
//		else {
//			animator.SetBool("Boost", false);
//
//			Vector3 newPos = new Vector3(0, curPos.y, curPos.z);
//			camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);
//			
//			isHorizBoosting = false;
//			isVertBoosting = false;
//
//			// Recharge fuel while not boosting
//			mechCombat.IncrementFuel();
//
//			updateSpeed(mechCombat.MoveSpeed());
//			photonView.RPC ("Boost", PhotonTargets.All, false);
//		}

//		CharacterController.Move (move);
	}

	void LateUpdate() {
		animator.SetFloat("Speed", speed);
		animator.SetFloat("Direction", direction);
		animator.SetBool("Jump", jump);
		animator.SetBool("Grounded", grounded);
		animator.SetBool("Boost", boost);
	}

	void updateSpeed(float horizontalSpeed) {
		move.x *= horizontalSpeed * Time.fixedDeltaTime;
		move.z *= horizontalSpeed * Time.fixedDeltaTime;
		move.y += ySpeed * Time.fixedDeltaTime;
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
