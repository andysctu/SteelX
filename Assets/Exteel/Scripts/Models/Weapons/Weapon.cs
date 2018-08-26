using UnityEngine;

public abstract class Weapon{
    protected GameObject weapon;
    protected WeaponData data;

    //Components
    protected MechCombat mcbt;
    protected PhotonView photonView;
    protected HeatBar HeatBar;
    protected Animator MechAnimator, WeaponAnimator;
    protected AnimationEventController AnimationEventController;
    protected AnimatorVars AnimatorVars;
    protected AudioSource AudioSource;

    //Another weapon
    protected Weapon anotherWeapon;
    protected WeaponData anotherWeaponData;

    //Weapon infos
    protected Transform WeapPos;
    protected int hand, pos;//Two-handed -> 0
    protected const int LEFT_HAND = 0, RIGHT_HAND = 1;    
    protected float timeOfLastUse;
    public bool allowBothWeaponUsing = true, isFiring = false;

    protected int TerrainLayerMask, PlayerLayerMask;

    public virtual void Init(WeaponData data, int pos, Transform WeapPos, MechCombat mcbt, Animator MechAnimator) {        
        this.data = data;
        this.mcbt = mcbt;
        this.MechAnimator = MechAnimator;
        this.WeapPos = WeapPos;
        this.hand = pos%2;
        this.pos = pos;

        InstantiateWeapon(data);
        InitComponents();
        InitAnotherWeaponInfo();
        InitLayerMask();
        LoadSoundClips();
        SwitchWeaponAnimationClips(WeaponAnimator);
    }

    protected virtual void InstantiateWeapon(WeaponData data) {
        weapon = Object.Instantiate(data.GetWeaponPrefab(hand % 2), Vector3.zero, Quaternion.identity) as GameObject;
        WeaponAnimator = weapon.GetComponent<Animator>();

        AdjustScale(weapon);
        SetWeaponParent(weapon);
    }

    protected virtual void InitComponents() {        
        photonView = mcbt.GetComponent<PhotonView>();
        HeatBar = mcbt.GetComponentInChildren< HeatBar >();
        AnimationEventController = MechAnimator.GetComponent< AnimationEventController >();
        AnimatorVars = mcbt.GetComponentInChildren< AnimatorVars >();
        AddAudioSource(weapon);
    }

    private void InitAnotherWeaponInfo() {
        int weaponOffset = pos-2 >= 0 ? 2 : 0;
        BuildMech bm = mcbt.GetComponent<BuildMech>();
        anotherWeapon = bm.Weapons[weaponOffset + (hand +1) % 2];
        anotherWeaponData = bm.WeaponDatas[weaponOffset + (hand + 1) % 2];
    }

    private void InitLayerMask() {
        TerrainLayerMask = LayerMask.GetMask("Terrain");
        PlayerLayerMask = LayerMask.GetMask("PlayerLayer");        
    }

    protected virtual void AddAudioSource(GameObject weapon) {
        AudioSource = weapon.AddComponent<AudioSource>();

        //Init AudioSource
        AudioSource.spatialBlend = 1;
        AudioSource.dopplerLevel = 0;
        AudioSource.volume = 1;
        AudioSource.playOnAwake = false;
        AudioSource.minDistance = 50;
        AudioSource.maxDistance = 350;
    }

    protected abstract void LoadSoundClips();

    protected virtual void SwitchWeaponAnimationClips(Animator WeaponAnimator) {
        if(WeaponAnimator == null || WeaponAnimator.runtimeAnimatorController == null) return;

        AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(WeaponAnimator.runtimeAnimatorController);
        WeaponAnimator.runtimeAnimatorController = animatorOverrideController;

        data.SwitchAnimationClips(WeaponAnimator);
    }

    public abstract void HandleCombat();//Process Input
    public abstract void HandleAnimation();

    public abstract void AttackTarget(GameObject target, bool isShield);

    public virtual void OnSkillAction(bool enter) {
    }

    public virtual void OnSwitchedWeaponAction() {
        ResetArmAnimatorState();
    }

    private void ResetArmAnimatorState() {
        MechAnimator.Play("Idle", 1 + LEFT_HAND);
        MechAnimator.Play("Idle", 1 + RIGHT_HAND);
    }

    public virtual bool IsOverHeat() {
        return HeatBar.IsOverHeat(mcbt.GetCurrentWeaponOffset() + hand);
    }

    public virtual void IncreaseHeat() {
        HeatBar.IncreaseHeatBar(mcbt.GetCurrentWeaponOffset() + hand, data.heat_increase_amount);
    }

    public virtual void OnAttackStateEnter(MechStateMachineBehaviour state) {//Some weapon logics are animation-dependent
    }

    public virtual void OnAttackStateExit(MechStateMachineBehaviour state) {
    }

    public virtual void OnAttackStateMachineExit(MechStateMachineBehaviour state) {
    }

    public virtual void ActivateWeapon(bool b) {//Not using setActive because if weapons have their own animations & are playing , disabling causes weapon animators to rebind the wrong rotation & position
        Renderer[] renderers = weapon.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = b;
        }
    }

    protected virtual void AdjustScale(GameObject weapon) {
        float newscale = mcbt.transform.root.localScale.x * mcbt.transform.localScale.x;
        weapon.transform.localScale = new Vector3(weapon.transform.localScale.x * newscale,
        weapon.transform.localScale.y * newscale, weapon.transform.localScale.z * newscale);
    }

    protected virtual void SetWeaponParent(GameObject weapon) {
        weapon.transform.SetParent(WeapPos);
        weapon.transform.localRotation = Quaternion.Euler(90, 0, 0);
        weapon.transform.localPosition = Vector3.zero;
    }

    public virtual void OnDestroy() {
        if(weapon != null)Object.Destroy(weapon);
    }

    private bool CheckTargetIsDead(GameObject target) {
        MechCombat mcbt = target.transform.root.GetComponent<MechCombat>();
        if (mcbt == null) {//Drone
            return target.transform.root.GetComponent<DroneCombat>().CurrentHP <= 0;
        } else {
            return mcbt.CurrentHP <= 0;
        }
    }

    public GameObject GetWeapon() {
        return weapon;
    }
}