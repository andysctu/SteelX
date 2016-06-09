using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NewMechController : MonoBehaviour {

	public CharacterController CharacterController;
	public Animator animator;

	public float Gravity = 2.0f;
	public float JumpPower = 50.0f;
	public float MoveSpeed = 40.0f;
	public float BoostSpeed;
	public float VerticalBoostSpeed = 1f;
	public bool isHorizBoosting = false;
//	public bool testGrounded;

	public float MaxFuel = 100.0f;
	public float CurrentFuel;
	public float FuelDrain = 1.0f;
	public float FuelGain = 1.0f;
	public float MinFuelRequired = 75f;

	private float marginOfError = 0.1f;
	private float minDownSpeed = 0.0f;

	public bool jumped = false;

	public float TimeBetweenFire = 0.25f;
	public float xSpeed = 0f, ySpeed = 0f, zSpeed = 0f;

	private Slider fuelBar;
	private bool startBoosting = false;

	private Vector3 move = Vector3.zero;

	bool ableToVertBoost = false;

	private MechCombat mechCombat;

	// Use this for initialization
	void Start () {
		CharacterController = GetComponent<CharacterController> ();
		CurrentFuel = MaxFuel;
		BoostSpeed = MoveSpeed + 20;
		Slider[] sliders = GameObject.Find("Canvas").GetComponentsInChildren<Slider>();
		if (sliders.Length > 1) {
			fuelBar = sliders[1];
			fuelBar.value = 1;
		} else {
			Debug.Log("Fuel bar null");
		}

		mechCombat = GetComponent<MechCombat>();
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

		// Need this to prevent starting a boost when below min fuel
		if (!isHorizBoosting) {
			startBoosting = Input.GetKey ("left shift") && CurrentFuel >= MinFuelRequired;
			isHorizBoosting = startBoosting;
		}

		if (!ableToVertBoost) {
			ableToVertBoost = jumped && (Input.GetKeyUp("space") || !Input.GetKey("space")) && CurrentFuel >= MinFuelRequired;
		}

		if ((startBoosting && Input.GetKey ("left shift") && CurrentFuel > 0) || (ableToVertBoost && Input.GetKey("space") && CurrentFuel > 0)) {
			isHorizBoosting = true;
			if (animator != null) animator.SetBool("Boost", true);
			CurrentFuel -= FuelDrain;
			move.x *= BoostSpeed * Time.fixedDeltaTime;
			move.z *= BoostSpeed * Time.fixedDeltaTime;
			move.y += ySpeed * Time.fixedDeltaTime;
			mechCombat.SetBoost(true);
		} else {
			if (animator != null) animator.SetBool("Boost", false);
			isHorizBoosting = false;
			CurrentFuel += FuelGain;
			if (CurrentFuel > MaxFuel) CurrentFuel = MaxFuel;
			move.x *= MoveSpeed * Time.fixedDeltaTime;
			move.z *= (MoveSpeed) * Time.fixedDeltaTime; // Walking backwards should be slower
			move.y += ySpeed * Time.fixedDeltaTime;
			mechCombat.SetBoost(false);
		}

		CharacterController.Move (move);

		if (fuelBar == null) {
			Slider[] sliders = GameObject.Find("Canvas").GetComponentsInChildren<Slider>();
			if (sliders.Length > 1) {
				fuelBar = sliders[1];
				Debug.Log("2Sliders length > 1");
			} else {
				Debug.Log("2Fuel bar null");
			}
		}
		float currentPercent = fuelBar.value;
		float targetPercent = CurrentFuel/(float)MaxFuel;
		float err = 0.01f;
		if (Mathf.Abs(currentPercent - targetPercent) > err) {
			currentPercent = currentPercent + (currentPercent > targetPercent ? -0.01f : 0.01f);
		}

		fuelBar.value = currentPercent;

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
	}
}
