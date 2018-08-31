using System.Collections;
using UnityEngine;

public class MechCombat : Combat {
    [SerializeField] private EffectController EffectController;
    [SerializeField] private Camera MainCam;
    private SkillController SkillController;
    private MechController MechController;
    private BuildMech bm;
    private HUD HUD;
    private Crosshair crosshair;

    // Combat variables
    private bool isWeaponOffsetSynced = false, onSkill;
    private int weaponOffset = 0;
    private bool isMeleePlaying = false;
    private int scanRange, MechSize;
    public bool IsSwitchingWeapon { get; private set; }
    public bool CanMeleeAttack = true;//This is false after melee attack in air

    //Switching weapon action
    private Coroutine SwitchWeaponCoroutine;
    public delegate void MechCombatAction();
    public MechCombatAction OnWeaponSwitched;

    // GameObjects in Game scene
    private GameObject BulletCollector;//collect all bullets

    //Animator
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController = null;
    private AnimationClipOverrides clipOverrides;

    //Movement clips
    private MovementClips defaultMovementClips, TwoHandedMovementClips;

    protected override void Awake() {
        base.Awake();
        InitComponents();
        LoadMovementClips();
        InitAnimatorControllers();

        RegisterOnMechBuilt();
        RegisterOnWeaponSwitched();
        RegisterOnSkill();
    }

    private void InitComponents() {
        Transform currentMech = transform.Find("CurrentMech");

        bm = GetComponent<BuildMech>();
        SkillController = GetComponent<SkillController>();
        MechController = GetComponent<MechController>();
        animator = currentMech.GetComponent<Animator>();
        crosshair = (MainCam == null) ? null : MainCam.GetComponent<Crosshair>();
        HUD = GetComponent<HUD>();
    }

    private void LoadMovementClips() {
        defaultMovementClips = Resources.Load<MovementClips>("Data/MovementClip/Default");
        TwoHandedMovementClips = Resources.Load<MovementClips>("Data/MovementClip/TwoHanded");
    }

    private void InitAnimatorControllers() {
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
    }

    private void RegisterOnMechBuilt() {
        bm.OnMechBuilt += OnMechBuilt;
    }

    private void OnMechBuilt() {
        LoadMechProperties();
        InitCombatVariables();
        InitMechStats();
    }

    private void RegisterOnWeaponSwitched() {
        OnWeaponSwitched += OnWeaponSwitchedAction;
    }

    private void OnWeaponSwitchedAction() {
        UpdateWeightRelatedVars();
        UpdateMovementClips();
    }

    private void RegisterOnSkill() {
        if (SkillController != null) SkillController.OnSkill += OnSkill;
    }

    protected override void Start() {
        base.Start();
        InitGameObjects();
    }

    private void LoadMechProperties() {
        MAX_HP = bm.MechProperty.HP;
        MAX_EN = bm.MechProperty.EN;
        MechSize = bm.MechProperty.Size;
        energyProperties.minENRequired = bm.MechProperty.MinENRequired;
        energyProperties.energyOutput = bm.MechProperty.ENOutputRate - bm.MechProperty.EnergyDrain; //TODO : improve this
        scanRange = bm.MechProperty.ScanRange;
    }

    private void UpdateWeightRelatedVars() {
        int TotalWeight = bm.MechProperty.Weight + ((bm.WeaponDatas[weaponOffset] == null) ? 0 : bm.WeaponDatas[weaponOffset].weight) + ((bm.WeaponDatas[weaponOffset + 1] == null) ? 0 : bm.WeaponDatas[weaponOffset + 1].weight);

        energyProperties.jumpENDrain = bm.MechProperty.GetJumpENDrain(TotalWeight);
        energyProperties.dashENDrain = bm.MechProperty.DashENDrain;
    }

    private void InitMechStats() {//call also when respawn
        CurrentHP = MAX_HP;
        CurrentEN = MAX_EN;
    }

    private void InitGameObjects() {
        BulletCollector = GameObject.Find("BulletCollector");
    }

    public void InitCombatVariables() {// this will be called also when respawn
        SyncWeaponOffset();

        if (photonView.isMine) {
            SetWeaponOffsetProperty(weaponOffset);
            animator.Play("Walk", 0);
        }
        if (SwitchWeaponCoroutine != null) {//die when switching weapons
            StopCoroutine(SwitchWeaponCoroutine);
            IsSwitchingWeapon = false;
            SwitchWeaponCoroutine = null;
        }
        onSkill = false;
    }

    private void SyncWeaponOffset() {
        if (photonView.owner == null) return;

        if (!photonView.isMine && !isWeaponOffsetSynced) {
            if (photonView.owner.CustomProperties["weaponOffset"] != null) {
                weaponOffset = int.Parse(photonView.owner.CustomProperties["weaponOffset"].ToString());
            } else//the player may just initialize
                weaponOffset = 0;

            isWeaponOffsetSynced = true;
        } else {
            weaponOffset = 0;
        }
    }

    [PunRPC]
    private void Shoot(int hand, Vector3 direction, int target_pvID, bool isShield, int target_handOnShield) {
        RangedWeapon rangedWeapon = bm.Weapons[weaponOffset + hand] as RangedWeapon;
        if (rangedWeapon == null) { Debug.LogWarning("Cannot cast from source type to RangedWeapon."); return; }

        rangedWeapon.Shoot(hand, direction, target_pvID, isShield, target_handOnShield);
    }

    [PunRPC]
    public override void OnHit(int damage, PhotonPlayer shooter, string weaponName) {//simple on hit
        base.OnHit(damage, shooter, weaponName);

        if (photonView.isMine) {//TODO : improve anti hack
            SkillController.IncreaseSP(damage / 2);
        }
    }

    [PunRPC]
    public override void OnHit(int damage, PhotonPlayer shooter, int weapPos) {//If weapon has some effects like slowing mech down
        base.OnHit(damage, shooter, weapPos);

        if (photonView.isMine) {//TODO : improve anti hack
            SkillController.IncreaseSP(damage / 2);
        }
    }

    [PunRPC]
    private void ShieldOnHit(int damage, PhotonPlayer shooter, int weapPos, int shieldPos, int attackType) {//attackType :　Shield.defendtype
        if (isDead) { return; }

        Shield shield = bm.Weapons[shieldPos] as Shield;
        if (shield == null) { Debug.LogWarning("shield is null."); return; }

        CurrentHP -= shield.DecreaseDmg(damage, attackType);

        //if (shield.IsOverHeat()) {
        //    //do nothing
        //} else {
        //    shield.IncreaseHeat();
        //}

        SkillController.IncreaseSP(damage / 2);

        GameObject shooterMech = ((GameObject)shooter.TagObject);
        BuildMech shooterBM = (shooterMech == null) ? null : shooterMech.GetComponent<BuildMech>();
        if (shooterBM == null) { Debug.LogWarning("OnHit bm null : tag Object is not init."); return; }

        Weapon shooterWeapon = shooterBM.Weapons[weapPos];
        if (shooterWeapon == null) { Debug.LogWarning("shooterWeapon is null."); return; }

        shooterWeapon.OnTargetEffect(gameObject, true);

        //if (photonView.isMine) {//TODO :　improve anti-hack
        //    if (shooterBM.Weapons[shieldPos] != null && !shooterBM.Weapons[shieldPos].IsOverHeat()) {
        //        shooterBM.Weapons[shieldPos].IncreaseHeat();
        //    }
        //    SkillController.IncreaseSP(damage / 2);
        //}   

        if (CurrentHP <= 0 && PhotonNetwork.isMasterClient) {
            photonView.RPC("DisablePlayer", PhotonTargets.All, shooter, shooterWeapon.WeaponName);
        }
    }

    //[PunRPC]
    //private void WeaponOverHeat(int pos, bool b) {
    //    Debug.Log("Set WeaponOverHeat : " + pos + " b : " + b);

    //    if (bm.Weapons[pos] == null) { Debug.LogError("WeaponOverHeat : weapon is null");return;}
    //    bm.Weapons[pos].SetOverHeat(b);
    //}

    [PunRPC]
    private void OnLocked() {//TODO : remake this (using raise event)
        if (photonView.isMine) crosshair.ShowLocked();
    }

    [PunRPC]
    public void KnockBack(Vector3 dir, float length) {//TODO : check this
        GetComponent<CharacterController>().Move(dir * length);
    }

    public void Skill_KnockBack(float length) {//TODO : check this
        Transform skillUser = SkillController.GetSkillUser();

        MechController.SkillSetMoving((skillUser != null) ? (transform.position - skillUser.position).normalized * length : -transform.forward * length);
    }

    [PunRPC]
    protected override void DisablePlayer(PhotonPlayer shooter, string weapon) {
        gm.OnPlayerDead(photonView.owner, shooter, weapon);//Broadcast msg

        isDead = true;

        if (photonView.isMine) {
            gm.SetBlockInput(BlockInputSet.Elements.PlayerIsDead, true);
        }

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
            gm.EnableRespawnPanel(true);
        }

        OnMechEnabled(false);

        EnableAllRenderers(false);
        animator.enabled = false;
    }

    // Enable MechController, Crosshair, Renderers, set layer to player layer, move player to spawn position
    [PunRPC]
    private void EnablePlayer(int respawnPoint, int mech_num) {
        base.EnablePlayer();

        bm.SetMechNum(mech_num);
        animator.enabled = true;

        if (photonView.isMine) { // build mech also init MechCombat
            Mech m = UserData.myData.Mech[mech_num];
            bm.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);

            gm.SetBlockInput(BlockInputSet.Elements.PlayerIsDead, false);
        }

        //TODO : improve this
        //this is to avoid trigger flag  ( when cc is on , setting the player position results smoothly moving to it
        gameObject.layer = default_layer;
        GetComponent<CharacterController>().enabled = false;
        transform.position = gm.GetRespawnPointPosition(respawnPoint);
        GetComponent<CharacterController>().enabled = true;
        gameObject.layer = playerlayer;

        isDead = false;
        EffectController.RespawnEffect();//TODO : remake this
    }

    protected override void Update() {
        base.Update();

        if (!photonView.isMine || gm.IsGameEnding() || !GameManager.gameIsBegin) return; //TODO : improve these checks
        if (onSkill || gm.BlockInput || IsSwitchingWeapon) return;

        bm.Weapons[weaponOffset].HandleCombat();
        bm.Weapons[weaponOffset + 1].HandleCombat();

        HandleSkillInput();
        HandleSwitchWeapon();
    }

    private void LateUpdate() {
        if (!photonView.isMine) return;

        bm.Weapons[weaponOffset].HandleAnimation();
        bm.Weapons[weaponOffset + 1].HandleAnimation();
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

    private void HandleSwitchWeapon() {
        if (Input.GetKeyDown(KeyCode.R) && !IsSwitchingWeapon && !isDead) {//TODO : anti-hack en
            CurrentEN -= (CurrentEN >= MAX_EN / 3) ? MAX_EN / 3 : CurrentEN;

            photonView.RPC("CallSwitchWeapons", PhotonTargets.All, null);
        }
    }

    [PunRPC]
    private void CallSwitchWeapons() {//Play switch weapon animation
        EffectController.SwitchWeaponEffect();//TODO : remake this
        IsSwitchingWeapon = true;
        SwitchWeaponCoroutine = StartCoroutine(SwitchWeaponsBegin());

        if (photonView.isMine) SetWeaponOffsetProperty(weaponOffset);
    }

    private IEnumerator SwitchWeaponsBegin() {
        yield return new WaitForSeconds(1);
        if (isDead) {
            SwitchWeaponCoroutine = null;
            IsSwitchingWeapon = false;
            yield break;
        }
        // Stop current attacks and reset
        if (bm.Weapons[weaponOffset] != null) bm.Weapons[weaponOffset].OnSwitchedWeaponAction();
        if (bm.Weapons[weaponOffset + 1] != null) bm.Weapons[weaponOffset + 1].OnSwitchedWeaponAction();

        weaponOffset = (weaponOffset + 2) % 4;

        // Switch weapons by enable/disable renderers
        ActivateWeapons();

        if (OnWeaponSwitched != null) OnWeaponSwitched();

        SwitchWeaponCoroutine = null;
        IsSwitchingWeapon = false;
    }

    private void SetWeaponOffsetProperty(int weaponOffset) {
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("weaponOffset", weaponOffset);
        photonView.owner.SetCustomProperties(h);
    }

    private void ActivateWeapons() {//Not using SetActive because it causes weapon Animator to bind the wrong rotation if the weapon animation is not finished (SMG reload)
        for (int i = 0; i < bm.Weapons.Length; i++) {
            if (bm.Weapons[i] != null) {
                bm.Weapons[i].ActivateWeapon(i == weaponOffset || i == weaponOffset + 1);
            }
        }
    }

    public void UpdateMovementClips() {
        if (bm.WeaponDatas == null) return;
        bool isCurrentWeaponTwoHanded = (bm.WeaponDatas[(weaponOffset) % 4] != null && bm.WeaponDatas[(weaponOffset) % 4].twoHanded);

        MovementClips movementClips = (isCurrentWeaponTwoHanded) ? TwoHandedMovementClips : defaultMovementClips;
        for (int i = 0; i < movementClips.clips.Length; i++) {
            clipOverrides[movementClips.clipnames[i]] = movementClips.clips[i];
        }
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public void EnableAllRenderers(bool b) {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = b;
        }
        if (b) ActivateWeapons();
    }

    public void EnableAllColliders(bool b) {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders) {
            if (!b) {
                if (collider.gameObject.layer != ignoreRayCastLayer)//slashDetector is on IgnoreRayCast layer
                    collider.gameObject.layer = default_layer;
            } else if (collider.gameObject.layer != ignoreRayCastLayer)
                collider.gameObject.layer = playerlayer;
        }
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
        if (bm.Weapons[weaponOffset] != null) bm.Weapons[weaponOffset].OnSkillAction(b);
        if (bm.Weapons[weaponOffset + 1] != null) bm.Weapons[weaponOffset + 1].OnSkillAction(b);

        if (b) {
            gameObject.layer = default_layer;
            EnableAllColliders(false);
            GetComponent<Collider>().enabled = true;//set to true to trigger exit (while layer changed)
            animator.Play("Walk", 0);
        } else {
            if (!isDead) {
                gameObject.layer = playerlayer;
                EnableAllColliders(true);
            }
        }
    }

    public int GetCurrentWeaponOffset() {
        return weaponOffset;
    }

    public Camera GetCamera() {
        return MainCam;
    }

    public void SetMeleePlaying(bool isPlaying) {
        isMeleePlaying = isPlaying;

        MechController.onInstantMoving = isPlaying;
    }

    public bool IsMeleePlaying() {
        return isMeleePlaying;
    }

    public void OnAttackStateEnter<T>(int hand, MechStateMachineBehaviour state) {
        if (bm.Weapons[weaponOffset + hand] != null && bm.Weapons[weaponOffset + hand] is T) {
            bm.Weapons[weaponOffset + hand].OnAttackStateEnter(state);
        } else {
            Debug.LogWarning("weapon is null or type mismatch");
        }
    }

    public void OnAttackStateExit<T>(int hand, MechStateMachineBehaviour state) {
        if (bm.Weapons[weaponOffset + hand] != null && bm.Weapons[weaponOffset + hand] is T) {
            bm.Weapons[weaponOffset + hand].OnAttackStateExit(state);
        } else {
            Debug.LogWarning("weapon is null or type mismatch");
        }
    }

    public void OnAttackStateMachineExit<T>(int hand, MechStateMachineBehaviour state) {
        if (bm.Weapons[weaponOffset + hand] != null && bm.Weapons[weaponOffset + hand] is T) {
            bm.Weapons[weaponOffset + hand].OnAttackStateMachineExit(state);
        } else {
            Debug.LogWarning("weapon is null or type mismatch");
        }
    }
}