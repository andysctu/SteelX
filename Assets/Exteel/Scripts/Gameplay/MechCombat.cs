﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using XftWeapon;
using System.Linq;

public class MechCombat : Combat {

    [SerializeField] Transform camTransform;
    [SerializeField] HeatBar HeatBar;
    [SerializeField] DisplayPlayerInfo displayPlayerInfo;
    [SerializeField] CrosshairImage crosshairImage;
    [SerializeField] EffectController EffectController;
    [SerializeField] LayerMask playerlayerMask;
    enum GeneralWeaponTypes { RANGED, ENG, MELEE, SHIELD, RCL, BCN, EMPTY };
    enum SpecialWeaponTypes { APS, BRF, LMG, SGN, ENG, SHL, ADR, SHS, RCL, BCN, EMPTY };
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
    public int BCNbulletNum;
    public bool isOnBCNPose;//called by BCNPoseState to check if on the right pose 
    public int weaponOffset = 0;
    private int[] curGeneralWeaponTypes = new int[4];//ranged , melee , ...
    private int[] curSpecialWeaponTypes = new int[4];//APS , BRF , ...
    private bool isBCNcanceled = false;//check if right click cancel
    private const float APS_INTERVAL = 0.2f, LMG_INTERVAL = 0.2f;

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
                                            // Transforms
    public Transform[] Hands;//other player use this to locate hand position quickly
    private Transform shoulderL;
    private Transform shoulderR;
    private Transform head;
    private Transform[] Gun_ends = new Transform[4];

    // GameObjects
    private GameObject[] weapons;
    private GameObject[] bullets;
    private List<Transform> targets;
    private InRoomChat InRoomChat;
    private Weapon[] weaponScripts;

    // HUD
    private Slider healthBar;
    private Slider fuelBar;
    private Image fuelBar_fill;
    private bool isNotEnoughEffectPlaying = false;
    private bool isFuelAvailable = true;
    private Text healthtext, fueltext;
    private HUD hud;
    private Camera cam;

    // Components
    public Crosshair crosshair;
    private BuildMech bm;
    private SlashDetector slashDetector;
    private MechController mechController;
    private Sounds Sounds;
    private Combo Combo;
    private ParticleSystem[] Muz = new ParticleSystem[4];
    private XWeaponTrail trailL, trailR;

    //Animator
    private AnimatorVars AnimatorVars;
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;
    private MovementClips MovementClips;
    private MechIK MechIK;

    private Coroutine bulletCoroutine;

    //for Debug
    public bool forceDead = false;

    public bool isInitFinished = false;
    void Start() {
        findGameManager();
        initMechStats();
        initComponents();
        initCombatVariables();
        UpdateWeaponInfo();
        initAnimatorControllers();
        initTransforms();
        initGameObjects();
        initCam();
        initCrosshair();
        UpdateSpecialCurWeaponType();
        UpdateGeneralCurWeaponType();
        UpdateArmAnimatorState();
        initSlashDetector();
        SyncWeaponOffset();
        FindTrail();
        UpdateMuz();
        initHUD();
        CallInitFinish();
    }

    void initMechStats() {
        currentHP = MAX_HP;
        currentFuel = MAX_FUEL;
    }

    void initTransforms() {
        head = transform.Find("CurrentMech/metarig/hips/spine/chest/fakeNeck/head");
        shoulderL = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.L");
        shoulderR = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.R");

        FindGunEnds();

        Hands = new Transform[2];
        Hands[0] = shoulderL.Find("upper_arm.L/forearm.L/hand.L");
        Hands[1] = shoulderR.Find("upper_arm.R/forearm.R/hand.R");
    }

    void initGameObjects() {
        InRoomChat = GameObject.Find("InRoomChat").GetComponent<InRoomChat>();
        //hud = GameObject.FindObjectOfType<HUD>();
        hud = GameObject.Find("ShowTextCanvas").GetComponent<HUD>();
        displayPlayerInfo.gameObject.SetActive(!photonView.isMine);//do not show my name & hp bar
    }

    void initComponents() {
        Transform currentMech = transform.Find("CurrentMech");
        Sounds = currentMech.GetComponent<Sounds>();
        AnimatorVars = currentMech.GetComponent<AnimatorVars>();
        Combo = currentMech.GetComponent<Combo>();
        animator = currentMech.GetComponent<Animator>();
        MechIK = currentMech.GetComponent<MechIK>();
        mechController = GetComponent<MechController>();
        bm = GetComponent<BuildMech>();
        MovementClips = GetComponent<MovementClips>();
    }

    void initAnimatorControllers() {
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        ChangeMovementClips((weaponScripts[0].isTwoHanded) ? 1 : 0);
        //ChangeWeaponClips ();
    }

    public void UpdateWeaponInfo() {
        weapons = bm.weapons;
        bullets = bm.bulletPrefabs;
        weaponScripts = bm.weaponScripts;
        Sounds.ShotSounds = bm.ShotSounds;
    }

    public void initCombatVariables() {// this will be called also when respawn
        weaponOffset = 0;
        if (photonView.isMine) SetWeaponOffsetProperty(weaponOffset);

        fireL = false;
        fireR = false;
        timeOfLastShotL = Time.time;
        timeOfLastShotR = Time.time;
        isSwitchingWeapon = false;
        CanMeleeAttack = true;
        receiveNextSlash = true;
        isOnBCNPose = false;
        BCNbulletNum = 2;
        setIsFiring(0, false);
        setIsFiring(1, false);

        HeatBar.InitVars();
        MechIK.UpdateMechIK();
        mechController.FindBoosterController();
    }

    void initHUD() {
        if (!photonView.isMine)
            return;
        initHealthAndFuelBars();
    }

    void initCam() {
        cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
    }

    void initCrosshair() {
        crosshair = cam.GetComponent<Crosshair>();
    }

    void CallInitFinish() {
        isInitFinished = true;
    }

    public void FindGunEnds() {
        for (int i = 0; i < 4; i++) {
            if (weapons[i] != null) {
                Gun_ends[i] = weapons[i].transform.Find("End");
            }
        }
    }

    public void UpdateMuz() {
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

    float timeOfLastShot(int handPosition) {
        return handPosition == LEFT_HAND ? timeOfLastShotL : timeOfLastShotR;
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
        SetSlashDetector();
    }

    void SetSlashDetector() {
        bool b = ((curGeneralWeaponTypes[weaponOffset] == (int)GeneralWeaponTypes.MELEE || curGeneralWeaponTypes[weaponOffset + 1] == (int)GeneralWeaponTypes.MELEE) && photonView.isMine);
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

    void FireRaycast(Vector3 start, Vector3 direction, int handPosition) {
        Transform target = ((handPosition == 0) ? crosshair.getCurrentTargetL() : crosshair.getCurrentTargetR());//target might be shield collider
        int damage = bm.weaponScripts[weaponOffset + handPosition].Damage;

        if (target != null) {
            PhotonView targetpv = target.transform.root.GetComponent<PhotonView>();
            int target_viewID = targetpv.viewID;
            string weaponName = bm.curWeaponNames[weaponOffset + handPosition];

            if (curGeneralWeaponTypes[weaponOffset + handPosition] != (int)GeneralWeaponTypes.ENG) {
                if (target.tag != "Shield") {//not shield => player or drone

                    photonView.RPC("RegisterBulletTrace", PhotonTargets.All, handPosition, direction, target_viewID, false, -1);

                    targetpv.RPC("OnHit", PhotonTargets.All, damage, photonView.viewID, weaponName, weaponScripts[weaponOffset + handPosition].isSlowDown);

                    if (target.gameObject.GetComponent<Combat>().CurrentHP() <= 0) {
                        hud.ShowText(cam, target.position + new Vector3(0, 5, 0), "Kill");
                    } else {
                        hud.ShowText(cam, target.position + new Vector3(0, 5, 0), "Hit");
                    }
                } else {
                    //check what hand is it
                    int hand = (target.transform.parent.name[target.transform.parent.name.Length - 1] == 'L') ? 0 : 1;

                    photonView.RPC("RegisterBulletTrace", PhotonTargets.All, handPosition, direction, target_viewID, true, hand);

                    MechCombat targetMcbt = target.transform.root.GetComponent<MechCombat>();

                    if (targetMcbt != null) {
                        if (targetMcbt.is_overheat[targetMcbt.weaponOffset + hand]) {
                            targetpv.RPC("ShieldOnHit", PhotonTargets.All, damage, photonView.viewID, hand, weaponName);
                        } else {
                            targetpv.RPC("ShieldOnHit", PhotonTargets.All, damage / 2, photonView.viewID, hand, weaponName);
                        }
                    }

                    hud.ShowText(cam, target.position, "Defense");
                }
            } else {//ENG
                photonView.RPC("RegisterBulletTrace", PhotonTargets.All, handPosition, direction, target_viewID, false, -1);

                targetpv.RPC("OnHeal", PhotonTargets.All, photonView.viewID, damage);

                hud.ShowText(cam, target.position, "Hit");
            }
        } else {
            photonView.RPC("RegisterBulletTrace", PhotonTargets.All, handPosition, direction, -1, false, -1);
        }
    }

    public void SlashDetect(int handPosition) {
        if ((targets = slashDetector.getCurrentTargets()).Count != 0) {

            int damage = bm.weaponScripts[weaponOffset + handPosition].Damage;
            string weaponName = bm.curWeaponNames[weaponOffset + handPosition];
            Sounds.PlaySlashOnHit(weaponOffset + handPosition);

            foreach (Transform target in targets) {
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
                    int hand = (t.transform.parent.name[t.transform.parent.name.Length - 1] == 'L') ? 0 : 1;//which hand holds the shield?
                    target.GetComponent<PhotonView>().RPC("ShieldOnHit", PhotonTargets.All, damage / 2, photonView.viewID, hand, weaponName);
                } else {
                    target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, photonView.viewID, weaponName, true);
                }

                if (target.gameObject.GetComponent<Combat>().CurrentHP() <= 0) {
                    hud.ShowText(cam, target.position, "Kill");
                } else {
                    if (isHitShield)
                        hud.ShowText(cam, t.transform.position, "Defense");
                    else
                        hud.ShowText(cam, target.position, "Hit");
                }
            }
        }
    }

    [PunRPC]
    void RegisterBulletTrace(int handPosition, Vector3 direction, int playerPVid, bool isShield, int hand_shield) {
        bulletCoroutine = StartCoroutine(InstantiateBulletTrace(handPosition, direction, playerPVid, isShield, hand_shield));
    }

    IEnumerator InstantiateBulletTrace(int handPosition, Vector3 direction, int playerPVid, bool isShield, int hand_shield) {
        GameObject Target;

        Target = (playerPVid != -1)? PhotonView.Find(playerPVid).gameObject : null;

        yield return new WaitForSeconds(0.05f);//wait for hand on right position

        if (Muz[weaponOffset + handPosition] != null) {
            Muz[weaponOffset + handPosition].Play();
        }

        if (bullets[weaponOffset + handPosition] == null) {//it happens when player die when shooting or switching weapons
            yield break;
        }

        if (curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.RCL) {
            //GameObject bullet = Instantiate(bullets[weaponOffset], (Hands[handPosition].position + Hands[handPosition + 1].position) / 2 + transform.forward * 3f + transform.up * 3f, Quaternion.LookRotation(direction)) as GameObject;
            GameObject bullet = null;
            if (photonView.isMine) {
                bullet = PhotonNetwork.Instantiate("RCL034B", (Hands[handPosition].position + Hands[handPosition + 1].position) / 2 + transform.forward * 3f + transform.up * 3f, Quaternion.LookRotation(direction), 0);
                RCLBulletTrace bulletTrace = bullet.GetComponent<RCLBulletTrace>();
                bulletTrace.SetShooterInfo(gameObject, hud, cam);
            } else
                yield break;
        } else if (curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.ENG) {
            GameObject bullet = Instantiate(bullets[weaponOffset + handPosition], Gun_ends[weaponOffset + handPosition].position, Quaternion.LookRotation(direction)) as GameObject;
            bullet.transform.SetParent(Gun_ends[weaponOffset + handPosition]);
            bullet.GetComponent<ElectricBolt>().dir = direction;
            bullet.GetComponent<ElectricBolt>().cam = cam;
            if (Target != null) {
                bullet.GetComponent<ElectricBolt>().Target = Target.transform;
            }
        } else {
            int bulletNum;
            float interval;
            switch (curSpecialWeaponTypes[weaponOffset + handPosition]) {
                case (int)SpecialWeaponTypes.APS:
                bulletNum = 4;
                interval = APS_INTERVAL;
                break;
                case (int)SpecialWeaponTypes.LMG:
                bulletNum = 6;
                interval = LMG_INTERVAL;
                break;
                default:
                bulletNum = 1;
                interval = 1;
                break;

            }
            GameObject b = bullets[weaponOffset + handPosition];
            MechCombat mcbt = (Target == null) ? null : Target.transform.root.GetComponent<MechCombat>();

            if (photonView.isMine) {
                crosshairImage.ShakingEffect(handPosition, bm.weaponScripts[weaponOffset + handPosition].Rate, bulletNum);
                if (Target != null && !CheckTargetIsDead(Target)) {
                    if (!isShield) {
                        hud.ShowMultipleHitMsg(cam, Target.transform, new Vector3(0, 5, 0), "Hit", bulletNum, 0.25f);
                    } else {
                        hud.ShowMultipleHitMsg(cam, (mcbt != null) ? mcbt.Hands[hand_shield] : Target.transform.root.GetComponent<DroneCombat>().Hands[hand_shield], Vector3.zero, "Defense", bulletNum, interval);
                    }
                }
            }


            for (int i = 0; i < bulletNum; i++) {
                GameObject bullet = Instantiate(b, Gun_ends[weaponOffset + handPosition].position, Quaternion.LookRotation(direction)) as GameObject;
                BulletTrace bulletTrace = bullet.GetComponent<BulletTrace>();
                bulletTrace.SetCamera(cam);
                bulletTrace.SetShooterName(gameObject.name);

                if (Target != null) {
                    if (isShield) {
                        bulletTrace.SetTarget((mcbt != null) ? mcbt.Hands[hand_shield] : Target.transform.root.GetComponent<DroneCombat>().Hands[hand_shield], true);
                    } else {
                        bulletTrace.SetTarget(Target.transform, false);
                    }
                    //Vector3 scale = bullet.transform.localScale;
                    //bullet.transform.SetParent(Target.transform);
                    //bullet.transform.localScale = scale;
                } else {
                    bulletTrace.SetTarget(null, false);
                }

                yield return new WaitForSeconds(1 / bm.weaponScripts[weaponOffset + handPosition].Rate / bulletNum);
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

        if (CheckIsMeleeByStr(weapon)) {
            EffectController.ShieldOnHitEffect(shield);
        }

        if(photonView.isMine)
            currentHP -= damage;

        //Debug.Log("HP: " + currentHP);
        if (currentHP <= 0) {

            DisablePlayer();

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

        if (bulletCoroutine != null)
            StopCoroutine(bulletCoroutine);

        setIsFiring(0, false);
        setIsFiring(1, false);

        displayPlayerInfo.gameObject.SetActive(false);

        Crosshair ch = GetComponentInChildren<Crosshair>();
        if (ch != null) {
            ch.NoAllCrosshairs();
            ch.enabled = false;
        }
        mechController.enabled = false;
        EnableAllRenderers(false);
        EnableAllColliders(false);

        GetComponent<Collider>().enabled = true;

        crosshairImage.gameObject.SetActive(false);
        HeatBar.gameObject.SetActive(false);

        //transform.Find("Camera/Canvas/CrosshairImage").gameObject.SetActive(false);
        //transform.Find("Camera/Canvas/HeatBar").gameObject.SetActive(false);
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
        Sounds.UpdateSounds(weaponOffset);
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

        //transform.Find("Camera/Canvas/CrosshairImage").gameObject.SetActive(true);
        //transform.Find("Camera/Canvas/HeatBar").gameObject.SetActive(true);

        EffectController.RespawnEffect();
    }

    // Update is called once per frame
    void Update() {
        if (!photonView.isMine || gm.GameOver()) return;

        // Drain HP bar gradually
        if (isDead) {
            if (healthBar.value > 0) healthBar.value = healthBar.value - 0.01f;
            return;
        }

        //For debug , TODO : remove this
        if (forceDead) {
            forceDead = false;
            photonView.RPC("OnHit", PhotonTargets.All, 3000, photonView.viewID, "ForceDead", true);
        }

        if (!gm.GameIsBegin)
            return;
        // Animate left and right combat
        handleCombat(LEFT_HAND);
        handleCombat(RIGHT_HAND);

        // Switch weapons
        if (Input.GetKeyDown(KeyCode.R) && !isSwitchingWeapon && !isDead) {
            currentFuel -= (currentFuel >= MAX_FUEL / 3) ? MAX_FUEL / 3 : currentFuel;

            photonView.RPC("CallSwitchWeapons", PhotonTargets.All, null);
        }

        updateHUD();
    }

    // Set animations and tweaks
    void LateUpdate() {
        handleAnimation(LEFT_HAND);
        handleAnimation(RIGHT_HAND);
    }

    void handleCombat(int handPosition) {
        switch (curGeneralWeaponTypes[weaponOffset + handPosition]) {
            case (int)GeneralWeaponTypes.RANGED:
            if (curSpecialWeaponTypes[weaponOffset + handPosition] == (int)SpecialWeaponTypes.APS || curSpecialWeaponTypes[weaponOffset + handPosition] == (int)SpecialWeaponTypes.LMG) {//has a delay before putting down hands
                if (!Input.GetKey(handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + handPosition]) {
                    if (handPosition == LEFT_HAND) {
                        if (Time.time - timeOfLastShotL >= 1 / bm.weaponScripts[weaponOffset + handPosition].Rate * 0.95f)
                            setIsFiring(handPosition, false);
                        return;
                    } else {
                        if (Time.time - timeOfLastShotR >= 1 / bm.weaponScripts[weaponOffset + handPosition].Rate * 0.95f)
                            setIsFiring(handPosition, false);
                        return;
                    }
                }
            } else {
                if (!Input.GetKeyDown(handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + handPosition]) {
                    if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 0.1f)//0.1 < time of playing shoot animation once , to make sure other player catch this
                        setIsFiring(handPosition, false);
                    return;
                }
            }
            break;
            case (int)GeneralWeaponTypes.MELEE:
            if (!Input.GetKeyDown(handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + handPosition]) {
                setIsFiring(handPosition, false);
                return;
            }
            break;
            case (int)GeneralWeaponTypes.SHIELD:
            if (!Input.GetKey(handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || getIsFiring((handPosition + 1) % 2)) {
                setIsFiring(handPosition, false);
                return;
            }
            break;
            case (int)GeneralWeaponTypes.RCL:
            if (!Input.GetKeyDown(handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset]) {
                if (Time.time - timeOfLastShotL >= 0.4f)//0.4 < time of playing shoot animation once , to make sure other player catch this
                    setIsFiring(handPosition, false);
                return;
            }
            break;
            case (int)GeneralWeaponTypes.BCN:
            if (Time.time - timeOfLastShotL >= 0.5f)
                setIsFiring(handPosition, false);
            if (Input.GetKeyDown(KeyCode.Mouse1) || is_overheat[weaponOffset]) {//right click cancel BCNPose
                isBCNcanceled = true;
                animator.SetBool(AnimatorVars.BCNPose_id, false);
                return;
            } else if (Input.GetKey(KeyCode.Mouse0) && !isBCNcanceled && !animator.GetBool(AnimatorVars.BCNPose_id) && mechController.grounded && !animator.GetBool("BCNLoad")) {
                if (!is_overheat[weaponOffset]) {
                    if (!animator.GetBool(AnimatorVars.BCNPose_id)) {
                        Combo.BCNPose();
                        animator.SetBool(AnimatorVars.BCNPose_id, true);
                        timeOfLastShotL = Time.time - 1 / bm.weaponScripts[weaponOffset + handPosition].Rate / 2;
                    }
                } else {
                    animator.SetBool(AnimatorVars.BCNPose_id, false);
                }
            } else if (!Input.GetKey(KeyCode.Mouse0)) {
                isBCNcanceled = false;
            }
            break;
            case (int)GeneralWeaponTypes.ENG:
            if (!Input.GetKey(handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + handPosition]) {
                if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts[weaponOffset + handPosition].Rate)
                    setIsFiring(handPosition, false);
                return;
            }
            break;

            default: //Empty weapon
            return;
        }

        if (curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.RANGED || curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.SHIELD || curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.ENG)
            if (curGeneralWeaponTypes[weaponOffset + (handPosition + 1) % 2] == (int)GeneralWeaponTypes.MELEE && animator.GetBool("OnMelee"))
                return;

        if (isSwitchingWeapon) {
            return;
        }

        switch (curGeneralWeaponTypes[weaponOffset + handPosition]) {

            case (int)GeneralWeaponTypes.RANGED:
            if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts[weaponOffset + handPosition].Rate) {
                setIsFiring(handPosition, true);
                FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, handPosition);
                if (handPosition == 1) {
                    HeatBar.IncreaseHeatBarR(20);
                    timeOfLastShotR = Time.time;
                } else {
                    HeatBar.IncreaseHeatBarL(20);
                    timeOfLastShotL = Time.time;
                }
            }
            break;
            case (int)GeneralWeaponTypes.MELEE:
            if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts[weaponOffset + handPosition].Rate) {
                if (!receiveNextSlash || !CanMeleeAttack) {
                    return;
                }

                if (curGeneralWeaponTypes[weaponOffset + (handPosition + 1) % 2] == (int)GeneralWeaponTypes.SHIELD && getIsFiring((handPosition + 1) % 2))
                    return;

                if ((animator.GetBool(AnimatorVars.slashL3_id) || animator.GetBool(AnimatorVars.slashR3_id)) && curSpecialWeaponTypes[(handPosition + 1) % 2 + weaponOffset] != (int)SpecialWeaponTypes.SHL)//if not both sword
                    return;

                if ((handPosition == 0 && isRMeleePlaying == 1) || (handPosition == 1 && isLMeleePlaying == 1))
                    return;

                CanMeleeAttack = false;
                receiveNextSlash = false;
                setIsFiring(handPosition, true);
                if (handPosition == 0) {
                    HeatBar.IncreaseHeatBarL(5);
                    timeOfLastShotL = Time.time;
                    if (curGeneralWeaponTypes[weaponOffset + 1] == (int)GeneralWeaponTypes.MELEE)
                        timeOfLastShotR = Time.time;
                } else if (handPosition == 1) {
                    HeatBar.IncreaseHeatBarR(5);
                    timeOfLastShotR = Time.time;
                    if (curGeneralWeaponTypes[weaponOffset] == (int)GeneralWeaponTypes.MELEE)
                        timeOfLastShotL = Time.time;
                }
            }
            break;
            case (int)GeneralWeaponTypes.SHIELD:
            if (!getIsFiring((handPosition + 1) % 2))
                setIsFiring(handPosition, true);
            break;
            case (int)GeneralWeaponTypes.RCL:
            if (Time.time - timeOfLastShotL >= 1 / bm.weaponScripts[weaponOffset + handPosition].Rate) {
                setIsFiring(handPosition, true);
                HeatBar.IncreaseHeatBarL(25);

                FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, handPosition);
                timeOfLastShotL = Time.time;
            }
            break;
            case (int)GeneralWeaponTypes.BCN:
            if (Time.time - timeOfLastShotL >= 1 / bm.weaponScripts[weaponOffset + handPosition].Rate && isOnBCNPose) {
                if (Input.GetKey(KeyCode.Mouse0) || !animator.GetBool(AnimatorVars.BCNPose_id) || !mechController.grounded)
                    return;

                BCNbulletNum--;
                if (BCNbulletNum <= 0)
                    animator.SetBool("BCNLoad", true);

                setIsFiring(handPosition, true);
                HeatBar.IncreaseHeatBarL(45);
                //**Start Position
                FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, handPosition);
                timeOfLastShotL = Time.time;
            }
            break;
            case (int)GeneralWeaponTypes.ENG:
            if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts[weaponOffset + handPosition].Rate) {
                setIsFiring(handPosition, true);
                FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, handPosition);
                if (handPosition == 1) {
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

    void handleAnimation(int handPosition) {
        // Name of animation, i.e. ShootR, SlashL, etc
        string animationStr = animationString(handPosition);

        if (getIsFiring(handPosition)) {
            // Rotate arm to point to where you are looking (left hand is opposite)
            float x = cam.transform.rotation.eulerAngles.x * (handPosition == LEFT_HAND ? -1 : 1);

            // Tweaks
            switch (curGeneralWeaponTypes[weaponOffset + handPosition]) {
                case (int)GeneralWeaponTypes.RANGED:
                animator.SetBool(animationStr, true);
                break;
                case (int)GeneralWeaponTypes.MELEE:
                if (curSpecialWeaponTypes[weaponOffset + handPosition] == (int)SpecialWeaponTypes.SHL)//sword
                    Combo.Slash(handPosition);
                else//spear
                    Combo.Smash(handPosition);
                break;
                case (int)GeneralWeaponTypes.SHIELD:
                animator.SetBool(animationStr, true);
                break;
                case (int)GeneralWeaponTypes.RCL:
                animator.SetBool(animationStr, true);
                break;
                case (int)GeneralWeaponTypes.BCN:
                animator.SetBool(animationStr, true);
                //animator.SetBool ("BCNPose", false);
                break;
                case (int)GeneralWeaponTypes.ENG:
                animator.SetBool(animationStr, true);
                break;
            }
        } else {// melee is set to false by animation
            if (curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.RANGED || curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.RCL || curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.SHIELD)
                animator.SetBool(animationStr, false);
            else if (curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.BCN)
                animator.SetBool("BCNShoot", false);
            else if (curGeneralWeaponTypes[weaponOffset + handPosition] == (int)GeneralWeaponTypes.ENG) {
                animator.SetBool(animationStr, false);
            }
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
        Invoke("SwitchWeaponsBegin", 1f);
    }

    void SwitchWeaponsBegin() {
        if (isDead) {
            return;
        }
        // Stop current attacks
        setIsFiring(LEFT_HAND, false);
        setIsFiring(RIGHT_HAND, false);

        StopCurWeaponAnimations();

        // Switch weapons by toggling each weapon's activeSelf
        ActivateWeapons();

        weaponOffset = (weaponOffset + 2) % 4;
        if (photonView.isMine) SetWeaponOffsetProperty(weaponOffset);
        HeatBar.UpdateHeatBar();
        MechIK.UpdateMechIK();
        SetSlashDetector();
        FindTrail();

        UpdateArmAnimatorState();
        //Switch movement clips
        ChangeMovementClips(weaponScripts[weaponOffset].isTwoHanded ? 1 : 0);

        //Check crosshair
        crosshair.UpdateCrosshair();

        isSwitchingWeapon = false;
    }

    void SetWeaponOffsetProperty(int weaponOffset) {
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("weaponOffset", weaponOffset);
        photonView.owner.SetCustomProperties(h);
    }

    void StopCurWeaponAnimations() {
        animator.SetBool(AnimatorVars.BCNPose_id, false);

        string strL = animationString(LEFT_HAND), strR = animationString(RIGHT_HAND);
        if (strL != "" && curSpecialWeaponTypes[weaponOffset] != (int)GeneralWeaponTypes.MELEE) { // not empty weapon or melee
            animator.SetBool(strL, false);
        }
        if (strR != "" && curSpecialWeaponTypes[weaponOffset + 1] != (int)GeneralWeaponTypes.MELEE) {
            animator.SetBool(strR, false);
        }

        if (bulletCoroutine != null)
            StopCoroutine(bulletCoroutine);
    }

    void ActivateWeapons() {
        for (int i = 0; i < weapons.Length; i++) {
            weapons[i].SetActive(!weapons[i].activeSelf);
        }
    }


    public void UpdateArmAnimatorState() {
        for (int i = 0; i < 2; i++) {
            switch (curSpecialWeaponTypes[weaponOffset + i]) {
                case (int)SpecialWeaponTypes.APS:
                animator.Play("APS", 1 + i);//left hand is layer 1
                break;
                case (int)SpecialWeaponTypes.BRF:
                animator.Play("BRF", 1 + i);
                break;
                case (int)SpecialWeaponTypes.ENG:
                animator.Play("ENG", 1 + i);
                break;
                case (int)SpecialWeaponTypes.LMG:
                animator.Play("LMG", 1 + i);
                break;
                case (int)SpecialWeaponTypes.RCL:
                animator.Play("RCL", 1);
                animator.Play("RCL", 2);
                i++;
                break;
                case (int)SpecialWeaponTypes.SGN:
                animator.Play("SGN", 1 + i);
                break;
                case (int)SpecialWeaponTypes.SHS:
                animator.Play("SHS", 1 + i);
                break;
                case (int)SpecialWeaponTypes.BCN:
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

    public void ChangeMovementClips(int num) {
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

    bool getIsFiring(int handPosition) {
        return handPosition == LEFT_HAND ? fireL : fireR;
    }

    void setIsFiring(int handPosition, bool isFiring) {
        if (handPosition == LEFT_HAND) {
            fireL = isFiring;
        } else {
            fireR = isFiring;
        }
    }

    public void UpdateSpecialCurWeaponType() {//those types all use different animations
        for (int i = 0; i < 4; i++) {
            string name = bm.curWeaponNames[i];
            if (name.Contains("APS")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.APS;
            } else if (name.Contains("RF")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.BRF;
            } else if (name.Contains("SGN")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.SGN;
            } else if (name.Contains("BCN") || name.Contains("MSR")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.BCN;
            } else if (name.Contains("LMG")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.LMG;
            } else if (name.Contains("ENG")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.ENG;
            } else if (name.Contains("RCL")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.RCL;
            } else if (name.Contains("DR")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.ADR;
            } else if (name.Contains("SHL") || name.Contains("LSN")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.SHL;
            } else if (name.Contains("SHS")) {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.SHS;
            } else {
                curSpecialWeaponTypes[i] = (int)SpecialWeaponTypes.EMPTY;
            }
        }
    }

    public void UpdateGeneralCurWeaponType() {
        for (int i = 0; i < 4; i++) {
            switch (curSpecialWeaponTypes[i]) {
                case (int)SpecialWeaponTypes.APS:
                case (int)SpecialWeaponTypes.BRF:
                case (int)SpecialWeaponTypes.SGN:
                case (int)SpecialWeaponTypes.LMG:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.RANGED;
                break;
                case (int)SpecialWeaponTypes.ADR:
                case (int)SpecialWeaponTypes.SHL:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.MELEE;
                break;
                case (int)SpecialWeaponTypes.SHS:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.SHIELD;
                break;
                case (int)SpecialWeaponTypes.RCL:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.RCL;
                break;
                case (int)SpecialWeaponTypes.ENG:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.ENG;
                break;
                case (int)SpecialWeaponTypes.BCN:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.BCN;
                break;
                case (int)SpecialWeaponTypes.EMPTY:
                curGeneralWeaponTypes[i] = (int)GeneralWeaponTypes.EMPTY;
                break;
            }
        }
    }

    [PunRPC]
    void SetOverHeat(bool b, int weaponOffset) {//let other player know if shield overheat
        is_overheat[weaponOffset] = b;
    }

    bool CheckIsMeleeByStr(string weaponName) {
        return (weaponName.Contains("DR") || weaponName.Contains("SHL") || weaponName.Contains("LSN"));
    }

    string animationString(int handPosition) {
        switch (curGeneralWeaponTypes[weaponOffset + handPosition]) {
            case (int)GeneralWeaponTypes.RCL:
            case (int)GeneralWeaponTypes.BCN:
            return weaponScripts[weaponOffset + handPosition].Animation;
            case (int)GeneralWeaponTypes.EMPTY:
            return "";
            default:
            return weaponScripts[weaponOffset + handPosition].Animation + (handPosition == LEFT_HAND ? "L" : "R");
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
                if (collider.gameObject.name != "Slash Detector")
                    collider.gameObject.layer = default_layer;
            } else if (collider.gameObject.name != "Slash Detector")
                collider.gameObject.layer = 8;
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
        if (curGeneralWeaponTypes[weaponOffset] == (int)GeneralWeaponTypes.MELEE) {
            trailL = weapons[weaponOffset].GetComponentInChildren<XWeaponTrail>(true);
            if (trailL != null) {
                trailL.Deactivate();
            }
        } else {
            trailL = null;
        }

        if (curGeneralWeaponTypes[weaponOffset + 1] == (int)GeneralWeaponTypes.MELEE) {
            trailR = weapons[weaponOffset + 1].GetComponentInChildren<XWeaponTrail>(true);
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
