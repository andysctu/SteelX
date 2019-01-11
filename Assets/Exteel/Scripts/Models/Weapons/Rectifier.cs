using UnityEngine;

public class Rectifier : RangedWeapon {
    public override void OnStateCallBack(int type, MechStateMachineBehaviour state) {
        throw new System.NotImplementedException();
    }

    public override void OnTargetEffect(GameObject target, Weapon targetWeapon, bool isShield) {
        throw new System.NotImplementedException();
    }

    protected override void UpdateAnimationSpeed() {
        throw new System.NotImplementedException();
    }

    protected override void DisplayBullet(Vector3 direction, GameObject Target, Weapon targetWeapon) {
        throw new System.NotImplementedException();
    }

    protected override void InitAttackType() {
        throw new System.NotImplementedException();
    }



    protected override void UpdateMuzzleEffect() {
        throw new System.NotImplementedException();
    }

    protected override void LoadSoundClips() {
        throw new System.NotImplementedException();
    }

    protected override void UpdateMechArmState() {
        throw new System.NotImplementedException();
    }
}

//        case (int)GeneralWeaponTypes.Rectifier:
//        if (!Input.GetKey(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + hand]) {
//            if (Time.time - ((hand == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.WeaponDatas[weaponOffset + hand].Rate)
//                setIsFiring(hand, false);
//            return;
//        }
//        break;


//        case (int)GeneralWeaponTypes.Rectifier:
//        if (Time.time - ((hand == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.WeaponDatas[weaponOffset + hand].Rate) {
//            setIsFiring(hand, true);
//            FireRaycast(MainCam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), MainCam.transform.forward, hand);
//            if (hand == 1) {
//                HeatBar.IncreaseHeatBarR(30);
//                timeOfLastShotR = Time.time;
//            } else {
//                HeatBar.IncreaseHeatBarL(30);
//                timeOfLastShotL = Time.time;
//            }
//        }
//        break;
//    }
//}


//    } else if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Rectifier) {
//        GameObject bullet = Instantiate(bullets[weaponOffset + hand], Effect_Ends[weaponOffset + hand].position, Quaternion.LookRotation(bullet_directions[hand])) as GameObject;
//        ElectricBolt eb = bullet.GetComponent<ElectricBolt>();
//        bullet.transform.SetParent(Effect_Ends[weaponOffset + hand]);
//        bullet.transform.localPosition = Vector3.zero;
//        eb.SetCamera(MainCam);
//        eb.SetTarget((Targets[hand] == null) ? null : Targets[hand].transform);