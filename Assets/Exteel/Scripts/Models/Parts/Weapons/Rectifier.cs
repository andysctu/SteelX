﻿using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Rectifier", order = 4)]
public class Rectifier : RangedWeapon {
    Rectifier() {
        weaponType = "Rectifier";
        slowDown = false;
        twoHanded = false;
    }

    public override void SwitchAnimationClips(Animator weaponAniamtor) {
        throw new System.NotImplementedException();
    }
}