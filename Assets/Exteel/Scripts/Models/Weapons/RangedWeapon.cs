using UnityEngine;

public abstract class RangedWeapon : Weapon {
    protected GameObject BulletPrefab;
    protected ParticleSystem Muz;

    protected Transform Effect_End;
    protected Camera MechCam;
    protected Crosshair Crosshair;

    public override void Init(WeaponData data, int hand, Transform handTransform, MechCombat mcbt, Animator Animator) {
        base.Init(data, hand, handTransform, mcbt, Animator);

        BulletPrefab = ((RangedWeaponData)data).bulletPrefab;

    }

    protected override void InitComponents() {
        base.InitComponents();

        MechCam = mcbt.GetCamera();
        FindMuz(Effect_End);
        FindEffectEnd();
        Crosshair = MechCam.GetComponent<Crosshair>();
    }

    private void FindMuz(Transform Effect_End) {
        if (Effect_End != null) {
            Transform MuzTransform = Effect_End.transform.Find("Muz");
            if (MuzTransform != null) {
                Muz = MuzTransform.GetComponent<ParticleSystem>();
            }
        } else {
            Muz = null;
        }
    }

    private void FindEffectEnd() {
        Effect_End = TransformExtension.FindDeepChild(weapon.transform, "EffectEnd");        
    }

    public override void HandleCombat() {
        if (!Input.GetKey(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || !isUsable()) {
            isFiring = false;
            return;
        }

        if(anotherWeapon != null && !anotherWeapon.allowBothWeaponUsing && anotherWeapon.isFiring)return;

        //FireRaycast(MainCam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), MainCam.transform.forward, hand);

        //Increase Heat       

        timeOfLastUse = Time.time;
    }

    public override void HandleAnimation() {
    }

    protected virtual bool isUsable() {
        return (Time.time - timeOfLastUse >= 1 / data.Rate);
    }

    public override void OnDestroy() {
        
    }


    protected virtual void PlayShotSound() {

    }

    protected virtual void PlayReloadSound() {

    }

    public override void OnSwitchedWeaponAction() {
        throw new System.NotImplementedException();
    }

    public virtual void Shoot(int hand, Vector3 direction, int target_pvID, bool isShield, int target_handOnShield) {
        SetTargetInfo(hand, direction, target_pvID, isShield, target_handOnShield);

        //string clipName = "";

        //switch (curSpecialWeaponTypes[weaponOffset + hand]) {
        //    case (int)SpecialWeaponTypes.APS:
        //    clipName += "APS";
        //    break;
        //    case (int)SpecialWeaponTypes.Rifle:
        //    clipName += "BRF";
        //    break;
        //    case (int)SpecialWeaponTypes.Shotgun:
        //    clipName += "SGN";
        //    break;
        //    case (int)SpecialWeaponTypes.LMG:
        //    clipName += "LMG";
        //    break;
        //    case (int)SpecialWeaponTypes.Rocket:
        //    clipName += "RCL";
        //    animator.Play("RCLShootR", 2);
        //    break;
        //    case (int)SpecialWeaponTypes.Rectifier:
        //    clipName += "ENG";
        //    break;
        //    case (int)SpecialWeaponTypes.Cannon:
        //    return;
        //    default:
        //    Debug.LogError("Should never get here with type : " + curSpecialWeaponTypes[weaponOffset + hand]);
        //    return;
        //}
        //clipName += "Shoot" + ((hand == 0) ? "L" : "R");
        //animator.Play(clipName, hand + 1, 0);
    }

    protected virtual void SetTargetInfo(int hand, Vector3 direction, int target_pvID, bool isShield, int target_handOnShield) {
        //    if (playerPVid != -1) {
        //        GameObject target = PhotonView.Find(playerPVid).gameObject;

        //        if (isShield) {
        //            if (target.tag != "Drone")
        //                Targets[hand] = target.GetComponent<BuildMech>().weapons[target.GetComponent<MechCombat>().GetCurrentWeaponOffset() + target_handOnShield];
        //            else
        //                Targets[hand] = target.GetComponent<DroneCombat>().Shield.gameObject;
        //        } else
        //            Targets[hand] = target;

        //        isTargetShield[hand] = isShield;
        //        target_HandOnShield[hand] = target_handOnShield;
        //        bullet_directions[hand] = direction;
        //    } else {
        //        Targets[hand] = null;
        //        bullet_directions[hand] = direction;
        //    }
    }

    //public void InstantiateBulletTrace(int hand) {//aniamtion event driven
    //    if (bullets[weaponOffset + hand] == null) {
    //        Debug.LogWarning("bullet is null");
    //        return;
    //    }
    //    //Play Muz
    //    if (Muz[weaponOffset + hand] != null) {
    //        Muz[weaponOffset + hand].Play();
    //    }
    //    //Play Sound
    //    MechSoundController.PlayShot(hand);

    //    if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Rocket) {
    //        if (photonView.isMine) {
    //            GameObject bullet = PhotonNetwork.Instantiate(bm.WeaponDatas[weaponOffset].GetWeaponName() + "B", transform.position + new Vector3(0, 5, 0) + transform.forward * 10, Quaternion.LookRotation(bullet_directions[hand]), 0);
    //            RCLBulletTrace bulletTrace = bullet.GetComponent<RCLBulletTrace>();
    //            bulletTrace.SetShooterInfo(gameObject, MainCam);
    //            bulletTrace.SetBulletPropertis(weaponScripts[weaponOffset].damage, ((RocketData)weaponScripts[weaponOffset]).bullet_speed, ((RocketData)weaponScripts[weaponOffset]).impact_radius);
    //            bulletTrace.SetSPIncreaseAmount(bm.WeaponDatas[weaponOffset + hand].SPincreaseAmount);
    //        }
    //    } else if (curGeneralWeaponTypes[weaponOffset + hand] == (int)GeneralWeaponTypes.Rectifier) {
    //        GameObject bullet = Instantiate(bullets[weaponOffset + hand], Effect_Ends[weaponOffset + hand].position, Quaternion.LookRotation(bullet_directions[hand])) as GameObject;
    //        ElectricBolt eb = bullet.GetComponent<ElectricBolt>();
    //        bullet.transform.SetParent(Effect_Ends[weaponOffset + hand]);
    //        bullet.transform.localPosition = Vector3.zero;
    //        eb.SetCamera(MainCam);
    //        eb.SetTarget((Targets[hand] == null) ? null : Targets[hand].transform);
    //    } else {
    //        GameObject b = bullets[weaponOffset + hand];
    //        MechCombat mcbt = (Targets[hand] == null) ? null : Targets[hand].transform.root.GetComponent<MechCombat>();

    //        if (photonView.isMine) {
    //            crosshair.CallShakingEffect(hand);
    //            if (Targets[hand] != null && !CheckTargetIsDead(Targets[hand])) {
    //                //only APS & LMG have multiple msgs.
    //                if (curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.APS || curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.LMG) {
    //                    if (!isTargetShield[hand]) {
    //                        if (mcbt != null)
    //                            mcbt.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, MainCam);
    //                        else
    //                            Targets[hand].transform.root.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, MainCam);
    //                    } else {
    //                        if (mcbt != null)
    //                            mcbt.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, MainCam);
    //                        else
    //                            Targets[hand].transform.root.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, MainCam);
    //                    }
    //                }
    //            }
    //        }

    //        GameObject bullet = Instantiate(b, Effect_Ends[weaponOffset + hand].position, Quaternion.identity, BulletCollector.transform) as GameObject;
    //        BulletTrace bulletTrace = bullet.GetComponent<BulletTrace>();
    //        bulletTrace.SetStartDirection(MainCam.transform.forward);

    //        if (curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.Cannon)
    //            bulletTrace.interactWithTerrainWhenOnTarget = false;

    //        if (Targets[hand] != null) {
    //            if (isTargetShield[hand]) {
    //                bulletTrace.SetTarget(Targets[hand].transform, true);
    //            } else {
    //                bulletTrace.SetTarget(Targets[hand].transform, false);
    //            }
    //        } else {
    //            bulletTrace.SetTarget(null, false);
    //        }
    //    }
    //}

    //private void FireRaycast(Vector3 start, Vector3 direction, int hand) {
    //    Transform target = ((hand == 0) ? crosshair.getCurrentTargetL() : crosshair.getCurrentTargetR());//target might be shield collider
    //    int damage = bm.WeaponDatas[weaponOffset + hand].damage;

    //    if (target != null) {
    //        PhotonView targetpv = target.transform.root.GetComponent<PhotonView>();
    //        int target_viewID = targetpv.viewID;
    //        string weaponName = bm.curWeaponNames[weaponOffset + hand];

    //        if (curGeneralWeaponTypes[weaponOffset + hand] != (int)GeneralWeaponTypes.Rectifier) {
    //            if (target.tag != "Shield") {//not shield => player or drone
    //                photonView.RPC("Shoot", PhotonTargets.All, hand, direction, target_viewID, false, -1);

    //                targetpv.RPC("OnHit", PhotonTargets.All, damage, photonView.viewID, weaponName, weaponScripts[weaponOffset + hand].slowDown);

    //                if (target.gameObject.GetComponent<Combat>().CurrentHP <= 0) {
    //                    targetpv.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.KILL, MainCam);
    //                } else {
    //                    targetpv.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, MainCam);
    //                }
    //            } else {
    //                //check what hand is it
    //                ShieldUpdater shieldUpdater = target.parent.GetComponent<ShieldUpdater>();
    //                int target_handOnShield = shieldUpdater.GetHand();

    //                photonView.RPC("Shoot", PhotonTargets.All, hand, direction, target_viewID, true, target_handOnShield);

    //                MechCombat targetMcbt = target.transform.root.GetComponent<MechCombat>();

    //                if (targetMcbt != null) {
    //                    if (targetMcbt.is_overheat[targetMcbt.weaponOffset + target_handOnShield]) {
    //                        targetpv.RPC("ShieldOnHit", PhotonTargets.All, damage, photonView.viewID, target_handOnShield, weaponName);
    //                    } else {
    //                        targetpv.RPC("ShieldOnHit", PhotonTargets.All, (int)(damage * shieldUpdater.GetDefendEfficiency(false)), photonView.viewID, target_handOnShield, weaponName);
    //                    }
    //                } else {//target is drone
    //                    targetpv.RPC("ShieldOnHit", PhotonTargets.All, (int)(damage * shieldUpdater.GetDefendEfficiency(false)), photonView.viewID, target_handOnShield, weaponName);
    //                }
    //                targetpv.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.DEFENSE, MainCam);
    //            }
    //        } else {//ENG
    //            photonView.RPC("Shoot", PhotonTargets.All, hand, direction, target_viewID, false, -1);

    //            targetpv.RPC("OnHeal", PhotonTargets.All, photonView.viewID, damage);

    //            targetpv.GetComponent<DisplayHitMsg>().Display(DisplayHitMsg.HitMsg.HIT, MainCam);
    //        }

    //        //increase SP
    //        SkillController.IncreaseSP(weaponScripts[weaponOffset + hand].SPincreaseAmount);
    //    } else {
    //        photonView.RPC("Shoot", PhotonTargets.All, hand, direction, -1, false, -1);
    //    }
    //}
}


//        case (int)GeneralWeaponTypes.Ranged:
//        if (curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.APS || curSpecialWeaponTypes[weaponOffset + hand] == (int)SpecialWeaponTypes.LMG) {//has a delay before putting down hands
//            if (!Input.GetKey(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + hand]) {
//                if (hand == LEFT_HAND) {
//                    if (Time.time - timeOfLastShotL >= 1 / bm.WeaponDatas[weaponOffset + hand].Rate * 0.95f)
//                        setIsFiring(hand, false);
//                    return;
//                } else {
//                    if (Time.time - timeOfLastShotR >= 1 / bm.WeaponDatas[weaponOffset + hand].Rate * 0.95f)
//                        setIsFiring(hand, false);
//                    return;
//                }
//            }
//        } else {
//            if (!Input.GetKeyDown(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || is_overheat[weaponOffset + hand]) {
//                if (Time.time - ((hand == 1) ? timeOfLastShotR : timeOfLastShotL) >= 0.1f)//0.1 < time of playing shoot animation once , to make sure other player catch this
//                    setIsFiring(hand, false);
//                return;
//            }
//        }
//        break;


//        case (int)GeneralWeaponTypes.Ranged:
//        if (Time.time - ((hand == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.WeaponDatas[weaponOffset + hand].Rate) {
//            setIsFiring(hand, true);
//            FireRaycast(MainCam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), MainCam.transform.forward, hand);
//            if (hand == 1) {
//                HeatBar.IncreaseHeatBarR(weaponScripts[weaponOffset + hand].heat_increase_amount);
//                timeOfLastShotR = Time.time;
//            } else {
//                HeatBar.IncreaseHeatBarL(weaponScripts[weaponOffset + hand].heat_increase_amount);
//                timeOfLastShotL = Time.time;
//            }
//        }