using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XftWeapon;

public class MechCombat : Combat {
    [SerializeField] private MechController MechController;
    [SerializeField] private HeatBar HeatBar;
    [SerializeField] private DisplayPlayerInfo displayPlayerInfo;
    [SerializeField] private CrosshairImage crosshairImage;
    [SerializeField] private EffectController EffectController;
    [SerializeField] private LayerMask playerlayerMask;
    [SerializeField] private Camera cam;
    [SerializeField] private BuildMech bm;
    [SerializeField] private Animator animator;
    [SerializeField] private MovementClips defaultMovementClips, TwoHandedMovementClips;
    [SerializeField] private SkillController SkillController;
    private InRoomChat InRoomChat;

    private string[] SpecialWeaponTypeStrs = new string[11] { "APS", "LMG", "Rifle", "Shotgun", "Rectifier", "Sword", "Spear", "Shield", "Rocket", "Cannon", "EMPTY" };
    private enum GeneralWeaponTypes { Ranged, Rectifier, Melee, Shield, Rocket, Cannon, Empty };//for efficiency
    private enum SpecialWeaponTypes { APS, LMG, Rifle, Shotgun, Rectifier, Sword, Spear, Shield, Rocket, Cannon, EMPTY };//These types all use different animations

    // EN
    [SerializeField] private EnergyProperties energyProperties = new EnergyProperties();
    private float currentEN;

    // Game variables
    public Score score;
    private const int playerlayer = 8, default_layer = 0;

    // Combat variables
    public bool isDead;
    public bool[] is_overheat = new bool[4]; // this is handled by HeatBar.cs , but other player also need to access it (shield)
    public int scanRange, MechSize, TotalWeight;//TODO : implement scanRange & MechSize
    public int BCNbulletNum = 2;
    public bool isOnBCNPose, onSkill = false;//called by BCNPoseState to check if on the right pose
    private bool on_BCNShoot = false;
    public bool On_BCNShoot {
        get { return on_BCNShoot;}
        set {
            on_BCNShoot = value;
            MechController.onInstantMoving = value;
            if(value)BCNbulletNum--;
            if (BCNbulletNum <= 0) {
                animator.Play("BCN", 1);
                animator.Play("BCN", 2);
                animator.SetBool("BCNLoad", true);
            }
        }
    }

    private int weaponOffset = 0;
    private int[] curGeneralWeaponTypes = new int[4];//ranged , melee , ...
    private int[] curSpecialWeaponTypes = new int[4];//APS , BRF , ...
    private bool isBCNcanceled = false;//check if right click cancel

    // Left
    private const int LEFT_HAND = 0;
    private float timeOfLastShotL;
    private bool fireL = false;
    public bool isLMeleePlaying { get; private set;}

    // Right
    private const int RIGHT_HAND = 1;
    private float timeOfLastShotR;
    private bool fireR = false;
    public bool isRMeleePlaying { get;private set;}

    public bool CanMeleeAttack = true;

    public bool IsSwitchingWeapon { get ; private set; }

    private bool receiveNextSlash = true;
    private const int slashMaxDistance = 30;//the ray which checks if hitting shield max distance
    public float slashL_threshold, slashR_threshold;

    [HideInInspector] public Transform[] Hands;//other player use this to locate hand position quickly
    private Transform shoulderL;
    private Transform shoulderR;
    private Transform head;
    private Transform camTransform;
    private Transform[] Effect_Ends = new Transform[4];

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
    private Slider ENBar;
    private Image ENBar_fill;
    private bool ENNotEnoughEffectIsPlaying = false;
    private bool isENAvailable = true;
    private Text healthtext, ENtext;

    // Components
    private Crosshair crosshair;
    private SlashDetector slashDetector;    
    private Sounds Sounds;
    private AnimationEventController AnimationEventController;
    private ParticleSystem[] Muz = new ParticleSystem[4];
    private XWeaponTrail trailL, trailR;

    //Animator
    private AnimatorVars AnimatorVars;
    private AnimatorOverrideController animatorOverrideController = null;
    private AnimationClipOverrides clipOverrides;

    //for Debug
    public bool forceDead = false;

    public delegate void WeaponSwitchedAction();
    public WeaponSwitchedAction OnWeaponSwitched;
    private Coroutine SwitchWeaponcoroutine;

    private void Awake() {
        InitAnimatorControllers();
        RegisterOnWeaponSwitched();
        RegisterOnWeaponBuilt();
        RegisterOnSkill();
    }

    private void Start() {
        findGameManager();
        initMechStats();
        initComponents();
        initCombatVariables();
        initTransforms();
        initGameObjects();
        initTargetProperties();
        initSlashDetector();

        SyncWeaponOffset();//TODO : check this
        initHUD();
    }

    private void RegisterOnWeaponSwitched() {
        OnWeaponSwitched += UpdateSlashAnimationThreshold;
        OnWeaponSwitched += UpdateSMGAnimationSpeed;
        OnWeaponSwitched += ResetArmAnimatorState;
        OnWeaponSwitched += FindTrail;
        OnWeaponSwitched += UpdateSlashDetector;
        OnWeaponSwitched += UpdateWeightRelatedVars;

        OnWeaponSwitched += UpdateMovementClips;
    }

    private void RegisterOnWeaponBuilt() {
        bm.OnMechBuilt += InitWeapons;
        //bm.OnMechBuilt += UpdateMovementClips;
    }

    private void RegisterOnSkill() {
        if (SkillController != null) SkillController.OnSkill += OnSkill;
    }

    public void LoadMechProperties() {
        MAX_HP = bm.MechProperty.HP;
        MAX_EN = bm.MechProperty.EN;
        MechSize = bm.MechProperty.Size;
        TotalWeight = bm.MechProperty.Weight;
        energyProperties.minENRequired = bm.MechProperty.MinENRequired;
        energyProperties.energyOutput = bm.MechProperty.ENOutputRate - bm.MechProperty.EnergyDrain;

        scanRange = bm.MechProperty.ScanRange;
        
        
    }

    private void UpdateWeightRelatedVars() {
        TotalWeight = bm.MechProperty.Weight + ((weaponScripts[weaponOffset]==null)?0 : weaponScripts[weaponOffset].weight) + ((weaponScripts[weaponOffset+1] == null) ? 0 : weaponScripts[weaponOffset+1].weight);

        energyProperties.jumpENDrain = bm.MechProperty.GetJumpENDrain(TotalWeight);
        energyProperties.dashENDrain = bm.MechProperty.DashENDrain;

        MechController.UpdateWeightRelatedVars(TotalWeight);
    }

    private void initMechStats() {//call also when respawn
        CurrentHP = MAX_HP;
        currentEN = MAX_EN;
    }

    private void initTransforms() {
        camTransform = cam.transform;
        head = transform.Find("CurrentMech/metarig/hips/spine/chest/fakeNeck/head");
        shoulderL = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.L");
        shoulderR = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.R");

        Hands = new Transform[2];
        Hands[0] = shoulderL.Find("upper_arm.L/forearm.L/hand.L");
        Hands[1] = shoulderR.Find("upper_arm.R/forearm.R/hand.R");
    }

    private void initGameObjects() {
        BulletCollector = GameObject.Find("BulletCollector");
        GameObject g = GameObject.Find("InRoomChat");
        if (g != null)
            InRoomChat = g.GetComponent<InRoomChat>();
    }

    private void initTargetProperties() {
        Targets = new GameObject[2];
        isTargetShield = new bool[2];
        target_HandOnShield = new int[2];
        bullet_directions = new Vector3[2];
    }

    private void initComponents() {
        Transform currentMech = transform.Find("CurrentMech");
        if (currentMech == null) {
            Debug.Log("Can't find currentMech");
            return;
        }
        Sounds = currentMech.GetComponent<Sounds>();
        AnimatorVars = currentMech.GetComponent<AnimatorVars>();
        AnimationEventController = currentMech.GetComponent<AnimationEventController>();
        animator = currentMech.GetComponent<Animator>();
        crosshair = cam.GetComponent<Crosshair>();
    }

    private void InitAnimatorControllers() {
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
    }

    private void InitWeapons() {
        weaponOffset = 0;
        weapons = bm.weapons;
        bullets = bm.bulletPrefabs;
        weaponScripts = bm.weaponScripts;

        UpdateCurWeaponType();
        FindEffectEnd();
        FindMuz();
    }

    public void initCombatVariables() {// this will be called also when respawn
        weaponOffset = 0;
        if (photonView.isMine) SetWeaponOffsetProperty(weaponOffset);
        if (SwitchWeaponcoroutine != null) {//die when switching weapons
            StopCoroutine(SwitchWeaponcoroutine);
            IsSwitchingWeapon = false;
            SwitchWeaponcoroutine = null;
        }
        onSkill = false;
        isRMeleePlaying = false;
        isLMeleePlaying = false;
        setIsFiring(0, false);
        setIsFiring(1, false);

        animator.SetBool("BCNLoad", false);
        BCNbulletNum = 2;
    }

    private void initHUD() {
        if (!photonView.isMine)
            return;
        initHealthAndENBars();//other player should not call this ( they share hud )
    }

    private void FindEffectEnd() {
        for (int i = 0; i < 4; i++) {
            if (weapons[i] != null) {
                Effect_Ends[i] = TransformDeepChildExtension.FindDeepChild(weapons[i].transform, "EffectEnd");
            }
        }
    }

    public void FindMuz() {
        for (int i = 0; i < 4; i++) {
            if (Effect_Ends[i] != null) {
                Transform MuzTransform = Effect_Ends[i].transform.Find("Muz");
                if (MuzTransform != null) {
                    Muz[i] = MuzTransform.GetComponent<ParticleSystem>();
                }
            }
        }
    }

    private float TimeOfLastShot(int hand) {
        return hand == LEFT_HAND ? timeOfLastShotL : timeOfLastShotR;
    }

    private void initHealthAndENBars() {
        Slider[] sliders = GameObject.Find("PanelCanvas").GetComponentsInChildren<Slider>();
        if (sliders.Length > 0) {
            healthBar = sliders[0];
            healthBar.value = 1;
            healthtext = healthBar.GetComponentInChildren<Text>();
            if (sliders.Length > 1) {
                ENBar = sliders[1];
                ENBar_fill = ENBar.transform.Find("Fill Area/Fill").GetComponent<Image>();
                ENBar.value = 1;
                ENtext = ENBar.GetComponentInChildren<Text>();
            }
        }
    }

    private void initSlashDetector() {
        slashDetector = GetComponentInChildren<SlashDetector>();
    }

    private void UpdateSlashDetector() {
        if (slashDetector == null)
            initSlashDetector();

        bool b = ((curGeneralWeaponTypes[weaponOffset] == (int)GeneralWeaponTypes.Melee || curGeneralWeaponTypes[weaponOffset + 1] == (int)GeneralWeaponTypes.Melee) && photonView.isMine);
        slashDetector.EnableDetector(b);
    }

    private void SyncWeaponOffset() {
        //sync other player weapon offset
        if (photonView.owner != null && !photonView.isMine) {
            if (photonView.owner.CustomProperties["weaponOffset"] != null) {
                weaponOffset = int.Parse(photonView.owner.CustomProperties["weaponOffset"].ToString());
            } else//the player may just initialize
                weaponOffset = 0;

            for (int i = 0; i < 4; i++) {
                int num = (weaponOffset + i) % 4;
                if (weapons[num] != null) weapons[num].SetActive(num == weaponOffset || num == weaponOffset + 1);
            }
        }
    }

    private void FireRaycast(Vector3 start, Vector3 direction, int hand) {
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

                    if (target.gameObject.GetComponent<Combat>().CurrentHP <= 0) {
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

            //increase SP
            SkillController.IncreaseSP(weaponScripts[weaponOffset + hand].SPincreaseAmount);
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

                if (target.GetComponent<Combat>().CurrentHP <= 0) {
                    target.GetComponent<HUD>().DisplayKill(cam);
                } else {
                    if (isHitShield)
                        target.GetComponent<HUD>().DisplayDefense(cam);
                    else
                        target.GetComponent<HUD>().DisplayHit(cam);
                }

                //increase SP
                SkillController.IncreaseSP(weaponScripts[weaponOffset + hand].SPincreaseAmount);
            }
        }
    }

    [PunRPC]
    private void Shoot(int hand, Vector3 direction, int playerPVid, bool isShield, int target_handOnShield) {
        SetTargetInfo(hand, direction, playerPVid, isShield, target_handOnShield);
        string clipName = "";

        switch (curSpecialWeaponTypes[weaponOffset + hand]) {
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

    private void SetTargetInfo(int hand, Vector3 direction, int playerPVid, bool isShield, int target_handOnShield) {
        if (playerPVid != -1) {
            Targets[hand] = PhotonView.Find(playerPVid).gameObject;
            isTargetShield[hand] = isShield;
            target_HandOnShield[hand] = target_handOnShield;
            bullet_directions[hand] = direction;
        } else {
            Targets[hand] = null;
            bullet_directions[hand] = direction;
        }
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

        if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Rocket) {
            if (photonView.isMine) {
                GameObject bullet = PhotonNetwork.Instantiate(bm.weaponScripts[weaponOffset].weaponPrefab.name + "B", transform.position + new Vector3(0, 5, 0) + transform.forward * 10, Quaternion.LookRotation(bullet_directions[hand]), 0);
                RCLBulletTrace bulletTrace = bullet.GetComponent<RCLBulletTrace>();
                bulletTrace.SetShooterInfo(gameObject, cam);
                bulletTrace.SetBulletPropertis(weaponScripts[weaponOffset].damage, ((Rocket)weaponScripts[weaponOffset]).bullet_speed, ((Rocket)weaponScripts[weaponOffset]).impact_radius);
                bulletTrace.SetSPIncreaseAmount(bm.weaponScripts[weaponOffset + hand].SPincreaseAmount);
            }
        } else if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Rectifier) {
            GameObject bullet = Instantiate(bullets[weaponOffset + hand], Effect_Ends[weaponOffset + hand].position, Quaternion.LookRotation(bullet_directions[hand])) as GameObject;
            ElectricBolt eb = bullet.GetComponent<ElectricBolt>();
            bullet.transform.SetParent(Effect_Ends[weaponOffset + hand]);
            bullet.transform.localPosition = Vector3.zero;
            eb.SetCamera(cam);
            eb.SetTarget((Targets[hand] == null) ? null : Targets[hand].transform);
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
                                mcbt.GetComponent<HUD>().DisplayHit(cam);
                            else
                                Targets[hand].transform.root.GetComponent<HUD>().DisplayHit(cam);
                        } else {
                            if (mcbt != null)
                                mcbt.GetComponent<HUD>().DisplayDefense(cam);
                            else
                                Targets[hand].transform.root.GetComponent<HUD>().DisplayDefense(cam);
                        }
                    }
                }
            }

            GameObject bullet = Instantiate(b, Effect_Ends[weaponOffset + hand].position, Quaternion.identity, BulletCollector.transform) as GameObject;
            BulletTrace bulletTrace = bullet.GetComponent<BulletTrace>();
            bulletTrace.SetStartDirection(cam.transform.forward);

            if (curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.Cannon)
                bulletTrace.interactWithTerrainWhenOnTarget = false;

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

    private bool CheckTargetIsDead(GameObject target) {
        MechCombat mcbt = target.transform.root.GetComponent<MechCombat>();
        if (mcbt == null)//Drone
        {
            return target.transform.root.GetComponent<DroneCombat>().CurrentHP <= 0;
        } else {
            return mcbt.CurrentHP <= 0;
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
        } else if (CheckIsSpearByStr(weapon)) {
            EffectController.SlashOnHitEffect(false, 0);
        }

        if (photonView.isMine) {
            if (isSlowDown)
                MechController.SlowDown();

            SkillController.IncreaseSP((int)damage / 2);
        }

        CurrentHP -= damage;

        if (CurrentHP <= 0) {
            isDead = true;

            DisablePlayer();

            // Update scoreboard
            gm.RegisterKill(shooter_viewID, photonView.viewID);
            PhotonView shooterpv = PhotonView.Find(shooter_viewID);
            DisplayKillMsg(shooterpv.owner.NickName, photonView.name, weapon);
        }
    }

    [PunRPC]
    private void ShieldOnHit(int damage, int shooter_viewID, int shield, string weapon) {
        if (isDead) {
            return;
        }

        if (CheckIsSwordByStr(weapon)) {
            EffectController.SlashOnHitEffect(true, shield);
        } else if (CheckIsSpearByStr(weapon)) {
            EffectController.SlashOnHitEffect(true, shield);
        }

        CurrentHP -= damage;

        if (CurrentHP <= 0) {
            DisablePlayer();//TODO : sync this

            // Update scoreboard
            gm.RegisterKill(shooter_viewID, photonView.viewID);
            PhotonView shooterpv = PhotonView.Find(shooter_viewID);
            DisplayKillMsg(shooterpv.owner.NickName, photonView.name, weapon);
        }

        if (photonView.isMine) {//heat
            if (!is_overheat[weaponOffset + shield]) {
                if (shield == 0) {
                    HeatBar.IncreaseHeatBarL(30);
                } else {
                    HeatBar.IncreaseHeatBarR(30);
                }
            }

            SkillController.IncreaseSP((int)damage / 2);
        }
    }
    private bool CheckIsSwordByStr(string name) {
        return name.Contains("SHL");
    }
    private bool CheckIsSpearByStr(string name) {
        return name.Contains("ADR");
    }

    private void DisplayKillMsg(string shooter, string target, string weapon) {
        InRoomChat.AddLine(shooter + " killed " + photonView.name + " by " + weapon);
    }

    [PunRPC]
    private void OnHeal(int viewID, int amount) {
        if (isDead) {
            return;
        }

        if (CurrentHP + amount >= MAX_HP) {
            CurrentHP = MAX_HP;
        } else {
            CurrentHP += amount;
        }
    }

    [PunRPC]
    private void OnLocked(string name) {//TODO : remake this check ( use event )
        if (PhotonNetwork.playerName != name)
            return;
        crosshair.ShowLocked();
    }

    [PunRPC]
    public void KnockBack(Vector3 dir, float length) {
        GetComponent<CharacterController>().Move(dir * length);
    }

    public void Skill_KnockBack(float length) {
        Transform skillUser = SkillController.GetSkillUser();

        MechController.SkillSetMoving((skillUser != null) ? (transform.position - skillUser.position).normalized * length : -transform.forward * length);
    }

    [PunRPC]
    private void DisablePlayer() {
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
            CurrentHP = 0;
            animator.SetBool(AnimatorVars.BCNPose_id, false);
            gm.ShowRespawnPanel();
        }

        gameObject.layer = default_layer;
        setIsFiring(0, false);
        setIsFiring(1, false);

        StartCoroutine(DisablePlayerWhenNotOnSkill());

        MechController.enabled = false;
        EnableAllColliders(false);
        GetComponent<Collider>().enabled = true;//set to true to trigger exit (while layer changed)
    }

    private IEnumerator DisablePlayerWhenNotOnSkill() {
        yield return new WaitWhile(() => onSkill);
        displayPlayerInfo.gameObject.SetActive(false);

        crosshair.ShutDownAllCrosshairs();
        crosshair.enabled = false;

        EnableAllRenderers(false);
        animator.enabled = false;

        crosshairImage.gameObject.SetActive(false);
        HeatBar.gameObject.SetActive(false);
    }

    // Enable MechController, Crosshair, Renderers, set layer to player layer, move player to spawn position
    [PunRPC]
    private void EnablePlayer(int respawnPoint, int mech_num) {
        bm.SetMechNum(mech_num);
        animator.enabled = true;
        if (photonView.isMine) { // build mech also init MechCombat
            Mech m = UserData.myData.Mech[mech_num];
            bm.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);
        }

        initMechStats();

        MechController.initControllerVar();
        displayPlayerInfo.gameObject.SetActive(!photonView.isMine);

        transform.position = gm.SpawnPoints[respawnPoint].position;
        gameObject.layer = playerlayer;
        isDead = false;
        EffectController.RespawnEffect();

        if (!photonView.isMine) return;

        MechController.enabled = true;
        crosshair.enabled = true;

        crosshairImage.gameObject.SetActive(true);
        HeatBar.gameObject.SetActive(true);
    }

    // Update is called once per frame
    private void Update() {
        if (!photonView.isMine || gm.GameOver() || !gm.GameIsBegin) return;

        updateHUD(); // this is also called when on skill & dead

        // Drain HP bar gradually
        if (isDead) {
            if (healthBar.value > 0) { healthBar.value = healthBar.value - 0.01f; CurrentHP = 0; };
            return;
        }

        //TODO : remove this
        if (forceDead) {
            forceDead = false;
            photonView.RPC("OnHit", PhotonTargets.All, 3000, photonView.viewID, "ForceDead", true);
        }

        if (onSkill) return;

        // Animate left and right combat
        handleCombat(LEFT_HAND);
        handleCombat(RIGHT_HAND);
        HandleSkillInput();

        // Switch weapons
        if (Input.GetKeyDown(KeyCode.R) && !IsSwitchingWeapon && !isDead) {
            currentEN -= (currentEN >= MAX_EN / 3) ? MAX_EN / 3 : currentEN;

            photonView.RPC("CallSwitchWeapons", PhotonTargets.All, null);
        }
    }

    // Set animations and tweaks
    private void LateUpdate() {
        HandleAnimation(LEFT_HAND);
        HandleAnimation(RIGHT_HAND);
    }

    private void handleCombat(int hand) {
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
            } else if (Input.GetKey(KeyCode.Mouse0) && !isBCNcanceled && !On_BCNShoot && !animator.GetBool(AnimatorVars.BCNPose_id) && MechController.grounded && !animator.GetBool("BCNLoad")) {
                AnimationEventController.BCNPose();
                animator.SetBool(AnimatorVars.BCNPose_id, true);
                timeOfLastShotL = Time.time - 1 / bm.weaponScripts[weaponOffset + hand].Rate / 2;
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

        if (IsSwitchingWeapon) {
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

                if ((hand == 0 && isRMeleePlaying) || (hand == 1 && isLMeleePlaying))
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
                if (Input.GetKey(KeyCode.Mouse0) || !animator.GetBool(AnimatorVars.BCNPose_id) || !MechController.grounded)
                    return;

                setIsFiring(hand, true);
                HeatBar.IncreaseHeatBarL(45);
                //TODO : check the start position : cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH)
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

    private void HandleAnimation(int hand) {
        if (getIsFiring(hand)) {
            switch (curGeneralWeaponTypes[weaponOffset + hand]) {
                case (int)GeneralWeaponTypes.Melee:
                if (curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.Sword)//sword
                    AnimationEventController.Slash(hand);
                else//spear
                    AnimationEventController.Smash(hand);
                break;
                case (int)GeneralWeaponTypes.Shield:
                animator.SetBool((hand==0)? AnimatorVars.blockL_id : AnimatorVars.blockR_id, true);
                break;
                case (int)GeneralWeaponTypes.Cannon:
                animator.SetBool(AnimatorVars.BCNShoot_id, true);
                break;
            }
        } else {// melee is set to false by animation
            if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Shield) 
                animator.SetBool((hand == 0) ? AnimatorVars.blockL_id : AnimatorVars.blockR_id, false);
            else if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Cannon)
                animator.SetBool(AnimatorVars.BCNShoot_id, false);
        }
    }

    private void HandleSkillInput() {
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

    private void updateHUD() {
        // Update Health bar gradually
        healthBar.value = calculateSliderPercent(healthBar.value, CurrentHP / (float)MAX_HP);
        healthtext.text = UIExtensionMethods.BarValueToString((int)(MAX_HP * healthBar.value), MAX_HP);
        // Update EN bar gradually
        ENBar.value = calculateSliderPercent(ENBar.value, currentEN / (float)MAX_EN);
        ENtext.text = UIExtensionMethods.BarValueToString((int)(MAX_EN* ENBar.value), (int)MAX_EN);
    }

    // Returns currentPercent + 0.01 if currentPercent < targetPercent, else - 0.01
    private float calculateSliderPercent(float currentPercent, float targetPercent) {
        float err = 0.015f;
        if (Mathf.Abs(currentPercent - targetPercent) > err) {
            currentPercent = currentPercent + (currentPercent > targetPercent ? -err : err);
        } else {
            currentPercent = targetPercent;
        }
        return currentPercent;
    }

    [PunRPC]
    private void CallSwitchWeapons() {//Play switch weapon animation
        EffectController.SwitchWeaponEffect();
        IsSwitchingWeapon = true;
        SwitchWeaponcoroutine = StartCoroutine(SwitchWeaponsBegin());
    }

    private IEnumerator SwitchWeaponsBegin() {
        yield return new WaitForSeconds(1);
        if (isDead) {
            SwitchWeaponcoroutine = null;
            IsSwitchingWeapon = false;
            yield break;
        }
        // Stop current attacks and reset
        ResetWeaponAnimationVariables();

        weaponOffset = (weaponOffset + 2) % 4;

        // Switch weapons by enable/disable renderers
        ActivateWeapons();

        if (OnWeaponSwitched != null) OnWeaponSwitched();
        UpdateMovementClips();
        if (photonView.isMine) SetWeaponOffsetProperty(weaponOffset);

        SwitchWeaponcoroutine = null;
        IsSwitchingWeapon = false;
    }

    private void SetWeaponOffsetProperty(int weaponOffset) {
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("weaponOffset", weaponOffset);
        photonView.owner.SetCustomProperties(h);
    }

    private void ResetArmAnimatorState() {
       animator.Play("Idle",1);
       animator.Play("Idle",2);
    }

    private void ActivateWeapons() {//Not using SetActive because it causes weapon Animator to bind the wrong rotation if the weapon animation is not finish (SMG reload)
        for (int i = 0; i < weapons.Length; i++) {
            if (weapons[i] != null) {
                Renderer[] renderers = weapons[i].GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers) {
                    renderer.enabled = (i==weaponOffset || i==weaponOffset+1) ;
                }
            }
        }        
    }

    public void UpdateMovementClips() {
        if (weaponScripts == null) return;
        bool isPreviousWeaponTwoHanded = (weaponScripts[(weaponOffset+2)%4] != null && weaponScripts[(weaponOffset + 2) % 4].twoHanded) ;
        bool isCurrentWeaponTwoHanded = (weaponScripts[(weaponOffset) % 4] != null && weaponScripts[(weaponOffset) % 4].twoHanded);

        if(isPreviousWeaponTwoHanded == isCurrentWeaponTwoHanded)return;
        
        MovementClips movementClips = (isCurrentWeaponTwoHanded) ? TwoHandedMovementClips : defaultMovementClips;
        for (int i = 0; i < movementClips.clips.Length; i++) {
            clipOverrides[movementClips.clipnames[i]] = movementClips.clips[i];
        }
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    private bool getIsFiring(int hand) {
        return hand == LEFT_HAND ? fireL : fireR;
    }

    private void setIsFiring(int hand, bool isFiring) {
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

    private void UpdateSpecialCurWeaponType() {
        for (int i = 0; i < 4; i++) {
            if (weaponScripts[i] == null) continue;
            
            for(int j=0;j< SpecialWeaponTypeStrs.Length; j++) {
                if(weaponScripts[i].weaponType == SpecialWeaponTypeStrs[j]) {
                    curSpecialWeaponTypes[i] = j;
                    break;
                }
            }
        }
    }

    private void UpdateGeneralCurWeaponType() {
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
                default:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.Empty;
                break;
            }
        }
    }

    public void EnableAllRenderers(bool b) {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = b;
        }

        ActivateWeapons();
    }

    public void EnableAllColliders(bool b) {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders) {
            if (!b) {
                if (collider.gameObject != slashDetector.gameObject)//slashDetector is on IgnoreRayCast layer
                    collider.gameObject.layer = default_layer;
            } else if (collider.gameObject != slashDetector.gameObject)
                collider.gameObject.layer = playerlayer;
        }
    }

    public Transform GetEffectEnd(int num) {
        return Effect_Ends[num];
    }

    public void IncrementEN() {
        currentEN += energyProperties.energyOutput * Time.fixedDeltaTime;
        if (currentEN > MAX_EN) currentEN = MAX_EN;
    }

    public void DecrementEN() {
        if(MechController.grounded)
            currentEN -= energyProperties.dashENDrain * Time.fixedDeltaTime;
        else
            currentEN -= energyProperties.jumpENDrain * Time.fixedDeltaTime ;       

        if (currentEN < 0)
            currentEN = 0;
    }

    public bool EnoughENToBoost() {
        if (currentEN >= energyProperties.minENRequired) {
            isENAvailable = true;
            return true;
        } else {//false -> play effect if not already playing
            if (!ENNotEnoughEffectIsPlaying) {
                StartCoroutine(ENNotEnoughEffect());
            }
            if (!animator.GetBool("Boost"))//can set to false in transition to grounded state but not in transition from grounded state to boost state
                isENAvailable = false;
            return false;
        }
    }

    private IEnumerator ENNotEnoughEffect() {
        ENNotEnoughEffectIsPlaying = true;
        for (int i = 0; i < 4; i++) {
            ENBar_fill.color = new Color32(133, 133, 133, 255);
            yield return new WaitForSeconds(0.15f);
            ENBar_fill.color = new Color32(255, 255, 255, 255);
            yield return new WaitForSeconds(0.15f);
        }
        ENNotEnoughEffectIsPlaying = false;
    }

    public bool IsENAvailable() {
        if (IsENEmpty()) {
            isENAvailable = false;
        }
        return isENAvailable;
    }

    public bool IsENEmpty() {
        return currentEN <= 0;
    }

    private void OnSkill(bool b) {
        onSkill = b;

        if (b) {
            gameObject.layer = default_layer;
            EnableAllColliders(false);
            GetComponent<Collider>().enabled = true;//set to true to trigger exit (while layer changed)
            ResetWeaponAnimationVariables();
        } else {
            if (!isDead) {
                gameObject.layer = playerlayer;
                EnableAllColliders(true);
            }
        }
    }

    private void ResetWeaponAnimationVariables() {//TODO : improve this        
            setIsFiring(0, false);
            setIsFiring(1, false);

            isLMeleePlaying = false;
            isRMeleePlaying = false;
            ShowTrail(0, false);
            ShowTrail(1, false);

        if (photonView.isMine) {
            animator.SetBool(AnimatorVars.onMelee_id, false);
            animator.SetBool(AnimatorVars.BCNPose_id, false);
            animator.SetBool(AnimatorVars.blockL_id, false);
            animator.SetBool(AnimatorVars.blockR_id, false);            
        }
    }

    private void UpdateSMGAnimationSpeed() {//Use SMG rate to adjust animation speed
        if (weaponScripts[weaponOffset] != null)
            if (curSpecialWeaponTypes[weaponOffset] == (int)SpecialWeaponTypes.APS) {//APS animation clip length 1.066s
                animator.SetFloat("rateL", (((SMG)weaponScripts[weaponOffset]).Rate) * 1.066f);
            } else if (curSpecialWeaponTypes[weaponOffset] == (int)SpecialWeaponTypes.LMG) {//LMG animation clip length 0.8s
                animator.SetFloat("rateL", (((SMG)weaponScripts[weaponOffset]).Rate) * 0.8f);
            }

        if (weaponScripts[weaponOffset + 1] != null)
            if (curSpecialWeaponTypes[weaponOffset + 1] == (int)SpecialWeaponTypes.APS) {
                animator.SetFloat("rateR", (((SMG)weaponScripts[weaponOffset + 1]).Rate) * 1.066f);
            } else if (curSpecialWeaponTypes[weaponOffset + 1] == (int)SpecialWeaponTypes.LMG) {
                animator.SetFloat("rateR", (((SMG)weaponScripts[weaponOffset + 1]).Rate) * 0.8f);
            }
    }

    private void UpdateSlashAnimationThreshold() {
        if (curSpecialWeaponTypes[weaponOffset] == (int)SpecialWeaponTypes.Sword) {
            slashL_threshold = ((Sword)weaponScripts[weaponOffset]).threshold;
        }
        if (curSpecialWeaponTypes[weaponOffset + 1] == (int)SpecialWeaponTypes.Sword)
            slashR_threshold = ((Sword)weaponScripts[weaponOffset + 1]).threshold;
    }

    public void SetMeleePlaying(int hand, bool isPlaying) { 
        if (hand == 0) isLMeleePlaying = isPlaying;
        else isRMeleePlaying = isPlaying;

        MechController.onInstantMoving = isPlaying;
    }

    public void SetReceiveNextSlash(int receive) { // this is called in the animation clip
        receiveNextSlash = (receive == 1) ? true : false;
    }

    public void FindTrail() {//TODO : Add Spear case
        if (curSpecialWeaponTypes[weaponOffset] == (int)SpecialWeaponTypes.Sword) {
            trailL = weapons[weaponOffset].transform.Find("trail").GetComponent<XWeaponTrail>();
            if (trailL != null) {
                trailL.Deactivate();
            }
        } else {
            trailL = null;
        }

        if (curSpecialWeaponTypes[weaponOffset + 1] == (int)SpecialWeaponTypes.Sword) {
            trailR = weapons[weaponOffset + 1].transform.Find("trail").GetComponent<XWeaponTrail>();
            if (trailR != null)
                trailR.Deactivate();
        } else {
            trailR = null;
        }
    }

    public void ShowTrail(int hand, bool show) {
        if (hand == 0 && trailL != null)
            if (show) trailL.Activate();
            else trailL.StopSmoothly(0.1f);

        if (hand == 1 && trailR != null)
            if (show) trailR.Activate();
            else trailR.StopSmoothly(0.1f);
    }

    public int GetCurrentWeaponOffset() {
        return weaponOffset;
    }

    [System.Serializable]
    private struct EnergyProperties {
        public float jumpENDrain, dashENDrain;
        public float energyOutput;
        public float minENRequired;       
    }
}