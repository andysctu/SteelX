using UnityEngine;
using System.Collections.Generic;

namespace Weapons.Bullets
{
    public abstract class Bullet : MonoBehaviour
    {
        [SerializeField] protected GameObject ImpactPrefab;
        protected Camera cam;
        protected PhotonView shooterPv;

        protected ParticleSystem bullet_ps;
        protected BulletImpact bulletImpact;
        protected List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

        //Targets components
        protected DisplayHitMsg displayHitMsg;
        protected Combat targetCbt;
        protected IDamageable Target;
        protected bool IsTargetShield, Isfollow;

        //Bullet variables
        public float bulletSpeed = 280;
        protected Vector3 startDirection;
        protected int ShieldLayer;
        protected virtual void Awake(){
            InitComponents();
        }

        private void InitComponents(){
            bullet_ps = GetComponent<ParticleSystem>();
            bulletImpact = Instantiate(ImpactPrefab).GetComponent<BulletImpact>();
            ShieldLayer = LayerMask.NameToLayer("Shield");
        }

        public virtual void InitBullet(Camera cam, PhotonView shooter_pv, Vector3 startDirection, IDamageable target){
            this.cam = cam;
            this.shooterPv = shooter_pv;
            this.startDirection = startDirection;
            InitTargetInfos(target);
        }

        protected virtual void InitTargetInfos(IDamageable target){
            Isfollow = (target != null);
            IsTargetShield = target != null && target.GetTransform().gameObject.layer == ShieldLayer;
            this.Target = target;

            if (target != null){//todo : remake this
                displayHitMsg = target.GetTransform().GetComponent<DisplayHitMsg>();
            }
        }

        protected virtual void LateUpdate(){
        }

        public abstract void Play();

        public abstract void StopBulletEffect();

        protected void ShowHitMsg(Transform target){//todo : remake this
            if (shooterPv.isMine){
                if (displayHitMsg == null || targetCbt == null) return;

                if (targetCbt.CurrentHP <= 0)
                    displayHitMsg.Display(DisplayHitMsg.HitMsg.KILL, cam);
                else{
                    if (IsTargetShield){
                        displayHitMsg.Display(DisplayHitMsg.HitMsg.DEFENSE, cam);
                    } else{
                        displayHitMsg.Display(DisplayHitMsg.HitMsg.HIT, cam);
                    }
                }
            }
        }

        protected virtual void OnParticleCollision(GameObject other){
            int numCollisionEvents = bullet_ps.GetCollisionEvents(other, collisionEvents);
            int i = 0;
            while (i < numCollisionEvents){
                Vector3 collisionHitLoc = collisionEvents[i].intersection;
                bulletImpact.Play(collisionHitLoc);
                i++;
            }
        }

        protected virtual void PlayImpact(Vector3 impactPoint){
            if (!IsTargetShield){
                bulletImpact.Play(impactPoint);
                if (Target != null){
                    Target.PlayOnHitEffect();
                }
            } else{//don't play bullet on hit effect
                if (IsTargetShield && Target != null){
                    Target.PlayOnHitEffect();
                }
            }
        }

        protected virtual void OnDestroy(){
            //When destroy this , also destroy bullet impact object
            if (bulletImpact != null){
                Destroy(bulletImpact.gameObject, 2);
            }
        }
    }
}