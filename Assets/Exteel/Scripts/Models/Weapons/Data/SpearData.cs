using UnityEngine;
using Weapons;
using Weapons.Crosshairs;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Spear", order = 0)]
public class SpearData : MeleeWeaponData {
    [SerializeField] private AnimationClip Atk;
    public AudioClip smash_sound = new AudioClip();
    public AudioClip smash_hit_sound = new AudioClip();

    SpearData() {
        WeaponType = typeof(Spear);
        attackType = Weapon.AttackType.Melee;
        AllowBothWeaponUsing = false;
        Slowdown = true;
        IsTwoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)weaponAniamtor.runtimeAnimatorController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["Atk"] = Atk;

        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public override Crosshair GetCrosshair() {
        return null;
    }

    public override object GetWeaponObject() {
        return new Spear();
    }
}

