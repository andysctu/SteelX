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
    protected Combat cbt;
    protected Transform target;
    protected Weapon targetWeapon;
    protected bool isTargetShield, isfollow = false;

    //Bullet variables
    protected float psStartSpeed;
    protected Vector3 MECH_MID_POINT = new Vector3(0, 5, 0);

    protected virtual void Awake() {
        InitComponents();
    }

    private void InitComponents() {
        bullet_ps = GetComponent<ParticleSystem>();
        bulletImpact = Instantiate(ImpactPrefab).GetComponent<BulletImpact>();
        psStartSpeed = bullet_ps.main.startSpeed.constant;
    }

    public void InitBulletTrace(Camera cam, PhotonView shooter_pv) {
        this.cam = cam;
        this.shooter_pv = shooter_pv;
    }

    public void SetTarget(Transform TargetPlayer, Weapon TargetWeapon) {
        this.targetWeapon = TargetWeapon;

        isTargetShield = (TargetWeapon != null && TargetWeapon.IsShield());
        isfollow = (TargetPlayer != null);

        if (TargetPlayer != null) {
            displayHitMsg = TargetPlayer.GetComponent<DisplayHitMsg>();
            cbt = TargetPlayer.GetComponent<Combat>();
        }

        if (isTargetShield) {
            this.target = TargetWeapon.GetWeapon().transform;
        } else {
            this.target = TargetPlayer;
        }
    }

    protected abstract void LateUpdate();

    public abstract void Play();

    public abstract void Stop();

    protected void ShowHitMsg(Transform target) {
        if (shooter_pv.isMine) {
            if (displayHitMsg == null || cbt == null) return;

            if (cbt.CurrentHP <= 0)
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
        if (target == null) return;

        if (!isTargetShield) {
            bulletImpact.Play(impactPoint);
        } else {
            if (isTargetShield) targetWeapon.PlayOnHitEffect();
            else Debug.LogError("targetWeapon is null");
        }
    }

    protected virtual void OnDestroy() {
        if (bulletImpact != null) {
            Destroy(bulletImpact.gameObject, 2);
        }
    }
}