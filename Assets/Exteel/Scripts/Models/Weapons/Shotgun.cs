using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Shotgun", order = 3)]
public class Shotgun : RangedWeapon {
    [SerializeField] private AnimationClip Atk, Reload;

    Shotgun() {
        weaponType = "Shotgun";
        slowDown = true;
        twoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)weaponAniamtor.runtimeAnimatorController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["Atk"] = Atk;
        clipOverrides["Reload"] = Reload;
        if (Atk == null || Reload == null) {
            Debug.Log("You need to assign Shotgun animation clip : Atk || Reload . Ignore this if use empty clips.");
        }
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }
}