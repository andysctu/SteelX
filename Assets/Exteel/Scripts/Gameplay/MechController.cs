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
    [SerializeField] private Transform camTransform;
    [SerializeField] private CharacterController CharacterController;
    [SerializeField] private LayerMask Terrain;
    [SerializeField] private SkillController SkillController;

    private GameManager gm;
    private BoosterController BoosterController;

    private float runDir_coeff = 3, runDecel_rate = 0.5f;
    private float curboostingSpeed;//global space
    private Vector2 xzDir;
    private Vector2 run_xzDir;
    private float marginOfError = 0.1f;
    private Vector3 move = Vector3.zero;

    private bool isBoostFlameOn = false;
    private bool isSlowDown = false;
    private const float slowDownDuration = 0.3f;
    private Coroutine coroutine = null;

    private float instantMoveSpeed, curInstantMoveSpeed;
    private Vector3 instantMoveDir;
    private bool canVerticalBoost = false, getJumpWhenSlash;
    private float v_boost_start_yPos;
    private float v_boost_upperbound;
    private float boostStartTime = 0;//this is for jump delay
    private float slashTeleportMinDistance = 3f;

    // Animation
    private float speed;
    private float direction;

    private float xSpeed = 0f, ySpeed = 0f, zSpeed = 0f;
    public float Gravity = 4.5f;
    public float maxDownSpeed = -140f;
    public float InAirSpeedCoeff = 0.7f;
    public float lerpCam_coeff = 5;
    public bool grounded = true; //changes with animator bool "grounded"
    public bool on_BCNShoot = false;
    public float cam_lerpSpeed = 10, LocalxOffset = -4, cam_orbitradius = 19, cam_angleoffset = 33;

    private void Awake() {
        RegisterOnSkill();
    }

    private void Start() {
        initComponents();
        initControllerVar();
        FindBoosterController();
        initCam(cam_orbitradius, cam_angleoffset);
    }

    private void RegisterOnSkill() {
        SkillController.OnSkill += InterruptCurrentMovement;
    }

    public void initControllerVar() {
        grounded = true;
        canVerticalBoost = false;
        isSlowDown = false;
        curboostingSpeed = mechCombat.MoveSpeed();
        Animator.SetBool("Grounded", true);
        run_xzDir = Vector2.zero;
    }

    public void FindBoosterController() {//TODO : put this in respawn update delegate
        BoosterController = transform.Find("CurrentMech").GetComponentInChildren<BoosterController>();
    }

    private void initComponents() {
        Transform currentMech = transform.Find("CurrentMech");
        CharacterController = GetComponent<CharacterController>();
        SkillController = GetComponent<SkillController>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void initCam(float radius, float offset) {
        mechCamera.LockMechRotation(false);
        mechCamera.SetLerpFakePosSpeed(cam_lerpSpeed);
        mechCamera.orbitRadius = radius;
        mechCamera.angleOffset = offset;
    }

    private void Update() {
        GetXZDirection();
    }

    private void FixedUpdate() {
        if (CharacterController == null || !CharacterController.enabled) {
            return;
        }

        if (!grounded) {
            ySpeed -= (ySpeed < maxDownSpeed || Animator.GetBool(AnimatorVars.boost_id)) ? 0 : Gravity * Time.fixedDeltaTime * 50;
        } else {
            ySpeed = (-CharacterController.stepOffset / Time.fixedDeltaTime) * 0.2f;
        }
        if (Animator == null) {
            return;
        }

        if (Animator.GetBool(AnimatorVars.boost_id)) {
            DynamicCam();
            mechCombat.DecrementFuel();
        } else {
            ResetCam();
            mechCombat.IncrementFuel();
        }

        UpdateSpeed();
    }

    public void UpdateSpeed() {
        // instant move
        if (mechCombat.isLMeleePlaying == 1 || mechCombat.isRMeleePlaying == 1 || on_BCNShoot) {
            InstantMove();
            return;
        }

        move = Vector3.right * xSpeed * Time.fixedDeltaTime;
        move += Vector3.forward * zSpeed * Time.fixedDeltaTime;
        move.y += ySpeed * Time.fixedDeltaTime;

        if (isSlowDown) {
            move.x = move.x * 0.2f;
            move.z = move.z * 0.2f;
        }

        if (!gm.GameIsBegin) {//player can't move but can rotate
            move.x = 0;
            move.z = 0;
        }

        CharacterController.Move(move);
    }

    public void SetMoving(float speed) {//called by animation
        instantMoveSpeed = speed;
        instantMoveDir = cam.transform.forward;
        curInstantMoveSpeed = instantMoveSpeed;
    }

    private void InstantMove() {
        if (grounded) {
            instantMoveDir = new Vector3(instantMoveDir.x, 0, instantMoveDir.z);    // make sure not slashing to the sky
        }
        if (curInstantMoveSpeed == instantMoveSpeed)
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
            }
        }
    }

    /*
    //TODO : test ( this works well!)
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
            ySpeed = mechCombat.MaxVerticalBoostSpeed();
        } else {
            if (transform.position.y >= v_boost_start_yPos + mechCombat.MaxVerticalBoostSpeed() * 1.25f) {
                ySpeed = 0;
            } else {
                ySpeed = mechCombat.MaxVerticalBoostSpeed();
            }
        }
        float inAirSpeed = mechCombat.MaxHorizontalBoostSpeed() * InAirSpeedCoeff;
        xSpeed = inAirSpeed * xzDir.x * transform.right.x + inAirSpeed * xzDir.y * transform.forward.x;
        zSpeed = inAirSpeed * xzDir.x * transform.right.z + inAirSpeed * xzDir.y * transform.forward.z;
    }

    public void Jump() {
        v_boost_start_yPos = 0;
        ySpeed = mechCombat.JumpPower();
    }

    public void Run() {
        run_xzDir.x = Mathf.Lerp(run_xzDir.x, xzDir.x, Time.deltaTime * runDir_coeff);//smooth slow down (boosting -> Idle&Walk
        run_xzDir.y = Mathf.Lerp(run_xzDir.y, xzDir.y, Time.deltaTime * runDir_coeff);//not achieving by gravity because we don't want walk smooth slow down
                                                                                      //decelerating
        if (curboostingSpeed >= mechCombat.MoveSpeed() && !Animator.GetBool(AnimatorVars.boost_id)) {//not in transition to boost
            xSpeed = (run_xzDir.x * curboostingSpeed * transform.right).x + (run_xzDir.y * curboostingSpeed * transform.forward).x;
            zSpeed = (run_xzDir.x * curboostingSpeed * transform.right).z + (run_xzDir.y * curboostingSpeed * transform.forward).z;
            curboostingSpeed -= mechCombat.deceleration * Time.deltaTime * runDecel_rate;
        } else {
            xSpeed = mechCombat.MoveSpeed() * xzDir.x * transform.right.x + mechCombat.MoveSpeed() * ((xzDir.y < 0) ? xzDir.y / 2 : xzDir.y) * transform.forward.x;
            zSpeed = mechCombat.MoveSpeed() * xzDir.x * transform.right.z + mechCombat.MoveSpeed() * ((xzDir.y < 0) ? xzDir.y / 2 : xzDir.y) * transform.forward.z;
        }
    }

    public void ResetCurBoostingSpeed() {
        curboostingSpeed = mechCombat.MoveSpeed();
    }

    public void ResetCurSpeed() {
        xSpeed = 0;
        zSpeed = 0;
    }

    public void JumpMoveInAir() {
        if (curboostingSpeed >= mechCombat.MoveSpeed() && !Animator.GetBool(AnimatorVars.boost_id)) {//not in transition to boost
            curboostingSpeed -= mechCombat.deceleration * Time.deltaTime * runDecel_rate * 2;

            xSpeed = (xzDir.x * curboostingSpeed * transform.right).x + (xzDir.y * curboostingSpeed * transform.forward).x;
            zSpeed = (xzDir.x * curboostingSpeed * transform.right).z + (xzDir.y * curboostingSpeed * transform.forward).z;
        } else {
            float xRawDir = Input.GetAxisRaw("Horizontal");

            xSpeed = mechCombat.MoveSpeed() * xRawDir * transform.right.x + mechCombat.MoveSpeed() * xzDir.y * transform.forward.x;
            zSpeed = mechCombat.MoveSpeed() * xRawDir * transform.right.z + mechCombat.MoveSpeed() * xzDir.y * transform.forward.z;
        }
    }

    public void Boost(bool b) {
        if (b != isBoostFlameOn) {
            photonView.RPC("BoostFlame", PhotonTargets.All, b, grounded);
            isBoostFlameOn = b;

            if (b) {//toggle on the boost first call
                if (grounded) {
                    curboostingSpeed = mechCombat.MinHorizontalBoostSpeed();
                    xSpeed = (curboostingSpeed * xzDir.x * transform.right).x + (curboostingSpeed * xzDir.y * transform.forward).x;
                    zSpeed = (curboostingSpeed * xzDir.y * transform.forward).z + (curboostingSpeed * xzDir.x * transform.right).z;
                }
            }
        }
        if (b) {
            if (grounded) {
                if (Mathf.Abs(speed) < 0.1f && Mathf.Abs(direction) < 0.1f) {//boosting but not moving => lower current boosting speed
                    if (curboostingSpeed >= mechCombat.MinHorizontalBoostSpeed())
                        curboostingSpeed -= mechCombat.acceleration * Time.deltaTime * 10;
                } else {
                    if (curboostingSpeed <= mechCombat.MaxHorizontalBoostSpeed())
                        curboostingSpeed += mechCombat.acceleration * Time.deltaTime * 5;
                }

                //ideal speed
                float idealSpeed_x = (curboostingSpeed * xzDir.x * transform.right).x + (curboostingSpeed * xzDir.y * transform.forward).x,
                idealSpeed_z = (curboostingSpeed * xzDir.y * transform.forward).z + (curboostingSpeed * xzDir.x * transform.right).z;

                Vector2 dir = new Vector2(idealSpeed_x, idealSpeed_z).normalized;
                float acc_x = Mathf.Abs(mechCombat.acceleration * dir.x), acc_z = Mathf.Abs(mechCombat.acceleration * dir.y);

                Vector2 decreaseDir = new Vector2(Mathf.Abs(xSpeed), Mathf.Abs(zSpeed)).normalized;

                if (Mathf.Abs(dir.x) < 0.1f && xSpeed != 0) {
                    xSpeed += Mathf.Sign(0 - xSpeed) * mechCombat.acceleration * decreaseDir.x * Time.deltaTime * 40;
                } else {
                    xSpeed += Mathf.Sign(idealSpeed_x - xSpeed) * acc_x * Time.deltaTime * 100;
                }
                if (Mathf.Abs(dir.y) < 0.1f && zSpeed != 0) {
                    zSpeed += Mathf.Sign(0 - zSpeed) * mechCombat.acceleration * decreaseDir.y * Time.deltaTime * 40;
                } else {
                    zSpeed += Mathf.Sign(idealSpeed_z - zSpeed) * acc_z * Time.deltaTime * 100;
                }

                run_xzDir.x = xzDir.x;
                run_xzDir.y = xzDir.y;
            } else {//boost in air
                float inAirSpeed = mechCombat.MaxHorizontalBoostSpeed() * InAirSpeedCoeff;
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
            if (coroutine != null)
                StopCoroutine(coroutine);

            coroutine = StartCoroutine("SlowDownCoroutine");
        } else {
            coroutine = StartCoroutine("SlowDownCoroutine");
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
        coroutine = null;
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

        speed = v;
        direction = h;

        xzDir = new Vector2(direction, speed).normalized;
    }

    public bool CheckIsGrounded() {
        return Physics.CheckSphere(transform.position + new Vector3(0, 1.7f, 0), 2.0f, Terrain);
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
            ResetCurBoostingSpeed();
            ResetCurSpeed();
        }
    }
}