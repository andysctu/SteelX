using UnityEngine;

public abstract class Weapon {
    protected GameObject weapon;
    protected WeaponData data;

    //Components
    protected Transform WeaponTransform;
    protected Combat Cbt;
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
    public string WeaponName;
    protected int hand, weapPos;//Two-handed -> 0
    protected KeyCode BUTTON;
    protected const int LEFT_HAND = 0, RIGHT_HAND = 1;
    protected float timeOfLastUse, rate;
    public bool allowBothWeaponUsing = true, isFiring = false;

    protected int TerrainLayer = 10, TerrainLayerMask, PlayerLayerMask, PlayerAndTerrainMask;

    public enum AttackType { Melee, Ranged, Cannon, Rocket, Skill, Debuff, None };
    protected AttackType attackType;

    public virtual void Init(WeaponData data, int pos, Transform WeapPos, Combat Cbt, Animator MechAnimator) {
        InitDataRelatedVars(data);
        this.Cbt = Cbt;
        this.MechAnimator = MechAnimator;
        this.WeaponTransform = WeapPos;
        this.hand = pos % 2;
        this.weapPos = pos;

        BUTTON = (hand == LEFT_HAND) ? KeyCode.Mouse0 : KeyCode.Mouse1;

        InstantiateWeapon(data);
        InitAttackType();
        InitComponents();
        InitAnotherWeaponInfo();
        InitLayerMask();
        LoadSoundClips();
        SwitchWeaponAnimationClips(WeaponAnimator);
    }

    protected virtual void InitDataRelatedVars(WeaponData data) {
        this.data = data;
        rate = data.Rate;
        WeaponName = data.weaponName;
    }

    protected virtual void InstantiateWeapon(WeaponData data) {
        weapon = Object.Instantiate(data.GetWeaponPrefab(hand % 2), Vector3.zero, Quaternion.identity) as GameObject;
        WeaponAnimator = weapon.GetComponent<Animator>();

        AdjustScale(weapon);
        SetWeaponParent(weapon);
    }

    protected abstract void InitAttackType();

    private void InitComponents() {
        photonView = Cbt.GetComponent<PhotonView>();
        HeatBar = Cbt.GetComponentInChildren<HeatBar>(true);
        AnimationEventController = MechAnimator.GetComponent<AnimationEventController>();
        AnimatorVars = Cbt.GetComponentInChildren<AnimatorVars>();
        AddAudioSource(weapon);
    }

    private void InitAnotherWeaponInfo() {
        int weaponOffset = weapPos - 2 >= 0 ? 2 : 0;
        anotherWeapon = Cbt.GetWeapon(weaponOffset + (hand + 1) % 2);
        anotherWeaponData = Cbt.GetWeaponData(weaponOffset + (hand + 1) % 2);
    }

    private void InitLayerMask() {
        TerrainLayerMask = LayerMask.GetMask("Terrain");
        PlayerLayerMask = LayerMask.GetMask("PlayerLayer");
        PlayerAndTerrainMask = TerrainLayerMask | PlayerLayerMask;
    }

    protected virtual void AddAudioSource(GameObject weapon) {
        AudioSource = weapon.AddComponent<AudioSource>();

        //Init AudioSource
        AudioSource.spatialBlend = 1;
        AudioSource.dopplerLevel = 0;
        AudioSource.volume = 0.8f;
        AudioSource.playOnAwake = false;
        AudioSource.minDistance = 20;
        AudioSource.maxDistance = 250;
    }

    protected abstract void LoadSoundClips();

    protected virtual void SwitchWeaponAnimationClips(Animator WeaponAnimator) {
        if (WeaponAnimator == null || WeaponAnimator.runtimeAnimatorController == null) return;

        AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(WeaponAnimator.runtimeAnimatorController);
        WeaponAnimator.runtimeAnimatorController = animatorOverrideController;

        data.SwitchAnimationClips(WeaponAnimator);
    }

    public abstract void HandleCombat();//Process Input
    public abstract void HandleAnimation();

    public abstract void OnTargetEffect(GameObject target, Weapon targetWeapon, bool isShield);

    public virtual void PlayOnHitEffect() {
    }

    public virtual void OnHitAction(Combat shooter, Weapon shooterWeapon) {
    }

    public virtual void OnSkillAction(bool enter) {
    }

    public virtual void OnSwitchedWeaponAction(bool b) {
    }

    public virtual void OnOverHeatAction(bool b) {
    }

    public virtual bool IsOverHeat() {
        if(HeatBar ==null)return false;

        return HeatBar.IsOverHeat(weapPos);
    }

    public virtual void SetOverHeat(bool b) {
        HeatBar.SetOverHeat(weapPos, b);

        if (b) {
            OnOverHeatAction(b);
        }
    }

    public virtual int ProcessDamage(int damage, AttackType type) {
        return damage;
    }

    public virtual void IncreaseHeat(int amount) {
        if(HeatBar!=null)HeatBar.IncreaseHeat(weapPos, amount);
    }

    public virtual void ActivateWeapon(bool b) {//Not using setActive because if weapons have their own animations & are playing , disabling causes weapon animators to rebind the wrong rotation & position
        Renderer[] renderers = weapon.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = b;
        }
    }

    protected virtual void AdjustScale(GameObject weapon) {
        float newscale = Cbt.transform.root.localScale.x * Cbt.transform.localScale.x;
        weapon.transform.localScale = new Vector3(weapon.transform.localScale.x * newscale,
        weapon.transform.localScale.y * newscale, weapon.transform.localScale.z * newscale);
    }

    protected virtual void SetWeaponParent(GameObject weapon) {
        weapon.transform.SetParent(WeaponTransform);
        weapon.transform.localRotation = Quaternion.Euler(90, 0, 0);
        weapon.transform.localPosition = Vector3.zero;
    }

    public virtual void OnDestroy() {
        if (weapon != null) Object.Destroy(weapon);
    }

    private bool CheckTargetIsDead(GameObject target) {
        MechCombat mcbt = target.transform.root.GetComponent<MechCombat>();
        if (mcbt == null) {//Drone
            return target.transform.root.GetComponent<DroneCombat>().CurrentHP <= 0;
        } else {
            return mcbt.CurrentHP <= 0;
        }
    }

    public virtual bool IsShield() {//general meaning of a shield
        return false;
    }

    public int GetRawDamage() {
        return data.damage;
    }

    public GameObject GetWeapon() {
        return weapon;
    }

    public AttackType GetWeaponAttackType() {
        return attackType;
    }

    public abstract void OnStateCallBack(int type, MechStateMachineBehaviour state);
}