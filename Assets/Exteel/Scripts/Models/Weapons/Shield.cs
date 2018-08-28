using UnityEngine;

public class Shield : Weapon {
    private ShieldActionReceiver ShieldActionReceiver;
    private AudioClip OnHitSound;
    private ParticleSystem shieldOnHitEffect;
    private int block_id;

    public Shield() {
        allowBothWeaponUsing = false;
    }

    public override void Init(WeaponData data, int hand, Transform handTransform, MechCombat mcbt, Animator Animator) {
        base.Init(data, hand, handTransform, mcbt, Animator);
        InitComponents();
        AddShieldActionReceiver();
        ResetAnimationVars();        
    }

    protected override void InitComponents() {
        //shieldOnHitEffect
        block_id = (hand == 0)? AnimatorVars.blockL_id : AnimatorVars.blockR_id;
    }

    private void AddShieldActionReceiver() {
        ShieldActionReceiver = weapon.AddComponent<ShieldActionReceiver>();
        ShieldActionReceiver.SetHand(hand);
    }

    private void ResetAnimationVars() {
        isFiring = false;
        MechAnimator.SetBool(block_id, false);
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

        if(anotherWeapon != null && !anotherWeapon.allowBothWeaponUsing && anotherWeapon.isFiring) return;

        isFiring = true;
    }

    public override void HandleAnimation() {
        if (isFiring) {
            if (!MechAnimator.GetBool(block_id)) {
                MechAnimator.SetBool(block_id, true);
            }
        } else {
            if(MechAnimator.GetBool(block_id)) {
                MechAnimator.SetBool(block_id, false);
            }
        }
    }

    protected override void LoadSoundClips() {
        //OnHitSound = ((ShieldData)data)
    }

    public override void OnTargetEffect(GameObject target, bool isShield) {
        Debug.LogError("This should not get called.");
    }

    public void OnHitEffect() {
        //Play Onhit sound


    }

    public override void OnDestroy() {
        base.OnDestroy();
    }
}

/*
     public void SlashOnHitEffect(bool isShield, int hand) {//TODO : remake this
        if (Hands == null) {
            Debug.Log("Hands is null");
            return;
        }

        if (isShield) {
            if (transform.root.tag != "Drone") {
                transform.root.GetComponent<BuildMech>().Weapons[mcbt.GetCurrentWeaponOffset() + hand].GetWeapon().GetComponent<ParticleSystem>().Play();
            }else
                transform.root.GetComponent<DroneCombat>().Shield.GetComponent<ParticleSystem>().Play();
            //GameObject g = Instantiate(shieldOnHit, Hands[hand].position - Hands[hand].transform.forward * 2, Quaternion.identity, Hands[hand]);
            //g.GetComponent<ParticleSystem>().Play();
        } else {
            GameObject g = Instantiate(slashOnHitEffect, transform.position + MECH_MID_POINT, Quaternion.identity, transform);
            g.GetComponent<ParticleSystem>().Play();
        }
    }
     */
