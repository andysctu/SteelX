using System.Collections;
using UnityEngine;

// MechController controls the position of the player
public class MechController : Photon.MonoBehaviour {
    [SerializeField] private PhotonView _rootPv;
    [SerializeField] public MovementVariables movementVariables = new MovementVariables();
    private BuildMech _buildMech;
    private MechCombat _mechCombat;
    private MechCamera _mechCam;
    private CharacterController _characterController;
    private SkillController _skillController;
    private Transform _camTransform;
    private AnimatorVars _animatorVars;
    private Animator _mainAnimator;
    private LayerMask _terrainMask;

    //Booster 
    [SerializeField] private Transform boosterBone;
    private BoosterController _boosterController;
    private bool isBoostFlameOn = false;

    //To remake
    [SerializeField] private EffectController EffectController;

    //Jump effect
    [SerializeField] private GameObject JumpEffectPrefab;
    private ParticleSystem _jumpEffect;

    //Sounds
    private Sounds _sounds;
    private AudioSource _audioSource;
    [SerializeField] private AudioClip LandingSound, JumpSound;
    private bool _isFootStepVarPositive = false;//used for playing foot sound


    //Slow down TODO : remake this part
    private bool isSlowDown = false;
    private const float slowDownDuration = 0.3f, slowDownCoeff = 0.2f;
    private Coroutine slowDownCoroutine = null;

    private float instantMoveSpeed, curInstantMoveSpeed;
    private Vector3 instantMoveDir;

    // Animation
    public float Speed;
    public float Direction;

    private float _curBoostingSpeed;//global space
    private Vector3 _move = Vector3.zero;

    private bool _canVerticalBoost = false, _getJumpWhenSlash;
    private float _verticalBoostStartYPos;
    private float _slashTeleportMinDistance = 3f;


    //Movement states
    private const float MarginOfError = 0.1f;
    private Vector3 _xzDirNormalized;

    public float XSpeed, YSpeed, ZSpeed;
    public float Gravity = 4;
    public float maxDownSpeed = -140f;
    public float InAirSpeedCoeff = 0.7f;
    public float lerpCam_coeff = 5;
    public bool Grounded, IsBoosting, IsAvailableVerBoost;
    public bool onSkill = false; //changes with animator bool "grounded";
    public float LocalxOffset = -4, cam_orbitradius = 19, cam_angleoffset = 33;

    private delegate void MovementState(HandleInputs.UserCmd userCmd);
    private MovementState CurMovementState;

    //Rotate legs
    private Transform _pelvis, _spine1;
    private float _rLPelvisDegree = 30, _rLSpineDegree = -45, _rLLerpSpeed = 10, _rLDir;

    public bool onSkillMoving = false, onInstantMoving = false, IsJumping = false, isVerticalBoosting = false;
    private bool _jumpReleased;

    //Input
    //public bool[] Buttons = new bool[2];

    private void Awake() {
        InitComponents();

        RegisterOnMechBuilt();
        RegisterOnWeaponSwitched();
        RegisterOnSkill();
        RegisterOnMechEnabled();
    }

    private void InitComponents() {
        Transform currentMech = transform.Find("CurrentMech");
        _animatorVars = currentMech.GetComponent<AnimatorVars>();
        _mainAnimator = currentMech.GetComponent<Animator>();
        _mechCombat = GetComponent<MechCombat>();
        _mechCam = GetComponentInChildren<MechCamera>();
        _camTransform = _mechCam.transform;
        _sounds = currentMech.GetComponent<Sounds>();

        _characterController = GetComponent<CharacterController>();
        _skillController = GetComponent<SkillController>();

        //Disable animation & pos sync TODO : move this elsewhere
        if (_rootPv.isMine) currentMech.GetComponent<PhotonView>().ObservedComponents.Clear();

        _pelvis = transform.Find("CurrentMech/Bip01/Bip01_Pelvis");
        _spine1 = transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1");

        _jumpEffect = Instantiate(JumpEffectPrefab, transform).GetComponent<ParticleSystem>();
        TransformExtension.SetLocalTransform(_jumpEffect.transform);

        GetLayerMask();
        AddAudioSource();

        CurMovementState = GroundedState;
    }

    private void GetLayerMask() {
        _terrainMask = LayerMask.GetMask("Terrain");
    }

    private void Start() {
        InitCam(cam_orbitradius, cam_angleoffset);
    }

    private void RegisterOnMechBuilt() {
        if ((_buildMech = GetComponent<BuildMech>()) != null) {
            _buildMech.OnMechBuilt += FindBoosterController;
            _buildMech.OnMechBuilt += InitControllerVar;
        }
    }

    private void RegisterOnWeaponSwitched() {
        _mechCombat.OnWeaponSwitched += UpdateWeightRelatedVars;
    }

    private void RegisterOnMechEnabled() {
        _mechCombat.OnMechEnabled += OnMechEnabled;
    }

    private void RegisterOnSkill() {
        if (_skillController != null) {
            _skillController.OnSkill += OnSkill;
            _skillController.OnSkill += InterruptCurrentMovement;
        }
    }

    private void OnSkill(bool b) {
        onSkill = b;

        if (!b) {
            CallLockMechRot(false); //on skill when slashing & smashing
        }
    }

    private void OnMechEnabled(bool b) {
        enabled = b;
    }

    private void InitControllerVar() {
        onInstantMoving = false;
        _canVerticalBoost = false;
        isSlowDown = false;
        _mechCam.LockMechRotation(false);
        _curBoostingSpeed = movementVariables.moveSpeed;
        _mainAnimator.SetBool("Boost", false);
        _mainAnimator.SetBool("Jump", false);
    }

    private void FindBoosterController() {
        _boosterController = boosterBone.GetComponentInChildren<BoosterController>();
    }

    public void UpdateWeightRelatedVars() {
        int partWeight = _buildMech.MechProperty.Weight, weaponOffset = _mechCombat.GetCurrentWeaponOffset();
        int weaponWeight = ((_buildMech.WeaponDatas[weaponOffset] == null) ? 0 : _buildMech.WeaponDatas[weaponOffset].weight) + ((_buildMech.WeaponDatas[weaponOffset + 1] == null) ? 0 : _buildMech.WeaponDatas[weaponOffset + 1].weight);

        movementVariables.moveSpeed = _buildMech.MechProperty.GetMoveSpeed(partWeight, weaponWeight) * 0.08f;
        movementVariables.maxHorizontalBoostSpeed = _buildMech.MechProperty.GetDashSpeed(partWeight + weaponWeight) * 0.1f;
        movementVariables.minBoostSpeed = (movementVariables.moveSpeed + movementVariables.maxHorizontalBoostSpeed) / 2f;

        movementVariables.acceleration = _buildMech.MechProperty.GetDashAcceleration(partWeight + weaponWeight);
        movementVariables.deceleration = _buildMech.MechProperty.GetDashDecelleration(partWeight + weaponWeight);

        //jump boost speed
        movementVariables.verticalBoostSpeed = _buildMech.MechProperty.VerticalBoostSpeed;
        //jump speed
    }

    private void AddAudioSource() {
        _audioSource = gameObject.AddComponent<AudioSource>();

        //Init AudioSource
        _audioSource.spatialBlend = 1;
        _audioSource.dopplerLevel = 0;
        _audioSource.volume = 0.8f;
        _audioSource.playOnAwake = false;
        _audioSource.minDistance = 20;
        _audioSource.maxDistance = 250;
    }

    private void InitCam(float radius, float offset) {
        if (!_rootPv.isMine){
            _mechCam.orbitRadius = radius;
            _mechCam.angleOffset = offset;
        }
    }

    private void Update() {
        //TODO : remake this (test use)
        if (_isFootStepVarPositive){
            if (_mainAnimator.GetFloat("FootStep") < 0) {
                _isFootStepVarPositive = false;
                _sounds.PlayWalk();
            }
        }else if (_mainAnimator.GetFloat("FootStep") > 0) {
            _isFootStepVarPositive = true;
            _sounds.PlayWalk();
        }
    }

    public void UpdatePosition(HandleInputs.UserCmd userCmd){
        //Rotate mech so the transform.right/forward are correct directions
        float rotateDelta = userCmd.ViewAngle - transform.eulerAngles.y;
        transform.Rotate(new Vector3(0, rotateDelta, 0));

        Speed = (userCmd.Vertical > -MarginOfError && userCmd.Vertical < MarginOfError) ? 0 : userCmd.Vertical;
        Direction = (userCmd.Horizontal > -MarginOfError && userCmd.Horizontal < MarginOfError) ? 0 : userCmd.Horizontal;
        _xzDirNormalized = new Vector2(Direction, Speed).normalized;

        if (CurMovementState != null)CurMovementState(userCmd);

        //Apply the speed
        _move = Vector3.right * XSpeed * userCmd.msec;
        _move += Vector3.forward * ZSpeed * userCmd.msec;
        _move.y += YSpeed * userCmd.msec;

        _characterController.Move(_move);

        //Rotate mech back
        transform.Rotate(new Vector3(0, -rotateDelta, 0));
    }

    private void FixedUpdate() {
        if (PhotonNetwork.isMasterClient || _rootPv.isMine) {
            if (IsBoosting) {
                _mechCombat.DecrementEN(Time.fixedDeltaTime);
            } else {
                _mechCombat.IncrementEN(Time.fixedDeltaTime);
            }
        }

        if (_rootPv.isMine) {
            if (_mainAnimator.GetBool(_animatorVars.BoostHash)) {
                DynamicCam();
            } else {
                ResetCam();
            }
        }
    }

    private void GroundedState(HandleInputs.UserCmd userCmd) {
        Grounded = CheckIsGrounded();

        if (Grounded) {
            if (userCmd.Buttons[(int)HandleInputs.Button.LeftShift] && _mechCombat.IsENAvailable()) {
                HorizontalBoosting(_xzDirNormalized.x, _xzDirNormalized.y, userCmd.msec);
            } else{
                IsBoosting = false;

                Run(_xzDirNormalized.x, _xzDirNormalized.y, userCmd.msec);
            }

            //Detect jump
            if (userCmd.Buttons[(int)HandleInputs.Button.Space]) {
                IsJumping = true;
                _jumpReleased = false;
                IsAvailableVerBoost = true;
                YSpeed = movementVariables.jumpPower;
                IsBoosting = false;

                //Switch state
                CurMovementState = JumpState;
                return;
            }

            //Apply gravity
            YSpeed = -_characterController.stepOffset * userCmd.msec * 40;
        } else{
            CurMovementState = JumpState;
        }
    }

    private void Run(float horizontal_nor, float vertical_nor, float msec) {
        if (_curBoostingSpeed >= movementVariables.moveSpeed && !_mainAnimator.GetBool(_animatorVars.BoostHash)) {//not in transition to boost
            XSpeed = (horizontal_nor * _curBoostingSpeed * transform.right).x + (vertical_nor * _curBoostingSpeed * transform.forward).x;
            ZSpeed = (horizontal_nor * _curBoostingSpeed * transform.right).z + (vertical_nor * _curBoostingSpeed * transform.forward).z;

            _curBoostingSpeed -= movementVariables.deceleration * msec * 5;
        } else {
            XSpeed = movementVariables.moveSpeed * horizontal_nor * transform.right.x + movementVariables.moveSpeed * ((vertical_nor < 0) ? vertical_nor / 2 : vertical_nor) * transform.forward.x;
            ZSpeed = movementVariables.moveSpeed * horizontal_nor * transform.right.z + movementVariables.moveSpeed * ((vertical_nor < 0) ? vertical_nor / 2 : vertical_nor) * transform.forward.z;
        }
    }

    private void HorizontalBoosting(float horizontal_nor, float vertical_nor, float msec) {
        //boost
        if (!IsBoosting) {
            //First call
            _curBoostingSpeed = movementVariables.minBoostSpeed;
            XSpeed = (_curBoostingSpeed * horizontal_nor * transform.right).x + (_curBoostingSpeed * vertical_nor * transform.forward).x;
            ZSpeed = (_curBoostingSpeed * vertical_nor * transform.forward).z + (_curBoostingSpeed * horizontal_nor * transform.right).z;
        }

        IsBoosting = true;

        if (Mathf.Abs(horizontal_nor) < 0.1f && Mathf.Abs(vertical_nor) < 0.1f) {//boosting but not moving => decrease current boosting speed
            if (_curBoostingSpeed >= movementVariables.minBoostSpeed)
                _curBoostingSpeed -= movementVariables.acceleration * msec * 10;
        } else {//increase magnitude if < max
            if (_curBoostingSpeed <= movementVariables.maxHorizontalBoostSpeed)
                _curBoostingSpeed += movementVariables.acceleration * msec * 3;
        }

        //ideal speed
        float idealSpeed_x = (_curBoostingSpeed * horizontal_nor * transform.right).x + (_curBoostingSpeed * vertical_nor * transform.forward).x,
        idealSpeed_z = (_curBoostingSpeed * vertical_nor * transform.forward).z + (_curBoostingSpeed * horizontal_nor * transform.right).z;

        Vector2 dir = new Vector2(idealSpeed_x, idealSpeed_z).normalized;
        float acc_x = Mathf.Abs(movementVariables.acceleration * dir.x), acc_z = Mathf.Abs(movementVariables.acceleration * dir.y);

        Vector2 decreaseDir = new Vector2(Mathf.Abs(XSpeed), Mathf.Abs(ZSpeed)).normalized;

        if (Mathf.Abs(dir.x) < 0.1f && XSpeed != 0) {
            XSpeed += Mathf.Sign(0 - XSpeed) * movementVariables.acceleration * decreaseDir.x * msec * 10;
        } else {
            if (Mathf.Abs(idealSpeed_x - XSpeed) < movementVariables.acceleration) acc_x = 0;

            XSpeed += Mathf.Sign(idealSpeed_x - XSpeed) * acc_x * msec * 35;
        }

        if (Mathf.Abs(dir.y) < 0.1f && ZSpeed != 0) {
            ZSpeed += Mathf.Sign(0 - ZSpeed) * movementVariables.acceleration * decreaseDir.y * msec * 10;
        } else {
            if (Mathf.Abs(idealSpeed_z - ZSpeed) < movementVariables.acceleration) acc_z = 0;

            ZSpeed += Mathf.Sign(idealSpeed_z - ZSpeed) * acc_z * msec * 35;
        }
    }

    private void JumpState(HandleInputs.UserCmd userCmd){
        Grounded = CheckIsGrounded() && YSpeed <= 0;

        if (Grounded){
            IsJumping = false;
            IsBoosting = false;
            
            CurMovementState = GroundedState;
            return;
        }

        //Apply gravity
        YSpeed -= (YSpeed < maxDownSpeed || IsBoosting) ? 0 : Gravity * userCmd.msec * 40;

        if (!userCmd.Buttons[(int) HandleInputs.Button.Space]){
            _jumpReleased = true;
            IsBoosting = false;
        }

        if (_jumpReleased && IsAvailableVerBoost && userCmd.Buttons[(int)HandleInputs.Button.Space]) {

            if (IsAvailableVerBoost){
                //First call
                _verticalBoostStartYPos = transform.position.y;
                YSpeed = movementVariables.verticalBoostSpeed;
                IsAvailableVerBoost = false;
            }

            IsAvailableVerBoost = false;
            IsBoosting = true;
        }

        if (IsBoosting){
            VerticalBoost(_xzDirNormalized.x, _xzDirNormalized.y, userCmd.msec);
        } else{
            JumpMoveInAir(_xzDirNormalized.x, _xzDirNormalized.y, userCmd.msec);
        }
    }

    public bool CheckIsGrounded() {
        return Physics.CheckSphere(transform.position + new Vector3(0, 1.9f, 0), 2f, _terrainMask);
    }

    private void VerticalBoost(float horizontal_nor, float vertical_nor, float msec) {
        if (transform.position.y >= _verticalBoostStartYPos + movementVariables.verticalBoostSpeed * 1.25f) {//max height
            YSpeed = 0;
        } else {
            YSpeed = movementVariables.verticalBoostSpeed;
        }
        
        float inAirSpeed = movementVariables.maxHorizontalBoostSpeed * InAirSpeedCoeff;
        XSpeed = inAirSpeed * horizontal_nor * transform.right.x + inAirSpeed * vertical_nor * transform.forward.x;
        ZSpeed = inAirSpeed * horizontal_nor * transform.right.z + inAirSpeed * vertical_nor * transform.forward.z;
    }

    public void JumpMoveInAir(float horizontal_nor, float vertical_nor, float msec) {
        if (_curBoostingSpeed >= movementVariables.moveSpeed) {//not in transition to boost
            _curBoostingSpeed -= movementVariables.deceleration * msec * 10;

            XSpeed = (horizontal_nor * _curBoostingSpeed * transform.right).x + (vertical_nor * _curBoostingSpeed * transform.forward).x;
            ZSpeed = (horizontal_nor * _curBoostingSpeed * transform.right).z + (vertical_nor * _curBoostingSpeed * transform.forward).z;
        } else {
            XSpeed = movementVariables.moveSpeed * horizontal_nor * transform.right.x + movementVariables.moveSpeed * vertical_nor * transform.forward.x;
            ZSpeed = movementVariables.moveSpeed * horizontal_nor * transform.right.z + movementVariables.moveSpeed * vertical_nor * transform.forward.z;
        }
    }

    public void SetMoving(float speed) {//called by animation
        instantMoveSpeed = speed;
        instantMoveDir = _camTransform.forward;
        if (Grounded) instantMoveDir = new Vector3(instantMoveDir.x, 0, instantMoveDir.z);// make sure not slashing to the sky
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
            _getJumpWhenSlash = false;

        instantMoveSpeed /= 1.6f;//1.6 : decrease coeff.
        if (Mathf.Abs(instantMoveSpeed) > 0.01f) {
            YSpeed = 0;
        }
        _characterController.Move(instantMoveDir * instantMoveSpeed);
        _move.x = 0;
        _move.z = 0;

        if (_mainAnimator.GetBool(_animatorVars.JumpHash))
            _getJumpWhenSlash = true;

        //cast a ray downward to check if not jumping but not grounded => if so , directly teleport to ground
        RaycastHit hit;
        if (!_getJumpWhenSlash && Physics.Raycast(transform.position, -Vector3.up, out hit, _terrainMask)) {
            if (Vector3.Distance(hit.point, transform.position) >= _slashTeleportMinDistance && !Physics.CheckSphere(hit.point + new Vector3(0, 2.1f, 0), _characterController.radius, _terrainMask)) {
                transform.position = hit.point;

                //TODO : map edge case
            }
        }
    }

    private void LateUpdate() {
        if(_mechCombat.IsMeleePlaying() || onSkill)return;

        RotateLegs();//Other player has to run this 
    }

    private void RotateLegs(){
        _rLDir = _mainAnimator.GetFloat("Direction");
        if (_mainAnimator.GetFloat("Speed") < 0) _rLDir *= -1;

        _rLSpineDegree = Mathf.Lerp(_rLSpineDegree, -30, _rLLerpSpeed * Time.deltaTime);

        _pelvis.rotation = Quaternion.Euler(_pelvis.rotation.eulerAngles.x, _pelvis.rotation.eulerAngles.y + _rLDir * _rLPelvisDegree, _pelvis.rotation.eulerAngles.z);
        _spine1.rotation = Quaternion.Euler(_spine1.rotation.eulerAngles.x, _spine1.rotation.eulerAngles.y + _rLDir * _rLSpineDegree, _spine1.rotation.eulerAngles.z);
    }

    public void SetCanVerticalBoost(bool canVBoost) {
        _canVerticalBoost = canVBoost;
    }

    public bool CanVerticalBoost() {
        return _canVerticalBoost;
    }

    private void Jump() {
        _verticalBoostStartYPos = 0;
        YSpeed = movementVariables.jumpPower;
    }

    public void ResetCurBoostingSpeed() {
        _curBoostingSpeed = movementVariables.moveSpeed;
    }

    public void ResetCurSpeed() {
        XSpeed = 0;
        ZSpeed = 0;
    }

    public void Boost(bool b, float horizontal_nor, float vertical_nor, float msec) {//TODO : improve this using animator bool to decide open/close
        if (b != isBoostFlameOn) {
            if(_rootPv.isMine)photonView.RPC("BoostFlame", PhotonTargets.All, b, Grounded);//TODO : check this

            isBoostFlameOn = b;

            if (b) {//toggle on the boost first call
                if (Grounded) {
                    _curBoostingSpeed = movementVariables.minBoostSpeed;
                    XSpeed = (_curBoostingSpeed * horizontal_nor * transform.right).x + (_curBoostingSpeed * vertical_nor * transform.forward).x;
                    ZSpeed = (_curBoostingSpeed * vertical_nor * transform.forward).z + (_curBoostingSpeed * horizontal_nor * transform.right).z;
                }
            }
        }
        if (b) {
            //if (grounded) {
                if (Mathf.Abs(vertical_nor) < 0.1f && Mathf.Abs(horizontal_nor) < 0.1f) {//boosting but not moving => decrease current boosting speed
                    if (_curBoostingSpeed >= movementVariables.minBoostSpeed)
                        _curBoostingSpeed -= movementVariables.acceleration * msec * 10;
                } else {//increase magnitude if < max
                    if (_curBoostingSpeed <= movementVariables.maxHorizontalBoostSpeed)
                        _curBoostingSpeed += movementVariables.acceleration * msec * 3;
                }

                //ideal speed
                float idealSpeed_x = (_curBoostingSpeed * horizontal_nor * transform.right).x + (_curBoostingSpeed * vertical_nor * transform.forward).x,
                idealSpeed_z = (_curBoostingSpeed * vertical_nor * transform.forward).z + (_curBoostingSpeed * horizontal_nor * transform.right).z;

                Vector2 dir = new Vector2(idealSpeed_x, idealSpeed_z).normalized;
                float acc_x = Mathf.Abs(movementVariables.acceleration * dir.x), acc_z = Mathf.Abs(movementVariables.acceleration * dir.y);

                Vector2 decreaseDir = new Vector2(Mathf.Abs(XSpeed), Mathf.Abs(ZSpeed)).normalized;

                if (Mathf.Abs(dir.x) < 0.1f && XSpeed != 0) {
                    XSpeed += Mathf.Sign(0 - XSpeed) * movementVariables.acceleration * decreaseDir.x * msec * 10;
                } else {
                    if (Mathf.Abs(idealSpeed_x - XSpeed) < movementVariables.acceleration) acc_x = 0;

                    XSpeed += Mathf.Sign(idealSpeed_x - XSpeed) * acc_x * msec * 35;
                }

                if (Mathf.Abs(dir.y) < 0.1f && ZSpeed != 0) {
                    ZSpeed += Mathf.Sign(0 - ZSpeed) * movementVariables.acceleration * decreaseDir.y * msec * 10;
                } else {
                    if (Mathf.Abs(idealSpeed_z - ZSpeed) < movementVariables.acceleration) acc_z = 0;

                    ZSpeed += Mathf.Sign(idealSpeed_z - ZSpeed) * acc_z * msec * 35;
                }
                //run_xzDir.x = horizontal_nor;
                //run_xzDir.y = vertical_nor;
            //} else {//boost in air
            //    float inAirSpeed = movementVariables.maxHorizontalBoostSpeed * InAirSpeedCoeff;
            //    xSpeed = inAirSpeed * horizontal_nor * transform.right.x + inAirSpeed * vertical_nor * transform.forward.x;
            //    zSpeed = inAirSpeed * horizontal_nor * transform.right.z + inAirSpeed * vertical_nor * transform.forward.z;
            //}
        }
    }

    public void CnPose() {
        XSpeed = 0;
        ZSpeed = 0;
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
        //SetCanVerticalBoost(false);
        //_mainAnimator.SetBool("Boost", false);
        ////Boost(false);
        //if (!CheckIsGrounded())//in air => small bump effect
        //    YSpeed = 0;

        yield return new WaitForSeconds(slowDownDuration);
        //isSlowDown = false;
        //slowDownCoroutine = null;
    }

    public void ResetCam() {
        Vector3 curPos = _camTransform.localPosition;
        Vector3 newPos = new Vector3(0, curPos.y, curPos.z);
        _camTransform.localPosition = Vector3.Lerp(_camTransform.localPosition, newPos, 0.1f);
    }

    public void DynamicCam() {
        Vector3 curPos = _camTransform.localPosition;
        Vector3 newPos = _camTransform.localPosition;

        if (Direction > 0) {
            newPos = new Vector3(LocalxOffset, curPos.y, curPos.z);
        } else if (Direction < 0) {
            newPos = new Vector3(-LocalxOffset, curPos.y, curPos.z);
        } else {
            newPos = new Vector3(0, curPos.y, curPos.z);
        }

        _camTransform.localPosition = Vector3.Lerp(_camTransform.localPosition, newPos, Time.fixedDeltaTime * lerpCam_coeff);
    }

    [PunRPC]
    private void BoostFlame(bool boost, bool boostdust) {
        if (boost) {
            _boosterController.StartBoost();
            if (boostdust)
                EffectController.BoostingDustEffect(true);
        } else {
            _boosterController.StopBoost();
            EffectController.BoostingDustEffect(false);
        }
    }

    public void OnJumpAction() {
        _audioSource.PlayOneShot(JumpSound);
    }

    public void OnLandingAction() {
        if (_jumpEffect != null) _jumpEffect.Play();
        _audioSource.PlayOneShot(LandingSound);
    }

    public void CallLockMechRot(bool b) {
        _mechCam.LockMechRotation(b);
    }

    private void InterruptCurrentMovement(bool b) {
        if (b) {//when entering
            ResetCurBoostingSpeed();
            ResetCurSpeed();
            //Boost(false);
        } else {
            onInstantMoving = false;
            ResetCurBoostingSpeed();
            ResetCurSpeed();
        }
    }

    [System.Serializable]
    public struct MovementVariables {
        public float verticalBoostSpeed;
        public float maxHorizontalBoostSpeed;
        public float jumpPower;
        public float moveSpeed;
        public float minBoostSpeed;

        public float acceleration;
        public float deceleration;
    }
}

//TODO : implement these
//if (onInstantMoving) {
//    InstantMove();
//    return;
//}

//if (isSlowDown) {
//    move.x = move.x * slowDownCoeff;
//    move.z = move.z * slowDownCoeff;
//}

//if (!gm.GameIsBegin) {//player can't move but can rotate
//    move.x = 0;
//    move.z = 0;
//}