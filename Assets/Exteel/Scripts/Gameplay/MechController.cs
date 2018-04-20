using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// MechController controls the position of the player
public class MechController : Photon.MonoBehaviour {

	[SerializeField]private Camera cam;
	[SerializeField]private ParticleSystem boostFlame;
	[SerializeField]private AnimatorVars AnimatorVars;
	[SerializeField]private Animator Animator;
	[SerializeField]private MechCombat mechCombat;
	[SerializeField]private MechCamera mechCamera;
	[SerializeField]private Sounds Sounds;
	[SerializeField]private ParticleSystem boostDust;
	private GameManager gm;

	public CharacterController CharacterController;
	public LayerMask Terrain;

	private int boost_id;
	private bool isHorizBoosting = false;
	private bool isVertBoosting = false;

	private float marginOfError = 0.1f;
	public float Gravity = 4.5f;
	public float maxDownSpeed = -140f;
	public float InAirSpeedCoeff = 0.7f;
	public float xSpeed = 0f, ySpeed = 0f, zSpeed = 0f;
	private float curboostingSpeed;//global space
	private Vector2 xzDir;
	private Vector2 run_xzDir;
	private bool startBoosting = false;

	private Vector3 move = Vector3.zero;

	private bool isBoostFlameOn = false;
	private bool isSlowDown = false;
	private const float slowDownDuration = 0.3f;
	private Coroutine coroutine = null;

	private Transform camTransform;
	private Vector3 originalCamPos;

	private float characterControllerSpeed;
	private float forcemove_speed;
	private Vector3 forcemove_dir;
	private bool canVerticalBoost = false;
	private float v_boost_start_yPos;
	private float v_boost_upperbound ;
	private float boostStartTime = 0;//this is for jump delay
	private float slashTeleportMinDistance = 5f;
	// Animation
	private float speed;
	private float direction;
	public bool grounded = true; //changes with animator bool "grounded"
	public bool jump;
	public bool on_BCNShoot = false;

	private bool slashInJump = false;//temp

	// Use this for initialization
	void Start () {
		initComponents();
		initTransforms();
		initControllerVar ();
	}

	public void initControllerVar(){
		grounded = true;
		canVerticalBoost = false;
		isSlowDown = false;
		curboostingSpeed = mechCombat.MoveSpeed();
		Animator.SetBool ("Grounded", true);
		mechCamera.LockCamRotation (false);
		mechCamera.LockMechRotation (false);
		run_xzDir = Vector2.zero;
		boostDust.Stop ();
	}

	public void InitVars(){//this is called by AniamtorVars
		boost_id = AnimatorVars.boost_id;
	}

	void initComponents() {
		Transform currentMech = transform.Find("CurrentMech");
		CharacterController = GetComponent<CharacterController> ();
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
		if (CharacterController == null || !CharacterController.enabled){
			return;
		}

		if(!grounded){
			ySpeed -= (ySpeed<maxDownSpeed)? 0 : Gravity*Time.fixedDeltaTime*50 ;
			
			if(!Animator.GetBool("OnMelee"))
				DynamicCamInAir ();
		} else {
			ySpeed = (-CharacterController.stepOffset / Time.fixedDeltaTime)*0.2f;
		}
		if (Animator == null) {
			return;
		}

		if (Animator.GetBool(boost_id)) {
			DynamicCam();
			mechCombat.DecrementFuel();
		} else {
			ResetCam();
			mechCombat.IncrementFuel();
		}

		UpdateSpeed();
	}

	public void DynamicCamInAir() {
		Vector3 curPos = camTransform.localPosition;
		Vector3 newPos = camTransform.localPosition;

		if (direction > 0) {
			newPos = new Vector3(-5, curPos.y, curPos.z);
		} else if (direction < 0) {
			newPos = new Vector3(5, curPos.y,  curPos.z);
		} else {
			newPos = new Vector3(0, curPos.y,  curPos.z);
		}
		camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.08f);
	}

	public void UpdateSpeed() {
		// slash z-offset
		if (mechCombat.isLMeleePlaying == 1 ||mechCombat.isRMeleePlaying == 1 || on_BCNShoot) {

			if(grounded){
				forcemove_dir = new Vector3 (forcemove_dir.x, 0, forcemove_dir.z);	// make sure not slashing to the sky
			}

			forcemove_speed /= 1.6f;//1.6 : decrease coeff.
			if (Mathf.Abs (forcemove_speed) > 0.01f) {
				ySpeed = 0;
			}
				
			CharacterController.Move(forcemove_dir * forcemove_speed);
			move.x = 0;
			move.z = 0;

			//cast a ray downward to check if not jumping but not grounded => if so , directly teleport to ground
			RaycastHit hit;
			if(!Animator.GetBool(AnimatorVars.jump_id) && Physics.Raycast(transform.position,-Vector3.up,out hit,Terrain)){
				if(Vector3.Distance(hit.point, transform.position) >= slashTeleportMinDistance && !Physics.CheckSphere(hit.point+new Vector3(0,2.1f,0), CharacterController.radius, Terrain)){
					transform.position = hit.point;
				}
			}
			return;
		}


		move = Vector3.zero;
		move += Vector3.right * xSpeed * Time.fixedDeltaTime;
		move += Vector3.forward * zSpeed * Time.fixedDeltaTime;
		move.y += ySpeed * Time.fixedDeltaTime;

		if(isSlowDown){
			move.x = move.x * 0.2f;
			move.z = move.z * 0.2f;
		}

		if(!gm.GameIsBegin){//player can't move but can rotate
			move.x = 0;
			move.z = 0;
		}

		CharacterController.Move(move);
	}

	public void SetMoving(float speed){//called by animation
		forcemove_speed = speed;
		forcemove_dir = cam.transform.forward;
	}
	public void SetCanVerticalBoost(bool canVBoost) {
		canVerticalBoost = canVBoost;
	}

	public bool CanVerticalBoost() {
		return canVerticalBoost;
	}

	public void VerticalBoost() {
		if(v_boost_start_yPos==0){
			v_boost_start_yPos = transform.position.y;
			ySpeed = mechCombat.MaxVerticalBoostSpeed();
		}else{
			if(transform.position.y >= v_boost_start_yPos + mechCombat.MaxVerticalBoostSpeed()*1.25f){
				ySpeed = Gravity;
			}else{
				ySpeed = mechCombat.MaxVerticalBoostSpeed();
			}
		}
		float inAirSpeed = mechCombat.MaxHorizontalBoostSpeed () * InAirSpeedCoeff;
		xSpeed = inAirSpeed * xzDir.x * transform.right.x + inAirSpeed * xzDir.y * transform.forward.x;
		zSpeed = inAirSpeed * xzDir.x * transform.right.z +  inAirSpeed * xzDir.y * transform.forward.z;
	}

	public void Jump() {
		v_boost_start_yPos = 0;
		ySpeed = mechCombat.JumpPower();
	}

	public void Run() {
		run_xzDir.x = Mathf.Lerp (run_xzDir.x, xzDir.x, Time.deltaTime * 8);//smooth slow down (boosting -> Idle&Walk
		run_xzDir.y = Mathf.Lerp (run_xzDir.y, xzDir.y, Time.deltaTime * 8);//not achieving by gravity because we don't want walk smooth slow down
		//decelerating
		if (curboostingSpeed >= mechCombat.MoveSpeed() && !Animator.GetBool (boost_id)) {//not in transition to boost
			xSpeed = (run_xzDir.x * curboostingSpeed * transform.right).x +(run_xzDir.y * curboostingSpeed * transform.forward).x;
			zSpeed =  (run_xzDir.x * curboostingSpeed * transform.right).z +(run_xzDir.y * curboostingSpeed * transform.forward).z;
			curboostingSpeed -= mechCombat.deceleration * Time.deltaTime/2 ;
		}else{		
			xSpeed = mechCombat.MoveSpeed()*xzDir.x*transform.right.x + mechCombat.MoveSpeed()*xzDir.y*transform.forward.x;
			zSpeed = mechCombat.MoveSpeed()*xzDir.x*transform.right.z + mechCombat.MoveSpeed()*xzDir.y*transform.forward.z;
		}			
	}

	public void JumpMoveInAir(){
		if (curboostingSpeed >= mechCombat.MoveSpeed() && !Animator.GetBool (boost_id)) {//not in transition to boost
			curboostingSpeed -= mechCombat.deceleration/4 * Time.deltaTime * 20;

			xSpeed = (xzDir.x * curboostingSpeed * transform.right).x +(xzDir.y * curboostingSpeed * transform.forward).x ;
			zSpeed = (xzDir.x * curboostingSpeed * transform.right).z +(xzDir.y * curboostingSpeed * transform.forward).z;
		}else{
			float xRawDir = Input.GetAxisRaw ("Horizontal");

			xSpeed = mechCombat.MoveSpeed()*xRawDir *transform.right.x + mechCombat.MoveSpeed()*xzDir.y*transform.forward.x;
			zSpeed = mechCombat.MoveSpeed()*xRawDir *transform.right.z + mechCombat.MoveSpeed()*xzDir.y*transform.forward.z;
		}
	}
	public void Boost(bool b) {
		if(b != isBoostFlameOn){
			photonView.RPC ("BoostFlame", PhotonTargets.All, b);
			isBoostFlameOn = b;

			if (!b) {//shut the boost first call
				boostDust.Stop ();
			}else{//toggle on the boost first call
				if (grounded) {
					curboostingSpeed = mechCombat.MinHorizontalBoostSpeed ();
					xSpeed = (curboostingSpeed * xzDir.x * transform.right).x + (curboostingSpeed * xzDir.y * transform.forward).x;
					zSpeed = (curboostingSpeed * xzDir.y * transform.forward).z + (curboostingSpeed * xzDir.x * transform.right).z;

					boostDust.transform.localRotation = Quaternion.Euler (-90, Vector3.SignedAngle (Vector3.up, new Vector3 (-direction, speed, 0), Vector3.forward),0);
					boostDust.Play ();
				}
			}
		}
		if (b) {
			if(grounded){
				if(Mathf.Abs(speed)<0.1f && Mathf.Abs(direction)<0.1f){//boosting but not moving => lower current boosting speed
					if (curboostingSpeed >= mechCombat.MinHorizontalBoostSpeed())
						curboostingSpeed -= mechCombat.acceleration * Time.deltaTime * 10;
				}else{
					if (curboostingSpeed <= mechCombat.MaxHorizontalBoostSpeed ())
						curboostingSpeed += mechCombat.acceleration * Time.deltaTime * 5;
				}

				//ideal speed
				float idealSpeed_x = (curboostingSpeed * xzDir.x * transform.right).x + (curboostingSpeed * xzDir.y * transform.forward).x,
				idealSpeed_z = (curboostingSpeed * xzDir.y * transform.forward).z + (curboostingSpeed * xzDir.x * transform.right).z;

				Vector2 dir = new Vector2 (idealSpeed_x, idealSpeed_z).normalized;
				float acc_x = Mathf.Abs (mechCombat.acceleration * dir.x), acc_z = Mathf.Abs(mechCombat.acceleration * dir.y);

				Vector2 decreaseDir = new Vector2 (Mathf.Abs (xSpeed), Mathf.Abs (zSpeed)).normalized;

				if(Mathf.Abs(dir.x)<0.1f && xSpeed!=0){
					xSpeed += Mathf.Sign(0 - xSpeed) * mechCombat.acceleration * decreaseDir.x * Time.deltaTime * 40;
				}else{
					xSpeed += Mathf.Sign(idealSpeed_x - xSpeed) * acc_x * Time.deltaTime *100;
				}
				if(Mathf.Abs(dir.y)<0.1f && zSpeed!=0){
					zSpeed += Mathf.Sign(0 - zSpeed) * mechCombat.acceleration *decreaseDir.y* Time.deltaTime * 40;
				}else{
					zSpeed += Mathf.Sign(idealSpeed_z - zSpeed) * acc_z * Time.deltaTime *100;
				}

				boostDust.transform.localRotation = Quaternion.Euler (-90,Vector3.SignedAngle (Vector3.up, new Vector3 (-direction, speed, 0), Vector3.forward),0);
				
			}else{//boost in air
				float inAirSpeed = mechCombat.MaxHorizontalBoostSpeed () * InAirSpeedCoeff;
				xSpeed = inAirSpeed * xzDir.x * transform.right.x +  inAirSpeed * xzDir.y * transform.forward.x;
				zSpeed = inAirSpeed * xzDir.x * transform.right.z +  inAirSpeed * xzDir.y * transform.forward.z;
			}
		}
	}

	public void BCNPose(){
		xSpeed = 0;
		zSpeed = 0;
	}

	public void SlowDown(){
		if(isSlowDown){
			if(coroutine!=null)
				StopCoroutine (coroutine);
			
			coroutine = StartCoroutine ("SlowDownCoroutine");
		}else{
			coroutine = StartCoroutine ("SlowDownCoroutine");
			isSlowDown = true;
		}
	}

	IEnumerator SlowDownCoroutine(){
		SetCanVerticalBoost (false);
		Animator.SetBool ("Boost", false);
		Boost (false);
		if (!CheckIsGrounded ())//in air => small bump effect
			ySpeed = 0;

		yield return new WaitForSeconds (slowDownDuration);
		isSlowDown = false;
		coroutine = null;
	}

	public void ResetCam() {
		Vector3 curPos = camTransform.localPosition;
		Vector3 newPos = new Vector3(0, curPos.y, curPos.z);
		camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);

		mechCamera.orbitRadius = Mathf.Lerp (mechCamera.orbitRadius, 19, 0.05f);
		mechCamera.angleOffset = Mathf.Lerp (mechCamera.angleOffset, 33, 0.05f);
	}

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

		if (grounded) {//lerp camera z offset when boosting 
			if (speed > 0) {
				mechCamera.orbitRadius = Mathf.Lerp (mechCamera.orbitRadius, 26, 0.05f);
				mechCamera.angleOffset = Mathf.Lerp (mechCamera.angleOffset, 36, 0.05f);
			} else if (speed < 0) {
				if(direction > 0 || direction < 0){
					mechCamera.orbitRadius = Mathf.Lerp (mechCamera.orbitRadius, 16, 0.05f);
					mechCamera.angleOffset = Mathf.Lerp (mechCamera.angleOffset, 30, 0.05f);
				} else {
					mechCamera.orbitRadius = Mathf.Lerp (mechCamera.orbitRadius, 14, 0.05f);
					mechCamera.angleOffset = Mathf.Lerp (mechCamera.angleOffset, 30, 0.05f);
				}
			}
		}
		camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.05f);
	}

	[PunRPC]
	void BoostFlame(bool boost) {
		if (boost) {
			boostFlame.Play ();
			Sounds.PlayBoostStart ();
			Sounds.PlayBoostLoop ();
		}
		else {
			boostFlame.Stop ();
			Sounds.StopBoostLoop ();
		}
	}

	void GetXZDirection() {
		//move = Vector3.zero;
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		if (v <= marginOfError && v >= -marginOfError){
			v = 0;
		}
		if (h <= marginOfError && h >= -marginOfError){
			h = 0;
		}
			
		speed = v;
		direction = h;

		xzDir = new Vector2 (direction, speed).normalized;
	}

	public bool CheckIsGrounded(){
		
		return Physics.CheckSphere (transform.position + new Vector3 (0, 1.7f, 0), 2.0f, Terrain);
	}

	public void CallLockMechRot(bool b){
		mechCamera.LockMechRotation (b);
	}

	/*void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position + new Vector3 (0, 1.7f, 0), 2.0f);
	}*/
}
