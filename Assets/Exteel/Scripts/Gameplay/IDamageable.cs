using UnityEngine;
using Weapons;

public interface IDamageable {
    void OnHit(int damage, PhotonView attacker, Weapon weapon);
    void PlayOnHitEffect();
    Transform GetTransform();
    Vector3 GetPosition();
    PhotonPlayer GetOwner();
    PhotonView GetPhotonView();
    bool IsEnemy(PhotonPlayer compareTo);
    int GetSpecID();//locate child IDamageable component
    int GetCurrentHP();
}
