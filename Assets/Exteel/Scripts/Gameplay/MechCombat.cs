using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XftWeapon;

public class MechCombat : Combat {
    [SerializeField] private MechController MechController;
    [SerializeField] private HeatBar HeatBar;
    [SerializeField] private EffectController EffectController;
    [SerializeField] private Camera cam;
    [SerializeField] private BuildMech bm;
    [SerializeField] private Animator animator;
    [SerializeField] private MovementClips defaultMovementClips, TwoHandedMovementClips;//TODO : remove this
    [SerializeField] private SkillController SkillController;

    private string[] SpecialWeaponTypeStrs = new string[11] { "APS", "LMG", "Rifle", "Shotgun", "Rectifier", "Sword", "Spear", "Shield", "Rocket", "Cannon", "EMPTY" };
    private enum GeneralWeaponTypes { Ranged, Rectifier, Melee, Shield, Rocket, Cannon, Empty };//for efficiency
    private enum SpecialWeaponTypes { APS, LMG, Rifle, Shotgun, Rectifier, Sword, Spear, Shield, Rocket, Cannon, EMPTY };//These types all use different animations
    private int[] curGeneralWeaponTypes = new int[4];//ranged , melee , ...
    private int[] curSpecialWeaponTypes = new int[4];//APS , BRF , ...

    // EN
    [SerializeField] private EnergyProperties energyProperties = new EnergyProperties();
    bool isENAvailable = true;

    // Game variables
    private const int playerlayer = 8, default_layer = 0;
    private int TerrainLayerMask, PlayerLayerMask;

    // Combat variables  //TODO : Remove this
    public bool isDead;
    public bool[] is_overheat = new bool[4];
    public int scanRange, MechSize, TotalWeight;
    public int BCNbulletNum = 2;
    public bool isOnBCNPose, onSkill = false;
    private bool on_BCNShoot = false;
    public bool On_BCNShoot {
        get { return on_BCNShoot; }
        set {
            on_BCNShoot = value;
            MechController.onInstantMoving = value;
            if (value) BCNbulletNum--;
            if (BCNbulletNum <= 0) {
                animator.Play("BCN", 1);
                animator.Play("BCN", 2);
                animator.SetBool("BCNLoad", true);
            }
        }
    }

    private int weaponOffset = 0;
    
    private bool isBCNcanceled = false;//check if right click cancel //TODO : *remove this

    // Left
    private const int LEFT_HAND = 0;
    private float timeOfLastShotL;
    private bool fireL = false;
    public bool isLMeleePlaying { get; private set; }

    // Right
    private const int RIGHT_HAND = 1;
    private float timeOfLastShotR;
    private bool fireR = false;
    public bool isRMeleePlaying { get; private set; }

    public bool CanMeleeAttack = true;

    public bool IsSwitchingWeapon { get; private set; }

    private bool receiveNextSlash;
    private const int slashMaxDistance = 30;//the ray which checks if hitting shield max distance
    public float slashL_threshold, slashR_threshold;

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

    // Components
    private HUD HUD;
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

    public delegate void MechCombatAction();
    public MechCombatAction OnWeaponSwitched;

    private Coroutine SwitchWeaponcoroutine;

    private void Awake() {
        InitAnimatorControllers();
        RegisterOnWeaponSwitched();
        RegisterOnMechBuilt();
        RegisterOnSkill();
    }

    private void Start() {
        findGameManager();
        initMechStats();
        initComponents();
        initGameObjects();
        initTargetProperties();
        initSlashDetector();

        SyncWeaponOffset();//TODO : check this
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

    private void RegisterOnMechBuilt() {
        if(bm== null)return;
        bm.OnMechBuilt += InitWeapons;
        bm.OnMechBuilt += initCombatVariables;
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
        energyProperties.energyOutput = bm.MechProperty.ENOutputRate - bm.MechProperty.EnergyDrain; //TODO : improve this

        scanRange = bm.MechProperty.ScanRange;
    }

    private void UpdateWeightRelatedVars() {
        TotalWeight = bm.MechProperty.Weight + ((weaponScripts[weaponOffset] == null) ? 0 : weaponScripts[weaponOffset].weight) + ((weaponScripts[weaponOffset + 1] == null) ? 0 : weaponScripts[weaponOffset + 1].weight);

        energyProperties.jumpENDrain = bm.MechProperty.GetJumpENDrain(TotalWeight);
        energyProperties.dashENDrain = bm.MechProperty.DashENDrain;

        MechController.UpdateWeightRelatedVars(bm.MechProperty.Weight, ((weaponScripts[weaponOffset] == null) ? 0 : weaponScripts[weaponOffset].weight) + ((weaponScripts[weaponOffset + 1] == null) ? 0 : weaponScripts[weaponOffset + 1].weight));
    }

    private void initMechStats() {//call also when respawn
        CurrentHP = MAX_HP;
        CurrentEN = MAX_EN;
    }

    private void initGameObjects() {
        TerrainLayerMask = LayerMask.GetMask("Terrain");
        PlayerLayerMask = LayerMask.GetMask("PlayerLayer");
        BulletCollector = GameObject.Find("BulletCollector");
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
        HUD = GetComponent<HUD>();
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
        if (photonView.isMine) {
            SetWeaponOffsetProperty(weaponOffset);
            animator.Play("Walk", 0);
            ResetArmAnimatorState();

            ResetMeleeVars();
            
        }
        if (SwitchWeaponcoroutine != null) {//die when switching weapons
            StopCoroutine(SwitchWeaponcoroutine);
            IsSwitchingWeapon = false;
            SwitchWeaponcoroutine = null;
        }
        onSkill = false;

        setIsFiring(0, false);
        setIsFiring(1, false);

        animator.SetBool("OnBCN", false);
        animator.SetBool("BCNLoad", false);
        BCNbulletNum = 2;
    }

    private void FindEffectEnd() {
        for (int i = 0; i < 4; i++) {
            if (weapons[i] != null) {
                Effect_Ends[i] = TransformExtension.FindDeepChild(weapons[i].transform, "EffectEnd");
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

    private void ResetMeleeVars() {
        if(!photonView.isMine)return;

        isRMeleePlaying = false;
        isLMeleePlaying = false;
        receiveNextSlash = true;

        animator.SetBool("SlashL", false);
        animator.SetBool("SlashL2", false);
        animator.SetBool("SlashL3", false);
        animator.SetBool("SlashL4", false);

        animator.SetBool("SlashR", false);
        animator.SetBool("SlashR2", false);
        animator.SetBool("SlashR3", false);
        animator.SetBool("SlashR4", false);
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


            ActivateWeapons();
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
                        targetpv.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, cam);
                    } else {
                        targetpv.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, cam);
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
                    targetpv.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, cam);
                }
            } else {//ENG
                photonView.RPC("Shoot", PhotonTargets.All, hand, direction, target_viewID, false, -1);

                targetpv.RPC("OnHeal", PhotonTargets.All, photonView.viewID, damage);

                targetpv.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, cam);
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

                hitpoints = Physics.RaycastAll(transform.position + new Vector3(0, 5, 0), (target.transform.root.position + new Vector3(0, 5, 0)) - transform.position - new Vector3(0, 5, 0), slashMaxDistance, PlayerLayerMask).OrderBy(h => h.distance).ToArray();
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
                    target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL ,cam);
                } else {
                    if (isHitShield)
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, cam);
                    else
                        target.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, cam);
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
            Debug.LogError("Should never get here with type : "+ curSpecialWeaponTypes[weaponOffset + hand]);
            return;
        }
        clipName += "Shoot" + ((hand == 0) ? "L" : "R");
        animator.Play(clipName, hand + 1, 0);
    }

    private void SetTargetInfo(int hand, Vector3 direction, int playerPVid, bool isShield, int target_handOnShield) {
        if (playerPVid != -1) {
            GameObject target = PhotonView.Find(playerPVid).gameObject;

            if (isShield) {
                if(target.tag != "Drone")
                    Targets[hand] = target.GetComponent<BuildMech>().weapons[target.GetComponent<MechCombat>().GetCurrentWeaponOffset() + target_handOnShield];
                else
                    Targets[hand] = target.GetComponent<DroneCombat>().Shield.gameObject;
            } else
                Targets[hand] = target;

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
            Debug.LogWarning("bullet is null");
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
                GameObject bullet = PhotonNetwork.Instantiate(bm.weaponScripts[weaponOffset].GetWeaponName() + "B", transform.position + new Vector3(0, 5, 0) + transform.forward * 10, Quaternion.LookRotation(bullet_directions[hand]), 0);
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
                crosshair.CallShakingEffect(hand);
                if (Targets[hand] != null && !CheckTargetIsDead(Targets[hand])) {
                    //only APS & LMG have multiple msgs.
                    if (curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.APS || curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.LMG) {
                        if (!isTargetShield[hand]) {
                            if (mcbt != null)
                                mcbt.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, cam);
                            else
                                Targets[hand].transform.root.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, cam);
                        } else {
                            if (mcbt != null)
                                mcbt.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, cam);
                            else
                                Targets[hand].transform.root.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, cam);
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
                    bulletTrace.SetTarget(Targets[hand].transform, true);
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

        if (CurrentHP <= 0 && PhotonNetwork.isMasterClient) {//sync disable player
            photonView.RPC("DisablePlayer", PhotonTargets.All, shooter_viewID, weapon);
        }
    }

    [PunRPC]
    private void ShieldOnHit(int damage, int shooter_viewID, int shield, string weapon) {
        if (isDead) {
            return;
        }

        if (CheckIsSwordByStr(weapon) || CheckIsSpearByStr(weapon)) {
            EffectController.SlashOnHitEffect(true, shield);
        }

        CurrentHP -= damage;

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

        if (CurrentHP <= 0 && PhotonNetwork.isMasterClient) {
            photonView.RPC("DisablePlayer", PhotonTargets.All, shooter_viewID, weapon);
        }
    }

    private bool CheckIsSwordByStr(string name) {//TODO : improve this
        return name.Contains("SHL");
    }
    private bool CheckIsSpearByStr(string name) {
        return name.Contains("ADR");
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
    private void OnLocked() {
        if (photonView.isMine)crosshair.ShowLocked();
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
    private void DisablePlayer(int shooter_viewID, string weapon) {
        gm.OnPlayerDead(gameObject, shooter_viewID, weapon);

        isDead = true;//TODO : check this again

        CurrentHP = 0;

        gameObject.layer = default_layer;

        StartCoroutine(DisablePlayerWhenNotOnSkill());

        MechController.enabled = false;//stop control immediately
        EnableAllColliders(false);
        GetComponent<Collider>().enabled = true;//set to true to trigger exit (while layer changed) //TODO : check this if necessary
    }

    private IEnumerator DisablePlayerWhenNotOnSkill() {
        yield return new WaitWhile(() => onSkill);

        if (photonView.isMine) {
            CurrentHP = 0;
            animator.SetBool(AnimatorVars.BCNPose_id, false);
            gm.EnableRespawnPanel(true);
        }

        OnMechEnabled(false);

        EnableAllRenderers(false);
        animator.enabled = false;
    }

    // Enable MechController, Crosshair, Renderers, set layer to player layer, move player to spawn position
    [PunRPC]
    private void EnablePlayer(int respawnPoint, int mech_num) {
        //Debug.Log("called enable num : "+mech_num);
        bm.SetMechNum(mech_num);
        animator.enabled = true;
        if (photonView.isMine) { // build mech also init MechCombat
            Mech m = UserData.myData.Mech[mech_num];
            bm.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);
        }

        initMechStats();
        
        OnMechEnabled(true);

        //this is to avoid trigger flag
        gameObject.layer = default_layer;
        GetComponent<CharacterController>().enabled = false;
        transform.position = gm.GetRespawnPointPosition(respawnPoint);
        GetComponent<CharacterController>().enabled = true;
        gameObject.layer = playerlayer;


        isDead = false;
        EffectController.RespawnEffect();
    }

    // Update is called once per frame
    private void Update() {
        if (!photonView.isMine || gm.GameOver() || !GameManager.gameIsBegin) return;

        //TODO : remove this
        if (forceDead) {
            forceDead = false;
            photonView.RPC("OnHit", PhotonTargets.All, 10000, photonView.viewID, "ForceDead", true);
        }

        if (onSkill || isDead) return;

        // Animate left and right combat
        handleCombat(LEFT_HAND);
        handleCombat(RIGHT_HAND);
        HandleSkillInput();

        // Switch weapons
        if (Input.GetKeyDown(KeyCode.R) && !IsSwitchingWeapon && !isDead) {
            CurrentEN -= (CurrentEN >= MAX_EN / 3) ? MAX_EN / 3 : CurrentEN;

            photonView.RPC("CallSwitchWeapons", PhotonTargets.All, null);
        }
    }

    // Set animations and tweaks
    private void LateUpdate() {
        if(!photonView.isMine)return;
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
                animator.SetBool((hand == 0) ? AnimatorVars.blockL_id : AnimatorVars.blockR_id, true);
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
        //UpdateMovementClips();
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
        animator.Play("Idle", 1);
        animator.Play("Idle", 2);
    }

    private void ActivateWeapons() {//Not using SetActive because it causes weapon Animator to bind the wrong rotation if the weapon animation is not finish (SMG reload)
        for (int i = 0; i < weapons.Length; i++) {
            if (weapons[i] != null) {
                Renderer[] renderers = weapons[i].GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers) {
                    renderer.enabled = (i == weaponOffset || i == weaponOffset + 1);
                }

                Collider[] colliders = weapons[i].GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders) {
                    collider.enabled = (i == weaponOffset || i == weaponOffset + 1);
                }
            } else {
                Debug.Log("weapon : "+i + " is null");
            }
        }
    }

    public void UpdateMovementClips() {
        if (weaponScripts == null) return;
        bool isCurrentWeaponTwoHanded = (weaponScripts[(weaponOffset) % 4] != null && weaponScripts[(weaponOffset) % 4].twoHanded);

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
            if (weaponScripts[i] == null) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.EMPTY;
                continue;
            }

            for (int j = 0; j < SpecialWeaponTypeStrs.Length; j++) {
                if (weaponScripts[i].weaponType == SpecialWeaponTypeStrs[j]) {
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
        if(b)ActivateWeapons();
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
        CurrentEN += energyProperties.energyOutput * Time.fixedDeltaTime;
        if (CurrentEN > MAX_EN) CurrentEN = MAX_EN;
    }

    public void DecrementEN() {
        if (MechController.grounded)
            CurrentEN -= energyProperties.dashENDrain * Time.fixedDeltaTime;
        else
            CurrentEN -= energyProperties.jumpENDrain * Time.fixedDeltaTime;

        if (CurrentEN < 0)
            CurrentEN = 0;
    }

    public bool EnoughENToBoost() {
        if (CurrentEN >= energyProperties.minENRequired) {
            isENAvailable = true;
            return true;
        } else {//play effect
            HUD.PlayENnotEnoughEffect();
            if (!animator.GetBool("Boost"))
                isENAvailable = false;
            return false;
        }
    }

    public bool IsENAvailable() {
        if (IsENEmpty()) {
            isENAvailable = false;
        }
        return isENAvailable;
    }

    public bool IsENEmpty() {
        return CurrentEN <= 0;
    }

    private void OnSkill(bool b) {
        onSkill = b;

        if (b) {
            gameObject.layer = default_layer;
            EnableAllColliders(false);
            GetComponent<Collider>().enabled = true;//set to true to trigger exit (while layer changed)
            animator.Play("Walk",0);
            ResetWeaponAnimationVariables();
            ResetMeleeVars();
        } else {
            if (!isDead) {
                gameObject.layer = playerlayer;
                EnableAllColliders(true);
            }
            receiveNextSlash = true;//on skill when melee attacking            
        }
    }

    private void ResetWeaponAnimationVariables() {//TODO : improve this
        setIsFiring(0, false);
        setIsFiring(1, false);

        isLMeleePlaying = false;
        isRMeleePlaying = false;
        ShowTrail(0, false);
        ShowTrail(1, false);
        animator.SetBool("OnBCN", false);
        animator.SetBool("BCNLoad",false);
        BCNbulletNum = 2;

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
            slashR_threshold = ((Sword)weaponScripts[weaponOffset + 1]).threshold;//TODO : no reference
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