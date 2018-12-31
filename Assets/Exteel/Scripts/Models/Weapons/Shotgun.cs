using StateMachine;
using UnityEngine;
using Weapons;
using Weapons.Bullets;

public class Shotgun : RangedWeapon {
    private AudioClip _shotSound, _reloadSound;
    private Bullet _bullet;

    private float _animationStartTime;

    protected override void LoadSoundClips() {
        _shotSound = ((ShotgunData)data).shotSound;
        _reloadSound = ((ShotgunData)data).reload_sound;
    }

    public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield) {
        if (data.Slowdown && !isShield) {//TODO : IController
            //target.GetComponent<MechController>().SlowDown();
        }
    }

    protected override void UpdateMechArmState() {
        MechAnimator.Play("SGN", 1 + Hand);
    }

    public override void HandleAnimation() {
        if (IsFiring) {
            if (Time.time - startShootTime >= 0.1f) { //0.1f : animation min time
                if (atkAnimationIsPlaying) {
                    atkAnimationIsPlaying = false;
                    MechAnimator.SetBool(AtkAnimHash, false);
                    IsFiring = false;
                }
            } else {
                if (!atkAnimationIsPlaying) {
                    MechAnimator.SetBool(AtkAnimHash, true);
                    atkAnimationIsPlaying = true;
                }
            }
        } else {
            if (atkAnimationIsPlaying) {
                MechAnimator.SetBool(AtkAnimHash, false);
                atkAnimationIsPlaying = false;
            }
        }
    }

    protected override void DisplayBullet(Vector3 direction, GameObject target, Weapon targetWeapon) {
        _bullet = Object.Instantiate(BulletPrefab).GetComponent<Bullet>();

        _bullet.InitBullet(MechCam, PlayerPv, direction, (target == null) ? null : target.transform, this, targetWeapon);
    }

    public override void OnSkillAction(bool enter) {
        base.OnSkillAction(enter);
        if (enter) {//Stop effects playing when entering
            Muzzle.Stop();
            AudioSource.Stop();
        }
    }

    public override void OnWeaponSwitchedAction(bool isThisWeaponActivated) {
        base.OnWeaponSwitchedAction(isThisWeaponActivated);

        if (!isThisWeaponActivated) {
            Muzzle.Stop();
            AudioSource.Stop();
        }
    }

    public override void OnStateCallBack(int type, MechStateMachineBehaviour state) {
        switch ((StateCallBackType)type) {
            case StateCallBackType.AttackStateEnter:
            _animationStartTime = Time.time;
            break;
            case StateCallBackType.AttackStateUpdate:
            if (_bullet != null && Time.time - _animationStartTime >= 0.05f) {//Play the effects here to fit the animation       
                _bullet.transform.position = EffectEnd.position;
                _bullet.Play();
                _bullet = null;
                Muzzle.Play();
                AudioSource.PlayOneShot(_shotSound);
            }
            break;
            case StateCallBackType.ReloadStateEnter:
            WeaponAnimator.SetTrigger("Reload");
            if (_reloadSound != null) AudioSource.PlayOneShot(_reloadSound);
            break;
        }
    }
}