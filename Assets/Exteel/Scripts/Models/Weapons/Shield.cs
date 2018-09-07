using UnityEngine;

public class Shield : Weapon {
    private Transform EffectEnd;
    private ShieldActionReceiver ShieldActionReceiver;
    private AudioClip OnHitSound;
    private ParticleSystem OnHitEffect, OverheatEffect;

    private int block_id = 0;

    public Shield() {
        allowBothWeaponUsing = false;
    }
    
    public override void Init(WeaponData data, int pos, Transform handTransform, Combat Cbt, Animator Animator) {
        base.Init(data, pos, handTransform, Cbt, Animator);
        InitComponents();
        AddShieldActionReceiver();
        AttachEffects();
        ResetAnimationVars();
    }

    private void InitComponents() {
        EffectEnd = weapon.transform.Find("EffectEnd");
        if (AnimatorVars != null)block_id = (hand == 0) ? AnimatorVars.blockL_id : AnimatorVars.blockR_id;
    }

    private void AddShieldActionReceiver() {
        ShieldActionReceiver = weapon.AddComponent<ShieldActionReceiver>();
        ShieldActionReceiver.SetPos(weapPos);        
    }

    private void AttachEffects() {
        //OnHitEffect
        OnHitEffect = Object.Instantiate(((ShieldData)data).OnHitEffect, EffectEnd);
        TransformExtension.SetLocalTransform(OnHitEffect.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));

        //OverheatEffect
        OverheatEffect = Object.Instantiate(((ShieldData)data).OverheatEffect, weapon.transform);
        TransformExtension.SetLocalTransform(OverheatEffect.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
    }

    private void ResetAnimationVars() {
        isFiring = false;
        if (block_id != 0) MechAnimator.SetBool(block_id, false);
    }

    public override void OnSkillAction(bool enter) {
        base.OnSkillAction(enter);
        ResetAnimationVars();
    }

    public override void OnSwitchedWeaponAction(bool b) {
        ResetAnimationVars();
    }

    public override void HandleCombat() {
        if (!Input.GetKey(BUTTON) || IsOverHeat()) {
            isFiring = false;
            return;
        }

        if (anotherWeapon != null && !anotherWeapon.allowBothWeaponUsing && anotherWeapon.isFiring) return;

        isFiring = true;
    }

    public override void HandleAnimation() {
        if (isFiring) {
            if (!MechAnimator.GetBool(block_id)) {
                MechAnimator.SetBool(block_id, true);
            }
        } else {
            if (MechAnimator.GetBool(block_id)) {
                MechAnimator.SetBool(block_id, false);
            }
        }
    }

    protected override void LoadSoundClips() {
        OnHitSound = ((ShieldData)data).OnHitSound;
    }

    public override void OnTargetEffect(GameObject target, Weapon targetWeapon, bool isShield) {
    }

    public override void OnHitAction(Combat shooter, Weapon shooterWeapon) {
        IncreaseHeat(shooterWeapon.GetRawDamage() / 10);//TODO : improve this
    }

    public override void PlayOnHitEffect() {
        AudioSource.PlayOneShot(OnHitSound);

        OnHitEffect.Play();
    }

    public override void OnOverHeatAction(bool b) {
        if(OverheatEffect==null)return;

        if (b) {
            var main = OverheatEffect.main;
            main.duration = HeatBar.GetCooldownLength(weapPos);

            OverheatEffect.Play();
        } else {
            OverheatEffect.Stop();
        }        
    }

    public override int ProcessDamage(int damage, AttackType attackType) {
        int newDmg = damage;
        float efficiencyCoeff = (IsOverHeat())? 1.5f : 1, efficiency = 1;

        switch (attackType) {
            case AttackType.Melee:
            efficiency = Mathf.Clamp(((ShieldData)data).defend_melee_efficiency * efficiencyCoeff, 0, 1);
            newDmg = (int)(newDmg * efficiency);
            break;
            case AttackType.Ranged:
            case AttackType.Cannon:
            case AttackType.Rocket:
            efficiency = Mathf.Clamp(((ShieldData)data).defend_ranged_efficiency * efficiencyCoeff, 0, 1);
            newDmg = (int)(newDmg * efficiency);
            break;
            case AttackType.None:
            break;
            default:
            break;
        }

        return newDmg;
    }

    public override bool IsShield() {
        return true;
    }

    public override void OnDestroy() {
        base.OnDestroy();
    }

    protected override void InitAttackType() {
        attackType = AttackType.None;
    }

    public override void OnStateCallBack(int type, MechStateMachineBehaviour state) {
        throw new System.NotImplementedException();
    }
}