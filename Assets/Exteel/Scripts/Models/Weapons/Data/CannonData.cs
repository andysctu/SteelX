using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Cannon", order = 6)]
public class CannonData : RangedWeaponData {
    [SerializeField] private AnimationClip Reload;

    [Tooltip("Call reload after running out of bullets")]
    public int maxBullet;

    CannonData() {
        WeaponType = typeof(Cannon);
        slowDown = true;
        twoHanded = true;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)weaponAniamtor.runtimeAnimatorController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["Reload"] = Reload;
        if (Reload == null) {
            Debug.Log("You need to assign Cannon animation clip : Reload . Ignore this if use empty clips.");
        }
        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public override object GetWeaponObject() {
        return new Cannon();
    }
}