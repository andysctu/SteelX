using StateMachine;
using UnityEngine;

namespace Weapons
{
    using Bullets;

    public class Cannon : RangedWeapon
    {
        private MechController Mctrl;
        private AudioClip _shotSound, _reloadSound;
        private Bullet _bullet;
        private int _bulletNum = 3;

        private bool _onPose = false, _onShoot = false, _onReload = false, _isCanceled = false;

        public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, hand, handTransform, Cbt, Animator);
            InitComponents();

            ResetAnimationVars();
            ResetBulletNum();
        }

        private void InitComponents(){
            Mctrl = Cbt.GetComponent<MechController>();
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);
            if (enter){
                ResetAnimationVars();
                ResetBulletNum();
            }
        }

        private void ResetAnimationVars(){
            //TODO : check this
            _onShoot = false;
            _onPose = false;
            _onReload = false;

            if (Cbt.photonView.isMine){
                MechAnimator.SetBool("CnPose", false);
                MechAnimator.SetBool("CnLoad", false);
                MechAnimator.SetBool("CnShoot", false);
            }
        }

        private void ResetBulletNum(){
            _bulletNum = ((CannonData) data).MaxBullet;
        }

        protected override void LoadSoundClips(){
            _shotSound = ((CannonData) data).shotSound;
            _reloadSound = ((CannonData) data).reload_sound;
        }

        public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield){
            if (!isShield && data.Slowdown){
                //TODO : implement this
            }
        }

        public override void OnWeaponSwitchedAction(bool isThisActivated){
            if (!isThisActivated){
                ResetAnimationVars();
            }
        }

        protected override void UpdateMechArmState(){
            MechAnimator.Play("Cn", 1);
            MechAnimator.Play("Cn", 2);
        }

        public override void HandleCombat(usercmd cmd) {
            if (Input.GetKeyDown(KeyCode.Mouse1) || IsOverHeat()){
                _isCanceled = true;
                MechAnimator.SetBool(AnimatorVars.CnPoseHash, false);
                return;
            }

            if (_isCanceled){
                if (!Input.GetKey(KeyCode.Mouse0)){
                    _isCanceled = false;
                } else{
                    return;
                }
            }

            if (Input.GetKey(KeyCode.Mouse0) && !_onPose && !_onShoot && _bulletNum >= 1 && Mctrl.Grounded){
                AnimationEventController.CnPose();
                MechAnimator.SetBool(AnimatorVars.CnPoseHash, true);
            }

            if (Time.time - TimeOfLastUse >= 1 / Rate && _onPose){
                if (Input.GetKey(KeyCode.Mouse0) || !MechAnimator.GetBool(AnimatorVars.CnPoseHash) || !Mctrl.Grounded) return;

                IsFiring = true;

                FireRaycast(MechCam.transform.TransformPoint(0, 0, CrosshairController.CamDistanceToMech), MechCam.transform.forward, Hand);

                TimeOfLastUse = Time.time;
            }
        }

        public override void Shoot(Vector3 direction, int targetPvId, int targetWeapPos){
            WeaponAnimator.SetTrigger("Atk");

            MechAnimator.SetBool("CnShoot", true);

            _bulletNum--;

            if (PlayerPv.isMine && _bulletNum <= 0){
                MechAnimator.SetBool("CnLoad", true);
            }

            IsFiring = true;

            GameObject target = null;
            PhotonView targetPv = PhotonView.Find(targetPvId);
            if (targetPv != null) target = targetPv.gameObject;

            if (target != null){
                Combat targetCbt = target.GetComponent<Combat>();
                targetCbt.OnHit(data.damage, PlayerPv.viewID, WeapPos, targetWeapPos);

                DisplayBullet(direction, target, (targetWeapPos == -1) ? null : targetCbt.GetWeapon(targetWeapPos));

                Cbt.IncreaseSP(data.SpIncreaseAmount); //TODO : check this
            } else{
                DisplayBullet(direction, null, null);
            }

            IncreaseHeat(data.HeatIncreaseAmount);
        }

        protected override void DisplayBullet(Vector3 direction, GameObject target, Weapon targetWeapon){
            _bullet = Object.Instantiate(BulletPrefab).GetComponent<Bullet>();
            _bullet.InitBullet(MechCam, PlayerPv, direction, (target == null) ? null : target.transform, this, targetWeapon);
            _bullet.transform.position = EffectEnd.position;

            _bullet.Play();
            Muzzle.Play();
            WeaponAudioSource.PlayOneShot(_shotSound);
        }

        public override void OnStateCallBack(int type, MechStateMachineBehaviour state){
            switch ((StateCallBackType) type){
                case StateCallBackType.AttackStateEnter:
                    _onShoot = true;

                    Cbt.KnockBack(-Cbt.transform.forward, 8);
                    break;
                case StateCallBackType.AttackStateExit:
                    _onShoot = false;
                    MechAnimator.SetBool("CnShoot", false);
                    break;
                case StateCallBackType.ReloadStateEnter:
                    _onReload = true;
                    WeaponAnimator.SetTrigger("Reload");
                    if (_reloadSound != null){
                        WeaponAudioSource.clip = _reloadSound;
                        WeaponAudioSource.PlayDelayed(0.2f); //To match the reload animation
                    }

                    break;
                case StateCallBackType.ReloadStateExit:
                    _onReload = false;
                    MechAnimator.SetBool("CnLoad", false);
                    _bulletNum = ((CannonData) data).MaxBullet;
                    break;
                case StateCallBackType.PoseStateEnter:
                    _onPose = true;
                    Mctrl.ResetCurBoostingSpeed();
                    //mechIK.SetIK (true, 1, 0);
                    break;
                case StateCallBackType.PoseStateExit:
                    _onPose = false;
                    MechAnimator.SetBool("CnPose", false);
                    //mechIK.SetIK (false, 1, 0);
                    break;
            }
        }
    }
}