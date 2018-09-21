using UnityEngine;

public abstract class RangedWeapon : Weapon {
    protected GameObject BulletPrefab, MuzzlePrefab;
    protected ParticleSystem Muzzle;

    protected Transform EffectEnd;
    protected Camera MechCam;
    protected Crosshair Crosshair;

    protected int AtkAnimHash;
    protected bool atkAnimationIsPlaying = false;
    protected float startShootTime;

    public enum StateCallBackType { ReloadStateEnter, AttackStateEnter, AttackStateUpdate, AttackStateExit }

    public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator) {
        base.Init(data, hand, handTransform, Cbt, Animator);

        InitComponents();
        InitAtkAnimHash();

        UpdateAnimationSpeed();
        UpdateMuzzleEffect();
    }

    protected override void InitDataRelatedVars(WeaponData data) {
        base.InitDataRelatedVars(data);

        BulletPrefab = ((RangedWeaponData)data).bulletPrefab;
        MuzzlePrefab = ((RangedWeaponData)data).muzzlePrefab;
    }

    private void InitComponents() {
        MechCam = Cbt.GetCamera();
        FindEffectEnd();

        AttachMuzzle(EffectEnd);

        if (MechCam != null) Crosshair = MechCam.GetComponent<Crosshair>();
    }

    protected virtual void InitAtkAnimHash() {
        AtkAnimHash = (hand == 0) ? Animator.StringToHash("AtkL") : Animator.StringToHash("AtkR");
    }

    protected virtual void FindEffectEnd() {
        EffectEnd = TransformExtension.FindDeepChild(weapon.transform, "EffectEnd");
        if (EffectEnd == null) { Debug.LogError("Can't find EffectEnd on this weapon : " + data.weaponName); };
    }

    protected virtual void AttachMuzzle(Transform Effect_End) {
        Muzzle = Object.Instantiate(MuzzlePrefab, Effect_End).GetComponent<ParticleSystem>();
        TransformExtension.SetLocalTransform(Muzzle.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
    }

    //Update muzzle particle system to fit the rate
    protected abstract void UpdateMuzzleEffect();

    //Play specific animator state
    protected abstract void UpdateMechArmState();

    //Adjust the animation to fit the rate
    protected abstract void UpdateAnimationSpeed();

    public override void HandleCombat() {
        if (!Input.GetKey(BUTTON) || IsOverHeat()) {
            return;
        }

        if (anotherWeapon != null && !anotherWeapon.allowBothWeaponUsing && anotherWeapon.isFiring) return;

        if (Time.time - timeOfLastUse >= 1 / rate) {
            FireRaycast(MechCam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), MechCam.transform.forward, hand);

            IncreaseHeat(data.heat_increase_amount);

            timeOfLastUse = Time.time;
        }
    }

    protected virtual void OnRateChanged() {
        UpdateAnimationSpeed();
        UpdateMuzzleEffect();
    }

    public override void HandleAnimation() {
        if (isFiring) {
            if (Time.time - startShootTime >= 1 / rate) {
                if (atkAnimationIsPlaying) {
                    atkAnimationIsPlaying = false;
                    MechAnimator.SetBool(AtkAnimHash, false);
                }
            } else {
                if (!atkAnimationIsPlaying) {
                    MechAnimator.SetBool(AtkAnimHash, true);
                    atkAnimationIsPlaying = true;
                }
            }
        } else {
            if (atkAnimationIsPlaying) {
                MechAnimator.SetBool(AtkAnimHash, false);
                atkAnimationIsPlaying = false;
            }
        }
    }

    public override void OnSwitchedWeaponAction(bool b) {
        if (b) {
            UpdateMechArmState();
        }
    }

    protected virtual void FireRaycast(Vector3 start, Vector3 direction, int hand) {
        Transform target = ((hand == 0) ? Crosshair.getCurrentTargetL() : Crosshair.getCurrentTargetR());

        if (target != null) {
            PhotonView targetPv = target.transform.root.GetComponent<PhotonView>();

            if (target.tag != "Shield") {
                playerPv.RPC("Shoot", PhotonTargets.All, weapPos, direction, targetPv.viewID, -1);
            } else {//check what hand is it
                ShieldActionReceiver shieldActionReceiver = target.parent.GetComponent<ShieldActionReceiver>();
                int targetShieldPos = shieldActionReceiver.GetPos();

                playerPv.RPC("Shoot", PhotonTargets.All, weapPos, direction, targetPv.viewID, targetShieldPos);
            }
        } else {
            playerPv.RPC("Shoot", PhotonTargets.All, weapPos, direction, -1, -1);
        }
    }

    public virtual void Shoot(Vector3 direction, int targetPvId, int targetWeapPos) {
        MechAnimator.SetBool(AtkAnimHash, true);
        WeaponAnimator.SetTrigger("Atk");

        isFiring = true;
        startShootTime = Time.time;

        GameObject target = null;
        PhotonView targetPv = PhotonView.Find(targetPvId);
        if(targetPv!=null) target = targetPv.gameObject;


        if (target != null) {
            Combat targetCbt = target.GetComponent<Combat>();
            targetCbt.OnHit(data.damage, playerPv.viewID, weapPos, targetWeapPos);

            DisplayBullet(direction, target, (targetWeapPos == -1) ? null : targetCbt.GetWeapon(targetWeapPos));

            Cbt.IncreaseSP(data.SPincreaseAmount);
        } else {
            DisplayBullet(direction, null, null);
        }
    }

    protected abstract void DisplayBullet(Vector3 direction, GameObject target, Weapon targetWeapon);
}