using UnityEngine;

public class Combat : Photon.MonoBehaviour {
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
            photonView.RPC("OnHit", PhotonTargets.All, 10000, photonView.viewID, "ForceDead", true);
        }
    }

    [PunRPC]
    public virtual void OnHit(int damage, PhotonPlayer shooter, int weapPos) {//If weapon has some effects
        if (isDead || shooter == null) { return; }

        GameObject shooterMech = ((GameObject)shooter.TagObject);
        BuildMech bm = (shooterMech == null) ? null : shooterMech.GetComponent<BuildMech>();
        if (bm == null) { Debug.LogWarning("OnHit bm null : tag Object is not init."); return; }

        Weapon shooterWeapon = bm.Weapons[weapPos];
        if (shooterWeapon == null) { Debug.LogWarning("shooterWeapon is null."); return; }

        shooterWeapon.OnTargetEffect(gameObject, false);

        if (CurrentHP - damage >= MAX_HP) {
            CurrentHP = MAX_HP;
        } else {
            CurrentHP -= damage;
        }

        if (CurrentHP <= 0 && PhotonNetwork.isMasterClient) {//sync disable player
            photonView.RPC("DisablePlayer", PhotonTargets.All, shooter, shooterWeapon.WeaponName);
        }
    }

    [PunRPC]
    public virtual void OnHit(int damage, PhotonPlayer shooter, string weaponName) {
        if (isDead || shooter == null) { return; }

        CurrentHP -= damage;

        if (CurrentHP <= 0 && PhotonNetwork.isMasterClient) {//sync disable player
            photonView.RPC("DisablePlayer", PhotonTargets.All, shooter, weaponName);
        }
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

    [System.Serializable]
    protected struct EnergyProperties {
        public float jumpENDrain, dashENDrain;
        public float energyOutput;
        public float minENRequired;
    }
}