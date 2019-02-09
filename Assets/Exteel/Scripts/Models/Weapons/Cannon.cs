using UnityEngine;

namespace Weapons
{
    using Bullets;

    public class Cannon : RangedWeapon
    {
        private AudioClip _shotSound, _reloadSound;
        private Bullet _bullet;
        private int _bulletNum = 3;

        private float _reloadStartTime, _reloadAnimationLength;
        private bool _onPose, _onShoot, _onReload, _isCanceled;

        public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, hand, handTransform, Cbt, Animator);
            InitComponents();

            ResetAnimationVars();
            ResetBulletNum();
        }

        protected override void InitDataRelatedVars(WeaponData data){
            base.InitDataRelatedVars(data);

            _reloadAnimationLength = ((CannonData)data).ReloadAnimationLength;
        }

        private void InitComponents(){
            Mctrl = Cbt.GetComponent<MechController>();
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);
            if (enter){
                ResetAnimationVars();
            }
        }

        private void ResetAnimationVars(){
            _onShoot = false;
            _onPose = false;
            _onReload = false;

            MechAnimator.SetBool("CnPose", false);
            MechAnimator.SetBool("CnLoad", false);
            MechAnimator.SetBool("CnShoot", false);
        }

        private void ResetBulletNum(){
            _bulletNum = ((CannonData) data).MaxBullet;
        }

        protected override void LoadSoundClips(){
            _shotSound = ((CannonData) data).shotSound;
            _reloadSound = ((CannonData) data).reload_sound;
        }

        public override void OnWeaponSwitchedAction(bool isThisActivated){
            if (!isThisActivated){
                ResetAnimationVars();

                Muzzle.Stop();
                WeaponAudioSource.Stop();
            }
        }

        protected override void UpdateMechArmState(){
            MechAnimator.Play("Cn", 1);
            MechAnimator.Play("Cn", 2);
        }

        public override void HandleCombat(usercmd cmd) {
            if (IsFiring){
                if (Time.time - TimeOfLastUse > AtkAnimationLength){
                    AttackEndAction();
                }
            }else if (_onReload){
                if (Time.time - _reloadStartTime > _reloadAnimationLength){
                    _onReload = false;
                }
            }

            if (_bulletNum < 1) {
                _reloadStartTime = Time.time;
                _onReload = true;
                _bulletNum = ((CannonData) data).MaxBullet;
            }

            if (Input.GetKeyDown(KeyCode.Mouse1) || IsOverHeat()){
                _isCanceled = true;
                OnPoseAction(false);
            }

            if (_isCanceled){
                if (!Input.GetKey(KeyCode.Mouse0)){
                    _isCanceled = false;
                } else{
                    return;
                }
            }

            if (Time.time - TimeOfLastUse >= 1 / Rate && _onPose && !_onReload){
                if (Input.GetKey(KeyCode.Mouse0) || !Mctrl.Grounded) return;
                AttackStartAction();
                TimeOfLastUse = Time.time;
            }

            bool b = Input.GetKey(KeyCode.Mouse0) && !_onReload && _bulletNum >= 1 && Mctrl.Grounded;
            if(_onPose != b)OnPoseAction(b);
        }

        public override void HandleAnimation(){
            if (MechAnimator.GetBool(AnimatorHashVars.CnPoseHash) != _onPose){
                MechAnimator.SetBool(AnimatorHashVars.CnPoseHash, _onPose);
            }

            if (MechAnimator.GetBool(AnimatorHashVars.CnLoadHash) != _onReload) {
                ReloadEffect();
                MechAnimator.SetBool(AnimatorHashVars.CnLoadHash, _onReload);
            }

            if (MechAnimator.GetBool(AnimatorHashVars.CnShootHash) != _onShoot) {
                MechAnimator.SetBool(AnimatorHashVars.CnShootHash, _onShoot);
            }
        }

        protected virtual void ReloadEffect(){
            WeaponAnimator.SetTrigger("Reload");

            if (_reloadSound != null) {
                WeaponAudioSource.clip = _reloadSound;
                WeaponAudioSource.PlayDelayed(0.2f); //To match the reload animation
            }
        }

        protected virtual void OnPoseAction(bool enter){
            _onPose = enter;

            Mctrl.LockMechMovement(enter);
            if (enter){
                Mctrl.ResetCurBoostingSpeed();
                //mechIK.SetIK (true, 1, 0);
            }
        }

        //public override void AttackRpc(Vector3 direction, int damage, int[] targetPvIDs, int[] specIDs, int[] additionalFields) {
        //    base.AttackRpc(direction, damage, targetPvIDs, specIDs, additionalFields);
            
        //}

        protected override void AttackStartAction(){
            base.AttackStartAction();
            _onShoot = true;
            Mctrl.SetInstantMoving(- Mctrl.GetForwardVector(), 16, 0.4f);
            MechAnimator.SetBool(AnimatorHashVars.CnShootHash, true);
            if(PhotonNetwork.isMasterClient)Cbt.Attack(WeapPos, MechCam.transform.forward, data.damage, null, null);

            _bulletNum --;
        }

        protected virtual void AttackEndAction(){
            IsFiring = false;

            MechAnimator.SetBool(AnimatorHashVars.CnShootHash, false);
        }

        protected override void PlayShootEffect(Vector3 direction, IDamageable target) {
            base.PlayShootEffect(direction, target);

            WeaponAudioSource.PlayOneShot(_shotSound);
        }

        protected override void DisplayBullet(Vector3 direction, IDamageable target) {
            _bullet = Object.Instantiate(BulletPrefab).GetComponent<Bullet>();
            _bullet.transform.position = EffectEnd.position;

            _bullet.InitBullet(MechCam, PlayerPv, direction, target);
            _bullet.Play();
        }

        public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
            if (stream.isReading){
                if(Cbt.GetOwner().IsLocal)return;

                if (stream.PeekNext() is bool == false) {
                    return;
                }
                _onPose = (bool)stream.ReceiveNext();

                if (stream.PeekNext() is bool == false) {
                    return;
                }
                _onReload = (bool)stream.ReceiveNext();

                if (stream.PeekNext() is bool == false) {
                    return;
                }
                _onShoot = (bool)stream.ReceiveNext();
            } else{
                stream.SendNext(_onPose);
                stream.SendNext(_onReload);
                stream.SendNext(_onShoot);
            }
        }
    }
}
