using UnityEngine;
using XftWeapon;

public class Sword : MeleeWeapon {
    private XWeaponTrail WeaponTrail;
    private AudioClip[] slashSounds = new AudioClip[4];
    private AudioClip slashOnHitSound;

    private bool receiveNextSlash, isAnotherWeaponSword;
    private const int slashMaxDistance = 30;
    private int curCombo = 0;

    public Sword() {
        allowBothWeaponUsing = false;
    }

    public override void Init(WeaponData data, int hand, Transform handTransform, MechCombat mcbt, Animator Animator) {
        base.Init(data, hand, handTransform, mcbt, Animator);

        threshold = ((SwordData)data).threshold;

        CheckIsAnotherWeaponSword();
    }

    protected override void InitComponents() {
        base.InitComponents();
        FindTrail(weapon);
    }

    private void CheckIsAnotherWeaponSword() {
        isAnotherWeaponSword = (anotherWeaponData.GetWeaponType() == typeof(Sword));
    }

    protected override void LoadSoundClips() {
        slashSounds = ((SwordData)data).slash_sound;
        slashOnHitSound = ((SwordData)data).slash_hit_sound;
    }

    private void FindTrail(GameObject weapon) {
        WeaponTrail = weapon.GetComponentInChildren<XWeaponTrail>(true);
        if (WeaponTrail != null) WeaponTrail.Deactivate();
    }

    public override void HandleCombat() {
        if (!Input.GetKeyDown(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || IsOverHeat()) {
            return;
        }

        if (Time.time - timeOfLastUse >= 1 / data.Rate) {
            if (!receiveNextSlash || !mcbt.CanMeleeAttack) {return;}

            if (curCombo == 3 && !isAnotherWeaponSword)return;

            if (!anotherWeapon.allowBothWeaponUsing && anotherWeapon.isFiring)return;

            mcbt.CanMeleeAttack = false;
            receiveNextSlash = false;
            timeOfLastUse = Time.time;

            IncreaseHeat();

            //Play Animation
            AnimationEventController.Slash(hand, curCombo);            
        }
    }

    public override void HandleAnimation() {
    }

    protected override void ResetMeleeVars() {//this is called when on skill or init
        base.ResetMeleeVars();
        curCombo = 0;

        if (!mcbt.photonView.isMine) return;

        receiveNextSlash = true;

        mcbt.CanMeleeAttack = true;
        mcbt.SetMeleePlaying(false);
        MechAnimator.SetBool("Slash", false);
    }

    public void EnableWeaponTrail(bool b) { 
        if (WeaponTrail == null) return;

        if (b) {
            WeaponTrail.Activate();
        } else {
            WeaponTrail.StopSmoothly(0.1f);
        }
    }

    public override void OnSkillAction(bool enter) {
        base.OnSkillAction(enter);

        if (enter) {
            receiveNextSlash = false;
        } else {
            receiveNextSlash = true;
        }
    }

    public override void OnAttackStateEnter(MechStateMachineBehaviour state) {//other player will also execute this
        ((SlashState)state).SetThreshold(threshold);//the state is confirmed SlashState in mechCombat        

        //Play slash sound
        if(slashSounds!=null && slashSounds[curCombo] != null)
            AudioSource.PlayClipAtPoint(slashSounds[curCombo], weapon.transform.position);

        //If not final slash
        if (!MechAnimator.GetBool(AnimatorVars.finalSlash_id))
            receiveNextSlash = true;


        if(photonView != null && photonView.isMine) {//TODO : master check this
            SlashDetect(hand);
        }

        isFiring = true;
        curCombo++;
    }

    public override void OnAttackStateMachineExit(MechStateMachineBehaviour state) {
        isFiring = false;
        receiveNextSlash = true;
        curCombo = 0;
    }

    public override void OnAttackStateExit(MechStateMachineBehaviour state) {
        if (((SlashState)state).IsInAir()) {
            isFiring = false;
            receiveNextSlash = true;
            curCombo = 0;
        }
    }

    private void UpdateSlashAnimationThreshold() {
       threshold = ((SwordData)data).threshold;
    }

    public override void AttackTarget(GameObject target, bool isShield) {
        throw new System.NotImplementedException();
    }
}
