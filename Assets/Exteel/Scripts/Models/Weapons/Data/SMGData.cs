using UnityEngine;
using Weapons;
using Weapons.Crosshairs;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/SMG", order = 3)]
public class SMGData : RangedWeaponData {
    [SerializeField]private AnimationClip Atk, Reload;
    public int bulletNum = 6;

    SMGData() {
        WeaponType = typeof(SMG);
        attackType = Weapon.AttackType.Ranged;
        AllowBothWeaponUsing = true;
        Slowdown = false;
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
        return new SMG();
    }
}
