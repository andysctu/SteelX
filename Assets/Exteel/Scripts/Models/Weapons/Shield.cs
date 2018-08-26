using UnityEngine;


public class Shield : Weapon {

    public Shield() {
        allowBothWeaponUsing = false;
    }

    public override void Init(WeaponData data, int hand, Transform handTransform, MechCombat mcbt, Animator Animator) {
        base.Init(data, hand, handTransform, mcbt, Animator);
        InitComponents();
        ResetAnimationVars();
    }

    protected override void InitComponents() {

    }

    private void ResetAnimationVars() {
        isFiring = false;
        MechAnimator.SetBool("BlockL", false);
        MechAnimator.SetBool("BlockR", false);
    }

    public override void OnSkillAction(bool enter) {
        base.OnSkillAction(enter);
        ResetAnimationVars();
    }

    public override void OnDestroy() {
        base.OnDestroy();
    }

    public override void OnSwitchedWeaponAction() {
        throw new System.NotImplementedException();
    }

    public override void HandleAnimation() {
        //does not process input if
        //animator.GetBool("OnMelee")

        throw new System.NotImplementedException();
    }

    public override void HandleCombat() {
        //Shield updater's update
        //    bulletPrefabs[i] = null;
        //    ShieldUpdater shieldUpdater = weapons[i].GetComponentInChildren<ShieldUpdater>();
        //    shieldUpdater.SetDefendEfficiency(((ShieldData)weaponDatas[i]).defend_melee_efficiency, ((ShieldData)weaponDatas[i]).defend_ranged_efficiency);
        //    shieldUpdater.SetHand(i % 2);
    }

    protected override void LoadSoundClips() {
        throw new System.NotImplementedException();
    }

    public override void AttackTarget(GameObject target, bool isShield) {
        throw new System.NotImplementedException();
    }
}

//        case (int)GeneralWeaponTypes.Shield:
//        if (!Input.GetKey(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || getIsFiring((hand + 1) % 2)) {
//            setIsFiring(hand, false);
//            return;
//        }
//        break;


//        case (int)GeneralWeaponTypes.Shield:
//        if (!getIsFiring((hand + 1) % 2))
//            setIsFiring(hand, true);
//        break;

//            case (int)GeneralWeaponTypes.Shield:
//            animator.SetBool((hand == 0) ? AnimatorVars.blockL_id : AnimatorVars.blockR_id, true);
//            break;
//ELSE 
//        if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Shield)
//            animator.SetBool((hand == 0) ? AnimatorVars.blockL_id : AnimatorVars.blockR_id, false);