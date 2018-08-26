using System.Collections;
using UnityEngine;

public class MechCombat : Combat {
    [SerializeField] private EffectController EffectController;
    [SerializeField] private Camera MainCam;
    [SerializeField] private SkillController SkillController;
    private MechController MechController;
    private BuildMech bm;
    private HUD HUD;
    private Crosshair crosshair;

    // Combat variables
    private bool isWeaponOffsetSynced = false, onSkill;
    private int weaponOffset = 0;
    public bool IsSwitchingWeapon { get; private set; }
    public bool CanMeleeAttack = true;
    private bool isMeleePlaying = false;
    public int scanRange, MechSize, TotalWeight;

    //Switching weapon action
    public delegate void MechCombatAction();
    public MechCombatAction OnWeaponSwitched;
    private Coroutine SwitchWeaponCoroutine;

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
        TotalWeight = bm.MechProperty.Weight;
        energyProperties.minENRequired = bm.MechProperty.MinENRequired;
        energyProperties.energyOutput = bm.MechProperty.ENOutputRate - bm.MechProperty.EnergyDrain; //TODO : improve this
        scanRange = bm.MechProperty.ScanRange;
    }

    private void UpdateWeightRelatedVars() {
        TotalWeight = bm.MechProperty.Weight + ((bm.WeaponDatas[weaponOffset] == null) ? 0 : bm.WeaponDatas[weaponOffset].weight) + ((bm.WeaponDatas[weaponOffset + 1] == null) ? 0 : bm.WeaponDatas[weaponOffset + 1].weight);

        energyProperties.jumpENDrain = bm.MechProperty.GetJumpENDrain(TotalWeight);
        energyProperties.dashENDrain = bm.MechProperty.DashENDrain;

        //TODO : move this out
        MechController.UpdateWeightRelatedVars(bm.MechProperty.Weight, ((bm.WeaponDatas[weaponOffset] == null) ? 0 : bm.WeaponDatas[weaponOffset].weight) + ((bm.WeaponDatas[weaponOffset + 1] == null) ? 0 : bm.WeaponDatas[weaponOffset + 1].weight));
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
    public override void OnHit(int damage, int shooter_viewID, string weapon, bool isSlowDown = false) {//TODO : improve
        if (isDead) { return; }

        //if (CheckIsSwordByStr(weapon)) {//TODO : remake this
        //    EffectController.SlashOnHitEffect(false, 0);
        //} else if (CheckIsSpearByStr(weapon)) {
        //    EffectController.SlashOnHitEffect(false, 0);
        //}

        //Apply slow down
        if (photonView.isMine) {//TODO : improve anti hack
            if (isSlowDown) MechController.SlowDown();

            SkillController.IncreaseSP((int)damage / 2);
        }

        CurrentHP -= damage;

        if (CurrentHP <= 0 && PhotonNetwork.isMasterClient) {//sync disable player
            photonView.RPC("DisablePlayer", PhotonTargets.All, shooter_viewID, weapon);
        }
    }

    [PunRPC]
    private void ShieldOnHit(int damage, int shooter_viewID, int shield, string weapon) {
        if (isDead) { return; }

        if (CheckIsSwordByStr(weapon) || CheckIsSpearByStr(weapon)) {//TODO : remake this
            EffectController.SlashOnHitEffect(true, shield);
        }

        CurrentHP -= damage;

        if (photonView.isMine) {//heat
            if (bm.Weapons[weaponOffset + shield] != null && !bm.Weapons[weaponOffset + shield].IsOverHeat()) {
                bm.Weapons[weaponOffset + shield].IncreaseHeat();
            }
            SkillController.IncreaseSP(damage / 2);
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
    private void OnHeal(int viewID, int amount) {//TODO : remake this
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
    private void OnLocked() {//TODO : remake this
        if (photonView.isMine) crosshair.ShowLocked();
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
        bm.SetMechNum(mech_num);
        animator.enabled = true;
        if (photonView.isMine) { // build mech also init MechCombat
            Mech m = UserData.myData.Mech[mech_num];
            bm.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R, m.skillIDs);

            gm.SetBlockInput(BlockInputSet.Elements.PlayerIsDead, false);
        }

        InitMechStats();

        OnMechEnabled(true);

        //this is to avoid trigger flag //TODO : improve this
        gameObject.layer = default_layer;
        GetComponent<CharacterController>().enabled = false;
        transform.position = gm.GetRespawnPointPosition(respawnPoint);
        GetComponent<CharacterController>().enabled = true;
        gameObject.layer = playerlayer;

        isDead = false;
        EffectController.RespawnEffect();
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();

        if (!photonView.isMine || gm.IsGameEnding() || !GameManager.gameIsBegin) return; //TODO : improve these checks
        if (onSkill || gm.BlockInput || IsSwitchingWeapon) return;

        bm.Weapons[weaponOffset].HandleCombat();
        bm.Weapons[weaponOffset+1].HandleCombat();

        HandleSkillInput();
        HandleSwitchWeapon();
    }

    private void LateUpdate() {
        if (!photonView.isMine)return;

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
        if (Input.GetKeyDown(KeyCode.R) && !IsSwitchingWeapon && !isDead) {
            CurrentEN -= (CurrentEN >= MAX_EN / 3) ? MAX_EN / 3 : CurrentEN;

            photonView.RPC("CallSwitchWeapons", PhotonTargets.All, null);
        }
    }

    [PunRPC]
    private void CallSwitchWeapons() {//Play switch weapon animation
        EffectController.SwitchWeaponEffect();
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
        if(bm.Weapons[weaponOffset]!=null)bm.Weapons[weaponOffset].OnSkillAction(b);
        if (bm.Weapons[weaponOffset+1] != null) bm.Weapons[weaponOffset+1].OnSkillAction(b);

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
        if(bm.Weapons[weaponOffset + hand] != null && bm.Weapons[weaponOffset + hand] is T) {
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