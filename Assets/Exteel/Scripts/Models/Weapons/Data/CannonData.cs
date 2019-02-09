using UnityEngine;
using Weapons;
using Weapons.Crosshairs;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Cannon", order = 6)]
public class CannonData : RangedWeaponData {
    [SerializeField] private AnimationClip _reloadClip;

    [Tooltip("Call reload after shooting MaxBullet times")]
    public int MaxBullet;
    public float ReloadAnimationLength;

    CannonData() {
        WeaponType = typeof(Cannon);
        attackType = Weapon.AttackType.Cannon;
        AllowBothWeaponUsing = false;
        Slowdown = true;
        IsTwoHanded = true;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)weaponAniamtor.runtimeAnimatorController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["Reload"] = _reloadClip;
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public override Crosshair GetCrosshair(){
        return new NCrosshair();
    }

    public override object GetWeaponObject() {
        return new Cannon();
    }
}