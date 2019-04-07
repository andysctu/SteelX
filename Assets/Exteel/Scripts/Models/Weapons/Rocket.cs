using StateMachine;
using UnityEngine;

namespace Weapons
{
    using Bullets;

    public class Rocket : RangedWeapon
    {
        private AudioClip _shotSound, _reloadSound;
        private RocketBullet _RocketBullet;

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

        protected override void UpdateMechArmState(){
            MechAnimator.Play("Rocket", 1);
            MechAnimator.Play("Rocket", 2);
        }

        public override void HandleAnimation(){
            if (IsFiring) {
                if (Time.time - TimeOfLastUse >= AtkAnimationLength) {
                    MechAnimator.SetBool(AtkAnimHash, false);
                    IsFiring = false;

                    ReloadEffect();
                }

                //if (IsIkOn) {
                //    UpdateIk();
                //}
            }
        }

        protected override void AttackStartAction() {
            IsFiring = true;

            PlayShootEffect(MechCam.transform.forward);
            //if (Cbt.GetOwner().IsLocal || PhotonNetwork.isMasterClient) DisplayBullet(MechCam.transform.forward, PhotonNetwork.AllocateViewID());

            //if (Cbt.GetOwner().IsLocal) {//crosshair effect
            //    if (CrosshairController != null) CrosshairController.OnShootAction(WeapPos);
            //}

            //if (PhotonNetwork.isMasterClient) Cbt.Attack(WeapPos, MechCam.transform.forward, data.damage, null, null);
        }

        public override void AttackRpc(Vector3 direction, int damage, int[] targetPvIDs, int[] specIDs, int[] additionalFields) {
            int photonViewId = additionalFields[0];

            //if (Cbt.GetOwner().IsLocal){
            //    if (_RocketBullet == null){Debug.LogError("local client should not have rocket null");return; }
            //    _RocketBullet.SetPhotonViewID(photonViewId);
            //    return;
            //}

            TimeOfLastUse = Time.time;
            IsFiring = true;

            PlayShootEffect(direction);
            DisplayBullet(direction, additionalFields[0]);
        }

        protected void PlayShootEffect(Vector3 direction) {
            MechAnimator.SetBool(AtkAnimHash, true);
            MechAnimator.Play("RocketShootL", 1);
            MechAnimator.Play("RocketShootR", 2);
            MechAnimator.Update(0);
            WeaponAnimator.SetTrigger("Atk");

            Muzzle.Play();
        }

        protected void DisplayBullet(Vector3 direction, int photonViewID) {
            GameObject bulletPrefab = Resources.Load(BulletPrefab.name) as GameObject;
            _RocketBullet = Object.Instantiate(bulletPrefab, EffectEnd.position, Quaternion.LookRotation(direction, Vector3.up)).GetComponent<RocketBullet>();
            //_RocketBullet.InitBullet(MechCam, PlayerPv, direction, null);
            _RocketBullet.SetBulletProperties(this, data.damage, _bulletSpeed, _impactRadius);
            //_RocketBullet.SetShooter(Cbt.GetOwner());

            _RocketBullet.SetPhotonViewID(photonViewID);
        }

        public override void OnSkillAction(bool enter){
            base.OnSkillAction(enter);
            if (enter){
                //Stop effects playing when entering
                Muzzle.Stop();
                WeaponAudioSource.Stop();
            }
        }

        public override void OnWeaponSwitchedAction(bool isThisWeaponActivated){
            base.OnWeaponSwitchedAction(isThisWeaponActivated);

            if (!isThisWeaponActivated){
                Muzzle.Stop();
                WeaponAudioSource.Stop();
            }
        }

        private void ReloadEffect() {
            WeaponAnimator.SetTrigger("Reload");
            if (_reloadSound != null) WeaponAudioSource.PlayOneShot(_reloadSound);
        }
    }
}