using UnityEngine;

public class Shotgun : RangedWeapon {
    private AudioClip shotSound, reloadSound;
    private Bullet bullet;

    private float animationStartTime;

    public Shotgun() {
        allowBothWeaponUsing = true;
    }

    protected override void InitAttackType() {
        attackType = AttackType.Ranged;
    }

    protected override void LoadSoundClips() {
        shotSound = ((ShotgunData)data).shotSound;
        reloadSound = ((ShotgunData)data).reload_sound;
    }

    public override void HandleCombat() {
        base.HandleCombat();
    }

    public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield) {
        if (data.slowDown && !isShield) {//TODO : drone mech controller
            //target.GetComponent<MechController>().SlowDown();
        }
    }

    protected override void UpdateAnimationSpeed() {
    }

    private void UpdateBulletEffect(ParticleSystem Bullet_ps) {
    }

    protected override void UpdateMuzzleEffect() {
    }

    protected override void UpdateMechArmState() {
        MechAnimator.Play("SGN", 1 + hand);
    }

    public override void HandleAnimation() {
        if (isFiring) {
            if (Time.time - startShootTime >= 0.1f) { //0.1f : animation min time
                if (atkAnimationIsPlaying) {
                    atkAnimationIsPlaying = false;
                    MechAnimator.SetBool(AtkAnimHash, false);
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
        bullet = Object.Instantiate(BulletPrefab).GetComponent<Bullet>();

        //bullet.InitBulletTrace(MechCam, playerPv);
        //bullet.InitTargetInfos((target == null) ? null : target.transform, targetWeapon);
        //bullet.SetDirection(direction);
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
            animationStartTime = Time.time;
            break;
            case StateCallBackType.AttackStateUpdate:
            if (bullet != null && Time.time - animationStartTime >= 0.05f) {//Play the effects here to fit the animation 
                //MechAnimator.Update(0);//For the arm to be in right position , BUG : StateEnter won't trigger on two layer
                
                bullet.transform.position = EffectEnd.position;
                bullet.Play();
                bullet = null;
                Muzzle.Play();
                AudioSource.PlayOneShot(shotSound);
                if (playerPv.isMine) Crosshair.CallShakingEffect(hand);
            }
            break;
            case StateCallBackType.ReloadStateEnter:
            WeaponAnimator.SetTrigger("Reload");
            if (reloadSound != null) AudioSource.PlayOneShot(reloadSound);
            break;
            default:
            Debug.Log("should not go here");
            break;
        }
    }
}