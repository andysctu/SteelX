using UnityEngine;

public class Spear : MeleeWeapon {
    private AudioClip smashSound;

    public Spear() {
        allowBothWeaponUsing = false;
    }

    public override void Init(WeaponData data, int hand, Transform handTransform, MechCombat mcbt, Animator Animator) {
        base.Init(data, hand, handTransform, mcbt, Animator);

        //threshold = ((SpearData)data).threshold;

        //UpdateSmashAnimationThreshold();
    }

    protected override void InitComponents() {
        base.InitComponents();
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

        if (Time.time - timeOfLastUse >= 1 / data.Rate) {
            if (!mcbt.CanMeleeAttack) { return; }

            if (anotherWeapon != null && !anotherWeapon.allowBothWeaponUsing && anotherWeapon.isFiring) return;

            mcbt.CanMeleeAttack = false;
            timeOfLastUse = Time.time;

            IncreaseHeat();

            //Play Animation
            AnimationEventController.Smash(hand);
        }
    }

    public override void HandleAnimation() {
    }

    protected override void ResetMeleeVars() {//this is called when on skill or init
        base.ResetMeleeVars();

        if (!mcbt.photonView.isMine) return;

        mcbt.CanMeleeAttack = true;
        mcbt.SetMeleePlaying(false);
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
            mcbt.CanMeleeAttack = true;
        }
    }

    public override void OnAttackStateEnter(MechStateMachineBehaviour state) {//other player will also execute this
        //((SmashState)state).SetThreshold(threshold);//the state is confirmed SmashState in mechCombat      
        
        WeaponAnimator.SetTrigger("Atk");

        //Play slash sound
        if (smashSound != null)
            AudioSource.PlayClipAtPoint(smashSound, weapon.transform.position);

        if (photonView != null && photonView.isMine) {//TODO : master check this
            isFiring = true;

            MeleeAttack(hand);
        }
    }

    public override void OnAttackStateMachineExit(MechStateMachineBehaviour state) {
        isFiring = false;
    }

    public override void OnAttackStateExit(MechStateMachineBehaviour state) {
        if (((SmashState)state).IsInAir()) {
            isFiring = false;
        }
    }

    private void UpdateSlashAnimationThreshold() {
        //threshold = ((SpearData)data).threshold;
    }

    public override void OnTargetEffect(GameObject target, bool isShield) {

        if (isShield) {

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

}