using UnityEngine;
using System.Collections.Generic;

public abstract class Bullet : MonoBehaviour{
    [SerializeField] protected GameObject ImpactPrefab;
    protected Camera cam;
    protected PhotonView shooter_pv;

    protected ParticleSystem bullet_ps;
    protected BulletImpact bulletImpact;
    protected List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    //Targets components
    protected DisplayHitMsg displayHitMsg;
    protected Combat targetCbt;
    protected Transform target;//If isTargetShield is true , then target == Shield otherwise target == target mech
    protected Weapon shooterWeapon, targetWeapon;
    protected bool isTargetShield, isfollow = false;

    //Bullet variables
    public float bulletSpeed = 280;
    protected Vector3 startDirection, MECH_MID_POINT = new Vector3(0, 5, 0);

    protected virtual void Awake() {
        InitComponents();
    }

    private void InitComponents() {
        bullet_ps = GetComponent<ParticleSystem>();
        bulletImpact = Instantiate(ImpactPrefab).GetComponent<BulletImpact>();
    }

    public virtual void InitBullet(Camera cam, PhotonView shooter_pv, Vector3 startDirection, Transform target, Weapon shooterWeapon, Weapon targetWeapon) {
        this.cam = cam;
        this.shooter_pv = shooter_pv;
        this.startDirection = startDirection;
        this.target = target;
        this.shooterWeapon = shooterWeapon;
        this.targetWeapon = targetWeapon;

        InitTargetInfos(target, targetWeapon);
    }

    protected virtual void InitTargetInfos(Transform target, Weapon targetWeapon) {
        this.targetWeapon = targetWeapon;

        isTargetShield = (targetWeapon != null && targetWeapon.IsShield());
        isfollow = (target != null);

        if (target != null) {
            displayHitMsg = target.GetComponent<DisplayHitMsg>();
            targetCbt = target.GetComponent<Combat>();
        }

        if (isTargetShield) {
            this.target = targetWeapon.GetWeapon().transform;
        } else {
            this.target = target;
        }
    }

    protected abstract void LateUpdate();

    public abstract void Play();

    public abstract void Stop();

    protected void ShowHitMsg(Transform target) {
        if (shooter_pv.isMine) {
            if (displayHitMsg == null || targetCbt == null) return;

            if (targetCbt.CurrentHP <= 0)
                displayHitMsg.Display(DisplayHitMsg.HitMsg.KILL, cam);
            else {
                if (isTargetShield) {
                    displayHitMsg.Display(DisplayHitMsg.HitMsg.DEFENSE, cam);
                } else {
                    displayHitMsg.Display(DisplayHitMsg.HitMsg.HIT, cam);
                }
            }
        }
    }

    protected virtual void OnParticleCollision(GameObject other) {
        int numCollisionEvents = bullet_ps.GetCollisionEvents(other, collisionEvents);
        int i = 0;
        while (i < numCollisionEvents) {
            Vector3 collisionHitLoc = collisionEvents[i].intersection;
            bulletImpact.Play(collisionHitLoc);
            i++;
        }
    }

    protected virtual void PlayImpact(Vector3 impactPoint) {
        if (!isTargetShield) {
            bulletImpact.Play(impactPoint);
        } else {
            if (isTargetShield && targetWeapon != null) {
                targetWeapon.PlayOnHitEffect();
            }
        }
    }

    protected virtual void OnDestroy() {//When destroy this , also destroy bullet impact object
        if (bulletImpact != null) {
            Destroy(bulletImpact.gameObject, 2);
        }
    }
}