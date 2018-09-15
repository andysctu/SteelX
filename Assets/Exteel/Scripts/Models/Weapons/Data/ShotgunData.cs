﻿using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponDatas/Shotgun", order = 3)]
public class ShotgunData : RangedWeaponData {
    [SerializeField] private AnimationClip Atk, Reload;

    ShotgunData() {
        WeaponType = typeof(Shotgun);
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

    public override object GetWeaponObject() {
        return new Shotgun();
    }
}