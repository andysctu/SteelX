using UnityEngine;
using XftWeapon;

public class Sword : MeleeWeapon {
    private XWeaponTrail WeaponTrail;
    private AudioClip[] slashSounds = new AudioClip[4];

    private bool receiveNextSlash, isAnotherWeaponSword;
    private const int slashMaxDistance = 30;
    private int curCombo = 0;

    public Sword() {
        allowBothWeaponUsing = false;
    }

    public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator) {
        base.Init(data, pos, handTransform, Cbt, Animator);
        InitComponents();
        threshold = ((SwordData)data).threshold;

        CheckIsAnotherWeaponSword();

        UpdateSlashAnimationThreshold();
    }

    private void InitComponents() {
        FindTrail(weapon);
    }

    private void CheckIsAnotherWeaponSword() {
        isAnotherWeaponSword = (anotherWeaponData.GetWeaponType() == typeof(Sword));
    }

    protected override void LoadSoundClips() {
        slashSounds = ((SwordData)data).slash_sound;
    }

    private void FindTrail(GameObject weapon) {
        WeaponTrail = weapon.GetComponentInChildren<XWeaponTrail>(true);
        if (WeaponTrail != null) WeaponTrail.Deactivate();
    }

    public override void HandleCombat() {
        if (!Input.GetKeyDown(BUTTON) || IsOverHeat()) {
            return;
        }

        if (Time.time - timeOfLastUse >= 1 / rate) {
            if (!receiveNextSlash || !Cbt.CanMeleeAttack) {return;}

            if (curCombo == 3 && !isAnotherWeaponSword)return;

            if (anotherWeapon!=null && !anotherWeapon.allowBothWeaponUsing && anotherWeapon.isFiring)return;

            Cbt.CanMeleeAttack = false;
            receiveNextSlash = false;
            timeOfLastUse = Time.time;

            IncreaseHeat(data.heat_increase_amount);

            //Play Animation
            AnimationEventController.Slash(hand, curCombo);            
        }
    }

    public override void HandleAnimation() {
    }

    protected override void ResetMeleeVars() {//this is called when on skill or init
        base.ResetMeleeVars();
        curCombo = 0;

        if (!Cbt.photonView.isMine) return;

        receiveNextSlash = true;

        Cbt.CanMeleeAttack = true;
        Cbt.SetMeleePlaying(false);
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
            Cbt.CanMeleeAttack = true;
        }
    }

    protected override void OnAttackStateEnter(MechStateMachineBehaviour state) {//other player will also execute this
        ((SlashState)state).SetThreshold(threshold);//the state is confirmed SlashState in mechCombat        

        //Play slash sound
        if(slashSounds!=null && slashSounds[curCombo] != null)
            AudioSource.PlayClipAtPoint(slashSounds[curCombo], weapon.transform.position);

        if(player_pv != null && player_pv.isMine) {//TODO : master check this
            isFiring = true;

            //If not final slash
            if (!MechAnimator.GetBool(AnimatorVars.finalSlash_id))
                receiveNextSlash = true;

            MeleeAttack(hand);
        }

        curCombo++;
    }

    protected override void OnAttackStateMachineExit(MechStateMachineBehaviour state) {
        isFiring = false;
        receiveNextSlash = true;
        curCombo = 0;
    }

    protected override void OnAttackStateExit(MechStateMachineBehaviour state) {
        if (((SlashState)state).IsInAir()) {
            isFiring = false;
            receiveNextSlash = true;
            curCombo = 0;
        }
    }

    private void UpdateSlashAnimationThreshold() {
       threshold = ((SwordData)data).threshold;
    }

    public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield) {
        
        if (isShield) {
            if(targetWeapon != null) {
                targetWeapon.PlayOnHitEffect();
            }
        } else {
            //Apply slowing down effect
            if (data.slowDown) {
                MechController mctrl = target.GetComponent<MechController>();
                if(mctrl != null) {
                    mctrl.SlowDown();
                }
            }

            ParticleSystem p = Object.Instantiate(HitEffectPrefab, target.transform);
            TransformExtension.SetLocalTransform(p.transform, new Vector3(0, 5, 0));
        }
    }

    protected override void InitAttackType() {
        attackType = AttackType.Melee;
    }
}
