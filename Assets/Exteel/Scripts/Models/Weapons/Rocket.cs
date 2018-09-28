using UnityEngine;

namespace Weapons
{
    using Bullets;

    public class Rocket : RangedWeapon
    {
        private AudioClip _shotSound, _reloadSound;
        private Bullet _bullet;

        private float _bulletSpeed, _impactRadius;

        protected override void InitDataRelatedVars(WeaponData data){
            base.InitDataRelatedVars(data);

            _bulletSpeed = ((RocketData) data).bullet_speed;
            _impactRadius = ((RocketData) data).impact_radius;
        }

        protected override void LoadSoundClips(){
            _shotSound = ((RocketData) data).shotSound;
            _reloadSound = ((RocketData) data).reload_sound;
        }

        protected override void InitAtkAnimHash(){
            AtkAnimHash = Animator.StringToHash("AtkL");
        }

        public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield){
            if (isShield){
                if (targetWeapon != null){
                    targetWeapon.PlayOnHitEffect();
                }
            } else{
                if (data.Slowdown){
                    //TODO : implement this
                }
            }
        }

        protected override void UpdateMechArmState(){
            MechAnimator.Play("Rocket", 1);
            MechAnimator.Play("Rocket", 2);
        }

        public override void HandleAnimation(){
            if (IsFiring){
                if (Time.time - startShootTime >= 0.1f){
                    //0.1f : animation min time
                    if (atkAnimationIsPlaying){
                        atkAnimationIsPlaying = false;
                        MechAnimator.SetBool(AtkAnimHash, false);
                    }
                } else{
                    if (!atkAnimationIsPlaying){
                        MechAnimator.SetBool(AtkAnimHash, true);
                        atkAnimationIsPlaying = true;
                    }
                }
            } else{
                if (atkAnimationIsPlaying){
                    MechAnimator.SetBool(AtkAnimHash, false);
                    atkAnimationIsPlaying = false;
                }
            }
        }

        protected override void FireRaycast(Vector3 start, Vector3 direction, int hand){
            //TODO : implement rocket that follow targets
            //does Rocket follow target ?
            //Transform target = ((hand == 0) ? Crosshair.getCurrentTargetL() : Crosshair.getCurrentTargetR());//target might be shield collider

            Transform target = null;

            if (target != null){
                PhotonView targetpv = target.transform.root.GetComponent<PhotonView>();

                if (target.tag != "Shield"){
                    PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, targetpv.owner, targetpv.viewID, -1);
                } else{
                    //check which hand is it
                    ShieldActionReceiver ShieldActionReceiver = target.parent.GetComponent<ShieldActionReceiver>();
                    int target_ShieldPos = ShieldActionReceiver.GetPos();

                    PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, targetpv.owner, targetpv.viewID, target_ShieldPos);
                }
            } else{
                PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, -1, -1);
            }
        }

        public override void Shoot(Vector3 direction, int targetPvId, int targetWeapPos){
            //Play animation imm.
            MechAnimator.Play("RocketShootL", 1);
            MechAnimator.Play("RocketShootR", 2);
            MechAnimator.Update(0);

            WeaponAnimator.SetTrigger("Atk");

            IsFiring = true;
            startShootTime = Time.time;

            GameObject target = null;
            PhotonView targetPv = PhotonView.Find(targetPvId);
            if (targetPv != null) target = targetPv.gameObject;

            if (target != null){
                Combat targetCbt = target.GetComponent<Combat>();
                if (targetCbt == null) return;

                targetCbt.OnHit(data.damage, PlayerPv.viewID, WeapPos, targetWeapPos);

                if (PhotonNetwork.isMasterClient) DisplayBullet(direction, target, (targetWeapPos == -1) ? null : targetCbt.GetWeapon(targetWeapPos));

                Cbt.IncreaseSP(data.SpIncreaseAmount);
            } else{
                if (PhotonNetwork.isMasterClient) DisplayBullet(direction, null, null);
            }
        }

        protected override void DisplayBullet(Vector3 direction, GameObject target, Weapon targetWeapon){
            _bullet = PhotonNetwork.Instantiate(BulletPrefab.name, EffectEnd.position, Quaternion.LookRotation(direction, Vector3.up), 0).GetComponent<Bullet>();
            _bullet.InitBullet(MechCam, PlayerPv, direction, (target == null) ? null : target.transform, this, targetWeapon);
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);
            if (enter){
                //Stop effects playing when entering
                Muzzle.Stop();
                AudioSource.Stop();
            }
        }

        public override void OnWeaponSwitchedAction(bool isThisWeaponActivated){
            base.OnWeaponSwitchedAction(isThisWeaponActivated);

            if (!isThisWeaponActivated){
                Muzzle.Stop();
                AudioSource.Stop();
            }
        }

        public override void OnStateCallBack(int type, MechStateMachineBehaviour state){
            switch ((StateCallBackType) type){
                case StateCallBackType.AttackStateEnter:
                    Muzzle.Play();
                    AudioSource.PlayOneShot(_shotSound);
                    break;
                case StateCallBackType.AttackStateExit:
                    WeaponAnimator.SetTrigger("Reload");
                    if (_reloadSound != null) AudioSource.PlayOneShot(_reloadSound);
                    break;
            }
        }

        public float GetBulletSpeed(){
            return _bulletSpeed;
        }

        public float GetImpactRadius(){
            return _impactRadius;
        }
    }
}