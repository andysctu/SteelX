using System.Collections;
using UnityEngine;

// MechController controls the position of the player
public class MechController : Photon.MonoBehaviour {
    [SerializeField] private Camera cam;
    [SerializeField] private AnimatorVars AnimatorVars;
    [SerializeField] private Animator Animator;
    [SerializeField] private MechCombat mechCombat;
    [SerializeField] private MechCamera mechCamera;
    [SerializeField] private Sounds Sounds;
    [SerializeField] private EffectController EffectController;
    [SerializeField] private Transform camTransform, boosterBone;
    [SerializeField] private CharacterController CharacterController;
    [SerializeField] private LayerMask Terrain;
    [SerializeField] private SkillController SkillController;
    [SerializeField] private MovementVariables movementVariables = new MovementVariables();
    private BuildMech bm;
    private GameManager gm;
    private BoosterController BoosterController;

    private float runDir_coeff = 3, runDecel_rate = 0.5f;
    private float curboostingSpeed;//global space
    private Vector2 xzDir, run_xzDir;
    private float marginOfError = 0.1f;
    private Vector3 move = Vector3.zero;

    private bool isBoostFlameOn = false;
    private bool isSlowDown = false;
    private const float slowDownDuration = 0.3f, slowDownCoeff = 0.2f;
    private Coroutine slowDownCoroutine = null;

    private float instantMoveSpeed, curInstantMoveSpeed;
    private Vector3 instantMoveDir;
    private bool canVerticalBoost = false, getJumpWhenSlash;
    private float v_boost_start_yPos;
    private float slashTeleportMinDistance = 3f;

    // Animation
    public float speed { get;private set;}
    public float direction { get;private set;}

    private float xSpeed = 0f, ySpeed = 0f, zSpeed = 0f;
    public float Gravity = 4;
    public float maxDownSpeed = -140f;
    public float InAirSpeedCoeff = 0.7f;
    public float lerpCam_coeff = 5;
    public bool grounded = true, onSkill = false; //changes with animator bool "grounded"
    public float LocalxOffset = -4, cam_orbitradius = 19, cam_angleoffset = 33;

    public bool onSkillMoving = false, onInstantMoving = false;

    private void Awake() {
        RegisterOnMechBuilt();
        RegisterOnWeaponSwithed();
        RegisterOnSkill();
        RegisterOnMechEnabled();
    }

    private void Start() {
        initComponents();
        initCam(cam_orbitradius, cam_angleoffset);

        enabled = photonView.isMine;
    }

    private void RegisterOnMechBuilt() {
        if ((bm = GetComponent<BuildMech>()) != null) {
            bm.OnMechBuilt += FindBoosterController;
            bm.OnMechBuilt += initControllerVar;
        }
    }

    private void RegisterOnWeaponSwithed() {
        mechCombat.OnWeaponSwitched += UpdateWeightRelatedVars;
    }

    private void RegisterOnMechEnabled() {
        mechCombat.OnMechEnabled += OnMechEnabled;
    }

    private void RegisterOnSkill() {
        if (SkillController != null) {
            SkillController.OnSkill += OnSkill;
            SkillController.OnSkill += InterruptCurrentMovement;
        }
    }

    private void OnSkill(bool b) {
        onSkill = b;

        if (!b) {
            CallLockMechRot(false); //on skill when slashing & smashing
        }
    }

    private void OnMechEnabled(bool b) {
        if(!photonView.isMine)
            return;

        enabled = b;
    }

    public void initControllerVar() {
        //grounded = true;
        onInstantMoving = false;
        canVerticalBoost = false;
        isSlowDown = false;
        mechCamera.LockMechRotation(false);
        curboostingSpeed = movementVariables.moveSpeed;
        Animator.SetBool("Boost", false);
        Animator.SetBool("Jump", false);
        run_xzDir = Vector2.zero;
    }

    private void FindBoosterController() {
        BoosterController = boosterBone.GetComponentInChildren<BoosterController>();
    }

    public void UpdateWeightRelatedVars() {
        int partWeight = bm.MechProperty.Weight, weaponOffset = mechCombat.GetCurrentWeaponOffset();
        int weaponWeight = ((bm.WeaponDatas[weaponOffset] == null) ? 0 : bm.WeaponDatas[weaponOffset].weight) + ((bm.WeaponDatas[weaponOffset + 1] == null) ? 0 : bm.WeaponDatas[weaponOffset + 1].weight);

        movementVariables.moveSpeed = bm.MechProperty.GetMoveSpeed(partWeight, weaponWeight) * 0.08f;
        movementVariables.maxHorizontalBoostSpeed = bm.MechProperty.GetDashSpeed(partWeight + weaponWeight) * 0.1f;
        movementVariables.minBoostSpeed = (movementVariables.moveSpeed + movementVariables.maxHorizontalBoostSpeed) / 2f;

        movementVariables.acceleration = bm.MechProperty.GetDashAcceleration(partWeight + weaponWeight);
        movementVariables.deceleration = bm.MechProperty.GetDashDecelleration(partWeight + weaponWeight);

        //jump boost speed
        movementVariables.verticalBoostSpeed = bm.MechProperty.VerticalBoostSpeed;
        //jump speed
    }

    private void initComponents() {
        //Transform currentMech = transform.Find("CurrentMech");
        CharacterController = GetComponent<CharacterController>();
        SkillController = GetComponent<SkillController>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void initCam(float radius, float offset) {
        mechCamera.orbitRadius = radius;
        mechCamera.angleOffset = offset;
    }

    private void Update() {
        GetXZDirection();
    }

    private void FixedUpdate() {
        if (!grounded) {
            ySpeed -= (ySpeed < maxDownSpeed || Animator.GetBool(AnimatorVars.boost_id)) ? 0 : Gravity;
        } else {
            ySpeed = -CharacterController.stepOffset;
        }

        if (Animator.GetBool(AnimatorVars.boost_id)) {
            DynamicCam();
            mechCombat.DecrementEN();
        } else {
            ResetCam();
            mechCombat.IncrementEN();
        }

        UpdateSpeed();
    }

    public void UpdateSpeed() {
        if (onInstantMoving) {
            InstantMove(); 
            return;
        }

        move = Vector3.right * xSpeed * Time.fixedDeltaTime;
        move += Vector3.forward * zSpeed * Time.fixedDeltaTime;
        move.y += ySpeed * Time.fixedDeltaTime;

        if (isSlowDown) {
            move.x = move.x * slowDownCoeff;
            move.z = move.z * slowDownCoeff;
        }

        if (!GameManager.gameIsBegin) {//player can't move but can rotate
            move.x = 0;
            move.z = 0;
        }

        CharacterController.Move(move);
    }

    public void SetMoving(float speed) {//called by animation
        instantMoveSpeed = speed;
        instantMoveDir = cam.transform.forward;
        if (grounded) instantMoveDir = new Vector3(instantMoveDir.x, 0, instantMoveDir.z);// make sure not slashing to the sky
        curInstantMoveSpeed = instantMoveSpeed;
    }

    public void SkillSetMoving(Vector3 v) {
        onInstantMoving = true;
        instantMoveSpeed = v.magnitude;
        instantMoveDir = v;
        curInstantMoveSpeed = instantMoveSpeed;
    }

    private void InstantMove() {
        if (curInstantMoveSpeed == instantMoveSpeed)//the first time inside this function
            getJumpWhenSlash = false;

        instantMoveSpeed /= 1.6f;//1.6 : decrease coeff.
        if (Mathf.Abs(instantMoveSpeed) > 0.01f) {
            ySpeed = 0;
        }
        CharacterController.Move(instantMoveDir * instantMoveSpeed);
        move.x = 0;
        move.z = 0;

        if (Animator.GetBool(AnimatorVars.jump_id))
            getJumpWhenSlash = true;

        //cast a ray downward to check if not jumping but not grounded => if so , directly teleport to ground
        RaycastHit hit;
        if (!getJumpWhenSlash && Physics.Raycast(transform.position, -Vector3.up, out hit, Terrain)) {
            if (Vector3.Distance(hit.point, transform.position) >= slashTeleportMinDistance && !Physics.CheckSphere(hit.point + new Vector3(0, 2.1f, 0), CharacterController.radius, Terrain)) {
                transform.position = hit.point;

                //TODO : map edge case
            }
        }
    }

    /*
    //TODO : test
    public Transform pelvis; //put spine here
    public int degree = 30;
    private void LateUpdate() {
        if(pelvis != null) {
            float ind_dir = Input.GetAxis("Horizontal");
            pelvis.rotation = Quaternion.Euler(pelvis.rotation.eulerAngles.x, pelvis.rotation.eulerAngles.y + ind_dir * degree, pelvis.rotation.eulerAngles.z);
        }
    }*/

    public void SetCanVerticalBoost(bool canVBoost) {
        canVerticalBoost = canVBoost;
    }

    public bool CanVerticalBoost() {
        return canVerticalBoost;
    }

    public void VerticalBoost() {
        if (v_boost_start_yPos == 0) {
            v_boost_start_yPos = transform.position.y;
            ySpeed = movementVariables.verticalBoostSpeed;
        } else {
            if (transform.position.y >= v_boost_start_yPos + movementVariables.verticalBoostSpeed * 1.25f) {//max height
                ySpeed = 0;
            } else {
                ySpeed = movementVariables.verticalBoostSpeed;
            }
        }
        float inAirSpeed = movementVariables.maxHorizontalBoostSpeed * InAirSpeedCoeff;
        xSpeed = inAirSpeed * xzDir.x * transform.right.x + inAirSpeed * xzDir.y * transform.forward.x;
        zSpeed = inAirSpeed * xzDir.x * transform.right.z + inAirSpeed * xzDir.y * transform.forward.z;
    }

    public void Jump() {
        v_boost_start_yPos = 0;
        ySpeed = movementVariables.jumpPower;
    }

    public void Run() {
        run_xzDir.x = Mathf.Lerp(run_xzDir.x, xzDir.x, Time.deltaTime * runDir_coeff);//smooth slow down (boosting -> Idle&Walk
        run_xzDir.y = Mathf.Lerp(run_xzDir.y, xzDir.y, Time.deltaTime * runDir_coeff);//not achieving by gravity because we don't want walk smooth slow down
                                                                                      //decelerating
        if (curboostingSpeed >= movementVariables.moveSpeed && !Animator.GetBool(AnimatorVars.boost_id)) {//not in transition to boost
            xSpeed = (run_xzDir.x * curboostingSpeed * transform.right).x + (run_xzDir.y * curboostingSpeed * transform.forward).x;
            zSpeed = (run_xzDir.x * curboostingSpeed * transform.right).z + (run_xzDir.y * curboostingSpeed * transform.forward).z;

            curboostingSpeed -= movementVariables.deceleration * Time.deltaTime * 5;
        } else {
            xSpeed = movementVariables.moveSpeed * xzDir.x * transform.right.x + movementVariables.moveSpeed * ((xzDir.y < 0) ? xzDir.y / 2 : xzDir.y) * transform.forward.x;
            zSpeed = movementVariables.moveSpeed * xzDir.x * transform.right.z + movementVariables.moveSpeed * ((xzDir.y < 0) ? xzDir.y / 2 : xzDir.y) * transform.forward.z;
        }
    }

    public void ResetCurBoostingSpeed() {
        curboostingSpeed = movementVariables.moveSpeed;
    }

    public void ResetCurSpeed() {
        xSpeed = 0;
        zSpeed = 0;
    }

    public void JumpMoveInAir() {
        if (curboostingSpeed >= movementVariables.moveSpeed && !Animator.GetBool(AnimatorVars.boost_id)) {//not in transition to boost
            curboostingSpeed -= movementVariables.deceleration * Time.deltaTime * 10;

            xSpeed = (xzDir.x * curboostingSpeed * transform.right).x + (xzDir.y * curboostingSpeed * transform.forward).x;
            zSpeed = (xzDir.x * curboostingSpeed * transform.right).z + (xzDir.y * curboostingSpeed * transform.forward).z;
        } else {
            float xRawDir = Input.GetAxisRaw("Horizontal");

            xSpeed = movementVariables.moveSpeed * xRawDir * transform.right.x + movementVariables.moveSpeed * xzDir.y * transform.forward.x;
            zSpeed = movementVariables.moveSpeed * xRawDir * transform.right.z + movementVariables.moveSpeed * xzDir.y * transform.forward.z;
        }
    }

    public void Boost(bool b) {
        if (b != isBoostFlameOn) {
            photonView.RPC("BoostFlame", PhotonTargets.All, b, grounded);
            isBoostFlameOn = b;

            if (b) {//toggle on the boost first call
                if (grounded) {
                    curboostingSpeed = movementVariables.minBoostSpeed;
                    xSpeed = (curboostingSpeed * xzDir.x * transform.right).x + (curboostingSpeed * xzDir.y * transform.forward).x;
                    zSpeed = (curboostingSpeed * xzDir.y * transform.forward).z + (curboostingSpeed * xzDir.x * transform.right).z;
                }
            }
        }
        if (b) {
            if (grounded) {
                if (Mathf.Abs(speed) < 0.1f && Mathf.Abs(direction) < 0.1f) {//boosting but not moving => decrease current boosting speed
                    if (curboostingSpeed >= movementVariables.minBoostSpeed)
                        curboostingSpeed -= movementVariables.acceleration * Time.deltaTime * 10;
                } else {//increase magnitude if < max
                    if (curboostingSpeed <= movementVariables.maxHorizontalBoostSpeed)
                        curboostingSpeed += movementVariables.acceleration * Time.deltaTime * 3;
                }
                //ideal speed
                float idealSpeed_x = (curboostingSpeed * xzDir.x * transform.right).x + (curboostingSpeed * xzDir.y * transform.forward).x,
                idealSpeed_z = (curboostingSpeed * xzDir.y * transform.forward).z + (curboostingSpeed * xzDir.x * transform.right).z;

                Vector2 dir = new Vector2(idealSpeed_x, idealSpeed_z).normalized;
                float acc_x = Mathf.Abs(movementVariables.acceleration * dir.x), acc_z = Mathf.Abs(movementVariables.acceleration * dir.y);

                Vector2 decreaseDir = new Vector2(Mathf.Abs(xSpeed), Mathf.Abs(zSpeed)).normalized;

                if (Mathf.Abs(dir.x) < 0.1f && xSpeed != 0) {
                    xSpeed += Mathf.Sign(0 - xSpeed) * movementVariables.acceleration * decreaseDir.x * Time.deltaTime * 10;
                } else {
                    if (Mathf.Abs(idealSpeed_x - xSpeed) < movementVariables.acceleration) acc_x = 0;

                    xSpeed += Mathf.Sign(idealSpeed_x - xSpeed) * acc_x * Time.deltaTime * 35;
                }

                if (Mathf.Abs(dir.y) < 0.1f && zSpeed != 0) {
                    zSpeed += Mathf.Sign(0 - zSpeed) * movementVariables.acceleration * decreaseDir.y * Time.deltaTime * 10;
                } else {
                    if (Mathf.Abs(idealSpeed_z - zSpeed) < movementVariables.acceleration) acc_z = 0;

                    zSpeed += Mathf.Sign(idealSpeed_z - zSpeed) * acc_z * Time.deltaTime * 35;
                }
                run_xzDir.x = xzDir.x;
                run_xzDir.y = xzDir.y;
            } else {//boost in air
                float inAirSpeed = movementVariables.maxHorizontalBoostSpeed * InAirSpeedCoeff;
                xSpeed = inAirSpeed * xzDir.x * transform.right.x + inAirSpeed * xzDir.y * transform.forward.x;
                zSpeed = inAirSpeed * xzDir.x * transform.right.z + inAirSpeed * xzDir.y * transform.forward.z;
            }
        }
    }

    public void BCNPose() {
        xSpeed = 0;
        zSpeed = 0;
    }

    public void SlowDown() {
        if (isSlowDown) {
            if (slowDownCoroutine != null)
                StopCoroutine(slowDownCoroutine);

            slowDownCoroutine = StartCoroutine("SlowDownCoroutine");
        } else {
            slowDownCoroutine = StartCoroutine("SlowDownCoroutine");
            isSlowDown = true;
        }
    }

    private IEnumerator SlowDownCoroutine() {
        SetCanVerticalBoost(false);
        Animator.SetBool("Boost", false);
        Boost(false);
        if (!CheckIsGrounded())//in air => small bump effect
            ySpeed = 0;

        yield return new WaitForSeconds(slowDownDuration);
        isSlowDown = false;
        slowDownCoroutine = null;
    }

    public void ResetCam() {
        Vector3 curPos = camTransform.localPosition;
        Vector3 newPos = new Vector3(0, curPos.y, curPos.z);
        camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, 0.1f);
    }

    public void DynamicCam() {
        Vector3 curPos = camTransform.localPosition;
        Vector3 newPos = camTransform.localPosition;

        if (direction > 0) {
            newPos = new Vector3(LocalxOffset, curPos.y, curPos.z);
        } else if (direction < 0) {
            newPos = new Vector3(-LocalxOffset, curPos.y, curPos.z);
        } else {
            newPos = new Vector3(0, curPos.y, curPos.z);
        }

        camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, newPos, Time.fixedDeltaTime * lerpCam_coeff);
    }

    [PunRPC]
    private void BoostFlame(bool boost, bool boostdust) {
        if (boost) {
            BoosterController.StartBoost();
            if (boostdust)
                EffectController.BoostingDustEffect(true);
        } else {
            BoosterController.StopBoost();
            EffectController.BoostingDustEffect(false);
        }
    }

    private void GetXZDirection() {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (v <= marginOfError && v >= -marginOfError) {
            v = 0;
        }
        if (h <= marginOfError && h >= -marginOfError) {
            h = 0;
        }

        if (gm.BlockInput) {
            speed = 0;
            direction = 0;
        } else {
            speed = v;
            direction = h;
        }        

        xzDir = new Vector2(direction, speed).normalized;
    }

    public bool CheckIsGrounded() {
        return Physics.CheckSphere(transform.position + new Vector3(0, 1.9f, 0), 2f, Terrain);
    }

    public void OnJumpAction() {

    }

    public void OnLandingAction() {

    }

    public void CallLockMechRot(bool b) {
        mechCamera.LockMechRotation(b);
    }

    private void InterruptCurrentMovement(bool b) {
        if (b) {//when entering
            ResetCurBoostingSpeed();
            ResetCurSpeed();
            Boost(false);
        } else {
            onInstantMoving = false;
            ResetCurBoostingSpeed();
            ResetCurSpeed();
        }
    }

    [System.Serializable]
    private struct MovementVariables {
        public float verticalBoostSpeed;
        public float maxHorizontalBoostSpeed;
        public float jumpPower;
        public float moveSpeed;
        public float minBoostSpeed;

        public float acceleration;
        public float deceleration;
    }
}