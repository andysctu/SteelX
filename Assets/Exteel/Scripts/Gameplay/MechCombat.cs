using System.Collections;
using StateMachine;
using UnityEngine;
using Weapons;

public class MechCombat : Combat {
    [SerializeField] private EffectController EffectController;
    [SerializeField] private Camera MainCam;
    private SkillController SkillController;
    private MechController MechController;
    private BuildMech bm;
    private HUD HUD;
    private CrosshairController _crosshairController;

    // Combat variables
    public bool OnSkill;
    private bool isWeaponOffsetSynced = false;
    private int weaponOffset = 0;
    private int scanRange, MechSize;

    //Switching weapon action
    private Coroutine SwitchWeaponCoroutine;
    public delegate void MechCombatAction();
    public MechCombatAction OnWeaponSwitched;

    // GameObjects in Game scene
    private GameObject BulletCollector;//collect all bullets

    //Animator
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private AnimationClipOverrides clipOverrides;

    //Movement clips
    private MovementClips defaultMovementClips, TwoHandedMovementClips;

    //private PhotonPlayer _owner;

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
        bm = GetComponent<BuildMech>();
        SkillController = GetComponent<SkillController>();
        MechController = GetComponent<MechController>();
        animator = GetComponent<Animator>();
        _crosshairController = (MainCam == null) ? null : MainCam.GetComponent<CrosshairController>();
        HUD = GetComponent<HUD>();
    }

    private void LoadMovementClips() {
        defaultMovementClips = Resources.Load<MovementClips>("Data/MovementClip/Default");
        TwoHandedMovementClips = Resources.Load<MovementClips>("Data/MovementClip/TwoHanded");
    }

    private void InitAnimatorControllers() {
        if (animatorOverrideController != null) return;

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);
    }

    private void RegisterOnMechBuilt() {
        bm.OnMechBuilt += OnMechBuilt;
    }

    private void OnMechBuilt() {
        //_owner = bm.GetOwner();

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

        // Stop current attacks and reset
        for (int i = 0; i < bm.Weapons.Length; i++) {
            if (bm.Weapons[i] != null) {
                bm.Weapons[i].OnWeaponSwitchedAction(i == weaponOffset || i == weaponOffset + 1);
            }
        }

        // Switch weapons by enable/disable renderers
        ActivateWeapons();
    }

    private void RegisterOnSkill() {
        if (SkillController != null) SkillController.OnSkill += OnSkillAction;
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

        //if (_owner.IsLocal) {
        //    SetWeaponOffsetProperty(weaponOffset);
        //    animator.Play("Walk", 0);
        //}
        if (SwitchWeaponCoroutine != null) {//die when switching weapons
            StopCoroutine(SwitchWeaponCoroutine);
            IsSwitchingWeapon = false;
            SwitchWeaponCoroutine = null;
        }
        OnSkill = false;
    }

    private void SyncWeaponOffset() {
        //if (_owner == null) return;

        //if (!_owner.IsLocal && !isWeaponOffsetSynced) {
        //    if (_owner.CustomProperties["weaponOffset"] != null) {
        //        weaponOffset = int.Parse(_owner.CustomProperties["weaponOffset"].ToString());
        //    } else//the player may just initialize
        //        weaponOffset = 0;
		//
        //    isWeaponOffsetSynced = true;
        //} else {
        //    weaponOffset = 0;
        //}
    }

    public override void Attack(int weapPos, Vector3 direction, int damage, int[] targetPvIDs, int[] specIDs, int[] additionalFields = null){//always called by master
        //PhotonView.RPC("AttackRpc", PhotonTargets.Others, weapPos, direction, damage, targetPvIDs, specIDs, additionalFields);
    }

    //[PunRPC]
    public void AttackRpc(int weapPos, Vector3 direction, int damage, int[] targetPvIDs, int[] specIDs, int[] additionalFields = null) {//play effect use
        if(bm.Weapons == null || bm.Weapons[weapPos] == null)return;

        bm.Weapons[weapPos].AttackRpc(direction, damage, targetPvIDs, specIDs, additionalFields);
    }

    //[PunRPC]
    //private void WeaponOverHeat(int pos, bool b) {
    //    Debug.Log("Set WeaponOverHeat : " + pos + " b : " + b);

    //    if (bm.Weapons[pos] == null) { Debug.LogError("WeaponOverHeat : weapon is null");return;}
    //    bm.Weapons[pos].SetOverHeat(b);
    //}

    //[PunRPC]
    private void OnLocked() {
        //if (_owner.IsLocal) _crosshairController.ShowLocked();
    }

    public void Skill_KnockBack(float length) {//TODO : check this
        Transform skillUser = SkillController.GetSkillUser();
        Debug.LogError("no implemented");
    }

    //[PunRPC]
    //protected override void DisablePlayer(PhotonPlayer shooter, string weapon) {
    //    gm.OnPlayerDead(_owner, shooter, weapon);//Broadcast msg
	//
    //    isDead = true;
	//
    //    if (_owner.IsLocal) {
    //        gm.SetBlockInput(BlockInputSet.Elements.PlayerIsDead, true);
    //    }
	//
    //    CurrentHP = 0;
	//
    //    gameObject.layer = default_layer;
	//
    //    StartCoroutine(DisablePlayerWhenNotOnSkill());
	//
    //    MechController.enabled = false;//stop control immediately
    //    EnableAllColliders(false);
    //    GetComponent<Collider>().enabled = true;//set to true to trigger exit (while layer changed) //TODO : check this if necessary
    //}

    private IEnumerator DisablePlayerWhenNotOnSkill() {
        yield return new WaitWhile(() => OnSkill);

        //if (_owner.IsLocal) {
        //    CurrentHP = 0;
        //    gm.EnableRespawnPanel(true);
        //}

        OnMechEnabled(false);

        EnableAllRenderers(false);
        animator.enabled = false;
    }

    // Enable MechController, Crosshair, Renderers, set layer to player layer, move player to spawn position
    //[PunRPC]
    private void EnablePlayer(int respawnPoint, int mech_num) {
        base.EnablePlayer();

        bm.SetMechNum(mech_num);
        animator.enabled = true;

        //if (_owner.IsLocal) { // build mech also init MechCombat //todo :check this
        //    Mech m = UserData.myData.Mech[mech_num];
        //    bm.Build(PhotonNetwork.player, m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);
		//
        //    gm.SetBlockInput(BlockInputSet.Elements.PlayerIsDead, false);
        //}

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

    private void LateUpdate() {
        if (bm.Weapons[weaponOffset] != null) bm.Weapons[weaponOffset].HandleAnimation();
        if (bm.Weapons[weaponOffset + 1] != null) bm.Weapons[weaponOffset + 1].HandleAnimation();
    }

    public void ProcessInputs(usercmd cmd){
        if (gm == null || gm.IsGameEnding() || !gm.GameIsBegin) return; //TODO : improve these checks
        //if (onSkill || gm.BlockInput || IsSwitchingWeapon) return;

        if (bm.Weapons[weaponOffset] != null) bm.Weapons[weaponOffset].HandleCombat(cmd);
        if (bm.Weapons[weaponOffset + 1] != null) bm.Weapons[weaponOffset + 1].HandleCombat(cmd);

        HandleSkillInput();
        HandleSwitchWeapon(cmd);
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

    private void HandleSwitchWeapon(usercmd cmd) {
        if (cmd.buttons[(int)UserButton.R] && !IsSwitchingWeapon && !isDead) {//TODO : anti-hack en
            CurrentEN -= (CurrentEN >= MAX_EN / 3) ? MAX_EN / 3 : CurrentEN;

            //PhotonView.RPC("CallSwitchWeapons", PhotonTargets.All, null);
        }
    }

    //[PunRPC]
    private void CallSwitchWeapons() {//Play switch weapon animation
        EffectController.SwitchWeaponEffect();//TODO : remake this
        IsSwitchingWeapon = true;
        SwitchWeaponCoroutine = StartCoroutine(SwitchWeaponsBegin());

        //if (_owner.IsLocal) SetWeaponOffsetProperty(weaponOffset);
    }

    private IEnumerator SwitchWeaponsBegin() {
        yield return new WaitForSeconds(1);
        if (isDead) {
            SwitchWeaponCoroutine = null;
            IsSwitchingWeapon = false;
            yield break;
        }

        weaponOffset = (weaponOffset + 2) % 4;

        if (OnWeaponSwitched != null) OnWeaponSwitched();

        SwitchWeaponCoroutine = null;
        IsSwitchingWeapon = false;
    }

    private void SetWeaponOffsetProperty(int weaponOffset) {
        //ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        //h.Add("weaponOffset", weaponOffset);
        //_owner.SetCustomProperties(h);
    }

    private void ActivateWeapons() {//Not using SetActive because it causes weapon Animator to bind the wrong rotation if the weapon animation is not finished (SMG reload)
        for (int i = 0; i < bm.Weapons.Length; i++){
            if (bm.Weapons[i] != null){
                bm.Weapons[i].ActivateWeapon(i == weaponOffset || i == weaponOffset + 1);
            }
        }
    }

    private void UpdateMovementClips() {
        if (bm.WeaponDatas == null) return;
        bool isCurrentWeaponTwoHanded = (bm.WeaponDatas[weaponOffset % 4] != null && bm.WeaponDatas[(weaponOffset) % 4].IsTwoHanded);

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

    public void IncrementEN(float msec) {
        CurrentEN += energyProperties.energyOutput * msec;
        if (CurrentEN > MAX_EN) CurrentEN = MAX_EN;
    }

    public void DecrementEN(float msec) {
        if (MechController.Grounded)
            CurrentEN -= energyProperties.dashENDrain * msec;
        else
            CurrentEN -= energyProperties.jumpENDrain * msec;

        if (CurrentEN < 0) CurrentEN = 0;
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

    private void OnSkillAction(bool b) {
        OnSkill = b;
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

    public override void IncreaseSP(int amount) {
        SkillController.IncreaseSP(amount);
    }

    //public override PhotonPlayer GetOwner(){
    //    return bm.GetOwner();
    //}

    public override Camera GetCamera() {
        return MainCam;
    }

    public override Weapon GetWeapon(int weapPos) {
        return bm.Weapons[weapPos];
    }

    public override WeaponData GetWeaponData(int weaponPos) {
        return bm.WeaponDatas[weaponPos];
    }

    //tmp put here , todo : improve these events
    public void CallShowTrailL(int show) {
        Sword sword = bm.Weapons[weaponOffset] as Sword;
        if (sword != null) sword.EnableWeaponTrail(show == 1);
    }

    public void CallShowTrailR(int show) {
        Sword sword = bm.Weapons[weaponOffset + 1] as Sword;
        if (sword != null) sword.EnableWeaponTrail(show == 1);
    }

    //public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    //    if (stream.isReading){
    //        
    //    } else{
	//
    //    }
	//
    //    for (int i = 0; i < bm.Weapons.Length; i++) {
    //        if (bm.Weapons[i] != null) bm.Weapons[i].OnPhotonSerializeView(stream, info);
    //    }
    //}
}