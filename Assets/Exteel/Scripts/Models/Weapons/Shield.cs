using UnityEngine;

public class Shield : Weapon {
    private ShieldActionReceiver ShieldActionReceiver;
    private AudioClip OnHitSound;
    private ParticleSystem OnHitEffect, OverheatEffect;

    private int block_id = 0;

    public enum DefendType { Melee, Ranged, Cannon, Rocket, Skill, Debuff, None }

    public Shield() {
        allowBothWeaponUsing = false;
    }
    
    public override void Init(WeaponData data, int hand, Transform handTransform, MechCombat mcbt, Animator Animator) {
        base.Init(data, hand, handTransform, mcbt, Animator);
        InitComponents();
        AddShieldActionReceiver();
        AttachEffects();
        ResetAnimationVars();
    }

    protected override void InitComponents() {
        base.InitComponents();

        if (AnimatorVars != null)
            block_id = (hand == 0) ? AnimatorVars.blockL_id : AnimatorVars.blockR_id;
    }

    private void AddShieldActionReceiver() {
        ShieldActionReceiver = weapon.AddComponent<ShieldActionReceiver>();
        ShieldActionReceiver.SetPos(hand);
    }

    private void AttachEffects() {
        //OnHitEffect
        OnHitEffect = Object.Instantiate(((ShieldData)data).OnHitEffect, weapon.transform);
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

    public override void OnSwitchedWeaponAction() {
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

    public override void OnTargetEffect(GameObject target, bool isShield) {
        Debug.LogError("This should not get called.");
    }

    public void OnHitAction() {
        AudioSource.PlayOneShot(OnHitSound);

        OnHitEffect.Play();
    }

    public override void OnOverHeatAction(bool b) {
        if(OverheatEffect==null)return;

        if (b) {
            var main = OverheatEffect.main;
            main.duration = HeatBar.GetCooldownLength(pos);

            OverheatEffect.Play();
        } else {
            OverheatEffect.Stop();
        }        
    }

    public int DecreaseDmg(int damage, int attackType) {
        int newDmg = damage;
        float efficiencyCoeff = (IsOverHeat())? 1.5f : 1;

        switch ((DefendType)attackType) {
            case DefendType.Melee:
            newDmg = (int)(newDmg * (((ShieldData)data).defend_melee_efficiency * efficiencyCoeff));
            break;
            case DefendType.Ranged:
            case DefendType.Cannon:
            case DefendType.Rocket:
            newDmg = (int)(newDmg * (((ShieldData)data).defend_ranged_efficiency * efficiencyCoeff));
            break;
            case DefendType.None:
            break;
            default:
            break;
        }

        return newDmg;
    }

    public override void OnDestroy() {
        base.OnDestroy();
    }
}