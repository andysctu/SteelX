using UnityEngine;
using Weapons;
using Weapons.Bullets;

public class Rifle : RangedWeapon {
    private AudioClip _shotSound, _reloadSound;
    private Bullet _bullet;
    private bool _playedBulletEffect;

    protected override void LoadSoundClips() {
        _shotSound = ((RifleData)data).shotSound;
        _reloadSound = ((RifleData)data).reload_sound;
    }

    protected override void UpdateMechArmState() {
        MechAnimator.Play("Rifle", 1 + Hand);
    }

    public override void HandleAnimation() {
        if (IsFiring) {
            if (Time.time - TimeOfLastUse >= 0.2f) { //0.2f : animation min time
                MechAnimator.SetBool(AtkAnimHash, false);
                IsFiring = false;

                ReloadEffect();
            }

            //if (IsIkOn) {
            //    UpdateIk();
            //}
        }
    }

    protected override void DisplayBullet(Vector3 direction, IDamageable target) {
        _bullet = Object.Instantiate(BulletPrefab).GetComponent<Bullet>();
        _bullet.transform.position = EffectEnd.position;

        //_bullet.InitBullet(MechCam, PlayerPv, direction, target);
        _bullet.Play();
    }

    public override void OnSkillAction(bool enter) {
        base.OnSkillAction(enter);
        if (enter) {//Stop effects playing when entering
            Muzzle.Stop();
            WeaponAudioSource.Stop();
        }
    }

    public override void OnWeaponSwitchedAction(bool isThisWeaponActivated) {
        base.OnWeaponSwitchedAction(isThisWeaponActivated);

        if (!isThisWeaponActivated) {
            Muzzle.Stop();
            WeaponAudioSource.Stop();
        }
    }

    private void ReloadEffect(){
        WeaponAnimator.SetTrigger("Reload");
        if (_reloadSound != null) WeaponAudioSource.PlayOneShot(_reloadSound);
    }

    protected override void PlayShootEffect(Vector3 direction, IDamageable target) {
        base.PlayShootEffect(direction, target);

        WeaponAudioSource.PlayOneShot(_shotSound);
    }
}