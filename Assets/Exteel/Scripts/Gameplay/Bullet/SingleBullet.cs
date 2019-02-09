using UnityEngine;

namespace Weapons.Bullets
{
    public class SingleBullet : Bullet
    {
        private Rigidbody Rigidbody;

        private bool calledStop; //make sure not trigger play impact multiple times

        protected override void Awake(){
            base.Awake();

            InitComponents();
        }

        private void InitComponents(){
            AttachRigidbody();
        }

        private void AttachRigidbody(){
            Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.useGravity = false;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        public override void Play(){
            if (Isfollow){
                if (Target == null) return;
                transform.LookAt(Target.GetPosition());
                bullet_ps.Play();
            } else{
                transform.LookAt(startDirection * 9999);
                Rigidbody.velocity = startDirection * bulletSpeed;
                bullet_ps.Play();
            }
        }

        public override void StopBulletEffect(){
            calledStop = true;

            bullet_ps.Clear();
            bullet_ps.Stop(true);
            Destroy(gameObject, 2);
            enabled = false;
        }

        protected override void LateUpdate(){
            if (Isfollow){
                if (Target == null){
                    StopBulletEffect();
                    return;
                }

                if (Vector3.Distance(transform.position, Target.GetPosition()) <= bulletSpeed * Time.deltaTime){
                    PlayImpact(transform.position);
                } else{
                    Rigidbody.velocity = (Target.GetPosition() - transform.position).normalized * bulletSpeed;
                }
            }
        }

        protected override void OnParticleCollision(GameObject other){
            int numCollisionEvents = bullet_ps.GetCollisionEvents(other, collisionEvents);
            int i = 0;
            while (i < numCollisionEvents){
                Vector3 collisionHitLoc = collisionEvents[i].intersection;
                PlayImpact(collisionHitLoc);
                i++;
            }
        }

        protected override void PlayImpact(Vector3 impactPoint){
            if (calledStop) return;

            StopBulletEffect();

            if (Target == null){
                bulletImpact.Play(impactPoint);
                return;
            } else{
                Target.PlayOnHitEffect();
                //ShowHitMsg(Target);
            }

            if (!IsTargetShield){
                bulletImpact.Play(impactPoint);
            } else{
                if(Target != null)Target.PlayOnHitEffect();
            }
        }
    }
}