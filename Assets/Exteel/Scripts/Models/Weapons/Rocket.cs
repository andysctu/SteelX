using UnityEngine;

public class Rocket : RangedWeapon {
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

//    if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Rocket) {
//        if (photonView.isMine) {
//            GameObject bullet = PhotonNetwork.Instantiate(bm.WeaponDatas[weaponOffset].GetWeaponName() + "B", transform.position + new Vector3(0, 5, 0) + transform.forward * 10, Quaternion.LookRotation(bullet_directions[hand]), 0);
//            RCLBulletTrace bulletTrace = bullet.GetComponent<RCLBulletTrace>();
//            bulletTrace.SetShooterInfo(gameObject, MainCam);
//            bulletTrace.SetBulletPropertis(weaponScripts[weaponOffset].damage, ((RocketData)weaponScripts[weaponOffset]).bullet_speed, ((RocketData)weaponScripts[weaponOffset]).impact_radius);
//            bulletTrace.SetSPIncreaseAmount(bm.WeaponDatas[weaponOffset + hand].SPincreaseAmount);
//        }