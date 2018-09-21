using UnityEngine;

public class SMG : RangedWeapon {
    private AudioClip shotSound, reloadSound;
    private MultiBullets bulletTrace;

    private float animationLength, totalAtkAnimationLength, speedCoeff, lastPlayShotSoundTime;
    private int bulletNum;

    public SMG() {
        allowBothWeaponUsing = true;
    }

    protected override void InitAttackType() {
        attackType = AttackType.Ranged;
    }

    public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator) {
        base.Init(data, pos, handTransform, Cbt, Animator);
    }

    protected override void InitDataRelatedVars(WeaponData data) {
        base.InitDataRelatedVars(data);

        bulletNum = ((SMGData)data).bulletNum;
    }

    protected override void LoadSoundClips() {
        shotSound = ((SMGData)data).shotSound;
        reloadSound = ((SMGData)data).reload_sound;
    }

    public override void HandleCombat() {
        base.HandleCombat();
    }

    public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield) {
    }

    protected override void UpdateAnimationSpeed() {
        animationLength = Cbt.GetAnimationLength((hand == 0) ? "Atk_SMG_Run_LH_F_02" : "Atk_SMG_Run_RH_F_02");
        totalAtkAnimationLength = animationLength * bulletNum;
        speedCoeff = totalAtkAnimationLength / (1 / rate);
        MechAnimator.SetFloat((hand == 0) ? "SpeedLCoeff" : "SpeedRCoeff", speedCoeff);
    }

    private void UpdateBulletEffect(ParticleSystem Bullet_ps) {
        var main = Bullet_ps.main;
        main.duration = totalAtkAnimationLength / speedCoeff;
        main.maxParticles = bulletNum;

        var emission = Bullet_ps.emission;
        emission.rateOverTime = 1 / (animationLength / speedCoeff);
    }

    protected override void UpdateMuzzleEffect() {
        var main = Muzzle.main;
        main.duration = totalAtkAnimationLength / speedCoeff;

        var emission = Muzzle.emission;
        emission.rateOverTime = 1 / (animationLength / speedCoeff);
    }

    protected override void UpdateMechArmState() {
        MechAnimator.Play("SMG", 1 + hand);
    }

    protected override void DisplayBullet(Vector3 direction, GameObject target, Weapon targetWeapon) {
        GameObject Bullet = Object.Instantiate(BulletPrefab, EffectEnd);
        TransformExtension.SetLocalTransform(Bullet.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));

        UpdateBulletEffect(Bullet.GetComponent<ParticleSystem>());

        bulletTrace = Bullet.GetComponent<MultiBullets>();
        bulletTrace.InitBullet(MechCam, playerPv, direction, (target == null) ? null : target.transform, this, targetWeapon);

        bulletTrace.SetParticleSystem(bulletNum, animationLength);

        bulletTrace.Play();
        Muzzle.Play();
    }

    public override void OnSkillAction(bool b) {
        base.OnSkillAction(b);
        if (b) {//Stop effects playing when entering
            if (bulletTrace != null) bulletTrace.Stop();
            Muzzle.Stop();
            AudioSource.Stop();
        }
    }

    public override void OnSwitchedWeaponAction(bool b) {
        base.OnSwitchedWeaponAction(b);

        if (!b) {
            if (bulletTrace != null) bulletTrace.Stop();
            Muzzle.Stop();
            AudioSource.Stop();
        }
    }

    public override void OnStateCallBack(int type, MechStateMachineBehaviour state) {
        switch ((StateCallBackType)type) {
            case StateCallBackType.AttackStateUpdate:
            if (Time.time - lastPlayShotSoundTime >= animationLength / speedCoeff) {
                lastPlayShotSoundTime = Time.time;
                AudioSource.PlayOneShot(shotSound);
                if(playerPv.isMine)Crosshair.CallShakingEffect(hand);
            }
            break;
            case StateCallBackType.ReloadStateEnter:
            WeaponAnimator.SetTrigger("Reload");
            AudioSource.PlayOneShot(reloadSound);
            break;
        }
    }
}