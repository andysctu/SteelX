using System.Collections;
using UnityEngine;

public class DroneCombat : Combat {
    [SerializeField] private SkillController SkillController;
    private Animator Animator;
    private EffectController EffectController;
    private bool onSkill = false, onSkillMoving = false;
    private float instantMoveSpeed;
    private Vector3 instantMoveDir;
    private CharacterController CharacterController;
    private float TeleportMinDistance = 3f;

    //Drone build mech
    private WeaponDataManager WeaponDataManager;

    public string[] weaponNames = new string[2];
    private WeaponData[] WeaponDatas = new WeaponData[2];
    private Weapon[] Weapons = new Weapon[2];
    public Transform[] Hands = new Transform[2];
    private int weaponOffset = 0;
    private string[] defaultWeapons = { "SHL009", "SHL501", "APS043", "SHS309", "RCL034", "BCN029", "BRF025", "SGN150", "LMG012", "ENG041", "ADR000", "Empty" };

    protected override void Awake() {
        base.Awake();
        InitComponents();
        SetDefaultWeaponDatas();
        if (SkillController != null) SkillController.OnSkill += OnSkill;
    }

    protected override void Start() {
        base.Start();

        FindHands();
        BuildDroneWeapons();
        InitCombatVariables();
        EffectController.RespawnEffect();
        //gm.RegisterPlayer(photonView.viewID);
    }

    private void InitComponents() {
        WeaponDataManager = Resources.Load<WeaponDataManager>("Data/Managers/WeaponDataManager");
        Animator = GetComponentInChildren< Animator >();
        EffectController = GetComponentInChildren<EffectController>();
        CharacterController = GetComponent<CharacterController>();
    }

    private void FindHands() {
        Transform shoulderL = transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_L_Clavicle");
        Transform shoulderR = transform.Find("CurrentMech/Bip01/Bip01_Pelvis/Bip01_Spine/Bip01_Spine1/Bip01_Spine2/Bip01_Spine3/Bip01_Neck/Bip01_R_Clavicle");

        if(shoulderL!=null)Hands[0] = shoulderL.Find("Bip01_L_UpperArm/Bip01_L_ForeArm/Bip01_L_Hand/Weapon_lft_Bone");
        if(shoulderR != null)Hands[1] = shoulderR.Find("Bip01_R_UpperArm/Bip01_R_ForeArm/Bip01_R_Hand/Weapon_rt_Bone");
    }

    private void SetDefaultWeaponDatas() {
        string weapon0 = defaultWeapons[3],   //Weapon set here
            weapon1 = defaultWeapons[3];

        for (int i = 0; i < weaponNames.Length; i++) {
            if (string.IsNullOrEmpty(weaponNames[i])) {
                weaponNames[i] = defaultWeapons[3];
            }
        }
    }

    private void BuildDroneWeapons() {        
        if (Hands[0] == null || Hands[1] == null) return;

        //Find and create corresponding weapon script
        for (int i = 0; i < weaponNames.Length; i++) {
            WeaponDatas[i] = (i >= weaponNames.Length || weaponNames[i] == "Empty" || string.IsNullOrEmpty(weaponNames[i])) ? null : WeaponDataManager.FindData(weaponNames[i]);

            if (WeaponDatas[i] == null) {
                if (i < weaponNames.Length && (weaponNames[i] == "Empty" || string.IsNullOrEmpty(weaponNames[i])))
                    Debug.LogError("Can't find weapon data : " + weaponNames[i]);
                continue;
            }

            Weapons[i] = (Weapon)(WeaponDatas[i].GetWeaponObject());

        }

        //Init weapon scripts
        for (int i = 0; i < WeaponDatas.Length; i++) {
            Transform weapPos = (WeaponDatas[i].twoHanded) ? Hands[(i + 1) % 2] : Hands[i % 2];
            Weapons[i].Init(WeaponDatas[i], i, weapPos, this, Animator);
        }

        //Enable renderers
        for (int i = 0; i < Weapons.Length; i++) {
            Weapons[i].ActivateWeapon((i == weaponOffset || i == weaponOffset + 1));
        }
    }

    private void InitCombatVariables() {
        CurrentHP = MAX_HP;
    }

    [PunRPC]
    public override void OnHit(int damage, PhotonPlayer shooter, int shooter_pvID, int shooterWeapPos, int targetWeapPos) {
        if (isDead || shooter == null) { return; }

        GameObject shooterMech = null;

        //If shooter is null, use photonView id to search
        if (shooter == null || shooter.TagObject == null) {
            PhotonView shooter_pv = PhotonView.Find(shooter_pvID);
            if (shooter_pv == null) {
                Debug.LogWarning("shooter_pv is null. Is the shooter not existed?");
                return;
            } else {
                shooterMech = shooter_pv.gameObject;
            }
        } else {
            shooterMech = ((GameObject)shooter.TagObject);
        }

        //Get shooter weapon
        Combat shooter_cbt = (shooterMech == null) ? null : shooterMech.GetComponent<Combat>();
        if (shooter_cbt == null) { Debug.LogWarning("cbt null."); return; }

        Weapon shooterWeapon = shooter_cbt.GetWeapon(shooterWeapPos);
        if (shooterWeapon == null) { Debug.LogWarning("shooterWeapon is null."); return; }

        Weapon.AttackType attackType = shooterWeapon.GetWeaponAttackType();

        //Get target weapon (this player )
        Weapon targetWeapon = null;
        if (targetWeapPos != -1) {
            targetWeapon = Weapons[targetWeapPos];
            if (targetWeapon == null) { Debug.LogWarning("targetWeapon is null."); return; }
        }

        //Play on hit effect
        bool isShield = (targetWeapon != null && targetWeapon.IsShield());//some weapons may be shield in some state ( the general meaning of a shield )
        shooterWeapon.OnTargetEffect(gameObject, targetWeapon, isShield);

        if (targetWeapon != null) targetWeapon.OnHitAction(shooter_cbt, shooterWeapon);

        //Process damage
        int dmg = damage;
        if (targetWeapon != null) dmg = targetWeapon.ProcessDamage(damage, attackType);

        //Master handle logic
        if (PhotonNetwork.isMasterClient) {
            if (CurrentHP - dmg >= MAX_HP) {
                CurrentHP = MAX_HP;
            } else {
                CurrentHP -= dmg;
            }

            if (CurrentHP <= 0) {//sync disable player
                photonView.RPC("DisablePlayer", PhotonTargets.All, shooter, shooterWeapon.WeaponName);
            }
        }

        //if (photonView.isMine) {//TODO : improve anti hack
        //    IncreaseSP(damage / 2);
        //}
    }

    [PunRPC]
    protected override void DisablePlayer(PhotonPlayer shooter, string weapon) {
        DisableDrone();
    }

    protected override void Update() {
        base.Update();
        if (onSkillMoving) {
            InstantMove();
        }
    }

    public void SkillSetMoving(Vector3 v) {
        onSkillMoving = true;
        instantMoveSpeed = v.magnitude;
        instantMoveDir = v;
        //curInstantMoveSpeed = instantMoveSpeed;
    }

    private void InstantMove() {
        instantMoveSpeed /= 1.6f;//1.6 : decrease coeff.

        CharacterController.Move(instantMoveDir * instantMoveSpeed);

        //cast a ray downward to check if not jumping but not grounded => if so , directly teleport to ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, TerrainLayerMask)) {
            if (Vector3.Distance(hit.point, transform.position) >= TeleportMinDistance && !Physics.CheckSphere(hit.point + new Vector3(0, 2.1f, 0), CharacterController.radius, TerrainLayerMask)) {
                transform.position = hit.point;
            }
        }
    }
    [PunRPC]
    private void KnockBack(Vector3 dir, float length) {
        transform.position += dir * length;
    }

    public void Skill_KnockBack(float length) {
        Transform skillUser = SkillController.GetSkillUser();

        onSkillMoving = true;
        SkillSetMoving((skillUser != null) ? (transform.position - skillUser.position).normalized * length : -transform.forward * length);
    }

    private void DisableDrone() {
        gameObject.layer = default_layer;
        StartCoroutine(DisableDroneWhenNotOnSkill());
    }

    private IEnumerator DisableDroneWhenNotOnSkill() {
        yield return new WaitWhile(() => onSkill);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = false;
        }
        StartCoroutine(RespawnAfterTime(2));
        OnMechEnabled(false);
    }

    private void EnableDrone() {
        OnMechEnabled(true);

        InitCombatVariables();

        EffectController.RespawnEffect();

        gameObject.layer = playerlayer;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.enabled = true;
        }
    }

    private void OnSkill(bool b) {
        onSkill = b;
    }

    public override Weapon GetWeapon(int weapPos) {
        return Weapons[weapPos];
    }

    private IEnumerator RespawnAfterTime(int time) {
        yield return new WaitForSeconds(time);
        EnableDrone();
    }

}