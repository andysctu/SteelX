using UnityEngine;

public class Spear : MeleeWeapon {
    private AudioClip smashSound;

    public Spear() {
        allowBothWeaponUsing = false;
    }

    public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator) {
        base.Init(data, pos, handTransform, Cbt, Animator);
        InitComponents();
        //threshold = ((SpearData)data).threshold;

        //UpdateSmashAnimationThreshold();
    }

    private void InitComponents() {
        //FindTrail(weapon);
    }

    protected override void LoadSoundClips() {
        smashSound = ((SpearData)data).smash_sound;
    }

    //private void FindTrail(GameObject weapon) {
    //    WeaponTrail = weapon.GetComponentInChildren<XWeaponTrail>(true);
    //    if (WeaponTrail != null) WeaponTrail.Deactivate();
    //}

    public override void HandleCombat() {
        if (!Input.GetKeyDown(BUTTON) || IsOverHeat()) {
            return;
        }

        if (Time.time - timeOfLastUse >= 1 / rate) {
            if (!Cbt.CanMeleeAttack) { return; }

            if (anotherWeapon != null && !anotherWeapon.allowBothWeaponUsing && anotherWeapon.isFiring) return;

            Cbt.CanMeleeAttack = false;
            timeOfLastUse = Time.time;

            IncreaseHeat(data.heat_increase_amount);

            //Play Animation
            AnimationEventController.Smash(hand);
        }
    }

    public override void HandleAnimation() {
    }

    protected override void ResetMeleeVars() {//this is called when on skill or init
        base.ResetMeleeVars();

        if (!Cbt.photonView.isMine) return;

        Cbt.CanMeleeAttack = true;
        Cbt.SetMeleePlaying(false);
    }

    //public void EnableWeaponTrail(bool b) {
    //    if (WeaponTrail == null) return;

    //    if (b) {
    //        WeaponTrail.Activate();
    //    } else {
    //        WeaponTrail.StopSmoothly(0.1f);
    //    }
    //}

    public override void OnSkillAction(bool enter) {
        base.OnSkillAction(enter);

        if (enter) {
        } else {
            Cbt.CanMeleeAttack = true;
        }
    }

    protected override void OnAttackStateEnter(MechStateMachineBehaviour state) {//other player will also execute this
        //((SmashState)state).SetThreshold(threshold);//the state is confirmed SmashState in mechCombat      
        
        WeaponAnimator.SetTrigger("Atk");

        //Play slash sound
        if (smashSound != null)
            AudioSource.PlayClipAtPoint(smashSound, weapon.transform.position);

        if (player_pv != null && player_pv.isMine) {//TODO : master check this
            isFiring = true;

            MeleeAttack(hand);
        }
    }

    protected override void OnAttackStateMachineExit(MechStateMachineBehaviour state) {
        isFiring = false;
    }

    protected override void OnAttackStateExit(MechStateMachineBehaviour state) {
        if (((SmashState)state).IsInAir()) {
            isFiring = false;
        }
    }

    private void UpdateSlashAnimationThreshold() {
        //threshold = ((SpearData)data).threshold;
    }

    public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield) {

        if (isShield) {
            if(targetWeapon != null)
                targetWeapon.PlayOnHitEffect();
        } else {
            //Apply slowing down effect
            if (data.slowDown) {
                MechController mctrl = target.GetComponent<MechController>();
                if (mctrl != null) {
                    mctrl.SlowDown();
                }
            }

            ParticleSystem p = Object.Instantiate(HitEffectPrefab, target.transform);
            TransformExtension.SetLocalTransform(p.transform, new Vector3(0,5,0));
        }

    }

    protected override void InitAttackType() {
        attackType = AttackType.Melee;
    }
}