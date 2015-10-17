using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class MechController : MonoBehaviour {

	public static CharacterController CharacterController;

	public float Gravity = 1.0f;
	public float JumpPower = 20.0f;
	public float MoveSpeed = 20.0f;
	public float BoostSpeed;
	public float VerticalBoostSpeed;
//	public float Friction = 1.0f;
	public bool isBoosting = false;

	public float Damage = 10f;

	public float MaxFuel = 100.0f;
	public float CurrentFuel;
	public float FuelDrain = 1.0f;
	public float FuelGain = 1.0f;
	public float MinFuelRequired;

	private float marginOfError = 0.1f;

	public bool jumped = false;

	public float TimeBetweenFire = 0.25f;
	public float xSpeed = 0f, ySpeed = 0f, zSpeed = 0f;

	//private bool isBoosting = false;
	private bool startBoosting = false;

	private Vector3 move = Vector3.zero;

	//private LayerMask layerMask = 1 << 8;
	private float timestamp;

	// Use this for initialization
	void Start () {
		CharacterController = GetComponent<CharacterController> ();
		CurrentFuel = MaxFuel;
		BoostSpeed = MoveSpeed + 20;
	}
	
	// Update is called once per frame
	void Update () {
		GetXZDirection();

		if (Input.GetKeyDown (KeyCode.Mouse0) ){//&& Time.time >= timestamp){
			timestamp =  Time.time + TimeBetweenFire;
			AudioSource audio = gameObject.GetComponent < AudioSource > ();
			audio.Play();
//			RaycastHit hit;
//			if (Physics.Raycast(transform.GetChild(0).position, transform.GetChild (0).forward, out hit, Mathf.Infinity, layerMask)) {
//				HitInfo hitInfo = new HitInfo();
//				hitInfo.damage = Damage;
//				hitInfo.raycastHit = hit;
//				hit.collider.SendMessage ("OnHit", hitInfo, SendMessageOptions.DontRequireReceiver);
//			}
		}
		//MechMotor.Instance.UpdateMotor ();
	}

	void FixedUpdate() {
		if (CharacterController == null) {
			CharacterController = GetComponent<CharacterController>();
			if (CharacterController == null) {
				Debug.Log ("char contr is null");
				return;
			}
		}
		move = transform.TransformDirection(move);
		move.y = 0;

		if (!CharacterController.isGrounded) {
			if (!isBoosting) {
				ySpeed -= Gravity;
			} else {
				if (jumped) ySpeed += VerticalBoostSpeed;
				if (ySpeed > MoveSpeed/2) ySpeed = MoveSpeed/2;
				//jumped = true;
			}
		} else {
			ySpeed = 0;
			jumped = false;
		}
		
		if (Input.GetKeyDown ("space") && !jumped) {
			ySpeed = JumpPower;
			jumped = true;
		}

		if (!isBoosting) {

			startBoosting = Input.GetKey ("left shift") && CurrentFuel >= MinFuelRequired;
			isBoosting = startBoosting;
		}

		if (isBoosting && CurrentFuel > 0 && Input.GetKey ("left shift")) {
			CurrentFuel -= FuelDrain;
			move.x *= BoostSpeed * Time.fixedDeltaTime;
			move.z *= BoostSpeed * Time.fixedDeltaTime;
			move.y += ySpeed * Time.fixedDeltaTime;
		} else {
			isBoosting = false;
			CurrentFuel += FuelGain;
			if (CurrentFuel > MaxFuel) CurrentFuel = MaxFuel;
			move.x *= MoveSpeed * Time.fixedDeltaTime;
			move.z *= MoveSpeed * Time.fixedDeltaTime;
			move.y += ySpeed * Time.fixedDeltaTime;
		}
		CharacterController.Move (move);


	}

	void GetXZDirection(){
		move = Vector3.zero;

		if (Input.GetAxis ("Vertical") > marginOfError || Input.GetAxis ("Vertical") < -marginOfError) {
			move += new Vector3(0, 0, Input.GetAxis ("Vertical"));
			//zAcc = Input.GetAxis ("Vertical");
		}

		if (Input.GetAxis ("Horizontal") > marginOfError || Input.GetAxis ("Horizontal") < -marginOfError) {
			move += new Vector3(Input.GetAxis ("Horizontal"), 0, 0);
			//xAcc = Input.GetAxis ("Horizontal");
		}

		if (move.magnitude > 1) {
			move = Vector3.Normalize (move);
		}
	}
}
