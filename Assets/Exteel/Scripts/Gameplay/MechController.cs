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
	public float Gravity = 6.5f;
	public float minDownSpeed = -30f;
	public float InAirSpeedCoeff = 0.7f;
	public float xSpeed = 0f, ySpeed = 0f, zSpeed = 0f;
	private float curboostingSpeed, curboostingVelocity_x, curboostingVelocity_z;

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
	public bool grounded = true; //this is the fastest way to check if not grounded , and also depends on characterController.isgrounded
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
		curboostingVelocity_x = mechCombat.MoveSpeed();
		curboostingVelocity_z = mechCombat.MoveSpeed();
		Animator.SetBool ("Grounded", true);
		mechCamera.LockCamRotation (false);
		mechCamera.LockMechRotation (false);

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

		if(!CheckIsGrounded()){
			ySpeed -= Gravity;
		} else {
			//ySpeed = 0;
			ySpeed = -CharacterController.stepOffset / Time.deltaTime;
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

	public void UpdateSpeed() {
		// slash z-offset
		if (mechCombat.isLSlashPlaying == 1 ||mechCombat.isRSlashPlaying == 1 || on_BCNShoot) {

			if(grounded){
				forcemove_dir = new Vector3 (forcemove_dir.x, 0, forcemove_dir.z);	// make sure not slashing to the sky
			}

			forcemove_speed /= 1.6f;//1.6 : decrease coeff.
			if (Mathf.Abs (forcemove_speed) > 0.01f) {
				if(!on_BCNShoot)
					mechCamera.LockMechRotation (true);
				ySpeed = 0;
			}else{
				mechCamera.LockMechRotation (false);
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
		}else
			mechCamera.LockMechRotation (false); //avoid stop too early

		move += transform.right * xSpeed * ((Mathf.Abs (direction) > marginOfError) ? 1 : 0) * Time.fixedDeltaTime;
		move += transform.forward * zSpeed * ((Mathf.Abs (speed) > marginOfError) ? 1 : 0) *  Time.fixedDeltaTime;
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

	public void SetSlashMoving(float speed){//called by animation
		forcemove_speed = speed;
		forcemove_dir = cam.transform.forward;
	}
	public void SetCanVerticalBoost(bool canVBoost) {
		canVerticalBoost = canVBoost;
	}

	public bool CanVerticalBoost() {
		return canVerticalBoost;
	}

	public void ApplyVerMinDownSpeed(){//called when exiting vertical boosting state
		ySpeed = minDownSpeed;
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
		Vector2 xzDir = new Vector2 (direction, speed).normalized;

		xSpeed = mechCombat.MaxHorizontalBoostSpeed ()*InAirSpeedCoeff * xzDir.x;
		zSpeed = mechCombat.MaxHorizontalBoostSpeed ()*InAirSpeedCoeff * xzDir.y;
	}

	public void Jump() {
		v_boost_start_yPos = 0;
		transform.position = new Vector3 (transform.position.x, transform.position.y + 0.2f, transform.position.z);
		ySpeed = mechCombat.JumpPower();
		UpdateSpeed();
	}

	public void Run() {
		//decelerating
		Vector2 xzDir = new Vector2 (direction, speed);
		if (xzDir.magnitude > 1)
			xzDir = Vector3.Normalize (xzDir);
		
		if (curboostingSpeed >= mechCombat.MoveSpeed() && !Animator.GetBool (boost_id)) {//not in transition to boost
			
			xSpeed = xzDir.x * curboostingSpeed;
			zSpeed = xzDir.y * curboostingSpeed;
		
			curboostingSpeed -= mechCombat.deceleration * Time.fixedDeltaTime;
		}else{		
			xSpeed = mechCombat.MoveSpeed()*xzDir.x;
			zSpeed = mechCombat.MoveSpeed()*xzDir.y;
		}			
	}

	public void JumpMoveInAir(){
		Vector2 xzDir = new Vector2 (direction, speed).normalized;
		if (curboostingSpeed >= mechCombat.MoveSpeed() && !Animator.GetBool (boost_id)) {//not in transition to boost

			xSpeed = xzDir.x * curboostingSpeed;
			zSpeed = xzDir.y * curboostingSpeed;

			curboostingSpeed -= mechCombat.deceleration/4 * Time.fixedDeltaTime;
		}else{		
			xSpeed = mechCombat.MoveSpeed()*xzDir.x;
			zSpeed = mechCombat.MoveSpeed()*xzDir.y;
		}
	}
	public void Boost(bool b) {
		Vector2 xzDir = new Vector2 (direction, speed).normalized;

		if(b != isBoostFlameOn){
			photonView.RPC ("BoostFlame", PhotonTargets.All, b);
			isBoostFlameOn = b;

			if (!b) {//shut the boost first call
				boostDust.Stop ();
			}else{//toggle on the boost first call
				if (grounded) {
					curboostingSpeed = mechCombat.MinHorizontalBoostSpeed ();
					curboostingVelocity_x = curboostingSpeed * xzDir.x;
					curboostingVelocity_z = curboostingSpeed * xzDir.y;

					boostDust.transform.localRotation = Quaternion.Euler (-90, Vector3.SignedAngle (Vector3.up, new Vector3 (-direction, speed, 0), Vector3.forward),0);
					boostDust.Play ();
				}
			}
		}
		if (b) {
			if(grounded){
				if (curboostingSpeed <= mechCombat.MaxHorizontalBoostSpeed ())
				curboostingSpeed += mechCombat.acceleration * Time.fixedDeltaTime;
				//ideal speed
				curboostingVelocity_x += Mathf.Sign (curboostingSpeed * xzDir.x - curboostingVelocity_x) * mechCombat.acceleration /2 ;
				curboostingVelocity_z += Mathf.Sign(curboostingSpeed * xzDir.y - curboostingVelocity_z) * mechCombat.acceleration /2 ;

				xSpeed = curboostingVelocity_x;
				zSpeed = curboostingVelocity_z;

				boostDust.transform.localRotation = Quaternion.Euler (-90,Vector3.SignedAngle (Vector3.up, new Vector3 (-direction, speed, 0), Vector3.forward),0);
				
			}else{//boost in air
				xSpeed = mechCombat.MaxHorizontalBoostSpeed ()*InAirSpeedCoeff * xzDir.x;
				zSpeed = mechCombat.MaxHorizontalBoostSpeed ()*InAirSpeedCoeff * xzDir.y;
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

		//Vector3 desiredPosition = (cam.transform.position - (transform.position + new Vector3 (0, 5, 0))).normalized * 15 + transform.position + new Vector3 (0, 5, 0);//15 : radius
		//cam.transform.position = Vector3.MoveTowards (cam.transform.position, desiredPosition, Time.deltaTime * 5f);
		mechCamera.orbitRadius = Mathf.Lerp (mechCamera.orbitRadius, 19, 0.06f);
		mechCamera.angleOffset = Mathf.Lerp (mechCamera.angleOffset, 33, 0.06f);
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
				//Vector3 desiredPosition = (cam.transform.position - (transform.position + new Vector3 (0, 5, 0))).normalized * 22 + transform.position + new Vector3 (0, 5, 0);
				//cam.transform.position = Vector3.MoveTowards (cam.transform.position, desiredPosition, Time.deltaTime * 6f);
				mechCamera.orbitRadius = Mathf.Lerp (mechCamera.orbitRadius, 26, 0.05f);
				mechCamera.angleOffset = Mathf.Lerp (mechCamera.angleOffset, 36, 0.06f);
			} else if (speed < 0) {
				if(direction > 0 || direction < 0){
					//Vector3 desiredPosition = (cam.transform.position - (transform.position + new Vector3 (0, 5, 0))).normalized * 14 + transform.position + new Vector3 (0, 5, 0);
					//cam.transform.position = Vector3.MoveTowards (cam.transform.position, desiredPosition, Time.deltaTime * 5f);
					mechCamera.orbitRadius = Mathf.Lerp (mechCamera.orbitRadius, 16, 0.05f);
					mechCamera.angleOffset = Mathf.Lerp (mechCamera.angleOffset, 30, 0.06f);
				} else {
					//Vector3 desiredPosition = (cam.transform.position - (transform.position + new Vector3 (0, 5, 0))).normalized * 12 + transform.position + new Vector3 (0, 5, 0);
					//cam.transform.position = Vector3.MoveTowards (cam.transform.position, desiredPosition, Time.deltaTime * 5f);
					mechCamera.orbitRadius = Mathf.Lerp (mechCamera.orbitRadius, 14, 0.05f);
					mechCamera.angleOffset = Mathf.Lerp (mechCamera.angleOffset, 30, 0.06f);
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
		move = Vector3.zero;
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		if (v <= marginOfError && v >= -marginOfError){
			v = 0;
		}
		if (h <= marginOfError && h >= -marginOfError){
			h = 0;
		}

		/*if (v > marginOfError || v < -marginOfError) {
			move += new Vector3(0, 0, v);
		}

		if (h > marginOfError || h < -marginOfError) {
			move += new Vector3(h, 0, 0);
		}

		move = transform.TransformDirection(move);
		if (move.magnitude > 1) {
			move = Vector3.Normalize(move);
		}*/
			
		speed = v;
		direction = h;

	}

	public bool CheckIsGrounded(){
		return Physics.CheckSphere (transform.position + new Vector3 (0, 1.8f, 0), 2.0f, Terrain);
	}
}
