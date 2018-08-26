using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Spear", order = 0)]
public class SpearData : MeleeWeaponData {
    [SerializeField] private AnimationClip Atk;
    public AudioClip smash_sound = new AudioClip();
    public AudioClip smash_hit_sound = new AudioClip();

    SpearData() {
        WeaponType = typeof(Spear);
        slowDown = true;
        twoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)weaponAniamtor.runtimeAnimatorController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["Atk"] = Atk;
        if (Atk == null) {
            Debug.Log("You need to assign spear animation clip : Atk . Ignore this if use empty animation");
        }

        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    public override object GetWeaponObject() {
        return new Spear();
    }
}

