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
	private Vector3 originalCamPos;

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
		animator = transform.Find("CurrentMech").gameObject.GetComponent<Animator>();
	}

	void initTransforms() {
		camTransform = transform.Find("Camera");
		originalCamPos = camTransform.localPosition;
	}

	// Update is called once per frame
	void Update () {
		// Update the Vector3 move variable
		GetXZDirection();

		// Do nothing if CharacterController not found
		if (CharacterController == null || !CharacterController.enabled){
			return;
		}

		if (!CharacterController.isGrounded) {
			ySpeed -= Gravity;
		} else {
			ySpeed = 0;
		}

		if (animator.GetBool("Boost")) {
			Debug.Log("Boosting");
			DynamicCam();
		}

		UpdateSpeed();
	}

	public void UpdateSpeed() {
		move.x *= xSpeed * Time.fixedDeltaTime;
		move.z *= zSpeed * Time.fixedDeltaTime;
		move.y += ySpeed * Time.fixedDeltaTime;
		CharacterController.Move(move);
	}

	public void VerticalBoost() {
		ySpeed = mechCombat.MaxVerticalBoostSpeed();
	}

	public void Jump() {
		ySpeed = mechCombat.JumpPower();
		UpdateSpeed();
	}

	public void Run() {
		xSpeed = mechCombat.MoveSpeed();
		zSpeed = mechCombat.MoveSpeed();
	}

	public void Boost() {
		xSpeed = mechCombat.BoostSpeed();
		zSpeed = mechCombat.BoostSpeed();
	}

	public void ResetCam() {
		camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, new Vector3(0, originalCamPos.y, originalCamPos.z), 0.1f);
	}

	public void DynamicCam() {
		Vector3 curPos = camTransform.localPosition;
		Vector3 newPos = camTransform.localPosition;

		if (direction > 0) {
			newPos = new Vector3(-7, curPos.y, curPos.z);
		} else if (direction < 0) {
			newPos = new Vector3(7, curPos.y, curPos.z);
		} else {
			newPos = new Vector3(0, curPos.y, curPos.z);
		}
		camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);
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
			move += new Vector3(h, 0, 0);
		}

		move = transform.TransformDirection(move);
		if (move.magnitude > 1) {
			move = Vector3.Normalize(move);
		}
			
//		speed = v;
//		direction = h;

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
