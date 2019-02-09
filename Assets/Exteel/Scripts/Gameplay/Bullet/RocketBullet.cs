using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Weapons.Bullets
{
    public class RocketBullet : Bullet
    {
        private Rocket _rocket;
        private Rigidbody _rigidBody;//todo : improve
        private PhotonView _rocketPv;
        private PhotonPlayer _shooter;
        private LayerMask PlayerLayerMask, TerrainLayerMask, ShieldLayerMask;

        //bullet info
        private int bulletDmg = 450, shooterWeapPos;
        private float impact_radius = 6;
        private bool isCollided = false, hasCalledPlayImpact = false;

        protected override void Awake(){
            base.Awake();

            InitComponents();
            InitGameVariables();
        }

        private void InitComponents(){
            _rocketPv = GetComponent<PhotonView>();
            AttachRigidbody();
        }

        private void InitVelocity(){
            _rigidBody.velocity = transform.forward * bulletSpeed;
            transform.LookAt(transform.forward * 9999);
        }

        protected void InitGameVariables(){
            TerrainLayerMask = LayerMask.GetMask("Terrain");
            PlayerLayerMask = LayerMask.GetMask("PlayerLayer");
            ShieldLayerMask = LayerMask.GetMask("Shield");
        }

        public void SetShooter(PhotonPlayer shooter){
            _shooter = shooter;
        }

        private void Start(){
            bullet_ps.Play();
            InitVelocity();
        }

        public void SetBulletProperties(Rocket rocket, int bulletDmg, float bulletSpeed, float impact_radius){
            _rocket = rocket;
            this.bulletDmg = bulletDmg;
            this.bulletSpeed = bulletSpeed;
            this.impact_radius = impact_radius;
        }

        protected override void OnParticleCollision(GameObject target){
            if (!PhotonNetwork.isMasterClient || isCollided) return;

            bullet_ps.GetCollisionEvents(target, collisionEvents);
            Vector3 impactPoint = collisionEvents[0].intersection;

            if (((1 << target.layer) & (PlayerLayerMask | ShieldLayerMask)) != 0) {//check if target is player
                IDamageable d;

                if ((d = target.GetComponent(typeof(IDamageable)) as IDamageable) != null){
                    if (!d.IsEnemy(_shooter)){
                        return;
                    }else if(((1 << target.layer) & ShieldLayerMask) != 0){//target is shield => only collide with the shield
                        _rocketPv.RPC("PlayImpact", PhotonTargets.All, bulletDmg, impactPoint, new[]{d.GetPhotonView().viewID}, new[] { d.GetSpecID() });
                        return;
                    }
                } else{
                    Debug.Log("rocket collides with no IDamageable component target");
                    return;
                }
            }

            isCollided = true;

            Collider[] hitPlayerColliders = Physics.OverlapSphere(impactPoint, impact_radius, PlayerLayerMask); // get overlap targets
            List<IDamageable> playerDamageables = new List<IDamageable>();

            for (int i = 0; i < hitPlayerColliders.Length; i++){
                IDamageable d = hitPlayerColliders[i].GetComponent(typeof(IDamageable)) as IDamageable;
                if(d == null || !d.IsEnemy(_shooter))continue;

                //check blocked by shield
                Debug.DrawLine(impactPoint, impactPoint + d.GetPosition() - impactPoint, Color.red, 2);
                RaycastHit[] rays = Physics.RaycastAll(impactPoint, d.GetPosition() - impactPoint, (d.GetPosition() - impactPoint).magnitude, ShieldLayerMask);
                foreach (var r in rays){
                    IDamageable rD = r.collider.GetComponent(typeof(IDamageable)) as IDamageable;
                    if(rD != null){
                        if (rD.GetPhotonView() == d.GetPhotonView()){
                            d = rD;//set target to shield instead of the player
                        }
                    }
                }

                //check duplicated
                if (!playerDamageables.Exists(x => x.GetPhotonView() == d.GetPhotonView()))playerDamageables.Add(d);
            }

            int[] playerViewIDs = new int[playerDamageables.Count];
            int[] playerSpecIDs = new int[playerDamageables.Count];
            for (int i=0;i<playerDamageables.Count;i++){
                playerViewIDs[i] = playerDamageables[i].GetPhotonView().viewID;
                playerSpecIDs[i] = playerDamageables[i].GetSpecID();
            }

            _rocketPv.RPC("PlayImpact", PhotonTargets.All, bulletDmg, impactPoint, playerViewIDs, playerSpecIDs);
        }

        [PunRPC]
        protected void PlayImpact(int damage, Vector3 impactPoint, int[] playerViewIDs, int[] playerSpecIDs){
            StopBulletEffect();

            GameObject impact = Instantiate(ImpactPrefab.gameObject, impactPoint, Quaternion.identity);
            BulletImpact BI = impact.GetComponent<BulletImpact>();
            BI.Play(impactPoint);
            Destroy(impact, 2);

            if(playerViewIDs == null || playerSpecIDs == null)return;
            for(int i=0;i<playerViewIDs.Length;i++){
                PhotonView pv = PhotonView.Find(playerViewIDs[i]);
                if(pv == null)continue;

                IDamageableManager dm = pv.GetComponent(typeof(IDamageableManager)) as IDamageableManager;
                IDamageable d;
                if (dm == null || (d = dm.FindDamageableComponent(playerSpecIDs[i]))== null){
                    continue;
                }

                d.OnHit(damage, shooterPv, _rocket);
                d.PlayOnHitEffect();
            }

            //if (targetCbt.CurrentHP - targetCbt.ProcessDmg(bulletDmg, Weapon.AttackType.Rocket, targetWeapon) <= 0){
            //    targetMech.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, shooterCbt.GetCamera());
            //} else{
            //    targetMech.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, shooterCbt.GetCamera());
            //}
        }

        public override void Play(){
            if (Isfollow){
                if (Target == null) return;
                transform.LookAt(Target.GetPosition());
                bullet_ps.Play();
            } else{
                transform.LookAt(startDirection * 9999);
                _rigidBody.velocity = startDirection * bulletSpeed;
                bullet_ps.Play();
            }
        }

        private void AttachRigidbody(){
            _rigidBody = gameObject.AddComponent<Rigidbody>();
            _rigidBody.useGravity = false;
            _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public override void StopBulletEffect(){
            bullet_ps.Stop(true);
            enabled = false;
            Destroy(gameObject);
        }

        public void SetPhotonViewID(int ID){
            _rocketPv.viewID = ID;
        }
    }
}
