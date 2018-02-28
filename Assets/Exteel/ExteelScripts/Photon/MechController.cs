using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// MechController controls the position of the player
public class MechController : Photon.MonoBehaviour {

	public CharacterController CharacterController;
	public Animator animator;
	public Sounds Sounds;
	public LayerMask Terrain;
	[SerializeField] GameObject boostFlame;

	public float Gravity = 4.0f;
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
	private bool isBoostFlameOn = false;
	private bool isSlowDown = false;
	private Coroutine coroutine = null;

	[SerializeField]
	private MechCombat mechCombat;
	private Transform camTransform;
	private GameManager gm;
	[SerializeField]
	private Camera cam;
	private Vector3 originalCamPos;

	private float characterControllerSpeed;
	private float SlashMovingSpeed;
	private Vector3 Slashdir;
	private bool canVerticalBoost = true;

	// Animation
	private float speed;
	private float direction;
	public bool boost;
	public bool grounded = true;
	public bool jump;


	// Unused
	[SerializeField] Transform[] Legs;

	// Use this for initialization
	void Start () {
		initComponents();
		initTransforms();
		animator.SetBool ("Grounded", true);
	}

	void initComponents() {
		CharacterController = GetComponent<CharacterController> ();
		animator = transform.Find("CurrentMech").gameObject.GetComponent<Animator>();
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	void initTransforms() {
		camTransform = transform.Find("Camera");
		originalCamPos = camTransform.localPosition;
	}

	// Update is called once per frame
	void Update () {
		// Update the Vector3 move variable
		GetXZDirection();
	}

	void FixedUpdate() {
		// Do nothing if CharacterController not found & slashing
		if (CharacterController == null || !CharacterController.enabled){
			return;
		}

		// slash z-offset
		if (mechCombat.isLSlashPlaying == 1 ||mechCombat.isRSlashPlaying == 1) {
			if(SlashMovingSpeed >0.1f){
				if(grounded){
					Slashdir = new Vector3 (Slashdir.x, 0, Slashdir.z);	// make sure not slashing to the sky
				}
				CharacterController.Move(Slashdir * SlashMovingSpeed);
				SlashMovingSpeed /= 1.5f;
			}
			return;
		}
		if(!CheckIsGrounded()){
			ySpeed -= Gravity;
		} else {
			//ySpeed = 0;
			ySpeed = -CharacterController.stepOffset / Time.deltaTime;
		}

		if (animator == null) {
			return;
		}

		if (animator.GetBool("Boost")) {
			DynamicCam();
			mechCombat.DecrementFuel();
		} else {
			ResetCam();
			mechCombat.IncrementFuel();
		}

		UpdateSpeed();
	}

	public void UpdateSpeed() {
		move.x *= xSpeed * Time.fixedDeltaTime;
		move.z *= zSpeed * Time.fixedDeltaTime;
		move.y += ySpeed * Time.fixedDeltaTime;

		if(isSlowDown){
			move.x = move.x * 0.2f;
			move.z = move.z * 0.2f;
		}

		if(!gm.GameIsBegin){
			move.x = 0;
			move.z = 0;
		}
		CharacterController.Move(move);
	}
	public void SetSlashMoving(Vector3 dir, float speed){
		SlashMovingSpeed = speed;
		Slashdir = dir;
	}
	public void SetCanVerticalBoost(bool canVBoost) {
		canVerticalBoost = canVBoost;
	}

	public bool CanVerticalBoost() {
		return canVerticalBoost;
	}

	public void VerticalBoost() {
		ySpeed = mechCombat.MaxVerticalBoostSpeed();
	}

	public void Jump() {
		grounded = true;
		transform.position = new Vector3 (transform.position.x, transform.position.y + 0.2f, transform.position.z);
		ySpeed = mechCombat.JumpPower();
		UpdateSpeed();
	}

	public void Run() {
		xSpeed = mechCombat.MoveSpeed();
		zSpeed = mechCombat.MoveSpeed();
	}

	public void Boost(bool boost) {
		if(boost != isBoostFlameOn){
			photonView.RPC ("BoostFlame", PhotonTargets.All, boost);
			isBoostFlameOn = boost;
		}
		if (boost) {
			if(grounded){
				xSpeed = mechCombat.BoostSpeed ();
				zSpeed = mechCombat.BoostSpeed ();
			}else{//boost in air
				xSpeed = mechCombat.BoostSpeed ()*0.4f;
				zSpeed = mechCombat.BoostSpeed ()*0.4f;
			}
		}
	}

	public void BCNPose(){
		xSpeed = 0;
		zSpeed = 0;
	}

	public void SlowDown(float duration){
		if(isSlowDown){
			StopCoroutine (coroutine);
			coroutine = StartCoroutine ("SlowDownCoroutine", duration);
		}else{
			coroutine = StartCoroutine ("SlowDownCoroutine", duration);
			isSlowDown = true;
		}
	}

	IEnumerator SlowDownCoroutine(float duration){
		SetCanVerticalBoost (false);
		animator.SetBool ("Boost", false);
		Boost (false);

		yield return new WaitForSeconds (duration);
		isSlowDown = false;
		coroutine = null;
	}

	public void ResetCam() {
		Vector3 curPos = camTransform.localPosition;
		Vector3 newPos = new Vector3(0, curPos.y, curPos.z);
		camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);
		cam.fieldOfView = Mathf.Lerp (cam.fieldOfView, 60, 0.07f);
	}
	int pre = 0;
	public void DynamicCam() {
		Vector3 curPos = camTransform.localPosition;
		Vector3 newPos = camTransform.localPosition;

		if (direction > 0) {
			newPos = new Vector3(-7, curPos.y, curPos.z);
		} else if (direction < 0) {
			newPos = new Vector3(7, curPos.y,  curPos.z);
		} else {
			newPos = new Vector3(0, curPos.y,  curPos.z);
		}


		float speed = animator.GetFloat ("Speed");
		if (speed > 0 && grounded)
			cam.fieldOfView = Mathf.Lerp (cam.fieldOfView, 70, 0.07f);
		else if (speed<0 && grounded)
			cam.fieldOfView = Mathf.Lerp (cam.fieldOfView, 50, 0.07f);
		else
			cam.fieldOfView = Mathf.Lerp (cam.fieldOfView, 60, 0.07f);
		
		camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);
	}

	[PunRPC]
	void BoostFlame(bool boost) {
		//print ("set to : " + boost);
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

	public bool CheckIsGrounded(){
		return Physics.CheckSphere (transform.position + new Vector3 (0, 1.8f, 0), 2.0f, Terrain);
		//return CharacterController.isGrounded;
	}


}
