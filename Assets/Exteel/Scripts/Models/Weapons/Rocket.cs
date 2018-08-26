using UnityEngine;

public class Rocket : RangedWeapon {
    public override void AttackTarget(GameObject target, bool isShield) {
        throw new System.NotImplementedException();
    }

    protected override void LoadSoundClips() {
        throw new System.NotImplementedException();
    }
}

//        case (int)GeneralWeaponTypes.Rocket:
//        if (!Input.GetKeyDown(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset]) {
//            if (Time.time - timeOfLastShotL >= 0.4f)//0.4 < time of playing shoot animation once , to make sure other player catch this
//                setIsFiring(hand, false);
//            return;
//        }
//        break;


//        case (int)GeneralWeaponTypes.Rocket:
//        if (Time.time - timeOfLastShotL >= 1 / bm.WeaponDatas[weaponOffset + hand].Rate) {
//            setIsFiring(hand, true);
//            HeatBar.IncreaseHeatBarL(25);

//            FireRaycast(MainCam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), MainCam.transform.forward, hand);
//            timeOfLastShotL = Time.time;
//        }
//        break;