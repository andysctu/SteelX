using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using XftWeapon;
using System.Linq;

public class MechCombat : Combat {

    [SerializeField]private HeatBar HeatBar;
    [SerializeField]private DisplayPlayerInfo displayPlayerInfo;
    [SerializeField]private CrosshairImage crosshairImage;
    [SerializeField]private EffectController EffectController;
    [SerializeField]private LayerMask playerlayerMask;
    [SerializeField]private Camera cam, skillcam;
    [SerializeField]private BuildMech bm;
    [SerializeField]private Animator animator;
    [SerializeField]private MovementClips MovementClips;
    [SerializeField]private SkillController SkillController;
    private InRoomChat InRoomChat;

    enum GeneralWeaponTypes { Ranged, Rectifier, Melee, Shield, Rocket, Cannon, Empty };//for efficiency
    enum SpecialWeaponTypes { APS, LMG, Rifle, Shotgun, Rectifier, Sword, Spear, Shield, Rocket, Cannon, EMPTY };

    // Boost variables
    private float fuelDrain = 250.0f;
    private float fuelGain = 1000.0f;
    private float minFuelRequired = 450;
    private float currentFuel;
    public float jumpPower = 70.0f;
    public float moveSpeed = 30.0f;
    public float minBoostSpeed = 50;
    public float acceleration = 2;
    public float deceleration = 30;
    private float verticalBoostSpeed = 1f;
    public float maxVerticalBoostSpeed = 30f;
    public float maxHorizontalBoostSpeed = 60f;

    // Game variables
    public Score score;
    private const int playerlayer = 8, default_layer = 0;

    // Combat variables
    public bool isDead;
    public bool[] is_overheat = new bool[4]; // this is handled by HeatBar.cs , but other player also need to access it (shield)
    public int MaxHeat = 100;
    public int cooldown = 5;
    public int BCNbulletNum = 2;
    public bool isOnBCNPose, onSkill = false;//called by BCNPoseState to check if on the right pose 
    private int weaponOffset = 0;
    private int[] curGeneralWeaponTypes = new int[4];//ranged , melee , ...
    private int[] curSpecialWeaponTypes = new int[4];//APS , BRF , ...
    private bool isBCNcanceled = false;//check if right click cancel

    // Left
    private const int LEFT_HAND = 0;
    private float timeOfLastShotL;
    private bool fireL = false;
    public int isLMeleePlaying = 0;

    // Right
    private const int RIGHT_HAND = 1;
    private float timeOfLastShotR;
    private bool fireR = false;
    public int isRMeleePlaying = 0;

    public bool CanMeleeAttack = true;
    private bool isSwitchingWeapon = false;
    private bool receiveNextSlash = true;
    private const int slashMaxDistance = 30;//the ray which checks if hitting shield max distance
    public float slashL_threshold, slashR_threshold;

    public Transform[] Hands;//other player use this to locate hand position quickly
    private Transform shoulderL;
    private Transform shoulderR;
    private Transform head;
    private Transform camTransform;
    private Transform[] Gun_ends = new Transform[4];


    //Targets
    private List<Transform> targets_in_collider;
    private GameObject[] Targets;
    private bool[] isTargetShield;
    private int[] target_HandOnShield;
    private Vector3[] bullet_directions;

    // GameObjects
    private GameObject[] weapons, bullets;
    private GameObject BulletCollector;//collect all bullets
    private Weapon[] weaponScripts;

    // HUD
    private Slider healthBar;
    private Slider fuelBar;
    private Image fuelBar_fill;
    private bool isNotEnoughEffectPlaying = false;
    private bool isFuelAvailable = true;
    private Text healthtext, fueltext;

    // Components
    public Crosshair crosshair;
    private SlashDetector slashDetector;
    private MechController mechController;
    private Sounds Sounds;
    private AnimationEventController AnimationEventController;
    private ParticleSystem[] Muz = new ParticleSystem[4];
    private XWeaponTrail trailL, trailR;

    //Animator
    private AnimatorVars AnimatorVars;
    private AnimatorOverrideController animatorOverrideController = null;
    private AnimationClipOverrides clipOverrides;
    //private MechIK MechIK;

    //for Debug
    public bool forceDead = false;

    public delegate void WeaponSwitchedAction();
    public WeaponSwitchedAction OnWeaponSwitched;
    Coroutine SwitchWeaponcoroutine;

    void Awake() {
       RegisterOnWeaponSwitched();
       RegisterOnWeaponBuilt();
       RegisterOnSkill();
    }

    void Start() {
        findGameManager();
        initMechStats();
        initComponents();
        initCombatVariables();
        initAnimatorControllers();
        initTransforms();
        initGameObjects();
        initTargetProperties();
        initSlashDetector();

        SyncWeaponOffset();//TODO : check this
        initHUD();
    }

    void RegisterOnWeaponSwitched() {
        OnWeaponSwitched += UpdateSlashAnimationThreshold;
        OnWeaponSwitched += UpdateSMGAnimationSpeed;
        OnWeaponSwitched += UpdateArmAnimatorState;
        OnWeaponSwitched += FindTrail;
        OnWeaponSwitched += UpdateSlashDetector;
    }

    void RegisterOnWeaponBuilt() {
        bm.OnWeaponBuilt += InitWeapons;
    }
    
    void RegisterOnSkill() {
        SkillController.OnSkill += OnSkill;
    }

    void initMechStats() {//call also when respawn
        currentHP = MAX_HP;
        currentFuel = MAX_FUEL;
    }

    void initTransforms() {
        camTransform = cam.transform;
        head = transform.Find("CurrentMech/metarig/hips/spine/chest/fakeNeck/head");
        shoulderL = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.L");
        shoulderR = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.R");

        Hands = new Transform[2];
        Hands[0] = shoulderL.Find("upper_arm.L/forearm.L/hand.L");
        Hands[1] = shoulderR.Find("upper_arm.R/forearm.R/hand.R");
    }

    void initGameObjects() {
        BulletCollector = GameObject.Find("BulletCollector");
        InRoomChat = GameObject.Find("InRoomChat").GetComponent<InRoomChat>();
    }

    void initTargetProperties() {
        Targets = new GameObject[2];
        isTargetShield = new bool[2];
        target_HandOnShield = new int[2];
        bullet_directions = new Vector3[2];
    }

    void initComponents() {
        Transform currentMech = transform.Find("CurrentMech");
        if (currentMech == null) {
            Debug.Log("Can't find currentMech");
            return;
        }
        Sounds = currentMech.GetComponent<Sounds>();
        AnimatorVars = currentMech.GetComponent<AnimatorVars>();
        AnimationEventController = currentMech.GetComponent<AnimationEventController>();
        animator = currentMech.GetComponent<Animator>();
        mechController = GetComponent<MechController>();
        MovementClips = GetComponent<MovementClips>();
        crosshair = cam.GetComponent<Crosshair>();
    }

    void initAnimatorControllers() {
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        UpdateMovementClips();//TODO : improve this
    }

    private void InitWeapons() {
        weaponOffset = 0;
        weapons = bm.weapons;
        bullets = bm.bulletPrefabs;
        weaponScripts = bm.weaponScripts;

        UpdateCurWeaponType();
        FindMuz();
        FindGunEnds();
    }

    public void initCombatVariables() {// this will be called also when respawn
        weaponOffset = 0;
        if (photonView.isMine) SetWeaponOffsetProperty(weaponOffset);
        if (SwitchWeaponcoroutine != null) {//die when switching weapons
            StopCoroutine(SwitchWeaponcoroutine);
            isSwitchingWeapon = false;
            SwitchWeaponcoroutine = null;
        }
        onSkill = false;
        setIsFiring(0, false);
        setIsFiring(1, false);
        mechController.FindBoosterController();//TODO : OnMechBuilt event
    }

    void initHUD() {
        if (!photonView.isMine)
            return;
        initHealthAndFuelBars();//other player should not call this ( they share hud )
    }

    public void FindGunEnds() {
        for (int i = 0; i < 4; i++) {
            if (weapons[i] != null) {
                Gun_ends[i] = weapons[i].transform.Find("End");
            }
        }
    }

    public void FindMuz() {
        for (int i = 0; i < 4; i++) {
            if (weapons[i] != null) {
                Transform MuzTransform = weapons[i].transform.Find("End/Muz");
                if (MuzTransform != null) {
                    Muz[i] = MuzTransform.GetComponent<ParticleSystem>();
                    Muz[i].Stop();
                }
            }
        }
    }

    float TimeOfLastShot(int hand) {
        return hand == LEFT_HAND ? timeOfLastShotL : timeOfLastShotR;
    }

    void initHealthAndFuelBars() {
        Slider[] sliders = GameObject.Find("PanelCanvas").GetComponentsInChildren<Slider>();
        if (sliders.Length > 0) {
            healthBar = sliders[0];
            healthBar.value = 1;
            healthtext = healthBar.GetComponentInChildren<Text>();
            if (sliders.Length > 1) {
                fuelBar = sliders[1];
                fuelBar_fill = fuelBar.transform.Find("Fill Area/Fill").GetComponent<Image>();
                fuelBar.value = 1;
                fueltext = fuelBar.GetComponentInChildren<Text>();
            }
        }
    }

    void initSlashDetector() {
        slashDetector = GetComponentInChildren<SlashDetector>();
    }

    void UpdateSlashDetector() {
        if (slashDetector == null)
            initSlashDetector();

        bool b = ((curGeneralWeaponTypes[weaponOffset] == (int)GeneralWeaponTypes.Melee || curGeneralWeaponTypes[weaponOffset + 1] == (int)GeneralWeaponTypes.Melee) && photonView.isMine);
        slashDetector.EnableDetector(b);
    }

    void SyncWeaponOffset() {
        //sync other player weapon offset
        if (!photonView.isMine) {
            if (photonView.owner.CustomProperties["weaponOffset"] != null) {
                weaponOffset = int.Parse(photonView.owner.CustomProperties["weaponOffset"].ToString());
            } else//the player may just initialize
                weaponOffset = 0;

            weapons[(weaponOffset)].SetActive(true);
            weapons[(weaponOffset + 1)].SetActive(true);
            weapons[(weaponOffset + 2) % 4].SetActive(false);
            weapons[(weaponOffset + 3) % 4].SetActive(false);
        }
    }

    void FireRaycast(Vector3 start, Vector3 direction, int hand) {
        Transform target = ((hand == 0) ? crosshair.getCurrentTargetL() : crosshair.getCurrentTargetR());//target might be shield collider
        int damage = bm.weaponScripts[weaponOffset + hand].damage;

        if (target != null) {
            PhotonView targetpv = target.transform.root.GetComponent<PhotonView>();
            int target_viewID = targetpv.viewID;
            string weaponName = bm.curWeaponNames[weaponOffset + hand];

            if (curGeneralWeaponTypes[weaponOffset + hand] != (int)GeneralWeaponTypes.Rectifier) {
                if (target.tag != "Shield") {//not shield => player or drone

                    photonView.RPC("Shoot", PhotonTargets.All, hand, direction, target_viewID, false, -1);

                    targetpv.RPC("OnHit", PhotonTargets.All, damage, photonView.viewID, weaponName, weaponScripts[weaponOffset + hand].slowDown);

                    if (target.gameObject.GetComponent<Combat>().CurrentHP() <= 0) {
                        targetpv.GetComponent<HUD>().DisplayKill(cam);
                    } else {
                        targetpv.GetComponent<HUD>().DisplayHit(cam);
                    }
                } else {
                    //check what hand is it
                    ShieldUpdater shieldUpdater = target.parent.GetComponent<ShieldUpdater>();
                    int target_handOnShield = shieldUpdater.GetHand();

                    photonView.RPC("Shoot", PhotonTargets.All, hand, direction, target_viewID, true, target_handOnShield);

                    MechCombat targetMcbt = target.transform.root.GetComponent<MechCombat>();

                    if (targetMcbt != null) {
                        if (targetMcbt.is_overheat[targetMcbt.weaponOffset + target_handOnShield]) {
                            targetpv.RPC("ShieldOnHit", PhotonTargets.All, damage, photonView.viewID, target_handOnShield, weaponName);
                        } else {                            
                            targetpv.RPC("ShieldOnHit", PhotonTargets.All, (int)(damage * shieldUpdater.GetDefendEfficiency(false)), photonView.viewID, target_handOnShield, weaponName);
                        }
                    } else {//target is drone
                        targetpv.RPC("ShieldOnHit", PhotonTargets.All, (int)(damage * shieldUpdater.GetDefendEfficiency(false)), photonView.viewID, target_handOnShield, weaponName);
                    }

                    targetpv.GetComponent<HUD>().DisplayDefense(cam);
                }
            } else {//ENG
                photonView.RPC("Shoot", PhotonTargets.All, hand, direction, target_viewID, false, -1);

                targetpv.RPC("OnHeal", PhotonTargets.All, photonView.viewID, damage);

                targetpv.GetComponent<HUD>().DisplayHit(cam);
            }
        } else {
            photonView.RPC("Shoot", PhotonTargets.All, hand, direction, -1, false, -1);
        }
    }

    public void SlashDetect(int hand) {
        if ((targets_in_collider = slashDetector.getCurrentTargets()).Count != 0) {

            int damage = bm.weaponScripts[weaponOffset + hand].damage;
            string weaponName = bm.curWeaponNames[weaponOffset + hand];
            Sounds.PlaySlashOnHit(weaponOffset + hand);

            foreach (Transform target in targets_in_collider) {
                if (target == null) {//it causes bug if target disconnect
                    continue;
                }

                //cast a ray to check if hitting shield
                bool isHitShield = false;
                RaycastHit[] hitpoints;
                Transform t = target;

                hitpoints = Physics.RaycastAll(transform.position + new Vector3(0, 5, 0), (target.transform.root.position + new Vector3(0, 5, 0)) - transform.position - new Vector3(0, 5, 0), slashMaxDistance, playerlayerMask).OrderBy(h => h.distance).ToArray();
                foreach (RaycastHit hit in hitpoints) {
                    if (hit.transform.root == target) {
                        if (hit.collider.transform.tag == "Shield") {
                            isHitShield = true;
                            t = hit.collider.transform;
                        }
                        break;
                    }
                }

                if (isHitShield) {
                    ShieldUpdater shieldUpdater = t.transform.parent.GetComponent<ShieldUpdater>();
                    int target_handOnShield = shieldUpdater.GetHand();//which hand holds the shield?
                    target.GetComponent<PhotonView>().RPC("ShieldOnHit", PhotonTargets.All, (int)(damage * shieldUpdater.GetDefendEfficiency(true)), photonView.viewID, target_handOnShield, weaponName);
                } else {
                    target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, photonView.viewID, weaponName, true);
                }

                if (target.GetComponent<Combat>().CurrentHP() <= 0) {
                    target.GetComponent<HUD>().DisplayKill(cam);
                } else {
                    if (isHitShield)
                        target.GetComponent<HUD>().DisplayDefense(cam);
                    else
                        target.GetComponent<HUD>().DisplayHit(cam);
                }
            }
        }
    }

    [PunRPC]
    void Shoot(int hand, Vector3 direction, int playerPVid, bool isShield, int target_handOnShield) {        
        SetTargetInfo(hand, direction, playerPVid, isShield, target_handOnShield);
        string clipName = "";

        switch (curSpecialWeaponTypes[weaponOffset+ hand]) {
            case (int)SpecialWeaponTypes.APS:
            clipName += "APS";
            break;
            case (int)SpecialWeaponTypes.Rifle:
            clipName += "BRF";
            break;
            case (int)SpecialWeaponTypes.Shotgun:
            clipName += "SGN";
            break;
            case (int)SpecialWeaponTypes.LMG:
            clipName += "LMG";
            break;
            case (int)SpecialWeaponTypes.Rocket:
            clipName += "RCL";
            animator.Play("RCLShootR", 2);
            break;
            case (int)SpecialWeaponTypes.Rectifier:
            clipName += "ENG";
            break;
            case (int)SpecialWeaponTypes.Cannon:
            return;
            default:
            Debug.LogError("Should never get here");
            break;
        }
        clipName += "Shoot" + ((hand == 0) ? "L" : "R");
        animator.Play(clipName, hand + 1, 0);
    }

    void SetTargetInfo(int hand, Vector3 direction, int playerPVid, bool isShield, int target_handOnShield) {
        if(playerPVid != -1) {
            Targets[hand] = PhotonView.Find(playerPVid).gameObject;
            isTargetShield[hand] = isShield;
            target_HandOnShield[hand] = target_handOnShield;
            bullet_directions[hand] = direction;
        } else {
            Targets[hand] = null;
            bullet_directions[hand] = direction;
        }
    }

    public void SetTargetInfo(int hand, Transform target) {//for skill
        Targets[hand] = target.gameObject;
        isTargetShield[hand] = false;
    }

    public void InstantiateBulletTrace(int hand) {//aniamtion event driven
        if (bullets[weaponOffset + hand] == null) {
            Debug.LogError("bullet is null");
            return;
        }
        //Play Muz
        if (Muz[weaponOffset + hand] != null) {
            Muz[weaponOffset + hand].Play();
        }
        //Play Sound
        Sounds.PlayShot(hand);

        Camera cam_enabled = (cam.enabled)? cam : skillcam;

        if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Rocket) {
            if (photonView.isMine) {
                GameObject bullet = PhotonNetwork.Instantiate("RCL034B", transform.position + new Vector3(0,5,0) +transform.forward * 10, Quaternion.LookRotation(bullet_directions[hand]), 0);
                RCLBulletTrace bulletTrace = bullet.GetComponent<RCLBulletTrace>();
                bulletTrace.SetShooterInfo(gameObject, cam_enabled);
                bulletTrace.SetBulletPropertis(weaponScripts[weaponOffset].damage, ((Rocket)weaponScripts[weaponOffset]).bullet_speed, ((Rocket)weaponScripts[weaponOffset]).impact_radius);
            }
        } else if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Rectifier) {
            GameObject bullet = Instantiate(bullets[weaponOffset + hand], Gun_ends[weaponOffset + hand].position, Quaternion.LookRotation(bullet_directions[hand])) as GameObject;
            ElectricBolt eb = bullet.GetComponent<ElectricBolt>();
            bullet.transform.SetParent(Gun_ends[weaponOffset + hand]);
            bullet.transform.localPosition = Vector3.zero;
            eb.SetCamera(cam_enabled);
            eb.SetTarget((Targets[hand]==null)? null : Targets[hand].transform);      
        } else {
            GameObject b = bullets[weaponOffset + hand];
            MechCombat mcbt = (Targets[hand] == null) ? null : Targets[hand].transform.root.GetComponent<MechCombat>();

            if (photonView.isMine) {
                crosshairImage.ShakingEffect(hand);
                if (Targets[hand] != null && !CheckTargetIsDead(Targets[hand])) {
                    //only APS & LMG have multiple msgs.
                    if (curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.APS || curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.LMG) {
                        if (!isTargetShield[hand]) {
                            if (mcbt != null)
                                mcbt.GetComponent<HUD>().DisplayHit(cam_enabled);
                            else
                                Targets[hand].transform.root.GetComponent<HUD>().DisplayHit(cam_enabled);
                        } else {
                            if (mcbt != null)
                                mcbt.GetComponent<HUD>().DisplayDefense(cam_enabled);
                            else
                                Targets[hand].transform.root.GetComponent<HUD>().DisplayDefense(cam_enabled);
                        }
                    }
                }
            }

            GameObject bullet = Instantiate(b, Gun_ends[weaponOffset + hand].position, Quaternion.identity, BulletCollector.transform) as GameObject;
            BulletTrace bulletTrace = bullet.GetComponent<BulletTrace>();
            bulletTrace.SetCamera(cam_enabled);
            bulletTrace.SetShooterName(gameObject.name);

            if (Targets[hand] != null) {
                if (isTargetShield[hand]) {
                    bulletTrace.SetTarget((mcbt != null) ? mcbt.Hands[target_HandOnShield[hand]] : Targets[hand].transform.root.GetComponent<DroneCombat>().Hands[target_HandOnShield[hand]], true);
                } else {
                    bulletTrace.SetTarget(Targets[hand].transform, false);
                }
            } else {
                bulletTrace.SetTarget(null, false);
            }
        }
    }

    bool CheckTargetIsDead(GameObject target) {
        MechCombat mcbt = target.transform.root.GetComponent<MechCombat>();
        if (mcbt == null)//Drone
        {
            return target.transform.root.GetComponent<DroneCombat>().CurrentHP() <= 0;
        } else {
            return mcbt.CurrentHP() <= 0;
        }
    }

    // Applies damage, and updates scoreboard + disables player on kill
    [PunRPC]
    public override void OnHit(int damage, int shooter_viewID, string weapon, bool isSlowDown = false) {
        if (isDead) {
            return;
        }

        if (CheckIsSwordByStr(weapon)) {
            EffectController.SlashOnHitEffect(false, 0);
        }else if (CheckIsSpearByStr(weapon)) {
            EffectController.SlashOnHitEffect(false, 0);
        }


        if (photonView.isMine && isSlowDown) {
            mechController.SlowDown();
        }

        currentHP -= damage;

        if (currentHP <= 0) {
            isDead = true;

            DisablePlayer();

            // Update scoreboard
            gm.RegisterKill(shooter_viewID, photonView.viewID);
            PhotonView shooterpv = PhotonView.Find(shooter_viewID);
            DisplayKillMsg(shooterpv.owner.NickName, photonView.name, weapon);
        }
    }

    [PunRPC]
    void ShieldOnHit(int damage, int shooter_viewID, int shield, string weapon) {
        if (isDead) {
            return;
        }

        if (CheckIsSwordByStr(weapon)) {
            EffectController.SlashOnHitEffect(true, shield);
        } else if (CheckIsSpearByStr(weapon)) {
            EffectController.SlashOnHitEffect(true, shield);
        }

        currentHP -= damage;

        //Debug.Log("HP: " + currentHP);
        if (currentHP <= 0) {

            DisablePlayer();//TODO : sync this

            // Update scoreboard
            gm.RegisterKill(shooter_viewID, photonView.viewID);
            PhotonView shooterpv = PhotonView.Find(shooter_viewID);
            DisplayKillMsg(shooterpv.owner.NickName, photonView.name, weapon);
        }

        if (photonView.isMine && !is_overheat[weaponOffset + shield]) {//heat
            if (shield == 0) {
                HeatBar.IncreaseHeatBarL(30);
            } else {
                HeatBar.IncreaseHeatBarR(30);
            }
        }
    }
    bool CheckIsSwordByStr(string name) {
        return name.Contains("SHL");
    }
    bool CheckIsSpearByStr(string name) {
        return name.Contains("ADR");
    }

    void DisplayKillMsg(string shooter, string target, string weapon) {
        InRoomChat.AddLine(shooter + " killed " + photonView.name + " by " + weapon);
    }

    [PunRPC]
    void OnHeal(int viewID, int amount) {
        if (isDead) {
            return;
        }

        if (currentHP + amount >= MAX_HP) {
            currentHP = MAX_HP;
        } else {
            currentHP += amount;
        }
    }

    public void SetCurrentHp(int amount) {
        currentHP = amount;
    }

    [PunRPC]
    void OnLocked(string name) {
        if (PhotonNetwork.playerName != name)
            return;
        crosshair.ShowLocked();
    }

    [PunRPC]
    public void KnockBack(Vector3 dir, float length) {
        GetComponent<CharacterController>().Move(dir * length);
        //transform.position += dir * length;
    }

    //direction =  back
    public void Skill_KnockBack(float length) {
        GetComponent<CharacterController>().Move(-transform.forward * length);
        //transform.position += dir * length;
    }

    // Disable MechController, Crosshair, Renderers, and set layer to 0
    [PunRPC]
    void DisablePlayer() {
        //check if he has the flag
        if (PhotonNetwork.isMasterClient) {
            if (photonView.owner.NickName == ((gm.BlueFlagHolder == null) ? "" : gm.BlueFlagHolder.NickName)) {
                print("that dead man has the flag.");
                gm.GetComponent<PhotonView>().RPC("DropFlag", PhotonTargets.All, photonView.viewID, 0, transform.position);
            } else if (photonView.owner.NickName == ((gm.RedFlagHolder == null) ? "" : gm.RedFlagHolder.NickName)) {
                gm.GetComponent<PhotonView>().RPC("DropFlag", PhotonTargets.All, photonView.viewID, 1, transform.position);
            }
        }

        if (photonView.isMine) {
            currentHP = 0;
            animator.SetBool(AnimatorVars.BCNPose_id, false);
            gm.ShowRespawnPanel();
        }

        gameObject.layer = default_layer;

        setIsFiring(0, false);
        setIsFiring(1, false);

        StartCoroutine(DisablePlayerWhenNotOnSkill());

        mechController.enabled = false;
        EnableAllColliders(false);

        GetComponent<Collider>().enabled = true;//set to true to trigger exit (while layer changed)
    }

    IEnumerator DisablePlayerWhenNotOnSkill() {
        yield return new WaitWhile(() => onSkill);

        displayPlayerInfo.gameObject.SetActive(false);
        Crosshair ch = GetComponentInChildren<Crosshair>();
        if (ch != null) {
            ch.ShutDownAllCrosshairs();
            ch.enabled = false;
        }

        EnableAllRenderers(false);

        crosshairImage.gameObject.SetActive(false);
        HeatBar.gameObject.SetActive(false);
    }

    // Enable MechController, Crosshair, Renderers, set layer to player layer, move player to spawn position
    [PunRPC]
    void EnablePlayer(int respawnPoint, int mech_num) {
        bm.SetMechNum(mech_num);
        if (photonView.isMine) { // build mech also init MechCombat
            Mech m = UserData.myData.Mech[mech_num];
            bm.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
        }

        initMechStats();

        mechController.initControllerVar();
        displayPlayerInfo.gameObject.SetActive(!photonView.isMine);

        transform.position = gm.SpawnPoints[respawnPoint].position;
        gameObject.layer = playerlayer;
        isDead = false;
        if (!photonView.isMine) return;

        // If this is me, enable MechController and Crosshair
        mechController.enabled = true;
        Crosshair ch = cam.GetComponent<Crosshair>();
        ch.enabled = true;

        crosshairImage.gameObject.SetActive(true);
        HeatBar.gameObject.SetActive(true);

        EffectController.RespawnEffect();
    }

    // Update is called once per frame
    void Update() {
        if (!photonView.isMine || gm.GameOver() || !gm.GameIsBegin) return;

        // Drain HP bar gradually
        if (isDead) {
            if (healthBar.value > 0) healthBar.value = healthBar.value - 0.01f;
            return;
        }

        //TODO : remove this
        if (forceDead) {
            forceDead = false;
            photonView.RPC("OnHit", PhotonTargets.All, 3000, photonView.viewID, "ForceDead", true);
        }

        updateHUD(); // this is called when on skill

        if (onSkill) return;

        // Animate left and right combat
        handleCombat(LEFT_HAND);
        handleCombat(RIGHT_HAND);
        handleSkillInput();

        // Switch weapons
        if (Input.GetKeyDown(KeyCode.R) && !isSwitchingWeapon && !isDead) {
            currentFuel -= (currentFuel >= MAX_FUEL / 3) ? MAX_FUEL / 3 : currentFuel;

            photonView.RPC("CallSwitchWeapons", PhotonTargets.All, null);
        }
    }

    // Set animations and tweaks
    void LateUpdate() {
        handleAnimation(LEFT_HAND);
        handleAnimation(RIGHT_HAND);
    }

    void handleCombat(int hand) {
        if (bm.weaponScripts[weaponOffset + hand] == null) return;

        switch (curGeneralWeaponTypes[weaponOffset + hand]) {
            case (int)GeneralWeaponTypes.Ranged:
            if (curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.APS || curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.LMG) {//has a delay before putting down hands
                if (!Input.GetKey(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + hand]) {
                    if (hand == LEFT_HAND) {
                        if (Time.time - timeOfLastShotL >= 1 / bm.weaponScripts[weaponOffset + hand].Rate * 0.95f)
                            setIsFiring(hand, false);
                        return;
                    } else {
                        if (Time.time - timeOfLastShotR >= 1 / bm.weaponScripts[weaponOffset + hand].Rate * 0.95f)
                            setIsFiring(hand, false);
                        return;
                    }
                }
            } else {
                if (!Input.GetKeyDown(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + hand]) {
                    if (Time.time - ((hand == 1) ? timeOfLastShotR : timeOfLastShotL) >= 0.1f)//0.1 < time of playing shoot animation once , to make sure other player catch this
                        setIsFiring(hand, false);
                    return;
                }
            }
            break;
            case (int)GeneralWeaponTypes.Melee:
            if (!Input.GetKeyDown(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + hand]) {
                setIsFiring(hand, false);
                return;
            }
            break;
            case (int)GeneralWeaponTypes.Shield:
            if (!Input.GetKey(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || getIsFiring((hand + 1) % 2)) {
                setIsFiring(hand, false);
                return;
            }
            break;
            case (int)GeneralWeaponTypes.Rocket:
            if (!Input.GetKeyDown(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset]) {
                if (Time.time - timeOfLastShotL >= 0.4f)//0.4 < time of playing shoot animation once , to make sure other player catch this
                    setIsFiring(hand, false);
                return;
            }
            break;
            case (int)GeneralWeaponTypes.Cannon:
            if (Time.time - timeOfLastShotL >= 0.5f)
                setIsFiring(hand, false);
            if (Input.GetKeyDown(KeyCode.Mouse1) || is_overheat[weaponOffset]) {//right click cancel BCNPose
                isBCNcanceled = true;
                animator.SetBool(AnimatorVars.BCNPose_id, false);
                return;
            } else if (Input.GetKey(KeyCode.Mouse0) && !isBCNcanceled && !animator.GetBool(AnimatorVars.BCNPose_id) && mechController.grounded && !animator.GetBool("BCNLoad")) {
                if (!is_overheat[weaponOffset]) {
                    if (!animator.GetBool(AnimatorVars.BCNPose_id)) {
                        AnimationEventController.BCNPose();
                        animator.SetBool(AnimatorVars.BCNPose_id, true);
                        timeOfLastShotL = Time.time - 1 / bm.weaponScripts[weaponOffset + hand].Rate / 2;
                    }
                } else {
                    animator.SetBool(AnimatorVars.BCNPose_id, false);
                }
            } else if (!Input.GetKey(KeyCode.Mouse0)) {
                isBCNcanceled = false;
            }
            break;
            case (int)GeneralWeaponTypes.Rectifier:
            if (!Input.GetKey(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + hand]) {
                if (Time.time - ((hand == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts[weaponOffset + hand].Rate)
                    setIsFiring(hand, false);
                return;
            }
            break;

            default: //Empty weapon
            return;
        }

        if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Ranged || curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Shield || curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Rectifier)
            if (curGeneralWeaponTypes[weaponOffset + (hand + 1) % 2] == (int)GeneralWeaponTypes.Melee && animator.GetBool("OnMelee"))
                return;

        if (isSwitchingWeapon) {
            return;
        }

        switch (curGeneralWeaponTypes[weaponOffset + hand]) {

            case (int)GeneralWeaponTypes.Ranged:
            if (Time.time - ((hand == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts[weaponOffset + hand].Rate) {
                setIsFiring(hand, true);
                FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, hand);
                if (hand == 1) {
                    HeatBar.IncreaseHeatBarR(weaponScripts[weaponOffset + hand].heat_increase_amount);
                    timeOfLastShotR = Time.time;
                } else {
                    HeatBar.IncreaseHeatBarL(weaponScripts[weaponOffset + hand].heat_increase_amount);
                    timeOfLastShotL = Time.time;
                }
            }
            break;
            case (int)GeneralWeaponTypes.Melee:
            if (Time.time - ((hand == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts[weaponOffset + hand].Rate) {
                if (!receiveNextSlash || !CanMeleeAttack) {
                    return;
                }

                if (curGeneralWeaponTypes[weaponOffset + (hand + 1) % 2] == (int)GeneralWeaponTypes.Shield && getIsFiring((hand + 1) % 2))
                    return;

                if ((animator.GetBool(AnimatorVars.slashL3_id) || animator.GetBool(AnimatorVars.slashR3_id)) && curSpecialWeaponTypes[(hand + 1) % 2 + weaponOffset] != (int)SpecialWeaponTypes.Sword)//if not both sword
                    return;

                if ((hand == 0 && isRMeleePlaying == 1) || (hand == 1 && isLMeleePlaying == 1))
                    return;

                CanMeleeAttack = false;
                receiveNextSlash = false;
                setIsFiring(hand, true);
                if (hand == 0) {
                    HeatBar.IncreaseHeatBarL(5);
                    timeOfLastShotL = Time.time;
                    if (curGeneralWeaponTypes[weaponOffset + 1] == (int)GeneralWeaponTypes.Melee)
                        timeOfLastShotR = Time.time;
                } else if (hand == 1) {
                    HeatBar.IncreaseHeatBarR(5);
                    timeOfLastShotR = Time.time;
                    if (curGeneralWeaponTypes[weaponOffset] == (int)GeneralWeaponTypes.Melee)
                        timeOfLastShotL = Time.time;
                }
            }
            break;
            case (int)GeneralWeaponTypes.Shield:
            if (!getIsFiring((hand + 1) % 2))
                setIsFiring(hand, true);
            break;
            case (int)GeneralWeaponTypes.Rocket:
            if (Time.time - timeOfLastShotL >= 1 / bm.weaponScripts[weaponOffset + hand].Rate) {
                setIsFiring(hand, true);
                HeatBar.IncreaseHeatBarL(25);

                FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, hand);
                timeOfLastShotL = Time.time;
            }
            break;
            case (int)GeneralWeaponTypes.Cannon:
            if (Time.time - timeOfLastShotL >= 1 / bm.weaponScripts[weaponOffset + hand].Rate && isOnBCNPose) {
                if (Input.GetKey(KeyCode.Mouse0) || !animator.GetBool(AnimatorVars.BCNPose_id) || !mechController.grounded)
                    return;

                BCNbulletNum--;
                if (BCNbulletNum <= 0)
                    animator.SetBool("BCNLoad", true);

                setIsFiring(hand, true);
                HeatBar.IncreaseHeatBarL(45);
                //**Start Position
                FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, hand);
                timeOfLastShotL = Time.time;
            }
            break;
            case (int)GeneralWeaponTypes.Rectifier:
            if (Time.time - ((hand == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts[weaponOffset + hand].Rate) {
                setIsFiring(hand, true);
                FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, hand);
                if (hand == 1) {
                    HeatBar.IncreaseHeatBarR(30);
                    timeOfLastShotR = Time.time;
                } else {
                    HeatBar.IncreaseHeatBarL(30);
                    timeOfLastShotL = Time.time;
                }
            }
            break;

        }
    }

    void handleAnimation(int hand) {
        // Name of animation, i.e. ShootR, SlashL, etc
        string animationStr = animationString(hand);

        if (getIsFiring(hand)) {
            // Tweaks
            switch (curGeneralWeaponTypes[weaponOffset + hand]) {
                case (int)GeneralWeaponTypes.Melee:
                if (curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.Sword)//sword                    
                    AnimationEventController.Slash(hand);
                else//spear
                    AnimationEventController.Smash(hand);
                break;
                case (int)GeneralWeaponTypes.Shield:
                animator.SetBool(animationStr, true);
                break;
                case (int)GeneralWeaponTypes.Cannon:
                animator.SetBool(animationStr, true);
                break;
            }
        } else {// melee is set to false by animation
            if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Shield)
                animator.SetBool(animationStr, false);
            else if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Cannon)
                animator.SetBool("BCNShoot", false);
        }
    }

    void handleSkillInput() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SkillController.CallUseSkill(0);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SkillController.CallUseSkill(1);
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            SkillController.CallUseSkill(2);
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            SkillController.CallUseSkill(3);
        }
    }

    void updateHUD() {
        // Update Health bar gradually
        healthBar.value = calculateSliderPercent(healthBar.value, currentHP / (float)MAX_HP);
        healthtext.text = BarValueToString(currentHP, MAX_HP);
        // Update Fuel bar gradually
        fuelBar.value = calculateSliderPercent(fuelBar.value, currentFuel / (float)MAX_FUEL);
        fueltext.text = BarValueToString((int)currentFuel, (int)MAX_FUEL);
    }

    // Returns currentPercent + 0.01 if currentPercent < targetPercent, else - 0.01
    float calculateSliderPercent(float currentPercent, float targetPercent) {
        float err = 0.005f;
        if (Mathf.Abs(currentPercent - targetPercent) > err) {
            currentPercent = currentPercent + (currentPercent > targetPercent ? -0.005f : 0.005f);
        } else {
            currentPercent = targetPercent;
        }
        return currentPercent;
    }

    [PunRPC]
    void CallSwitchWeapons() {
        //Play switch weapon animation
        EffectController.SwitchWeaponEffect();
        isSwitchingWeapon = true;
        SwitchWeaponcoroutine = StartCoroutine(SwitchWeaponsBegin());
    }

    IEnumerator SwitchWeaponsBegin() {
        yield return new WaitForSeconds(1);
        if (isDead) {
            SwitchWeaponcoroutine = null;
            isSwitchingWeapon = false;
            yield break;
        }

        // Stop current attacks
        setIsFiring(LEFT_HAND, false);
        setIsFiring(RIGHT_HAND, false);

        StopCurWeaponAnimations();

        // Switch weapons by toggling each weapon's activeSelf
        ActivateWeapons();

        weaponOffset = (weaponOffset + 2) % 4;
        if(OnWeaponSwitched!=null)OnWeaponSwitched();
        UpdateMovementClips();
        if (photonView.isMine) SetWeaponOffsetProperty(weaponOffset);

        SwitchWeaponcoroutine = null;
        isSwitchingWeapon = false;
    }

    void SetWeaponOffsetProperty(int weaponOffset) {
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("weaponOffset", weaponOffset);
        photonView.owner.SetCustomProperties(h);
    }

    void StopCurWeaponAnimations() {
        animator.SetBool("BCNPose", false);

        string strL = animationString(LEFT_HAND), strR = animationString(RIGHT_HAND);
        if (strL != "" && curSpecialWeaponTypes[weaponOffset] != (int)GeneralWeaponTypes.Melee) { // not empty weapon or melee
            animator.SetBool(strL, false);
        }
        if (strR != "" && curSpecialWeaponTypes[weaponOffset + 1] != (int)GeneralWeaponTypes.Melee) {
            animator.SetBool(strR, false);
        }
    }

    void ActivateWeapons() {
        for (int i = 0; i < weapons.Length; i++) {
            if(weapons[i]!=null)
                weapons[i].SetActive(!weapons[i].activeSelf);
        }
    }


    public void UpdateArmAnimatorState() {
        for (int i = 0; i < 2; i++) {
            switch (curSpecialWeaponTypes[weaponOffset + i]) {
                case (int)SpecialWeaponTypes.APS:
                animator.Play("APS", 1 + i);//left hand is layer 1
                break;
                case (int)SpecialWeaponTypes.Rifle:
                animator.Play("BRF", 1 + i);
                break;
                case (int)SpecialWeaponTypes.Rectifier:
                animator.Play("ENG", 1 + i);
                break;
                case (int)SpecialWeaponTypes.LMG:
                animator.Play("LMG", 1 + i);
                break;
                case (int)SpecialWeaponTypes.Rocket:
                animator.Play("RCL", 1);
                animator.Play("RCL", 2);
                i++;
                break;
                case (int)SpecialWeaponTypes.Shotgun:
                animator.Play("SGN", 1 + i);
                break;
                case (int)SpecialWeaponTypes.Shield:
                animator.Play("SHS", 1 + i);
                break;
                case (int)SpecialWeaponTypes.Cannon:
                animator.Play("BCN", 1);
                animator.Play("BCN", 2);
                i++;
                break;
                default:
                animator.Play("Idle", 1 + i);
                break;
            }
        }
    }

    public void UpdateMovementClips() {
        int num = (weaponScripts[weaponOffset] == null || !weaponScripts[weaponOffset].twoHanded) ? 0 : 1;

        clipOverrides["Idle"] = MovementClips.Idle[num];
        clipOverrides["Run_Left"] = MovementClips.Run_Left[num];
        clipOverrides["Run_Front"] = MovementClips.Run_Front[num]; ;
        clipOverrides["Run_Right"] = MovementClips.Run_Right[num];
        clipOverrides["BackWalk"] = MovementClips.BackWalk[num];
        clipOverrides["BackWalk_Left"] = MovementClips.BackWalk_Left[num];
        clipOverrides["BackWalk_Right"] = MovementClips.BackWalk_Right[num];

        clipOverrides["Hover_Back_01"] = MovementClips.Hover_Back_01[num];
        clipOverrides["Hover_Back_02"] = MovementClips.Hover_Back_02[num];
        clipOverrides["Hover_Back_03"] = MovementClips.Hover_Back_03[num];
        clipOverrides["Hover_Back_01_Left"] = MovementClips.Hover_Back_01_Left[num];
        clipOverrides["Hover_Back_02_Left"] = MovementClips.Hover_Back_02_Left[num];
        clipOverrides["Hover_Back_03_Left"] = MovementClips.Hover_Back_03_Left[num];
        clipOverrides["Hover_Back_01_Right"] = MovementClips.Hover_Back_01_Right[num];
        clipOverrides["Hover_Back_02_Right"] = MovementClips.Hover_Back_02_Right[num];
        clipOverrides["Hover_Back_03_Right"] = MovementClips.Hover_Back_03_Right[num];

        clipOverrides["Hover_Left_01"] = MovementClips.Hover_Left_01[num];
        clipOverrides["Hover_Left_02"] = MovementClips.Hover_Left_02[num];
        clipOverrides["Hover_Left_03"] = MovementClips.Hover_Left_03[num];
        clipOverrides["Hover_Right_01"] = MovementClips.Hover_Right_01[num];
        clipOverrides["Hover_Right_02"] = MovementClips.Hover_Right_02[num];
        clipOverrides["Hover_Right_03"] = MovementClips.Hover_Right_03[num];
        clipOverrides["Hover_Front_01"] = MovementClips.Hover_Front_01[num];
        clipOverrides["Hover_Front_02"] = MovementClips.Hover_Front_02[num];
        clipOverrides["Hover_Front_03"] = MovementClips.Hover_Front_03[num];

        clipOverrides["Jump01"] = MovementClips.Jump01[num];
        clipOverrides["Jump01_Left"] = MovementClips.Jump01_Left[num];
        clipOverrides["Jump01_Right"] = MovementClips.Jump01_Right[num];
        clipOverrides["Jump01_b"] = MovementClips.Jump01_b[num];
        clipOverrides["Jump01_Left_b"] = MovementClips.Jump01_Left_b[num];
        clipOverrides["Jump01_Right_b"] = MovementClips.Jump01_Right_b[num];
        clipOverrides["Jump02"] = MovementClips.Jump02[num];
        clipOverrides["Jump02_Left"] = MovementClips.Jump02_Left[num];
        clipOverrides["Jump02_Right"] = MovementClips.Jump02_Right[num];
        clipOverrides["Jump03"] = MovementClips.Jump03[num];
        clipOverrides["Jump03_Left"] = MovementClips.Jump03_Left[num];
        clipOverrides["Jump03_Right"] = MovementClips.Jump03_Right[num];
        clipOverrides["Jump06"] = MovementClips.Jump06[num];
        clipOverrides["Jump06_Left"] = MovementClips.Jump06_Left[num];
        clipOverrides["Jump06_Right"] = MovementClips.Jump06_Right[num];
        clipOverrides["Jump07"] = MovementClips.Jump07[num];
        clipOverrides["Jump07_Left"] = MovementClips.Jump07_Left[num];
        clipOverrides["Jump07_Right"] = MovementClips.Jump07_Right[num];
        clipOverrides["Jump08"] = MovementClips.Jump08[num];
        clipOverrides["Jump08_Left"] = MovementClips.Jump08_Left[num];
        clipOverrides["Jump08_Right"] = MovementClips.Jump08_Right[num];
        
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    bool getIsFiring(int hand) {
        return hand == LEFT_HAND ? fireL : fireR;
    }

    void setIsFiring(int hand, bool isFiring) {
        if (hand == LEFT_HAND) {
            fireL = isFiring;
        } else {
            fireR = isFiring;
        }
    }

    public void UpdateCurWeaponType() {
        UpdateSpecialCurWeaponType();
        UpdateGeneralCurWeaponType();
    }

    void UpdateSpecialCurWeaponType() {//those types all use different animations
        for (int i = 0; i < 4; i++) {
            if (weaponScripts[i] == null) continue;
            switch (weaponScripts[i].weaponType) {
                case "APS":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.APS;
                break;
                case "LMG":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.LMG;
                break;
                case "Rifle":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.Rifle;
                break;
                case "Shotgun":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.Shotgun;
                break;
                case "Rectifier":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.Rectifier;
                break;
                case "Rocket":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.Rocket;
                break;
                case "Spear":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.Spear;
                break;
                case "Sword":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.Sword;
                break;
                case "Shield":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.Shield;
                break;
                case "Cannon":
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.Cannon;
                break;
                default:
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.EMPTY;
                break;
            }
        }
    }

    void UpdateGeneralCurWeaponType() {
        for (int i = 0; i < 4; i++) {
            switch (curSpecialWeaponTypes[i]) {
                case (int)SpecialWeaponTypes.APS:
                case (int)SpecialWeaponTypes.Rifle:
                case (int)SpecialWeaponTypes.Shotgun:
                case (int)SpecialWeaponTypes.LMG:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.Ranged;
                break;
                case (int)SpecialWeaponTypes.Spear:
                case (int)SpecialWeaponTypes.Sword:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.Melee;
                break;
                case (int)SpecialWeaponTypes.Shield:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.Shield;
                break;
                case (int)SpecialWeaponTypes.Rocket:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.Rocket;
                break;
                case (int)SpecialWeaponTypes.Rectifier:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.Rectifier;
                break;
                case (int)SpecialWeaponTypes.Cannon:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.Cannon;
                break;
                case (int)SpecialWeaponTypes.EMPTY:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.Empty;
                break;
            }
        }
    }

    string animationString(int hand) {
        switch (curGeneralWeaponTypes[weaponOffset + hand]) {
            case (int)GeneralWeaponTypes.Rocket:
            return "RCLShoot";
            case (int)GeneralWeaponTypes.Cannon:
            return "BCNShoot";
            case (int)GeneralWeaponTypes.Shield:
            return "Block" + ((hand == 0) ? "L" : "R");
            default:
            return "";
        }
    }

    public void EnableAllRenderers(bool b) {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = b;
        }
    }

    public void EnableAllColliders(bool b) {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders) {
            if (!b) {
                if (collider.gameObject != slashDetector.gameObject)
                    collider.gameObject.layer = default_layer;
            } else if (collider.gameObject != slashDetector.gameObject)
                collider.gameObject.layer = playerlayer;
        }
    }
    // Public functions
    public void IncrementFuel() {
        currentFuel += fuelGain * Time.fixedDeltaTime;
        if (currentFuel > MAX_FUEL) currentFuel = MAX_FUEL;
    }

    public void DecrementFuel() {
        currentFuel -= fuelDrain * Time.fixedDeltaTime;
        if (currentFuel < 0)
            currentFuel = 0;
    }

    public bool EnoughFuelToBoost() {
        if (currentFuel >= minFuelRequired) {
            isFuelAvailable = true;
            return true;
        } else {//false -> play effect if not already playing
            if (!isNotEnoughEffectPlaying) {
                StartCoroutine(FuelNotEnoughEffect());
            }
            if (!animator.GetBool("Boost"))//can set to false in transition to grounded state but not in transition from grounded state to boost state 
                isFuelAvailable = false;
            return false;
        }
    }

    IEnumerator FuelNotEnoughEffect() {
        isNotEnoughEffectPlaying = true;
        for (int i = 0; i < 4; i++) {
            fuelBar_fill.color = new Color32(133, 133, 133, 255);
            yield return new WaitForSeconds(0.15f);
            fuelBar_fill.color = new Color32(255, 255, 255, 255);
            yield return new WaitForSeconds(0.15f);
        }
        isNotEnoughEffectPlaying = false;
    }

    public bool IsFuelAvailable() {
        if (FuelEmpty()) {
            isFuelAvailable = false;
        }
        return isFuelAvailable;
    }
    public bool FuelEmpty() {
        return currentFuel <= 0;
    }

    public float MoveSpeed() {
        return moveSpeed;
    }

    public float MinHorizontalBoostSpeed() {
        return minBoostSpeed;
    }

    public float JumpPower() {
        return jumpPower;
    }

    public float MaxHorizontalBoostSpeed() {
        return maxHorizontalBoostSpeed;
    }

    public float MaxVerticalBoostSpeed() {
        return maxVerticalBoostSpeed;
    }

    public bool IsHpFull() {
        return (currentHP >= MAX_HP);
    }

    public bool IsSwitchingWeapon() {
        return isSwitchingWeapon;
    }

    private void OnSkill(bool b) {
        onSkill = b;
    }

    void UpdateSMGAnimationSpeed() {
        if (weaponScripts[weaponOffset] != null)
            if (curSpecialWeaponTypes[weaponOffset] == (int)SpecialWeaponTypes.APS) {//APS animation clip length 1.066s
                animator.SetFloat("rateL", (((SMG)weaponScripts[weaponOffset]).Rate) *1.066f);
            } else if(curSpecialWeaponTypes[weaponOffset] == (int)SpecialWeaponTypes.LMG){//LMG animation clip length 0.8s
                animator.SetFloat("rateL", (((SMG)weaponScripts[weaponOffset]).Rate)*0.8f);
            }

        if(weaponScripts[weaponOffset + 1]!=null)
            if (curSpecialWeaponTypes[weaponOffset+1] == (int)SpecialWeaponTypes.APS) {
                animator.SetFloat("rateR",(((SMG)weaponScripts[weaponOffset+1]).Rate) * 1.066f);
            } else if (curSpecialWeaponTypes[weaponOffset+1] == (int)SpecialWeaponTypes.LMG) {
                animator.SetFloat("rateR", (((SMG)weaponScripts[weaponOffset+1]).Rate) * 0.8f);
            }
    }

    void UpdateSlashAnimationThreshold() {
        if (curSpecialWeaponTypes[weaponOffset] == (int)SpecialWeaponTypes.Sword)
            slashL_threshold = ((Sword)weaponScripts[weaponOffset]).threshold;
        if (curSpecialWeaponTypes[weaponOffset+1] == (int)SpecialWeaponTypes.Sword)
            slashR_threshold = ((Sword)weaponScripts[weaponOffset+1]).threshold;
    }

    public void SetLMeleePlaying(int isPlaying) {
        isLMeleePlaying = isPlaying;
    }

    public void SetRMeleePlaying(int isPlaying) {// this is true when RSlash is playing ( slashR1 , ... )
        isRMeleePlaying = isPlaying;
    }

    public void SetReceiveNextSlash(int receive) { // this is called in the animation clip
        receiveNextSlash = (receive == 1) ? true : false;
    }

    public void FindTrail() {
        if (curGeneralWeaponTypes[weaponOffset] == (int)GeneralWeaponTypes.Melee) {
            trailL = weapons[weaponOffset].transform.Find("trail").GetComponent<XWeaponTrail>();
            if (trailL != null) {
                trailL.Deactivate();
            }
        } else {
            trailL = null;
        }

        if (curGeneralWeaponTypes[weaponOffset + 1] == (int)GeneralWeaponTypes.Melee) {
            trailR = weapons[weaponOffset + 1].transform.Find("trail").GetComponent<XWeaponTrail>();
            if (trailR != null)
                trailR.Deactivate();
        } else {
            trailR = null;
        }
    }

    public void ShowTrailL(bool show) {
        if (trailL != null) {
            if (show) {
                trailL.Activate();
            } else {
                trailL.StopSmoothly(0.1f);
            }
        }
    }
    public void ShowTrailR(bool show) {
        if (trailR != null) {
            if (show) {
                trailR.Activate();
            } else {
                trailR.StopSmoothly(0.1f);
            }
        }
    }

    private string BarValueToString(int curvalue, int maxvalue) {
        string curvalueStr = curvalue.ToString();
        string maxvalueStr = maxvalue.ToString();

        string finalStr = string.Empty;
        for (int i = 0; i < 4 - curvalueStr.Length; i++) {
            finalStr += "0 ";
        }

        for (int i = 0; i < curvalueStr.Length; i++) {
            finalStr += (curvalueStr[i] + " ");

        }
        finalStr += "/ ";
        for (int i = 0; i < 3; i++) {
            finalStr += (maxvalueStr[i] + " ");
        }
        finalStr += maxvalueStr[3];

        return finalStr;
    }


    public int GetCurrentWeaponOffset() {
        return weaponOffset;
    }

    //	public void BulletTraceEvent() {
    //		photonView.RPC("BulletTraceRPC", PhotonTargets.All);
    //	}
    //
    //	[PunRPC]
    //	void BulletTraceRPC() {
    //		Camera cam = transform.FindChild("Camera").gameObject.GetComponent<Camera>();
    //		Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
    //		Vector3 diff = worldPoint - Hands[0].position;
    //		GameObject bulletTraceClone = Instantiate(bulletTrace, Hands[0].position, new Quaternion(diff.x, diff.y, diff.z, 1.0f)) as GameObject;
    //	}
    //
    //	[Server]
    //	public void RegisterKill(uint shooterId, uint victimId){
    //		int kills = gm.playerScores[shooterId].IncrKill();
    //		int deaths = gm.playerScores[victimId].IncrDeaths();
    //		Debug.Log(string.Format("Server: Registering a kill {0}, {1}, {2}, {3} ", shooterId, victimId, kills, deaths));
    //		if (isServer) {
    //			RpcUpdateScore(shooterId, victimId, kills, deaths);
    //		}
    //	}
    //
    //	[Server]
    //	public void EndGame() {
    //		Debug.Log("Ending game");
    //		RpcDisablePlayer();
    //		foreach (KeyValuePair<GameObject, Data> entry in gm.playerInfo) {
    //			entry.Key.GetComponent<MechCombat>().RpcDisablePlayer();
    //		}
    //	}
    //		
    //	[ClientRpc]
    //	void RpcUpdateScore(uint shooterId, uint victimId, int kills, int deaths){
    //		Debug.Log( string.Format("Client: Updating score {0}, {1}, {2}, {3} ", shooterId, victimId, kills, deaths)); 
    //		Score shooterScore = gm.playerScores[shooterId];
    //		Score victimScore = gm.playerScores[victimId];
    //		Score newShooterScore = new Score();
    //		Score newVictimScore = new Score();
    //		newShooterScore.Kills = kills;
    //		newShooterScore.Deaths = shooterScore.Deaths;
    //		newVictimScore.Deaths = deaths;
    //		newVictimScore.Kills = victimScore.Kills;
    //
    //		if (kills > gm.CurrentMaxKills) gm.CurrentMaxKills = kills;
    //		gm.playerScores[shooterId] = newShooterScore;
    //		gm.playerScores[victimId] = newVictimScore;
    //	}
}
