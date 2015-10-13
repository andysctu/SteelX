using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class MechController : MonoBehaviour {

	public static CharacterController CharacterController;
	public static MechController Instance;

	public float Gravity = 1.0f;
	public float JumpPower = 20.0f;
	public float MoveSpeed = 20.0f;
	public float BoostSpeed;
	public float VerticalBoostSpeed;
//	public float Friction = 1.0f;
	public bool isBoosting = false;

	public float MaxFuel = 100.0f;
	public float CurrentFuel;
	public float FuelDrain = 1.0f;
	public float FuelGain = 1.0f;
	public float MinFuelRequired;

	private float marginOfError = 0.1f;

	public bool jumped = false;

	public float xSpeed = 0f, ySpeed = 0f, zSpeed = 0f;
	public float xAcc = 0f, yAcc = 0f, zAcc = 0f;

	//private bool isBoosting = false;
	private bool startBoosting = false;

	private Vector3 move = Vector3.zero;

	// Use this for initialization
	void Awake () {
		CharacterController = GetComponent ("CharacterController") as CharacterController;
		Instance = this;
		CurrentFuel = MaxFuel;
		BoostSpeed = MoveSpeed + 20;
	}
	
	// Update is called once per frame
	void Update () {
		if (Camera.main == null) {
			return;
		}

		GetXZDirection();

		//MechMotor.Instance.UpdateMotor ();
	}

	void LateUpdate() {

		move = transform.TransformDirection(move);

//		xSpeed += xAcc;
//		zSpeed += zAcc;
//
//		if (xSpeed > 0 && xSpeed > Friction) {
//			xSpeed -= Friction;
//		}
//
//		if (xSpeed < 0 && xSpeed < -Friction) {
//			xSpeed += Friction;
//		}
//
//		if (zSpeed > 0 && zSpeed > Friction) {
//			zSpeed -= Friction;
//		}
//		
//		if (zSpeed < 0 && zSpeed < -Friction) {
//			zSpeed += Friction;
//		}
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
//		xAcc = 0;
//		yAcc = 0;
//		zAcc = 0;

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
