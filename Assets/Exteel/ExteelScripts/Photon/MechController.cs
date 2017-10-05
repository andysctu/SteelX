using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MechController : Photon.MonoBehaviour {

	public CharacterController CharacterController;
	public Animator animator;

	[SerializeField] GameObject boostFlame;

	public float Gravity = 2.0f;
	public float JumpPower = 50.0f;
	public float MoveSpeed = 40.0f;
	public float BoostSpeed;
	public float VerticalBoostSpeed = 1f;
	public bool isHorizBoosting = false;

	private float marginOfError = 0.1f;
	private float minDownSpeed = 0.0f;

	public bool jumped = false;

	public float TimeBetweenFire = 0.25f;
	public float xSpeed = 0f, ySpeed = 0f, zSpeed = 0f;

	private bool startBoosting = false;

	private Vector3 move = Vector3.zero;

	bool ableToVertBoost = false;

	private MechCombat mechCombat;

	[SerializeField] Transform[] Legs;

	private Transform camTransform;

	// Use this for initialization
	void Start () {
		CharacterController = GetComponent<CharacterController> ();
		BoostSpeed = MoveSpeed + 20;
		mechCombat = GetComponent<MechCombat>();
		camTransform = transform.Find("Camera");
	}

	// Update is called once per frame
	void Update () {
		// This updates the private Vector3 move variable
		GetXZDirection();
	}

	void FixedUpdate() {
		if (CharacterController == null || !CharacterController.enabled){
			return;
		}

		bool isWalkingBackwards = (Input.GetAxis ("Vertical") < 0) && !isHorizBoosting;
		move = transform.TransformDirection(move);
		move.y = minDownSpeed;

		if (!CharacterController.isGrounded) {
			if (!isHorizBoosting) {
				ySpeed -= Gravity;
			} else {
				if (jumped) ySpeed += VerticalBoostSpeed;
				if (ySpeed > MoveSpeed/2) ySpeed = MoveSpeed/2;
			}
		} else {
			ySpeed = 0;
			jumped = false;
			ableToVertBoost = false;
			if (animator != null) animator.SetBool("Grounded", true);
		}

		if (Input.GetButton ("Jump") && !jumped) {
			if (animator != null) {
				animator.SetBool("Jump", true);
				animator.SetBool("Grounded", false);
			}
			ySpeed = JumpPower;
			jumped = true;
		} else {
			if (animator != null) animator.SetBool("Jump", false);
		}
			
		Vector3 forwardPos = camTransform.localPosition;

		// Need this to prevent starting a boost when below min fuel
		if (!isHorizBoosting) {
			startBoosting = Input.GetKey ("left shift") && mechCombat.CanBoost();
			isHorizBoosting = startBoosting;
		} 

		if (!ableToVertBoost) {
			ableToVertBoost = jumped && (Input.GetKeyUp("space") || !Input.GetKey("space")) && mechCombat.CanBoost();
		}

		Vector3 curPos = camTransform.localPosition;
		if ((startBoosting && Input.GetKey ("left shift") && mechCombat.FuelEmpty()) || (ableToVertBoost && Input.GetKey("space") && mechCombat.FuelEmpty())) {
			isHorizBoosting = true;
			if (animator != null) animator.SetBool("Boost", true);

			Vector3 newPos = camTransform.localPosition;
			float h = Input.GetAxis("Horizontal");
			if (h > 0) {
				newPos = new Vector3(-7, curPos.y, curPos.z);
			} else if (h < 0) {
				newPos = new Vector3(7, curPos.y, curPos.z);
			} else {
				newPos = new Vector3(0, curPos.y, curPos.z);
			}
			camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);

			mechCombat.DecrementFuel();

			move.x *= BoostSpeed * Time.fixedDeltaTime;
			move.z *= BoostSpeed * Time.fixedDeltaTime;
			move.y += ySpeed * Time.fixedDeltaTime;
			photonView.RPC ("Boost", PhotonTargets.All, true);
		} else {
			if (animator != null) animator.SetBool("Boost", false);

			Vector3 newPos = new Vector3(0, curPos.y, curPos.z);
			camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);
			
			isHorizBoosting = false;

			mechCombat.IncrementFuel();

			move.x *= MoveSpeed * Time.fixedDeltaTime;
			move.z *= (MoveSpeed) * Time.fixedDeltaTime; // Walking backwards should be slower
			move.y += ySpeed * Time.fixedDeltaTime;
			photonView.RPC ("Boost", PhotonTargets.All, false);
		}

		CharacterController.Move (move);
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
		if (animator != null) {
			animator.SetFloat("Speed", v);
			animator.SetFloat("Direction", h);
		}

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
