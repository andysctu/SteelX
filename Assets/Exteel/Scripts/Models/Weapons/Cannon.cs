using UnityEngine;

public class Cannon : RangedWeapon {

    public override void Init(WeaponData data, int hand, Transform handTransform, MechCombat mcbt, Animator Animator) {
        base.Init(data, hand, handTransform, mcbt, Animator);
        InitComponents();
        ResetAnimationVars();
    }

    protected override void InitComponents() {

    }

    public override void OnSkillAction(bool enter) {
        base.OnSkillAction(enter);
        ResetAnimationVars();
    }

    private void ResetAnimationVars() {
        MechAnimator.SetBool("OnBCN", false);
        MechAnimator.SetBool("BCNLoad", false);

        //TODO : Reset bullet num

        if (mcbt.photonView.isMine) {
            MechAnimator.SetBool("BCNPose", false);
        }
    }

    protected override void LoadSoundClips() {
        throw new System.NotImplementedException();
    }

    public override void AttackTarget(GameObject target, bool isShield) {
        throw new System.NotImplementedException();
    }

    ////BCN // TODO : remake this part
    //public int BCNbulletNum = 2;
    //public bool isOnBCNPose, onSkill = false;
    //private bool on_BCNShoot = false;
    //public bool On_BCNShoot {
    //    get { return on_BCNShoot; }
    //    set {
    //        on_BCNShoot = value;
    //        MechController.onInstantMoving = value;
    //        if (value) BCNbulletNum--;
    //        if (BCNbulletNum <= 0) {
    //            animator.Play("BCN", 1);
    //            animator.Play("BCN", 2);
    //            animator.SetBool("BCNLoad", true);
    //        }
    //    }
    //}
    //private bool isBCNcanceled = false;//check if right click cancel

    //Init BCNbulletNum = 2;

    //Init Combat variable
    //animator.SetBool("OnBCN", false);
    //animator.SetBool("BCNLoad", false);


    //Disable player when not on skill : is mine : animator.SetBool(AnimatorVars.BCNPose_id, false);
}


//        case (int)GeneralWeaponTypes.Cannon:
//        if (Time.time - timeOfLastShotL >= 0.5f)
//            setIsFiring(hand, false);
//        if (Input.GetKeyDown(KeyCode.Mouse1) || is_overheat[weaponOffset]) {//right click cancel BCNPose
//            isBCNcanceled = true;
//            animator.SetBool(AnimatorVars.BCNPose_id, false);
//            return;
//        } else if (Input.GetKey(KeyCode.Mouse0) && !isBCNcanceled && !On_BCNShoot && !animator.GetBool(AnimatorVars.BCNPose_id) && MechController.grounded && !animator.GetBool("BCNLoad")) {
//            AnimationEventController.BCNPose();
//            animator.SetBool(AnimatorVars.BCNPose_id, true);
//            timeOfLastShotL = Time.time - 1 / bm.WeaponDatas[weaponOffset + hand].Rate / 2;
//        } else if (!Input.GetKey(KeyCode.Mouse0)) {
//            isBCNcanceled = false;
//        }
//        break;


//        case (int)GeneralWeaponTypes.Cannon:
//        if (Time.time - timeOfLastShotL >= 1 / bm.WeaponDatas[weaponOffset + hand].Rate && isOnBCNPose) {
//            if (Input.GetKey(KeyCode.Mouse0) || !animator.GetBool(AnimatorVars.BCNPose_id) || !MechController.grounded)
//                return;

//            setIsFiring(hand, true);
//            HeatBar.IncreaseHeatBarL(45);
//            //TODO : check the start position : cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH)
//            FireRaycast(MainCam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), MainCam.transform.forward, hand);
//            timeOfLastShotL = Time.time;
//        }
//        break;


//            case (int)GeneralWeaponTypes.Cannon:
//            animator.SetBool(AnimatorVars.BCNShoot_id, true);
//            break;
//ELSE
//        else if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Cannon)
//            animator.SetBool(AnimatorVars.BCNShoot_id, false);