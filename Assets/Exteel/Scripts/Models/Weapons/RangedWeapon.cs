using UnityEngine;
using Weapons;

public abstract class RangedWeapon : Weapon {
    protected GameObject BulletPrefab, MuzzlePrefab;
    protected ParticleSystem Muzzle;

    protected Transform EffectEnd;
    protected Camera MechCam;
    protected CrosshairController Crosshair;

    protected int AtkAnimHash;
    protected bool atkAnimationIsPlaying = false;
    protected float startShootTime;

    public enum StateCallBackType {AttackStateEnter, AttackStateUpdate, AttackStateExit , ReloadStateEnter, ReloadStateExit, PoseStateEnter, PoseStateExit }

    public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator) {
        base.Init(data, hand, handTransform, Cbt, Animator);

        InitComponents();
        InitAtkAnimHash();
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

        if (MechCam != null) Crosshair = MechCam.GetComponent<CrosshairController>();
    }

    protected virtual void InitAtkAnimHash() {
        AtkAnimHash = (Hand == 0) ? Animator.StringToHash("AtkL") : Animator.StringToHash("AtkR");
    }

    protected virtual void FindEffectEnd() {
        EffectEnd = TransformExtension.FindDeepChild(weapon.transform, "EffectEnd");
        if (EffectEnd == null) { Debug.LogError("Can't find EffectEnd on this weapon : " + data.weaponName); };
    }

    protected virtual void AttachMuzzle(Transform Effect_End) {
        Muzzle = Object.Instantiate(MuzzlePrefab, Effect_End).GetComponent<ParticleSystem>();
        TransformExtension.SetLocalTransform(Muzzle.transform, Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
    }

    public override void HandleCombat() {
        if (!Input.GetKey(BUTTON) || IsOverHeat()) return;

        if (AnotherWeapon != null && !AnotherWeapon.AllowBothWeaponUsing && AnotherWeapon.IsFiring) return;

        if (Time.time - TimeOfLastUse >= 1 / Rate) {
            FireRaycast(MechCam.transform.TransformPoint(0, 0, CrosshairController.CamDistanceToMech), MechCam.transform.forward, Hand);

            IncreaseHeat(data.HeatIncreaseAmount);

            TimeOfLastUse = Time.time;
        }
    }

    public override void HandleAnimation() {
        if (IsFiring) {
            if (Time.time - startShootTime >= 1 / Rate) {
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

    public override void OnWeaponSwitchedAction(bool isThisWeaponActivated) {
        if (isThisWeaponActivated) {
            UpdateMechArmState();
        }
    }

    public override void OnSkillAction(bool enter){
        base.OnSkillAction(enter);
        if (!enter) {
            UpdateMechArmState();
        }
    }

    protected abstract void UpdateMechArmState() ;

    protected virtual void FireRaycast(Vector3 start, Vector3 direction, int hand) {
        Transform target = ((hand == 0) ? Crosshair.GetCurrentTargetL() : Crosshair.GetCurrentTargetR());

        if (target != null) {
            PhotonView targetPv = target.transform.root.GetComponent<PhotonView>();

            if (target.tag != "Shield") {
                PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, targetPv.viewID, -1);
            } else {//check what hand is it
                ShieldActionReceiver shieldActionReceiver = target.parent.GetComponent<ShieldActionReceiver>();
                int targetShieldPos = shieldActionReceiver.GetPos();

                PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, targetPv.viewID, targetShieldPos);
            }
        } else {
            PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, -1, -1);
        }
    }

    public virtual void Shoot(Vector3 direction, int targetPvId, int targetWeapPos) {
        MechAnimator.SetBool(AtkAnimHash, true);
        WeaponAnimator.SetTrigger("Atk");

        IsFiring = true;
        startShootTime = Time.time;

        GameObject target = null;
        PhotonView targetPv = PhotonView.Find(targetPvId);
        if(targetPv!=null) target = targetPv.gameObject;

        if (target != null) {
            Combat targetCbt = target.GetComponent<Combat>();
            targetCbt.OnHit(data.damage, PlayerPv.viewID, WeapPos, targetWeapPos);

            DisplayBullet(direction, target, (targetWeapPos == -1) ? null : targetCbt.GetWeapon(targetWeapPos));

            Cbt.IncreaseSP(data.SpIncreaseAmount);//TODO : check this
        } else {
            DisplayBullet(direction, null, null);
        }
    }

    protected abstract void DisplayBullet(Vector3 direction, GameObject target, Weapon targetWeapon);
}