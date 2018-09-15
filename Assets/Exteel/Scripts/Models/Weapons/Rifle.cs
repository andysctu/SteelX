using UnityEngine;

public class Rifle : RangedWeapon {
    private AudioClip shotSound, reloadSound;
    private Bullet bullet;

    private float animationStartTime;

    public Rifle() {
        allowBothWeaponUsing = true;
    }

    protected override void InitAttackType() {
        attackType = AttackType.Ranged;
    }

    protected override void LoadSoundClips() {
        shotSound = ((RifleData)data).shotSound;
        reloadSound = ((RifleData)data).reload_sound;
    }

    public override void HandleCombat() {
        base.HandleCombat();
    }

    public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield) {
        if (data.slowDown && !isShield) {
            target.GetComponent<MechController>().SlowDown();
        }
    }

    protected override void UpdateAnimationSpeed() {
    }

    private void UpdateBulletEffect(ParticleSystem Bullet_ps) {
    }

    protected override void UpdateMuzzleEffect() {
    }

    protected override void UpdateMechArmState() {
        MechAnimator.Play("Rifle", 1 + hand);
    }

    public override void HandleAnimation() {
        if (isFiring) {
            if (Time.time - startShootTime >= 0.1f) { //0.1f : animation min time
                if (isAtkAnimationPlaying) {
                    isAtkAnimationPlaying = false;
                    MechAnimator.SetBool(AtkAnimHash, false);
                }
            } else {
                if (!isAtkAnimationPlaying) {
                    MechAnimator.SetBool(AtkAnimHash, true);
                    isAtkAnimationPlaying = true;
                }
            }
        } else {
            if (isAtkAnimationPlaying) {
                MechAnimator.SetBool(AtkAnimHash, false);
                isAtkAnimationPlaying = false;
            }
        }
    }

    protected override void DisplayBullet(Vector3 direction, GameObject Target, Weapon targetWeapon) {
        bullet = Object.Instantiate(BulletPrefab).GetComponent<Bullet>();

        bullet.InitBullet(MechCam, player_pv, direction, (Target == null) ? null : Target.transform, this, targetWeapon);   
    }

    public override void OnSkillAction(bool b) {
        base.OnSkillAction(b);
        if (b) {//Stop effects playing when entering
            Muzzle.Stop();
            AudioSource.Stop();
        }
    }

    public override void OnSwitchedWeaponAction(bool b) {
        base.OnSwitchedWeaponAction(b);

        if (!b) {
            Muzzle.Stop();
            AudioSource.Stop();
        }
    }

    public override void OnStateCallBack(int type, MechStateMachineBehaviour state) {
        switch ((StateCallBackType)type) {
            case StateCallBackType.AttackStateEnter:
            break;
            case StateCallBackType.AttackStateUpdate:
            if (bullet != null && Time.time - animationStartTime >= 0.05f) {//Play the effects here to fit the animation 
                                                                            //MechAnimator.Update(0);//For the arm to be in right position , BUG : StateEnter won't trigger on two layer
                bullet.transform.position = Effect_End.position;
                bullet.Play();
                bullet = null;
                Muzzle.Play();
                AudioSource.PlayOneShot(shotSound);
                if (player_pv.isMine) Crosshair.CallShakingEffect(hand);
            }
            break;
            case StateCallBackType.ReloadStateEnter:
            WeaponAnimator.SetTrigger("Reload");
            if(reloadSound!=null)AudioSource.PlayOneShot(reloadSound);
            break;
        }
    }
}