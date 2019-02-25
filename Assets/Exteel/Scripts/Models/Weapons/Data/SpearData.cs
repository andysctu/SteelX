using UnityEngine;
using Weapons;

[CreateAssetMenu(fileName = "New Spear", menuName = "WeaponDatas/Spear", order = 0)]
public class SpearData : MeleeWeaponData {
    [SerializeField] private AnimationClip Atk;
    public AudioClip SmashSound = new AudioClip();
    public AudioClip HitSound = new AudioClip();
    public float[] AnimationLengths = new float[2];//0 : normal , 1 : air

    SpearData() {
        WeaponType = typeof(Spear);
        attackType = Weapon.AttackType.Melee;
        AllowBothWeaponUsing = false;
        Slowdown = true;
        IsTwoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAnimator) {
        AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)weaponAnimator.runtimeAnimatorController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["Atk"] = Atk;

        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public override object GetWeaponObject() {
        return new Spear();
    }
}

