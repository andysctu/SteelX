using UnityEngine;
using Weapons;
using Weapons.Crosshairs;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Shotgun", order = 3)]
public class ShotgunData : RangedWeaponData {
    [SerializeField] private AnimationClip Atk, Reload;

    ShotgunData() {
        WeaponType = typeof(Shotgun);
        attackType = Weapon.AttackType.Ranged;
        AllowBothWeaponUsing = true;
        Slowdown = true;
        IsTwoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)weaponAniamtor.runtimeAnimatorController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["Atk"] = Atk;
        clipOverrides["Reload"] = Reload;
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public override Crosshair GetCrosshair() {
        return new NCrosshair();
    }

    public override object GetWeaponObject() {
        return new Shotgun();
    }
}