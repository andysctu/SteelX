using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Rocket", order = 5)]
public class Rocket : RangedWeapon {
    [SerializeField] private AnimationClip Atk, Reload;

    [Range(0,20)]
    public int impact_radius;

    [Range(150,350)]
    public int bullet_speed;

    Rocket() {
        weaponType = "Rocket";
        slowDown = true;
        twoHanded = true;
        impact_radius = 6;
        bullet_speed = 200;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)weaponAniamtor.runtimeAnimatorController;

        AnimationClipOverrides clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["Atk"] = Atk;
        clipOverrides["Reload"] = Reload;
        if(Atk == null || Reload == null) {
            Debug.Log("You need to assign rocket animation clip : Atk || Reload");
        }

        animatorOverrideController.ApplyOverrides(clipOverrides);
    }
}