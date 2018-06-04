using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/SMG", order = 3)]
public class SMG : RangedWeapon {
    [SerializeField]private AnimationClip Atk, Reload;

    SMG() {
        slowDown = false;
        twoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(weaponAniamtor.runtimeAnimatorController);
        weaponAniamtor.runtimeAnimatorController = animatorOverrideController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["Atk"] = Atk;
        clipOverrides["Reload"] = Reload;
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }
}
