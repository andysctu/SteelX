using StateMachine;
using UnityEngine;

namespace Weapons
{
    using Bullets;

    public class SMG : RangedWeapon
    {
        private AudioClip _shotSound, _reloadSound;
        private MultiBullets _bulletTrace;

        private float _animationLength, _totalAtkAnimationLength, _speedCoeff, _lastPlayShotSoundTime;
        private int _bulletNum;

        public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator){
            base.Init(data, pos, handTransform, Cbt, Animator);

            UpdateAnimationSpeed();
            UpdateMuzzleEffect();
        }

        protected override void InitDataRelatedVars(WeaponData data){
            base.InitDataRelatedVars(data);

            _bulletNum = ((SMGData) data).bulletNum;
        }

        protected override void LoadSoundClips(){
            _shotSound = ((SMGData) data).shotSound;
            _reloadSound = ((SMGData) data).reload_sound;
        }

        //Update muzzle particle system to fit the rate
        protected void UpdateMuzzleEffect(){
            var main = Muzzle.main;
            main.duration = _totalAtkAnimationLength / _speedCoeff;

            var emission = Muzzle.emission;
            emission.rateOverTime = 1 / (_animationLength / _speedCoeff);
        }

        protected override void UpdateMechArmState(){
            MechAnimator.Play("SMG", 1 + Hand);
        }

        //Adjust the animation to fit the rate
        protected void UpdateAnimationSpeed(){
            _animationLength = Cbt.GetAnimationLength((Hand == 0) ? "Atk_SMG_Run_LH_F_02" : "Atk_SMG_Run_RH_F_02");
            _totalAtkAnimationLength = _animationLength * _bulletNum;
            _speedCoeff = _totalAtkAnimationLength / (1 / Rate);
            MechAnimator.SetFloat((Hand == 0) ? "SpeedLCoeff" : "SpeedRCoeff", _speedCoeff);
        }

        protected override void UpdateIk(){
            base.UpdateIk();
            //Debug.Log("Update ik");
            if (CurTarget != null){
                //Face the target
                Debug.DrawRay(Cbt.transform.position, Spine1.up * 10, Color.red, 1);

                Debug.DrawRay(Cbt.transform.position, CurTarget.transform.position - Cbt.transform.position, Color.red, 1);

                //Debug.Log("Angle between : "+ Vector3.Angle(Spine1.up, CurTarget.transform.position - Cbt.transform.position));
            }
        }

        protected override void DisplayBullet(Vector3 direction, GameObject target, Weapon targetWeapon){
            GameObject bullet = Object.Instantiate(BulletPrefab, EffectEnd);
            TransformExtension.SetLocalTransform(bullet.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));

            UpdateBulletEffect(bullet.GetComponent<ParticleSystem>());

            _bulletTrace = bullet.GetComponent<MultiBullets>();
            _bulletTrace.InitBullet(MechCam, PlayerPv, direction, (target == null) ? null : target.transform, this, targetWeapon);
            _bulletTrace.SetParticleSystem(_bulletNum, _animationLength);

            _bulletTrace.Play();
            Muzzle.Play();
        }

        private void UpdateBulletEffect(ParticleSystem bulletPs){
            var main = bulletPs.main;
            main.duration = _totalAtkAnimationLength / _speedCoeff;
            main.maxParticles = _bulletNum;

            var emission = bulletPs.emission;
            emission.rateOverTime = 1 / (_animationLength / _speedCoeff);
        }

        public override void OnRateChanged(){
            UpdateAnimationSpeed();
            UpdateMuzzleEffect();
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);
            if (enter){
                StopBulletTrace();
            }
        }

        public override void OnWeaponSwitchedAction(bool isThisWeaponActivated){
            base.OnWeaponSwitchedAction(isThisWeaponActivated);
            if (!isThisWeaponActivated){
                StopBulletTrace();
            }
        }

        private void StopBulletTrace(){
            if (_bulletTrace != null) _bulletTrace.Stop();
            Muzzle.Stop();
            AudioSource.Stop();
        }

        public override void OnStateCallBack(int type, MechStateMachineBehaviour state){
            switch ((StateCallBackType) type){
                case StateCallBackType.AttackStateEnter:
                    IsIkOn = true;
                    break;
                case StateCallBackType.AttackStateUpdate:
                    if (Time.time - _lastPlayShotSoundTime >= _animationLength / _speedCoeff){
                        _lastPlayShotSoundTime = Time.time;
                        AudioSource.PlayOneShot(_shotSound);
                        WeaponAnimator.SetTrigger("Atk");
                        if (PlayerPv.isMine) {
                            if (CrosshairController != null) CrosshairController.OnShootAction(WeapPos);
                        }
                    }
                    break;
                case StateCallBackType.AttackStateExit:
                    IsIkOn = false;
                    break;
                case StateCallBackType.ReloadStateEnter:
                    WeaponAnimator.SetTrigger("Reload");
                    AudioSource.PlayOneShot(_reloadSound);
                    break;
            }
        }
    }
}