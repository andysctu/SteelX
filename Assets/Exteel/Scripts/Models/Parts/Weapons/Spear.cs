using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Spear", order = 0)]
public class Spear : MeleeWeapon {
    [SerializeField] private AnimationClip Atk;
    public AudioClip smash_sound = new AudioClip();
    public AudioClip smash_hit_sound = new AudioClip();

    Spear() {
        weaponType = "Spear";
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
}

