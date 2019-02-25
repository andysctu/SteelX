using System.Collections.Generic;
using UnityEngine;
using Weapons;

public abstract class Combat : MonoBehaviour, IDamageable, IDamageableManager, IPunObservable {
    protected GameManager gm;
    public PhotonView PhotonView;

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

    private readonly List<IDamageable> _damageableComponents = new List<IDamageable>();

    //For Debug
    public bool ForceDead;

    protected virtual void Awake() {
        PhotonView = GetPhotonView();
        RegisterDamageableComponent(this);
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
        if (ForceDead) {//Debug use
            ForceDead = false;
            OnHit(10000, PhotonView);
        }
    }

    public virtual Weapon GetWeapon(int weapPos) {
        return null;
    }

    public virtual WeaponData GetWeaponData(int weaponPos) {
        return null;
    }

    //This should always be called by master
    public virtual void Attack(int weapPos, Vector3 direction, int damage, int[] targetPvID, int[] specIDs, int[] additionalFields = null) {
    }

    public virtual void OnHit(int damage, PhotonView attacker, Weapon weapon = null) {
        if(isDead)return;

        if (PhotonNetwork.isMasterClient) {
            if (CurrentHP - damage >= MAX_HP) {
                CurrentHP = MAX_HP;
            } else {
                CurrentHP -= damage;
            }

            if (CurrentHP <= 0) {//sync disable player
                PhotonView.RPC("DisablePlayer", PhotonTargets.All, attacker.owner, "null");
            }
        } else{
            if (CurrentHP - damage >= MAX_HP) {
                CurrentHP = MAX_HP;
            } else {
                CurrentHP -= damage;
            }
        }

        IncreaseSP(damage / 2);
    }

    public virtual void PlayOnHitEffect(){
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
    protected virtual void DisablePlayer(PhotonPlayer shooter, string weapon) {//todo : improve this
        OnMechEnabled(false);
    }

    public bool IsHpFull() {
        return CurrentHP >= MAX_HP;
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

    public virtual Transform GetTransform(){
        return transform;
    }

    public Vector3 GetPosition(){
        return transform.position + new Vector3(0,5,0);
    }

    public abstract PhotonPlayer GetOwner();

    public PhotonView GetPhotonView(){
        return PhotonView;
    }

    public virtual bool IsEnemy(PhotonPlayer compareTo){
        if (GameManager.IsTeamMode){
            return compareTo.GetTeam() != GetOwner().GetTeam();
        }else if (GetOwner() != null && GetOwner() == compareTo){
            return false;
        }

        return true;
    }

    public int GetCurrentHP(){
        return CurrentHP;
    }

    public int GetSpecID(){
        return -1;
    }

    public virtual void RegisterDamageableComponent(IDamageable c){
        _damageableComponents.Add(c);
    }

    public virtual void DeregisterDamageableComponent(IDamageable c){
        _damageableComponents.Remove(c);
    }

    public virtual IDamageable FindDamageableComponent(int specID){
        foreach (var c in _damageableComponents){
            if(c.GetSpecID() == specID)return c;
        }
        Debug.LogError("Can't find damageable component with id : " + specID);
        return this;
    }

    [System.Serializable]
    protected struct EnergyProperties {
        public float jumpENDrain, dashENDrain;
        public float energyOutput;
        public float minENRequired;
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
    }
}