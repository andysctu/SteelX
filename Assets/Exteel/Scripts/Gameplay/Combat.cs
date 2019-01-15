using UnityEngine;
using Weapons;

public abstract class Combat : Photon.MonoBehaviour {
    protected GameManager gm;

    //Combat variable
    public int MAX_HP { get { return max_hp; } protected set { max_hp = value; } }
    public int CurrentHP { get; protected set; }
    private int max_hp = 2000;

    [SerializeField] protected EnergyProperties energyProperties = new EnergyProperties();
    public float MAX_EN { get { return max_EN; } protected set { max_EN = value; } }
    public float CurrentEN { get; protected set; }
    private float max_EN = 2000;
    protected bool isENAvailable = true;

    public bool IsSwitchingWeapon { get; protected set; }
    public bool CanMeleeAttack = true;//This is false after melee attack in air
    [HideInInspector] public bool isDead;

    //Game variables
    protected const int playerlayer = 8, ignoreRayCastLayer = 2, default_layer = 0;
    protected int TerrainLayerMask, PlayerLayerMask;

    //Mech action
    public delegate void EnablePlayerAction(bool b);
    public EnablePlayerAction OnMechEnabled;

    //For Debug
    public bool forceDead = false;

    protected virtual void Awake() {
    }

    protected virtual void Start() {
        FindGameManager();
        InitGameVariables();
    }

    protected void FindGameManager() {
        gm = FindObjectOfType<GameManager>();
    }

    protected void InitGameVariables() {
        TerrainLayerMask = LayerMask.GetMask("Terrain");
        PlayerLayerMask = LayerMask.GetMask("PlayerLayer");
    }

    protected virtual void Update() {
        if (forceDead) {//Debug use
            forceDead = false;
            photonView.RPC("OnHit", PhotonTargets.All, 10000, photonView.viewID);
        }
    }

    public virtual float GetAnimationLength(string name) {//TODO : improve this.
        return 1;
    }

    public virtual Weapon GetWeapon(int weapPos) {
        return null;
    }

    public virtual WeaponData GetWeaponData(int weaponPos) {
        return null;
    }

    public abstract int ProcessDmg(int dmg, Weapon.AttackType attackType, Weapon weapon);

    [PunRPC]
    public virtual void OnHit(int damage, int shooterPvID, int shooterWeapPos, int targetWeapPos) {
    }

    [PunRPC]
    public virtual void OnHit(int damage, int shooterPvID) {
        PhotonView shooterPv = PhotonView.Find(shooterPvID);
        if (shooterPv == null) return;

        if (PhotonNetwork.isMasterClient) {
            if (CurrentHP - damage >= MAX_HP) {
                CurrentHP = MAX_HP;
            } else {
                CurrentHP -= damage;
            }

            if (CurrentHP <= 0) {//sync disable player
                photonView.RPC("DisablePlayer", PhotonTargets.All, shooterPv.owner, "null");
            }
        }

        IncreaseSP(damage / 2);
    }

    [PunRPC]
    public virtual void KnockBack(Vector3 dir, float length) {//TODO : improve this
        GetComponent<CharacterController>().Move(dir * length);
    }

    [PunRPC]
    protected virtual void EnablePlayer() {
        OnMechEnabled(true);
    }

    [PunRPC]
    protected virtual void DisablePlayer(PhotonPlayer shooter, string weapon) {
        OnMechEnabled(false);
    }

    public bool IsHpFull() {
        return (CurrentHP >= MAX_HP);
    }

    public void SetMaxEN(int EN) {
        MAX_EN = EN;
        if (CurrentEN > MAX_EN) {
            CurrentEN = MAX_EN;
        }
    }

    public virtual Camera GetCamera() {
        return null;
    }

    public virtual void IncreaseSP(int amount) {
    }

    [System.Serializable]
    protected struct EnergyProperties {
        public float jumpENDrain, dashENDrain;
        public float energyOutput;
        public float minENRequired;
    }
}