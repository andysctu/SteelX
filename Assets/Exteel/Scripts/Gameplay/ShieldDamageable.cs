using UnityEngine;
using Weapons;

public class ShieldDamageable : MonoBehaviour, IDamageable {
    private Shield _shield;
    private Combat _cbt;
    //private PhotonView _photonView;
    private GameObject _boxCollider;
    private readonly Vector3 MECH_MID_POINT = new Vector3(0, 5, 0);
    private float rotOffset = 10;//make
    private int _hand;
    private bool _isOpen;
    private int _specID;

    //public void Init(Shield shield, Combat cbt, PhotonView photonView, int specID, int hand){
    //    _shield = shield;
    //    _cbt = cbt;
    //    _hand = hand;
    //    _photonView = photonView;
    //    _specID = specID;
	//
    //    InitComponents();
    //}

    private void InitComponents() {
        _boxCollider = GetComponentInChildren<BoxCollider>().gameObject;
        if (_boxCollider == null) Debug.LogError("Can't find shield collider as a child of Shield Mesh");
    }

    private void LateUpdate() {
        if (_isOpen) {
            _boxCollider.transform.LookAt(transform.root.position + MECH_MID_POINT);
            _boxCollider.transform.rotation = Quaternion.Euler(new Vector3(0, _boxCollider.transform.rotation.eulerAngles.y + ((_hand == 0) ? rotOffset : -rotOffset), 0));
        } else {
            _boxCollider.transform.localRotation = Quaternion.Euler(new Vector3(0, ((_hand == 0) ? 90 : -90), 0));
            _boxCollider.transform.rotation = Quaternion.Euler(new Vector3(0, _boxCollider.transform.rotation.eulerAngles.y, 0));
        }
    }

    public void EnableDefense(bool b){
        _isOpen = b;
    }

    //public void OnHit(int damage, PhotonView attacker, Weapon weapon){
    //    _shield.OnHit(damage, attacker, weapon);
    //}

    public void PlayOnHitEffect(){
        _shield.PlayOnHitEffect();
    }

    public Transform GetTransform(){
        return transform;
    }

    public Vector3 GetPosition(){
        return transform.position;
    }

    //public PhotonPlayer GetOwner(){
    //    return _shield == null ? null : _shield.GetOwner();
    //}

    //public PhotonView GetPhotonView(){
    //    return _photonView;
    //}

    //public bool IsEnemy(PhotonPlayer compareTo){
    //    if (GameManager.IsTeamMode) {
    //        return compareTo.GetTeam() != GetOwner().GetTeam();
    //    }
	//
    //    return true;
    //}

    public int GetSpecID(){
        return _specID;
    }

    public int GetCurrentHP() {
        return _cbt.CurrentHP;
    }
}